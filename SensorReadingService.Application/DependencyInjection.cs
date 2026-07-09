using Microsoft.Extensions.DependencyInjection;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IReadingImporter, ReadingImporter>();
            services.AddScoped<IAggregationService, AggregationService>();

            return services;
        }
    }
}
