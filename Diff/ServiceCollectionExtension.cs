using Blazorme;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazorme
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDiffService(this IServiceCollection services)
        {
            return services.AddScoped<IDiffApi, DiffApi>();
        }

    }
}
