using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;
        private readonly ILogger<YouTubeApiService> _logger;

        public YouTubeApiService(IConfiguration configuration, ILogger<YouTubeApiService> logger)
        {
            _apiKey = configuration["YouTubeApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "YouTube API key is missing in configuration.");
            _logger = logger;
        }

        public async Task<List<YouTubeVideoModel>> GetPopularVideosAsync()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YouTubeApiProject"
            });

            var searchRequest = youtubeService.Videos.List("snippet");
            searchRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            searchRequest.MaxResults = 20;

            _logger.LogInformation("Fetching popular videos");

            try
            {
                var searchResponse = await searchRequest.ExecuteAsync();

                var videos = searchResponse.Items.Select(item => new YouTubeVideoModel
                {
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                    VideoUrl = "https://www.youtube.com/watch?v=" + item.Id,
                    ChannelName = item.Snippet.ChannelTitle,
                    PublishedDate = (DateTime)item.Snippet.PublishedAt
                }).ToList();

                return videos;
            }
            catch (Exception ex)
            {
                // Log exception and return empty list
                _logger.LogError(ex, "Error occurred while fetching popular videos");
                return new List<YouTubeVideoModel>();
            }
        }

        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query, string[] categories, string uploadDate, string eventType)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YouTubeApiProject"
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = 10;
            searchRequest.Type = "video";

            // Apply filters
            if (categories != null && categories.Length > 0)
            {
                searchRequest.VideoCategoryId = string.Join(",", categories);
            }

            if (!string.IsNullOrEmpty(uploadDate))
            {
                DateTime? publishedAfter = uploadDate switch
                {
                    "lastDay" => DateTime.UtcNow.AddDays(-1),
                    "lastWeek" => DateTime.UtcNow.AddDays(-7),
                    "lastMonth" => DateTime.UtcNow.AddMonths(-1),
                    "last3Months" => DateTime.UtcNow.AddMonths(-3),
                    "last6Months" => DateTime.UtcNow.AddMonths(-6),
                    _ => (DateTime?)null
                };

                if (publishedAfter.HasValue)
                {
                    searchRequest.PublishedAfter = publishedAfter.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }

            if (!string.IsNullOrEmpty(eventType))
            {
                searchRequest.EventType = eventType switch
                {
                    "none" => SearchResource.ListRequest.EventTypeEnum.None,
                    "upcoming" => SearchResource.ListRequest.EventTypeEnum.Upcoming,
                    "live" => SearchResource.ListRequest.EventTypeEnum.Live,
                    "completed" => SearchResource.ListRequest.EventTypeEnum.Completed,
                    _ => null
                };
            }

            _logger.LogInformation("Query: {Query}, Categories: {Categories}, UploadDate: {UploadDate}, EventType: {EventType}", query, categories, uploadDate, eventType);

            try
            {
                var searchResponse = await searchRequest.ExecuteAsync();

                var videos = searchResponse.Items.Select(item => new YouTubeVideoModel
                {
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                    VideoUrl = "https://www.youtube.com/watch?v=" + item.Id.VideoId,
                    ChannelName = item.Snippet.ChannelTitle,
                    PublishedDate = (DateTime)item.Snippet.PublishedAt
                }).ToList();

                return videos;
            }
            catch (Exception ex)
            {
                // Log exception and return empty list
                _logger.LogError(ex, "Error occurred while searching videos");
                return new List<YouTubeVideoModel>();
            }
        }
    }
}