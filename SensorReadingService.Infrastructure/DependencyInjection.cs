using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorReadingService.Application.Interfaces;
using SensorReadingService.Infrastructure.FileReaders;
using SensorReadingService.Infrastructure.Persistence;
using SensorReadingService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<SensorDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IReadingRepository, ReadingRepository>();

            services.AddScoped<IFileReader, JsonlFileReader>();

            return services;
        }
    }
}
