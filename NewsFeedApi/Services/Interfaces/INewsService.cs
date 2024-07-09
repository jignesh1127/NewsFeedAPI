using NewsFeedApi.Models;

namespace NewsFeedApi.Services.Interfaces
{
    public interface INewsService
    {
        Task<IList<Story>> GetNewestStoriesAsync();

    }
}
