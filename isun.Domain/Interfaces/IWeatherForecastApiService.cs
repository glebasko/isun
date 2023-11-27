using isun.Domain.DTO;

namespace isun.Domain.Interfaces
{
	public interface IWeatherForecastApiService
	{
        public Task<IEnumerable<string>> GetCitiesAsync();
        public Task<WeatherRecordDto> GetWeatherRecordAsync();
    }
}