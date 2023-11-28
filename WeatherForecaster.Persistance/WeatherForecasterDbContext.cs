using WeatherForecaster.Persistance.Entities;
using Microsoft.EntityFrameworkCore;

namespace WeatherForecaster.Persistance
{
	public class WeatherForecasterDbContext : DbContext
	{
        public WeatherForecasterDbContext(DbContextOptions<WeatherForecasterDbContext> options) : base(options) { }

        public DbSet<WeatherRecordEntity> WeatherRecords { get; set; }
    }
}