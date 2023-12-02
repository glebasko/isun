using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecaster.Infrastructure;
using Microsoft.Extensions.Configuration;
using WeatherForecaster.Domain.Services;
using WeatherForecaster.Persistance.Repositories;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace WeatherForecaster.ConsoleUI
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			ConfigurationHelper.ConfigureSerilog();

			try
			{	
				Console.WriteLine("Starting the application..");
				Console.OutputEncoding = Encoding.UTF8;
				
				var serviceProvider = RegisterDependencies();
				var weatherForecastApiService = serviceProvider.GetRequiredService<IWeatherForecastApiService>();

				var apiCities = await weatherForecastApiService.GetCitiesAsync();

				if (args.Length == 0)
				{
					Console.WriteLine("\nNo cities were provided as arguments. Please restart application with the cities from the list below.");
					PrintOutCities(apiCities);

					Console.WriteLine("\nExiting the application..");
					Environment.Exit(1);
				}

				var db = serviceProvider.GetRequiredService<WeatherForecasterDbContext>();
				await DbInitializer.InitializeDbAsync(db);

				// eliminate dublicates
				string[] uniqueArgs = args.Distinct().ToArray();

				if (!ValidateIfArgsAreValidCities(uniqueArgs, apiCities))
				{
					Console.WriteLine("\nPlease run the application with the cities from the list above.");
					Console.WriteLine("Exiting the application..");
					Environment.Exit(1);
				}

				var weatherRecordRepository = serviceProvider.GetRequiredService<IWeatherRecordRepository>();
				var configuration = serviceProvider.GetRequiredService<IConfiguration>();

				string strInterval = configuration.GetRequiredSection("WeatherForecastUpdaterTaskIntervalInSec").Value
					?? throw new InvalidOperationException("Congifuration section 'WeatherForecastUpdaterTaskIntervalInSec' not found.");
				int interval = int.Parse(strInterval);

				var myReccuringTask = new WeatherForecastUpdaterTask(weatherForecastApiService, weatherRecordRepository, TimeSpan.FromSeconds(interval), uniqueArgs);
				myReccuringTask.Start();

				Console.WriteLine("Press any key to stop the application");
				Console.ReadKey();

				await myReccuringTask.StopAsync();			
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "An exception occurred. Exiting the application.");
				Console.WriteLine("\nUnexpected error occured. Exiting the application.. ");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
		private static ServiceProvider RegisterDependencies()
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