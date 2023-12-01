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

		public async Task<IEnumerable<string>> GetCitiesAsync()
		{
			string endpointUrl = _configuration["ApiSettings:Endpoints:Cities"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Endpoints:Cities' not found.");

			var response = await _httpClient.GetAsync(endpointUrl);
            if (!response.IsSuccessStatusCode)
            {
				throw new HttpRequestException($"API request to {_httpClient.BaseAddress + endpointUrl} failed. Status code: {response.StatusCode}");
			}

            string jsonString = await response.Content.ReadAsStringAsync();		
			if (string.IsNullOrEmpty(jsonString))
			{
				throw new InvalidOperationException($"API {_httpClient.BaseAddress + endpointUrl} response content is empty or null.");
			}

			var cities = System.Text.Json.JsonSerializer.Deserialize<string[]>(jsonString);
			if (cities is null)
			{
				throw new InvalidOperationException($"Failed to deserialize API {_httpClient.BaseAddress + endpointUrl} response.");
			}

			return cities;
		}

		public async Task<WeatherRecordDto> GetWeatherRecordAsync(string city)
		{
			string endpointUrl = _configuration["ApiSettings:Endpoints:Weathers"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Endpoints:Weathers' not found.");

			var response = await _httpClient.GetAsync(endpointUrl + "/" + city);
            if (!response.IsSuccessStatusCode)
            {
				throw new HttpRequestException($"API request to {_httpClient.BaseAddress + endpointUrl} failed. Status code: {response.StatusCode}");
			}

            string jsonString = await response.Content.ReadAsStringAsync();
			if (string.IsNullOrEmpty(jsonString))
			{
				throw new InvalidOperationException($"API {_httpClient.BaseAddress + endpointUrl} response content is empty or null.");
			}

			var weatherRecordDto = JsonConvert.DeserializeObject<WeatherRecordDto>(jsonString);
			if (weatherRecordDto is null)
			{
				throw new InvalidOperationException($"Failed to deserialize API {_httpClient.BaseAddress + endpointUrl} response.");
			}

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
			if (!response.IsSuccessStatusCode)
			{
				throw new HttpRequestException($"API request to {_httpClient.BaseAddress + endpointUrl} failed. Status code: {response.StatusCode}");
			}

			string jsonString = await response.Content.ReadAsStringAsync();
			if (string.IsNullOrEmpty(jsonString))
			{
				throw new InvalidOperationException($"API {_httpClient.BaseAddress + endpointUrl} response content is empty or null.");
			}

			var authorizationTokenDto = JsonConvert.DeserializeObject<AuthorizationTokenDto>(jsonString);
			if (authorizationTokenDto is null)
			{
				throw new InvalidOperationException($"Failed to deserialize API {_httpClient.BaseAddress + endpointUrl} response.");
			}

			_httpClient.DefaultRequestHeaders.Add("Authorization", authorizationTokenDto.Token);
		}
	}
}