using Microsoft.Extensions.Caching.Memory;
using NewsFeedApi.Models;
using NewsFeedApi.Services.Interfaces;
using Newtonsoft.Json;

namespace NewsFeedApi.Services.Implementation
{
    public class NewsService : INewsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;

        public NewsService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<IList<Story>> GetNewestStoriesAsync()
        {
            if (!_memoryCache.TryGetValue("newestStories", out IList<Story> stories))
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync("https://hacker-news.firebaseio.com/v0/newstories.json");
                var storyIds = JsonConvert.DeserializeObject<List<int>>(response);

                var tasks = storyIds.Take(100).Select(async id =>
                {
                    var storyResponse = await client.GetStringAsync($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
                    return JsonConvert.DeserializeObject<Story>(storyResponse);
                });

                stories = await Task.WhenAll(tasks);
                _memoryCache.Set("newestStories", stories, TimeSpan.FromMinutes(3));
            }

            return stories;
        }

    }
}
