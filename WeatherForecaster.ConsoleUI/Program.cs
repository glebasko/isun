using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Persistance;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecaster.Infrastructure;
using Microsoft.Extensions.Configuration;
using WeatherForecaster.Domain.Services;
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

				var serviceProvider = ConsoleHelper.RegisterDependencies();
				var weatherForecastApiService = serviceProvider.GetRequiredService<IWeatherForecastApiService>();

				var apiCities = await weatherForecastApiService.GetCitiesAsync();

				string[] uniqueArgs = ConsoleHelper.ValidateArgs(args, apiCities);

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
	}
}