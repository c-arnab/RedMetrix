using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using NRedisTimeSeries;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;
namespace RedMetrixProcessor
{
    class Program
    {
        public static IConfigurationRoot configuration;
        static void Main(string[] args)
        {
            ServiceCollection serviceDescriptors = new ServiceCollection();

            ConfigureServices(serviceDescriptors);
            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            var options = new ConfigurationOptions
                {
                    EndPoints = { configuration.GetSection("RedisOptions:EndPoints").Value },
                    Password = configuration.GetSection("RedisOptions:Password").Value,
                    Ssl = false
                };
            // Multiplexer is intended to be reused
            ConnectionMultiplexer redisMultiplexer = ConnectionMultiplexer.Connect(options);
            
            // The database reference is a lightweight passthrough object intended to be used and discarded
            IDatabase db = redisMultiplexer.GetDatabase();
            
            AnsiConsole.Write(new FigletText("RedMetrix").LeftAligned().Color(Color.Red));
            AnsiConsole.Write(new Markup("[bold red]Copyright(C)[/] [teal]2021 Arnab Choudhuri - Xanadu[/]"));
            Console.WriteLine("");
            var rule = new Rule("[red]Welcome to RedMetrix[/]");
            AnsiConsole.Write(rule);

            var selectedoption = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title("[bold yellow]Intitialize Application[/] [red]OR[/] [green]Process Data[/]?")
                                        .PageSize(5)
                                        .AddChoices(new[]
                                        {
                                            "Initialize Application", "Process Data", "Exit"
                                        }));
            if (selectedoption.ToString()=="Exit")
            {
                return;
            }else{
                if (!AnsiConsole.Confirm(selectedoption.ToString()))
                {
                    return;
                }
                else{
                     if (selectedoption.ToString()=="Initialize Application")
                     {
                        serviceProvider.GetService<ConfigureInitializationServices>().DeleteKeys(db);
                        serviceProvider.GetService<ConfigureInitializationServices>().InitializeTimeSeriesTotalPageViews(db);
                        serviceProvider.GetService<ConfigureInitializationServices>().InitializeTimeSeriesTotalOrderNValue(db);
                        serviceProvider.GetService<ConfigureInitializationServices>().InitializeTimeSeriesOrderByPaymentMethod(db);
                        serviceProvider.GetService<ConfigureInitializationServices>().InitializeTimeSeriesPagePerformance(db); 
                        serviceProvider.GetService<ConfigureInitializationServices>().InitializeTimeSeriesFunnel(db);  
                     }
                     if (selectedoption.ToString()=="Process Data")
                     {
                            serviceProvider.GetService<DataServices>().ProcessData(db);
                     } 
                }
            }
            redisMultiplexer.Close();
            AnsiConsole.Write(new Markup("[bold yellow]Press any key to [/] [red]Exit![/]"));
            Console.ReadKey(false);
        }
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json")
                .Build();

            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
            serviceCollection.AddTransient<ConfigureInitializationServices>();
            serviceCollection.AddTransient<DataServices>();

        }
    }
}
