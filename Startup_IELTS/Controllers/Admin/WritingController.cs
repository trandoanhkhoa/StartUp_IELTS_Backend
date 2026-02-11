using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_IELTS.DTOs;
using Startup_IELTS.Models;
using System;
using System.ComponentModel;

namespace Startup_IELTS.Controllers.Admin
{
    [ApiController]

    [Route("api/admin")]
    public class WritingController : Controller
    {
        private readonly StartupIeltsContext _db;
        public WritingController(StartupIeltsContext db)
        {
            _db = db;
        }
        [HttpPost("createWriting")]
        public async Task<IActionResult> Create(WritingCreateUpdateDto dto)
        {
            var writing = new Writing
            {
                TaskType = dto.TaskType,
                ImageUrl = dto.ImageUrl,
                Question = dto.Question,
                Title = dto.Title,
                Category = dto.Category,
                Type = dto.Type,
                Source = dto.Source,
                Hide = dto.Hide ?? false,

            };

            _db.Writings.Add(writing);
            await _db.SaveChangesAsync();

            return Ok(writing);
        }
        [HttpPut("updateWritingById/{id}")]
        public async Task<IActionResult> Update(int id, WritingCreateUpdateDto dto)
        {
            var writing = await _db.Writings.FindAsync(id);
            if (writing == null) return NotFound();

            writing.TaskType = dto.TaskType;
            writing.ImageUrl = dto.ImageUrl;
            writing.Question = dto.Question;
            writing.Title = dto.Title;
            writing.Category = dto.Category;
            writing.Type = dto.Type;
            writing.Source = dto.Source;
            writing.Hide = dto.Hide;

            await _db.SaveChangesAsync();
            return Ok(writing);
        }
        [HttpDelete("deleteWritingById/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var writing = await _db.Writings.FindAsync(id);
            if (writing == null) return NotFound();

            _db.Writings.Remove(writing);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        [HttpGet("getWritingById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var writing = await _db.Writings.FindAsync(id);
            if (writing == null) return NotFound();

            return Ok(writing);
        }
        [HttpPost("importwriting")]
        public async Task<IActionResult> ImportWriting(List<WritingCreateUpdateDto> list)
        {
            foreach (var item in list)
            {
                _db.Writings.Add(new Writing
                {
                    TaskType = item.TaskType,
                    ImageUrl = item.ImageUrl,
                    Question = item.Question,
                    Type = item.Type,
                    Title = item.Title,
                    Category = item.Category,
                    Source = item.Source,
                    Hide = item.Hide
                });
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

    }
}
