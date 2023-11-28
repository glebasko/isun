using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace WeatherForecaster.Domain.Services
{
	public class WeatherForecastApiService : IWeatherForecastApiService
	{
        private readonly HttpClient _httpClient;

        public WeatherForecastApiService() 
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://weather-api.isun.ch/api/"); //TODO: move to config file

            AuthorizeHttpClient().Wait();
        }

        private async Task AuthorizeHttpClient()
		{
            //TODO: move to config file
            var parameters = new
            {
                Username = "isun",
                Password = "passwrod"
            };

            string jsonParameters = JsonConvert.SerializeObject(parameters);

            var response = await _httpClient.PostAsync("authorize", new StringContent(jsonParameters, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                return;
            }
   
            string jsonString = await response.Content.ReadAsStringAsync();

			var authorizationTokenDto = JsonConvert.DeserializeObject<AuthorizationTokenDto>(jsonString);

            _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationTokenDto.Token); 
        }

        //TODO: refactor returns
		public async Task<IEnumerable<string>> GetCitiesAsync()
		{       
            var response = await _httpClient.GetAsync("cities");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string jsonString = await response.Content.ReadAsStringAsync();

            var cities = System.Text.Json.JsonSerializer.Deserialize<string[]>(jsonString);

            return cities;
		}

		public async Task<WeatherRecordDto> GetWeatherRecordAsync(string city)
		{
            var response = await _httpClient.GetAsync("weathers/" + city);

            if (!response.IsSuccessStatusCode)
            {
                // TODO: return more details
                return null;
            }

            string jsonString = await response.Content.ReadAsStringAsync();

            var weatherRecordDto = JsonConvert.DeserializeObject<WeatherRecordDto>(jsonString);

            return weatherRecordDto;
        }
	}
}