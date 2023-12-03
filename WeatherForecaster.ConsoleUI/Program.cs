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
			WeatherForecastUpdaterJob? weatherForecastUpdaterJob = null;

			try
			{
				Console.WriteLine("Starting the application..");
				Console.OutputEncoding = Encoding.UTF8; // to ensure that lithuanian letters are shown correctly

				var serviceProvider = RegisterDependencies();
				var weatherForecastApiService = serviceProvider.GetRequiredService<IWeatherForecastApiService>();

				var apiCities = await weatherForecastApiService.GetCitiesAsync();

				string[] uniqueArgs = ValidateArgs(args, apiCities);

				var db = serviceProvider.GetRequiredService<WeatherForecasterDbContext>();
				await DbInitializer.InitializeDbAsync(db);

				var weatherRecordRepository = serviceProvider.GetRequiredService<IWeatherRecordRepository>();
				var configuration = serviceProvider.GetRequiredService<IConfiguration>();

				var strInterval = configuration.GetRequiredSection("WeatherForecastUpdaterTaskIntervalInSec").Value
					?? throw new InvalidOperationException("Congifuration section 'WeatherForecastUpdaterTaskIntervalInSec' not found.");
				int interval = int.Parse(strInterval);

				weatherForecastUpdaterJob = new WeatherForecastUpdaterJob(weatherForecastApiService, weatherRecordRepository, TimeSpan.FromSeconds(interval), uniqueArgs);

				var jobTask = weatherForecastUpdaterJob.StartAsync();

				// giving user the opportunity to manually cancel the job and exit application

				Console.WriteLine("Press any key to stop the application");
				var keyTask = Task.Run(() => Console.ReadKey(true));

				await Task.WhenAny(jobTask, keyTask);

				// at this point either an exception happened, or user canceled the job

				if (jobTask.IsFaulted)
				{
					// we need to await for it to catch the exception
					await jobTask;
				}
			}
			catch (Exception ex)
			{
				Log.Logger.Error(ex, "An exception occurred. Exiting the application.");
				Console.WriteLine("\nUnexpected error occured.");
			}
			finally
			{
				if (weatherForecastUpdaterJob is not null)
				{
					await weatherForecastUpdaterJob.StopAsync();
				}
				
				Log.CloseAndFlush();

				Console.WriteLine("\nExiting the application..");
			}
		}

		// TODO: write tests for this method
		private static string[] ValidateArgs(string[] args, IEnumerable<string> apiCities)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("\nNo cities were provided as arguments. Please restart application with the cities from the list below.");
				PrintOutCities(apiCities);

				Console.WriteLine("\nExiting the application..");
				Environment.Exit(1);
			}

			// eliminate commas 
			string[] argsWithoutCommas = args.Select(s => s.Replace(",", "")).ToArray();

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