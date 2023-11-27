using isun.Domain.DTO;
using isun.Domain.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace isun.Domain.Services
{
	public class WeatherForecastApiService : IWeatherForecastApiService
	{
        private readonly HttpClient _httpClient;

        public WeatherForecastApiService() 
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://weather-api.isun.ch/api/");

            AuthorizeHttpClient().Wait();
        }

        private async Task AuthorizeHttpClient()
		{
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

		public async Task<IEnumerable<string>> GetCitiesAsync()
		{       
            var response = await _httpClient.GetAsync("https://weather-api.isun.ch/api/cities");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string jsonString = await response.Content.ReadAsStringAsync();

            var cities = System.Text.Json.JsonSerializer.Deserialize<string[]>(jsonString);

            return cities;
		}

		public async Task<WeatherRecordDto> GetWeatherRecordAsync()
		{
			throw new NotImplementedException();
		}
	}
}