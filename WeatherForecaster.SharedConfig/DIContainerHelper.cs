using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Domain.Services;
using WeatherForecaster.Persistance;

namespace WeatherForecaster.SharedConfig
{
	public static class DIContainerHelper
	{
		public static ServiceProvider RegisterDependencies()
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
	}
}