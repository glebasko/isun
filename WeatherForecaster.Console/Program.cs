using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecaster.SharedConfig;
using Microsoft.Extensions.Configuration;
using WeatherForecaster.Domain.Services;

namespace WeatherForecaster.ConsoleUI
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

				var serviceProvider = RegisterDependencies();

				var db = serviceProvider.GetRequiredService<WeatherForecasterDbContext>();

				await DbInitializer.InitializeDbAsync(db);

				var iWeatherForecastApiService = serviceProvider.GetRequiredService<IWeatherForecastApiService>();

				if (!await ValidateIfArgsAreValidCities(iWeatherForecastApiService, args))
				{
					Console.WriteLine("\nPlease run the application with the cities from the list above. Terminating the application..");
					Environment.Exit(1);
				}

				foreach (var arg in args)
				{
					WeatherRecordDto weatherRecordDto = await iWeatherForecastApiService.GetWeatherRecordAsync(arg);
					PrintOutWeatherForecast(weatherRecordDto);
				}
			}
			catch (Exception ex)
			{
				// TODO: log the exception

				Console.WriteLine("\nError occured: ");
				Console.WriteLine(ex.ToString());
			}
		}
		private static ServiceProvider RegisterDependencies()
		{
			var services = new ServiceCollection();

			IConfiguration configuration = ConfigurationHelper.BuildConfiguration();

			string connectionString = configuration.GetConnectionString("TestDatabaseConnection")
				?? throw new InvalidOperationException("Connection string 'TestDatabaseConnection' not found.");

			services.AddDbContext<WeatherForecasterDbContext>(options => options.UseSqlServer(connectionString));

			services.AddTransient<IWeatherForecastApiService, WeatherForecastApiService>(); //Or singleton?

			var serviceProvider = services.BuildServiceProvider();

			return serviceProvider;
		}

		private static async Task<bool> ValidateIfArgsAreValidCities(IWeatherForecastApiService weatherForecastApiService, string[] args)
		{
			var apiCities = await weatherForecastApiService.GetCitiesAsync();

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

		private static void PrintOutWeatherForecast(WeatherRecordDto weatherRecordDto)
		{
			Console.WriteLine("\nCity: " + weatherRecordDto.City);
			Console.WriteLine("Temperature: " + weatherRecordDto.Temperature);
			Console.WriteLine("Precipitation: " + weatherRecordDto.Precipitation);
			Console.WriteLine("WindSpeed: " + weatherRecordDto.WindSpeed);
			Console.WriteLine("Summary: " + weatherRecordDto.Summary);
		}
	}
}