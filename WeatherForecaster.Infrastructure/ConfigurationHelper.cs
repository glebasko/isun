using Microsoft.Extensions.Configuration;
using Serilog;

namespace WeatherForecaster.Infrastructure
{
	public class ConfigurationHelper
	{
		public static IConfiguration BuildConfiguration()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			return configuration;
		}

		public static void ConfigureSerilog()
		{
			var configuration = BuildConfiguration();

			Log.Logger = new LoggerConfiguration()
			 .ReadFrom.Configuration(configuration)
			 .CreateLogger();
		}
	}
}