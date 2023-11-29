using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecaster.SharedConfig;
using Microsoft.Extensions.Configuration;
using WeatherForecaster.Domain.Services;
using WeatherForecaster.Persistance.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace WeatherForecaster.ConsoleUI
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				Console.WriteLine("Starting the application..");

				var serviceProvider = RegisterDependencies();
				var weatherForecastApiService = serviceProvider.GetRequiredService<IWeatherForecastApiService>();

				var apiCities = await weatherForecastApiService.GetCitiesAsync();

				if (args.Length == 0)
				{
					Console.WriteLine("\nNo cities were provided as arguments.");
					PrintOutCities(apiCities);

					Environment.Exit(1);
				}

				var db = serviceProvider.GetRequiredService<WeatherForecasterDbContext>();
				await DbInitializer.InitializeDbAsync(db);

				if (!ValidateIfArgsAreValidCities(args, apiCities))
				{
					Console.WriteLine("\nPlease run the application with the cities from the list above.");
					Console.WriteLine("Terminating application..");
					Environment.Exit(1);
				}

				var weatherRecordRepository = serviceProvider.GetRequiredService<IWeatherRecordRepository>();
				var configuration = serviceProvider.GetRequiredService<IConfiguration>();

				string strInterval = configuration.GetRequiredSection("WeatherForecastUpdaterTaskIntervalInSec").Value
					?? throw new InvalidOperationException("Congifuration section 'WeatherForecastUpdaterTaskIntervalInSec' not found.");
				int interval = int.Parse(strInterval);

				var myReccuringTask = new WeatherForecastUpdaterTask(weatherForecastApiService, weatherRecordRepository, TimeSpan.FromSeconds(interval), args);
				myReccuringTask.Start();

				Console.WriteLine("Press any key to stop the task");
				Console.ReadKey();

				await myReccuringTask.StopAsync();			
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

			services.AddSingleton(configuration);

			string connectionString = configuration.GetConnectionString("TestDatabaseConnection")
				?? throw new InvalidOperationException("Connection string 'TestDatabaseConnection' not found.");

			services.AddDbContext<WeatherForecasterDbContext>(options => options.UseSqlServer(connectionString));

			services.AddTransient<IWeatherForecastApiService, WeatherForecastApiService>();
			services.AddTransient<IWeatherRecordRepository, WeatherRecordRepository>();

			var serviceProvider = services.BuildServiceProvider();

			return serviceProvider;
		}

		private static bool ValidateIfArgsAreValidCities(string[] args, IEnumerable<string> apiCities)
		{
			foreach (var arg in args)
			{
				if (!apiCities.Contains(arg))
				{
					Console.WriteLine($"\nThere is no weather forecast available for city \"{arg}\".");
					PrintOutCities(apiCities);

					return false;
				}
			}

			return true;
		}

		private static void PrintOutCities (IEnumerable<string> cities)
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