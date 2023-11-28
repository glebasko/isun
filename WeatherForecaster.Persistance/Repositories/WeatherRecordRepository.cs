using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Persistance.Entities;

namespace WeatherForecaster.Persistance.Repositories
{
	public class WeatherRecordRepository : IWeatherRecordRepository
	{
		private readonly WeatherForecasterDbContext _db;

		public WeatherRecordRepository(WeatherForecasterDbContext db)
		{
			_db = db;
		}

		public async Task AddAsync(WeatherRecordDto weatherRecord)
		{
			var weatherRecordEntity = new WeatherRecordEntity()
			{
				City = weatherRecord.City,
				Temperature = weatherRecord.Temperature,
				Precipitation = weatherRecord.Precipitation,
				WindSpeed = weatherRecord.WindSpeed,
				Summary = weatherRecord.Summary,
				CreatedOn = DateTime.Now
			};

			_db.Add(weatherRecordEntity);
			await _db.SaveChangesAsync();
		}

		public async Task AddRangeAsync(IEnumerable<WeatherRecordDto> weatherRecords)
		{
			var weatherRecordEntities = weatherRecords.Select(item => new WeatherRecordEntity()
			{
				City = item.City,
				Temperature = item.Temperature,
				Precipitation = item.Precipitation,
				WindSpeed = item.WindSpeed,
				Summary = item.Summary,
				CreatedOn = DateTime.Now
			});

			await _db.AddRangeAsync(weatherRecordEntities);
			await _db.SaveChangesAsync();
		}
	}
}