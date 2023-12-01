using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WeatherForecaster.Domain.Services
{
	public class WeatherForecastApiService : IWeatherForecastApiService
	{
		private readonly IConfiguration _configuration;
		private readonly HttpClient _httpClient;

		public WeatherForecastApiService(IConfiguration configuration) 
        {
			_configuration = configuration;

			string baseUrl = _configuration["ApiSettings:BaseUrl"] 
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:BaseUrl' not found.");

			_httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl);

            AuthorizeHttpClient().Wait();
        }

        //TODO: refactor returns
		public async Task<IEnumerable<string>> GetCitiesAsync()
		{
			string endpointUrl = _configuration["ApiSettings:Endpoints:Cities"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Endpoints:Cities' not found.");

			var response = await _httpClient.GetAsync(endpointUrl);

            if (!response.IsSuccessStatusCode)
            {
				//TODO: throw ex
				return null; 
            }

            string jsonString = await response.Content.ReadAsStringAsync();

            var cities = System.Text.Json.JsonSerializer.Deserialize<string[]>(jsonString);

            return cities;
		}

		public async Task<WeatherRecordDto> GetWeatherRecordAsync(string city)
		{
			string endpointUrl = _configuration["ApiSettings:Endpoints:Weathers"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Endpoints:Weathers' not found.");

			var response = await _httpClient.GetAsync(endpointUrl + "/" + city);

            if (!response.IsSuccessStatusCode)
            {
                // TODO: return more details
                return null;
            }

            string jsonString = await response.Content.ReadAsStringAsync();

            var weatherRecordDto = JsonConvert.DeserializeObject<WeatherRecordDto>(jsonString);

            return weatherRecordDto;
        }

		private async Task AuthorizeHttpClient()
		{
			string endpointUrl = _configuration["ApiSettings:Authorization:EndpointUrl"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Endpoints:Authorize' not found.");

			var parameters = new
			{
				Username = _configuration["ApiSettings:Authorization:Usr"]
					?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Authorization:Usr' not found."),
				Password = _configuration["ApiSettings:Authorization:Pwd"]
					?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Authorization:Pwd' not found.")
			};

			string jsonParameters = JsonConvert.SerializeObject(parameters);

			var response = await _httpClient.PostAsync(endpointUrl, new StringContent(jsonParameters, Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode || response.Content is null)
			{
				//TODO: throw ex
				return; 
			}

			string jsonString = await response.Content.ReadAsStringAsync();

			var authorizationTokenDto = JsonConvert.DeserializeObject<AuthorizationTokenDto>(jsonString);

			_httpClient.DefaultRequestHeaders.Add("Authorization", authorizationTokenDto.Token);
		}
	}
}