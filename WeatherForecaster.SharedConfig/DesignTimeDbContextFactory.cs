using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WeatherForecaster.Persistance;

namespace WeatherForecaster.SharedConfig
{
	// this class is required for the EF scaffolder to pick up the correct dbcontextoptions and create migrations
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WeatherForecasterDbContext>
	{
		public WeatherForecasterDbContext CreateDbContext(string[] args)
		{
			var configuration = ConfigurationHelper.BuildConfiguration();

			var builder = new DbContextOptionsBuilder<WeatherForecasterDbContext>();
			var connectionString = configuration.GetConnectionString("TestDatabaseConnection");
			builder.UseSqlServer(connectionString);

			return new WeatherForecasterDbContext(builder.Options);
		}
	}
}