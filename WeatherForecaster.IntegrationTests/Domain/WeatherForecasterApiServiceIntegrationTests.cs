using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Domain.Services;
using WeatherForecaster.Infrastructure;
using Xunit;

namespace WeatherForecaster.IntegrationTests.Domain
{
    public class WeatherForecasterApiServiceIntegrationTests
    {
		private static readonly IWeatherForecastApiService _weatherForecastApiService;

		static WeatherForecasterApiServiceIntegrationTests()
		{
			var configuration = ConfigurationHelper.BuildConfiguration();

			var baseApiUrl = configuration["ApiSettings:BaseUrl"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:BaseUrl' not found.");

			var httpClient = new HttpClient() { BaseAddress = new Uri(baseApiUrl) };

			_weatherForecastApiService = new WeatherForecastApiService(configuration, httpClient);
		}

		[Fact]
        public async Task GetCitiesAsync_ReturnsCities()
		{
            var cities = await _weatherForecastApiService.GetCitiesAsync();

			Assert.NotNull(cities);
			Assert.NotEmpty(cities);
        }

		[Theory]
		[InlineData("Vilnius")]
		[InlineData("Kaunas")]
		[InlineData("Klaipėda")]
		[InlineData("Riga")]
		[InlineData("Tallinn")]
		[InlineData("Rome")]
		[InlineData("Berlin")]
		[InlineData("Paris")]
		[InlineData("Tunis")]
		[InlineData("Oslo")]
		[InlineData("N'Djamena")]
		public async Task GetWeatherRecordAsync_ReturnsWeatherRecordDto(string city)
		{
			WeatherRecordDto weatherRecordDto = await _weatherForecastApiService.GetWeatherRecordAsync(city);

			Assert.NotNull(weatherRecordDto);
			Assert.Equal(city, weatherRecordDto.City);
		}

		[Theory]
		[InlineData("Vilniusssss")]
		[InlineData("dgdfgfdg")]
		[InlineData("000000")]
		public async Task GetWeatherRecordAsync_ReturnsException(string invalidCity)
		{
			await Assert.ThrowsAsync<HttpRequestException>(async () => await _weatherForecastApiService.GetWeatherRecordAsync(invalidCity));
		}
	}
}