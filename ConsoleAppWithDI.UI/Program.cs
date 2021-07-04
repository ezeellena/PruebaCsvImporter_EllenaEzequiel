using System;
using Microsoft.Extensions.DependencyInjection;

namespace CsvImporter
{
    public class Program
    {
        public static void Main(String[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            
            var services = Startup.ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<Csv>().Run(args);
        }
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Presionar enter para finalizar el proceso");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}