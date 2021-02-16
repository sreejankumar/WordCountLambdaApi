using AngleSharp;
using Microsoft.Extensions.DependencyInjection;

namespace WordCount.Api.Core.Extensions
{
    public static class BrowsingContextExtensions
    {
        public static void AddBrowsingContext(this IServiceCollection services)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            services.AddSingleton(x => context);
        }
    }
}