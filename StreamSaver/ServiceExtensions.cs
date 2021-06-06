using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public static class StreamSaverServiceExtensions
    {
        public static IServiceCollection AddStreamSaver(this IServiceCollection services)
        {
            services.AddSingleton<IStreamSaver, StreamSaver>();

            return services;
        }
    }
}
