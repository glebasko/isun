using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;

namespace WeatherForecaster.Domain.Services
{
	public class WeatherForecastUpdaterTask
	{
		private Task? _timerTask;
		private readonly PeriodicTimer _timer;
		private readonly CancellationTokenSource _cts = new();

		private readonly IWeatherForecastApiService _weatherForecastApiService;
		private readonly IWeatherRecordRepository _weatherRecordRepository;
		private readonly string[] _cities;
		private readonly List<WeatherRecordDto> _weatherRecordDtos = new();

		public WeatherForecastUpdaterTask(IWeatherForecastApiService weatherForecastApiService, IWeatherRecordRepository weatherRecordRepository, TimeSpan interval, string[] cities)
		{
			_weatherForecastApiService = weatherForecastApiService;
			_weatherRecordRepository = weatherRecordRepository;
			_timer = new PeriodicTimer(interval);
			_cities = cities;
		}

		public void Start()
		{
			_timerTask = DoWorkAsync();
		}

		public async Task StopAndSaveToDbAsync()
		{
			if (_timerTask is null)
			{
				return;
			}

			_cts.Cancel();
			await _timerTask;

			await SaveEntitiesToDb();

			_cts.Dispose();

			Console.WriteLine("\nTask was cancelled");
		}

		private async Task SaveEntitiesToDb()
		{
			await _weatherRecordRepository.AddRangeAsync(_weatherRecordDtos);
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
						_weatherRecordDtos.Add(weatherRecordDto);
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