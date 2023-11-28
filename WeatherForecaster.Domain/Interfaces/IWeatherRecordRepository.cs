using WeatherForecaster.Domain.DTO;

namespace WeatherForecaster.Domain.Interfaces
{
	public interface IWeatherRecordRepository
	{
		public Task Add(WeatherRecordDto weatherRecord);
		public Task AddRange(IEnumerable<WeatherRecordDto> weatherRecords);
	}
}
