using NewsFeedApi.Services.Implementation;
using NewsFeedApi.Services.Interfaces;

namespace NewsFeedApi.Extensions
{
    public static class ServiceDependancy
    {
        public static IServiceCollection AddServiceDependancies(this IServiceCollection services) =>
            services.AddScoped<INewsService, NewsService>();
    }
}
