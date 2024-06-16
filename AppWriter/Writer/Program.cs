using Writer.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sun.util.logging.resources;

namespace Writer
{
    public class Program
    {
        public static string HostName = System.Environment.MachineName;

        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting AppWriter on host: {HostName}");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {

            IConfiguration configuration;

            return Host
             .CreateDefaultBuilder(args)
             .ConfigureServices((hostContext, services) =>
             {
                 services.AddHostedService<WriterWorker>();
             })
            .ConfigureAppConfiguration((webHostBuilderContext, config) =>
            {
                var env = webHostBuilderContext.HostingEnvironment.EnvironmentName;

                config.SetBasePath(webHostBuilderContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.env.json", true, true)
                    .AddEnvironmentVariables();

                configuration = config.Build();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddLog4Net("log4net.config");
            });
        }
    }
}