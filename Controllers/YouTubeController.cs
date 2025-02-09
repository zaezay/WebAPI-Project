using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Search Page
        public IActionResult Index()
        {
            return View(new List<YouTubeVideoModel>()); // Pass an empty list initially
        }

        // Handle the search query
        [HttpPost]
        public async Task<IActionResult> Search(string query, string[] categories, string uploadDate, string eventType)
        {
            var videos = await _youtubeService.SearchVideosAsync(query, categories, uploadDate, eventType);
            return View("Index", videos);
        }
    }
}