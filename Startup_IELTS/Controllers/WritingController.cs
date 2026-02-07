using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Startup_IELTS.DTOs;
using Startup_IELTS.Models;

namespace Startup_IELTS.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/writing")]
    public class WritingController : Controller
    {

        private readonly StartupIeltsContext _db;
        public WritingController(StartupIeltsContext db)
        {
            _db = db;
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

    }
}
