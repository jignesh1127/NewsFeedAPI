using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsFeedApi.Controllers;
using NewsFeedApi.Services.Interfaces;
using NewsFeedApi.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFeedApi.Test
{
    public class NewsFeedControllerTests
    {
        private readonly Mock<INewsService> _newsServiceMock;
        private readonly NewsFeedController _controller;

        public NewsFeedControllerTests()
        {
            _newsServiceMock = new Mock<INewsService>();
            _controller = new NewsFeedController(_newsServiceMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsPagedStories()
        {
            // Arrange
            var stories = Enumerable.Range(1, 50).Select(i => new Story { Title = $"Story {i}", Url = $"http://example.com/story{i}" }).ToList();
            _newsServiceMock.Setup(service => service.GetNewestStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.Get(2, 10) as OkObjectResult;
            var pagedStories = result?.Value as IEnumerable<Story>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(pagedStories);
            Assert.Equal(10, pagedStories.Count());
            Assert.Equal("Story 11", pagedStories.First().Title);
        }

        [Fact]
        public async Task Get_ReturnsEmptyListWhenNoStories()
        {
            // Arrange
            var stories = new List<Story>();
            _newsServiceMock.Setup(service => service.GetNewestStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.Get() as OkObjectResult;
            var pagedStories = result?.Value as IEnumerable<Story>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(pagedStories);
            Assert.Empty(pagedStories);
        }

        [Fact]
        public async Task Search_ReturnsFilteredStories()
        {
            // Arrange
            var stories = new List<Story>
        {
            new Story { Title = "Story 1", Url = "http://example.com/story1" },
            new Story { Title = "Another Story", Url = "http://example.com/story2" },
            new Story { Title = "Story 3", Url = "http://example.com/story3" }
        };
            _newsServiceMock.Setup(service => service.GetNewestStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.Search("Another") as OkObjectResult;
            var filteredStories = result?.Value as IEnumerable<Story>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(filteredStories);
            Assert.Single(filteredStories);
            Assert.Equal("Another Story", filteredStories.First().Title);
        }

        [Fact]
        public async Task Search_ReturnsEmptyListWhenNoMatch()
        {
            // Arrange
            var stories = new List<Story>
        {
            new Story { Title = "Story 1", Url = "http://example.com/story1" },
            new Story { Title = "Another Story", Url = "http://example.com/story2" },
            new Story { Title = "Story 3", Url = "http://example.com/story3" }
        };
            _newsServiceMock.Setup(service => service.GetNewestStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.Search("Nonexistent") as OkObjectResult;
            var filteredStories = result?.Value as IEnumerable<Story>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(filteredStories);
            Assert.Empty(filteredStories);
        }
    }

}
