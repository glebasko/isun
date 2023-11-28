using Microsoft.Extensions.Configuration;

namespace WeatherForecaster.Domain
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
	}
}