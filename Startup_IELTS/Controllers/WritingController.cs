using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Startup_IELTS.DTOs;
using Startup_IELTS.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Startup_IELTS.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/writing")]
    public class WritingController : Controller
    {

        private readonly StartupIeltsContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        public WritingController(StartupIeltsContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet("getwritings")]
        public async Task<IActionResult> GetAll(int page = 1,int pageSize = 10,string? search = null,string? taskType = null,string? status = "all")
        {
            var query = _db.Writings.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Question.Contains(search));

            if (!string.IsNullOrEmpty(taskType) && taskType != "all")
                query = query.Where(x => x.TaskType == taskType);

            if (status == "visible")
                query = query.Where(x => x.Hide == false || x.Hide == null);
            else if (status == "hidden")
                query = query.Where(x => x.Hide == true);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Writing>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = items
            });
        }

        [HttpGet("getWritingById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var writing = await _db.Writings.FindAsync(id);
            if (writing == null) return NotFound();

            return Ok(writing);
        }
        [HttpPost("getresult")]
        public async Task<IActionResult> GetResult([FromBody]WritingTest data)
        {
            // groq_api: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
            //Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");

            string apiKey = "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";

            string endpoint = "https://api.groq.com/openai/v1/chat/completions";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            string finalPrompt;

            finalPrompt = $@"
You are a certified IELTS Examiner.

Task Type: {data.taskType}

Question:
{data.taskquestion}

Candidate's Essay:
{data.taskcontent}

You must evaluate this essay strictly according to official IELTS Writing Band Descriptors.

LANGUAGE REQUIREMENT (VERY IMPORTANT):
- All descriptions, explanations, and evaluations MUST be written in Vietnamese.
- Use formal academic Vietnamese.
- Do NOT mix English and Vietnamese (except extracted original phrases from essay).
- Band scores remain numeric.

CRITICAL INSTRUCTIONS:
- Return ONLY valid JSON.
- No markdown.
- No backticks.
- No comments.
- No explanation outside JSON.
- If something does not exist, return empty array [].
- Never fabricate errors.

SCORING RULES:
- Band range: 0 - 9
- Use 0.5 increments only (e.g., 6.0, 6.5, 7.0)
- overall = average of TR, CC, LR, GRA (rounded to nearest 0.5)

SEVERITY LEVEL DEFINITION:
- red = lỗi nghiêm trọng ảnh hưởng lớn đến band điểm
- yellow = lỗi trung bình ảnh hưởng đến độ rõ ràng hoặc chính xác
- blue = lỗi nhỏ, ảnh hưởng hạn chế

JSON STRUCTURE REQUIRED:

{{
  ""overall"": number,
  ""tr"": number,
  ""cc"": number,
  ""lr"": number,
  ""gra"": number,

  ""trDetail"": {{
    ""description"": ""3-5 câu nhận xét học thuật bằng tiếng Việt về Task Response."",
    ""json"": {{
      ""tr"": number,
      ""detail"": {{
        ""ClarityOfPosition"": number,
        ""DepthOfIdeas"": number,
        ""AppropriatenessOfFormat"": number,
        ""RelevantSpecificExamples"": number,
        ""appropriateWordCount"": number
      }}
    }}
  }},

  ""ccDetail"": {{
    ""description"": ""3-5 câu nhận xét học thuật bằng tiếng Việt về Coherence & Cohesion."",
    ""json"": {{
      ""cc"": number,
      ""detail"": {{
        ""LogicalOrganization"": number,
        ""EffectiveIntroductionConclusion"": number,
        ""SupportedMainPoints"": number,
        ""CohesiveDevicesUsage"": number,
        ""Paragraphing"": number
      }}
    }}
  }},

  ""lrDetail"": {{
    ""description"": ""3-5 câu nhận xét học thuật bằng tiếng Việt về Lexical Resource."",
    ""json"": {{
      ""lr"": number,
      ""detail"": {{
        ""VocabularyRange"": number,
        ""LexicalAccuracy"": number,
        ""SpellingAndWordFormation"": number
      }}
    }}
  }},

  ""graDetail"": {{
    ""description"": ""3-5 câu nhận xét học thuật bằng tiếng Việt về Grammatical Range & Accuracy."",
    ""json"": {{
      ""gra"": number,
      ""detail"": {{
        ""SentenceStructureVariety"": number,
        ""GrammarAccuracy"": number,
        ""PunctuationUsage"": number
      }}
    }}
  }},
 ""correct"": [
    [
      ""type"":""Loại lỗi (Grammar|Vocabulary|Spelling|Structure|Coherence|TaskResponse|Conjunction|Verb Errors|Preposition|Pronoun|Word Order|Plurality|Comparison|Conditional|Clause Errors)"",
      ""level"":""red | yellow | blue""
      ""wrong"":""Lỗi trích nguyên văn"",
      ""fix"" :""Phiên bản sửa đúng BẰNG TIẾNG ANH"",
      ""explain"":""Giải thích 5-7 câu bằng tiếng Việt"",     
    ]
  ],
}}

ERROR DETECTION PROCESS (BẮT BUỘC):

1. Phải phân tích toàn bộ bài theo từng câu một.
2. Trong mỗi câu, xác định TẤT CẢ lỗi tồn tại.
3. Mỗi lỗi riêng biệt phải được liệt kê thành một object riêng.
4. Không được bỏ qua lỗi nhỏ (mạo từ, giới từ, số nhiều, dấu câu).
5. Không được gộp nhiều lỗi trong một object.
6. Sau khi liệt kê xong, phải kiểm tra lại toàn bài lần nữa để đảm bảo không bỏ sót lỗi.

EXHAUSTIVE COVERAGE RULE:

- Bạn phải liệt kê tất cả lỗi có thể xác định được trong bài.
- Nếu bài có 15 lỗi thì phải trả về 15 object.
- Nếu chỉ trả về 2-3 lỗi trong khi bài có nhiều lỗi rõ ràng, output được xem là KHÔNG HỢP LỆ.
- Không được chọn lọc lỗi tiêu biểu.
- Không được tóm tắt lỗi.
- Không được bỏ qua lỗi ngữ pháp nhỏ.

ERROR SENSITIVITY LEVEL: STRICT ACADEMIC MODE

- Phát hiện cả lỗi ngữ pháp nhỏ.
- Phát hiện lỗi collocation không tự nhiên.
- Phát hiện lỗi diễn đạt thiếu học thuật.
- Phát hiện lỗi logic nếu có.

FINAL SELF-CHECK (BẮT BUỘC):

Trước khi trả về kết quả:
- Kiểm tra lại từng câu trong bài.
- Đảm bảo không bỏ sót lỗi nào.
- Nếu phát hiện thêm lỗi, phải thêm vào danh sách.
- Chỉ trả về kết quả khi chắc chắn đã liệt kê toàn bộ lỗi.

VALIDATION RULES:
- Chỉ trích lỗi thực sự tồn tại trong bài.
- ""correct"" liệt kê TẤT CẢ các lỗi có thể xác định được liên quan đến tiêu chí đó.
- Không được tự tạo lỗi nếu bài không có.
- Nếu không có lỗi trả về ""correct"": [].
- Mọi phần mô tả và giải thích phải viết hoàn toàn bằng tiếng Việt.
- Output phải là JSON hợp lệ có thể parse được.
";
            // 🔥 BƯỚC 3: GỬI PROMPT ĐẾN GROQ
            var requestPayload = new
            {
                model = "llama-3.3-70b-versatile", // hoặc mixtral-8x7b-32768
                temperature = 0.7,
                response_format = new { type = "json_object" }, // nếu bạn muốn ép trả JSON
                messages = new[]
                {
                    new { role = "user", content = finalPrompt }
                }
            };

            string json = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            string result = await response.Content.ReadAsStringAsync();

            var groqResponse = JsonSerializer.Deserialize<AIResponse>(
                result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            string aiContent = groqResponse?.choices?[0]?.message?.content;

            if (string.IsNullOrEmpty(aiContent))
            {
                return BadRequest("AI returned empty content");
            }

            // Parse JSON từ AI
            var finalResult = JsonSerializer.Deserialize<object>(aiContent);

            return Ok(finalResult);
        }

        [HttpPost("getvocabularyandstructure")]
        public async Task<IActionResult> GetVocabularyAndStructure([FromBody] WritingImprove data)
        {
            string apiKey = "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";

            string endpoint = "https://api.groq.com/openai/v1/chat/completions";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            string finalPrompt;

            finalPrompt = $@"You are an IELTS Writing expert.

Task:
Generate vocabulary and sentence structures suitable for IELTS Writing.

Topic Category: {data.Category}
Question: {data.Question}
Selected Band Level: {data.Level}

Requirements:

1. Generate vocabulary ONLY for the selected band level: {data.Level}
2. Provide EXACTLY 15 vocabulary items.
3. For each vocabulary item, include:
   - word
   - part_of_speech
   - pronounce
   - meaning_en
   - meaning_vi
   - example_sentence

4. Provide EXACTLY 5 useful sentence structures appropriate for band {data.Level}.
5. For each structure, include:
   - structure_name
   - pattern
   - meaning (explain when to use it)
   - example

6. The response MUST be valid JSON.
7. Return ONLY JSON.
8. No markdown.
9. No explanation text.

Follow EXACTLY this JSON structure:

{{
  ""topic"": ""string"",
  ""band"": ""{data.Level}"",
  ""vocabulary"": [
    {{
      ""word"": ""string"",
      ""part_of_speech"": ""string"",
      ""pronounce"": ""string"",
      ""meaning_en"": ""string"",
      ""meaning_vi"": ""string"",
      ""example_sentence"": ""string""
    }}
  ],
  ""structures"": [
    {{
      ""structure_name"": ""string"",
      ""pattern"": ""string"",
      ""meaning"": ""string"",
      ""example"": ""string""
    }}
  ]
}}";
            // 🔥 BƯỚC 3: GỬI PROMPT ĐẾN GROQ
            var requestPayload = new
            {
                model = "llama-3.3-70b-versatile", // hoặc mixtral-8x7b-32768
                temperature = 0.7,
                response_format = new { type = "json_object" }, // nếu bạn muốn ép trả JSON
                messages = new[]
                {
                    new { role = "user", content = finalPrompt }
                }
            };

            string json = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            string result = await response.Content.ReadAsStringAsync();

            var groqResponse = JsonSerializer.Deserialize<AIResponse>(
                result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            string aiContent = groqResponse?.choices?[0]?.message?.content;

            if (string.IsNullOrEmpty(aiContent))
            {
                return BadRequest("AI returned empty content");
            }

            // Parse JSON từ AI
            var finalResult = JsonSerializer.Deserialize<object>(aiContent);

            return Ok(finalResult);
        }

        [HttpPost("upgradewriting")]
        public async Task<IActionResult> UpgradeWriting([FromBody] WritingTest data)
        {
            string apiKey = "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";

            string endpoint = "https://api.groq.com/openai/v1/chat/completions";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            string finalPrompt;

            finalPrompt = $@"finalPrompt = $@""
You are a certified IELTS Writing examiner.

Your task is to rewrite and upgrade the candidate's essay to Band 7.0–8.0 level.

==============================
GENERAL REQUIREMENTS
==============================
- Maintain the original idea.
- Improve vocabulary to Band 7–8 level.
- Improve grammar accuracy and sentence variety.
- Use formal academic tone.
- Ensure logical coherence and cohesion.
- Do NOT change the main argument.
- Word count:
  • Task 1: at least 170 words
  • Task 2: at least 260 words

Return ONLY a valid JSON object.

==============================
INPUT INFORMATION
==============================
Task Type: {data.taskType}
Task Topic: {data.taskTopic}
Question: {data.taskquestion}

Original Essay:
{data.taskcontent}

==============================
TASK-SPECIFIC INSTRUCTIONS
==============================

IF Task Type = ""Task 1"":
- This is an IELTS Academic Task 1 report.
- Structure must include:
    1. Introduction (paraphrase the question)
    2. Overview (main trends/features)
    3. Body Paragraph 1 (key details)
    4. Body Paragraph 2 (key details)
- Use objective tone.
- Do NOT give opinions.
- Use appropriate data description language.
- Adapt language depending on Task Topic:
    • Line Chart → trends over time
    • Bar Chart → comparisons
    • Pie Chart → proportions
    • Table → numerical comparisons
    • Mixed Graph → compare multiple visuals
    • Map → changes over time or locations
    • Process → stages and sequence

IF Task Type = ""Task 2"":
- This is an IELTS Writing Task 2 essay.
- Structure must include:
    1. Introduction (paraphrase + clear thesis)
    2. Body Paragraph 1
    3. Body Paragraph 2
    4. Conclusion
- Follow the correct structure depending on Task Topic:

    • Agree or Disagree → clear opinion throughout
    • Advantages and Disadvantages → balanced discussion
    • Discussion → discuss both views + opinion
    • Causes Problems and Solutions → explain causes + solutions
    • Part Question → answer all questions fully

- Provide clear topic sentences.
- Use advanced linking devices.
- Include complex sentence structures.

==============================
OUTPUT FORMAT (STRICT JSON)
==============================

Return this structure:

{{
  ""taskType"": ""string"",
  ""bandTarget"": ""7.0-8.0"",
  ""improvedEssay"": ""Full upgraded essay here"",
  ""structureBreakdown"": {{
      ""introduction"": ""text"",
      ""overview_or_thesis"": ""text"",
      ""body1"": ""text"",
      ""body2"": ""text"",
      ""conclusion"": ""text (if Task 2 only, otherwise empty string)""
  }}
}}

Return ONLY JSON.
Do not include explanations.
Do not include markdown.
"";
";
            // 🔥 BƯỚC 3: GỬI PROMPT ĐẾN GROQ
            var requestPayload = new
            {
                model = "llama-3.3-70b-versatile", // hoặc mixtral-8x7b-32768
                temperature = 0.7,
                response_format = new { type = "json_object" }, // nếu bạn muốn ép trả JSON
                messages = new[]
                {
                    new { role = "user", content = finalPrompt }
                }
            };

            string json = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            string result = await response.Content.ReadAsStringAsync();

            var groqResponse = JsonSerializer.Deserialize<AIResponse>(
                result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            string aiContent = groqResponse?.choices?[0]?.message?.content;

            if (string.IsNullOrEmpty(aiContent))
            {
                return BadRequest("AI returned empty content");
            }

            // Parse JSON từ AI
            var finalResult = JsonSerializer.Deserialize<object>(aiContent);

            return Ok(finalResult);
        }
        
    }
}
