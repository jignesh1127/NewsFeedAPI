using Microsoft.AspNetCore.Mvc;
using NewsFeedApi.Services.Interfaces;

namespace NewsFeedApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsFeedController : ControllerBase
    {
        private readonly INewsService _newsService;
        public NewsFeedController(INewsService newsService)
        {
            _newsService = newsService;
        }

        [HttpGet("latestnews")]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var stories = await _newsService.GetNewestStoriesAsync();
            var pagedStories = stories.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(pagedStories);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var stories = await _newsService.GetNewestStoriesAsync();

            var filteredStories = stories.Where(s => s.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
            return Ok(filteredStories);
        }
    }
}
