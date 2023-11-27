using isun.Domain.Interfaces;
using isun.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace isun
{
	public class Program
	{
		static async Task Main(string[] args)
		{
            try
            {
                Console.WriteLine("Starting the application..");

                if (args.Length == 0)
                {
                    Console.WriteLine("\nNo command-line arguments provided.");
                    Environment.Exit(1);
                }

                //Console.WriteLine("\nCommand-line arguments:");

                //// Display each command-line argument
                //for (int i = 0; i < args.Length; i++)
                //{
                //    Console.WriteLine($"Argument {i + 1}: {args[i]}");
                //}


                await ValidateIfArgsAreValidCities(args);
            }
            catch (Exception ex)
            {
                // TODO: log the exception

                Console.WriteLine("Error occured.");
            }        
		}

        private static ServiceProvider RegisterDependencies()
        {
            var services = new ServiceCollection();

            services.AddTransient<IWeatherForecastApiService, WeatherForecastApiService>();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private static async Task<bool> ValidateIfArgsAreValidCities(string[] args)
        {
            var serviceProvider = RegisterDependencies();

            var iWeatherForecastApiService = serviceProvider.GetRequiredService<IWeatherForecastApiService>();

            var apiCities = await iWeatherForecastApiService.GetCitiesAsync();

            foreach (var arg in args)
            {
                if (!apiCities.Contains(arg))
                {
                    Console.WriteLine($"\nThere is no weather forecast available for city \"{arg}\".");
                    Console.WriteLine("Available cities are: \n");

                    foreach (var apiCity in apiCities)
                    {
                        Console.WriteLine(apiCity);
                    }

                    return false;
                }
            }

            return true;
        }
    }
}