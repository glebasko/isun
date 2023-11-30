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
            _httpClient.BaseAddress = new Uri(baseUrl); //TODO: move to config file

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
			//TODO: move to config file
			var parameters = new
			{
				Username = "isun",
				Password = "passwrod"
			};

			string jsonParameters = JsonConvert.SerializeObject(parameters);

			string endpointUrl = _configuration["ApiSettings:Endpoints:Authorize"]
				?? throw new InvalidOperationException("Congifuration key 'ApiSettings:Endpoints:Authorize' not found.");

			var response = await _httpClient.PostAsync(endpointUrl, new StringContent(jsonParameters, Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode || response.Content is null)
			{
				return; //TODO: throw ex
			}

			string jsonString = await response.Content.ReadAsStringAsync();

			var authorizationTokenDto = JsonConvert.DeserializeObject<AuthorizationTokenDto>(jsonString);

			_httpClient.DefaultRequestHeaders.Add("Authorization", authorizationTokenDto.Token);
		}
	}
}