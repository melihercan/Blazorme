using BlazormeFFmpeg;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazorme
{
    public static class FFmpegerviceExtensions
    {
        public static IServiceCollection AddFFmpeg(this IServiceCollection services)
        {
            services.AddSingleton<IFFmpeg, FFmpeg>();

            return services;
        }
    }
}
