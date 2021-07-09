using System.IO;
using ConsoleAppWithDI.UI.Models;
using ConsoleAppWithDI.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleAppWithDI.UI
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            IConfiguration configuration = builder.Build();
            services.AddSingleton(configuration);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.File("Logging-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                //.WriteTo.MSSqlServer()
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddSerilog();
                builder.AddConsole();
            });
                
            services.AddDbContext<AcmeCorporationContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("AcmeCorporation"));
            });

            services.AddSingleton<IDescargaCsvService, DescargaCsvService>();
            services.AddSingleton<IDescargaCsvService, DescargarCvsServiceAmazon>();

            services.AddSingleton<ISingletonService, SingletonScopedTransient>();
            services.AddScoped<IScopedService, SingletonScopedTransient>();
            services.AddTransient<ITransientService, SingletonScopedTransient>();

            services.AddTransient<DI>();
            services.AddSingleton<Csv>();

            return services;
        }
    }
}