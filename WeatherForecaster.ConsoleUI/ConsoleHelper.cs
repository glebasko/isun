using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Domain.Services;
using WeatherForecaster.Infrastructure;
using WeatherForecaster.Persistance.Repositories;
using WeatherForecaster.Persistance;
using Serilog;
using Microsoft.EntityFrameworkCore;

namespace WeatherForecaster.ConsoleUI
{
	public static class ConsoleHelper
	{
		// TODO: write tests for this method
		public static string[] ValidateArgs(string[] cmdArgs, IEnumerable<string> apiCities)
		{
			if (cmdArgs.Length == 0)
			{
				Console.WriteLine("\nNo cities were provided as arguments. Please restart application with the cities from the list below.");
				PrintOutCities(apiCities);

				Console.WriteLine("\nExiting the application..");
				Environment.Exit(1);
			}

			// eliminate commas 
			string[] argsWithoutCommas = cmdArgs.Select(s => s.Replace(",", "")).ToArray();

			// eliminate dublicates
			string[] uniqueArgs = argsWithoutCommas.Distinct().ToArray();

			foreach (var arg in uniqueArgs)
			{
				if (!apiCities.Contains(arg))
				{
					Console.WriteLine($"\nThere is no weather forecast available for city \"{arg}\".");

					PrintOutCities(apiCities);

					Console.WriteLine("\nPlease run the application with the cities from the list above.");
					Console.WriteLine("Exiting the application..");

					Environment.Exit(1);
				}
			}

			return uniqueArgs;
		}

		public static ServiceProvider RegisterDependencies()
		{
			var services = new ServiceCollection();

			IConfiguration configuration = ConfigurationHelper.BuildConfiguration();
			services.AddSingleton(configuration);

			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddSerilog();
			});

			string connectionString = configuration.GetConnectionString("TestDatabaseConnection")
				?? throw new InvalidOperationException("Connection string 'TestDatabaseConnection' not found.");

			string baseApiUrl = configuration["ApiSettings:BaseUrl"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:BaseUrl' not found.");

			services.AddDbContext<WeatherForecasterDbContext>(options => options.UseSqlServer(connectionString));

			services.AddHttpClient<IWeatherForecastApiService, WeatherForecastApiService>(client =>
			{
				client.BaseAddress = new Uri(baseApiUrl);
			});

			services.AddTransient<IWeatherRecordRepository, WeatherRecordRepository>();

			var serviceProvider = services.BuildServiceProvider();

			return serviceProvider;
		}

		internal static void PrintOutCities(IEnumerable<string> cities)
		{
			if (cities.IsNullOrEmpty())
			{
				return;
			}

			Console.WriteLine("Available cities are: \n");

			foreach (var city in cities)
			{
				Console.WriteLine(city);
			}
		}
	}
}