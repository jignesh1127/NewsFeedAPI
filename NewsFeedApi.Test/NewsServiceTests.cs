using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using NewsFeedApi.Services.Implementation;
using NewsFeedApi.Services.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace NewsFeedApi.Test
{
    public class NewsServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly IMemoryCache _memoryCache;
        private readonly NewsService _newsService;

        public NewsServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _newsService = new NewsService(_httpClientFactoryMock.Object, _memoryCache);
        }

        [Fact]
        public async Task GetNewestStoriesAsync_CacheMiss_FetchesFromApi()
        {
            // Arrange
            SetupHttpClientFactory();
            SetupStoryIdsResponse(new List<int> { 1 });
            SetupStoryResponse(1, new Story { Title = "API Story", Url = "http://example.com" });

            // Act
            var result = await _newsService.GetNewestStoriesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("API Story", result[0].Title);
        }

        [Fact]
        public async Task GetNewestStoriesAsync_HandlesNoStories()
        {
            // Arrange
            SetupHttpClientFactory();
            SetupStoryIdsResponse(new List<int>());

            // Act
            var result = await _newsService.GetNewestStoriesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetNewestStoriesAsync_HandlesError()
        {
            // Arrange
            SetupHttpClientFactory();
            SetupErrorResponse("/v0/newstories.json");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _newsService.GetNewestStoriesAsync());
        }

        private void SetupHttpClientFactory()
        {
            var httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);
        }

        private void SetupStoryIdsResponse(List<int> storyIds)
        {
            var storyIdsResponse = JsonConvert.SerializeObject(storyIds);
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsolutePath == "/v0/newstories.json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(storyIdsResponse)
                });
        }

        private void SetupStoryResponse(int storyId, Story story)
        {
            var storyResponse = JsonConvert.SerializeObject(story);
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsolutePath == $"/v0/item/{storyId}.json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(storyResponse)
                });
        }

        private void SetupErrorResponse(string requestUri)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsolutePath == requestUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                });
        }
    }
}