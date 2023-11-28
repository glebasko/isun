using WeatherForecaster.Domain.DTO;

namespace WeatherForecaster.Domain.Interfaces
{
	public interface IWeatherForecastApiService
	{
        public Task<IEnumerable<string>> GetCitiesAsync();
        public Task<WeatherRecordDto> GetWeatherRecordAsync(string city);
    }
}