using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using NewsFeedApi.Models;
using NewsFeedApi.Services.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace NewsFeedApi.Test
{
    public class NewsServiceTests
    {
        [Fact]
        public async Task GetNewestStoriesAsync_ReturnsStories()
        {
            //Test Data
            var storyIds = new List<int> { 1, 2, 3 };
            var stories = new List<Story>
            {
                new Story { Title = "Story 1", Url = "url1.com" },
                new Story { Title = "Story 2", Url = "url2.com" },
                new Story { Title = "Story 3", Url = "url3.com" }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsolutePath == "/v0/newstories.json"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(storyIds), Encoding.UTF8, "application/json")
                });

            foreach (var story in stories)
            {
                mockHttpMessageHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsolutePath == $"/v0/item/{storyIds[stories.IndexOf(story)]}.json"),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(story), Encoding.UTF8, "application/json")
                    });
            }

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockMemoryCache = new Mock<IMemoryCache>();
            var cacheEntry = Mock.Of<ICacheEntry>();

            mockMemoryCache
                .Setup(mc => mc.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntry);

            var service = new NewsService(mockHttpClientFactory.Object, mockMemoryCache.Object);

            // Act
            var result = await service.GetNewestStoriesAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Story 1", result[0].Title);
            Assert.Equal("Story 2", result[1].Title);
            Assert.Equal("Story 3", result[2].Title);

        }
    }
}