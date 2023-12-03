using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;

namespace WeatherForecaster.Domain.Services
{
	public class WeatherForecastUpdaterJob
	{
		private Task? _timerTask;
		private readonly PeriodicTimer _timer;
		private readonly CancellationTokenSource _cts = new();

		private readonly IWeatherForecastApiService _weatherForecastApiService;
		private readonly IWeatherRecordRepository _weatherRecordRepository;
		private readonly string[] _cities;

		public WeatherForecastUpdaterJob(IWeatherForecastApiService weatherForecastApiService, IWeatherRecordRepository weatherRecordRepository, TimeSpan interval, string[] cities)
		{
			_weatherForecastApiService = weatherForecastApiService;
			_weatherRecordRepository = weatherRecordRepository;
			_timer = new PeriodicTimer(interval);
			_cities = cities;
		}

		public async Task StartAsync()
		{
			_timerTask = DoWorkAsync();
			await _timerTask;
		}

		public async Task StopAsync()
		{
			if (_timerTask is null)
			{
				return;
			}

			_cts.Cancel();

			if (!_timerTask.IsFaulted)
			{
				await _timerTask;
			}

			_cts.Dispose();
		}

		private async Task DoWorkAsync()
		{
			try
			{
				while (await _timer.WaitForNextTickAsync(_cts.Token))
				{
					foreach (var city in _cities)
					{
						var weatherRecordDto = await _weatherForecastApiService.GetWeatherRecordAsync(city);

						await _weatherRecordRepository.AddAsync(weatherRecordDto);

						PrintOutWeatherForecast(weatherRecordDto);
					}
				}
			} 
			catch (OperationCanceledException) { }
		}

		private static void PrintOutWeatherForecast(WeatherRecordDto weatherRecordDto)
		{
			Console.WriteLine("\nCity: " + weatherRecordDto.City);
			Console.WriteLine("Temperature: " + weatherRecordDto.Temperature);
			Console.WriteLine("Precipitation: " + weatherRecordDto.Precipitation);
			Console.WriteLine("WindSpeed: " + weatherRecordDto.WindSpeed);
			Console.WriteLine("Summary: " + weatherRecordDto.Summary);
		}
	}
}