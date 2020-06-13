using Microsoft.Extensions.DependencyInjection;

namespace Blazorme
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDiff(this IServiceCollection services)
        {
            return services.AddScoped<IDiff, DiffApi>();
        }

    }
}
