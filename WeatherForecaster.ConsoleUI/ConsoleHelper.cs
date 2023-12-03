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
		public static bool ProcessAndValidateCmdArgs(string[] cmdArgs, IEnumerable<string> apiCities, out string[] outputArgs)
		{
			outputArgs = null;

			if (cmdArgs.Length == 0)
			{
				Console.WriteLine("\nNo cities were provided as arguments. Please restart application with the cities from the list below.");
				PrintOutCities(apiCities);
					
				return false;
			}

			// eliminate commas 
			string[] argsWithoutCommas = cmdArgs.Select(s => s.Replace(",", "")).ToArray();

			// eliminate dublicates
			outputArgs = argsWithoutCommas.Distinct().ToArray();

			foreach (var arg in outputArgs)
			{
				if (!apiCities.Contains(arg))
				{
					Console.WriteLine($"\nThere is no weather forecast available for city \"{arg}\".");

					PrintOutCities(apiCities);

					Console.WriteLine("\nPlease run the application with the cities from the list above.");

					return false;
				}
			}

			return true;
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