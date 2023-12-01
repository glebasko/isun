using WeatherForecaster.Domain.DTO;

namespace WeatherForecaster.Domain.Interfaces
{
	public interface IWeatherRecordRepository
	{
		public Task AddAsync(WeatherRecordDto weatherRecord);
		public Task AddRangeAsync(IEnumerable<WeatherRecordDto> weatherRecords);
	}
}