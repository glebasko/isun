using Microsoft.EntityFrameworkCore;

namespace WeatherForecaster.Persistance
{
	public static class DbInitializer
	{
		public static async Task InitializeDbAsync(WeatherForecasterDbContext db)
		{
			await db.Database.MigrateAsync();
		}
	}
}