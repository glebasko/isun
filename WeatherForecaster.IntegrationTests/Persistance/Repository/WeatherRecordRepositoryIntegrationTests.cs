using Microsoft.EntityFrameworkCore;
using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Persistance;
using WeatherForecaster.Persistance.Repositories;
using Xunit;

namespace WeatherForecaster.IntegrationTests.Persistance.Repository
{
	public class WeatherRecordRepositoryIntegrationTests
	{
		[Fact]
		public async Task AddAsync_ShouldAddWeatherRecordToDatabase()
		{
			var options = new DbContextOptionsBuilder<WeatherForecasterDbContext>()
				.UseInMemoryDatabase(databaseName: "AddAsync_ShouldAddWeatherRecordToDatabase")
				.Options;

			using var db = new WeatherForecasterDbContext(options);
			var repository = new WeatherRecordRepository(db);

			var weatherRecordDto = new WeatherRecordDto
			{
				City = "TestCity",
				Temperature = 25,
				Precipitation = 5,
				WindSpeed = 10,
				Summary = "Chill"
			};

			await repository.AddAsync(weatherRecordDto);

			var addedRecord = db.WeatherRecords.Single(); 

			Assert.Equal(weatherRecordDto.City, addedRecord.City);
			Assert.Equal(weatherRecordDto.Temperature, addedRecord.Temperature);
			Assert.Equal(weatherRecordDto.Precipitation, addedRecord.Precipitation);
			Assert.Equal(weatherRecordDto.WindSpeed, addedRecord.WindSpeed);
			Assert.Equal(weatherRecordDto.Summary, addedRecord.Summary);
		}

		[Fact]
		public async Task AddRangeAsync_ShouldAddMultipleWeatherRecordsToDatabase()
		{
			var options = new DbContextOptionsBuilder<WeatherForecasterDbContext>()
				.UseInMemoryDatabase(databaseName: "AddRangeAsync_ShouldAddMultipleWeatherRecordsToDatabase")
				.Options;

			using var db = new WeatherForecasterDbContext(options);
			var repository = new WeatherRecordRepository(db);

			var weatherRecordDtos = new List<WeatherRecordDto>
			{
				new WeatherRecordDto 
				{
					City = "TestCity",
					Temperature = 25,
					Precipitation = 5,
					WindSpeed = 10,
					Summary = "Chill"
				},
				new WeatherRecordDto 
				{
					City = "TestCity2",
					Temperature = -10,
					Precipitation = 10,
					WindSpeed = 5,
					Summary = "Cold"
				}
			};

			await repository.AddRangeAsync(weatherRecordDtos);

			var addedRecords = db.WeatherRecords.ToList();
			Assert.Equal(weatherRecordDtos.Count, addedRecords.Count);

            for (int i = 0; i < weatherRecordDtos.Count; i++)
			{
				Assert.Equal(weatherRecordDtos[i].City, addedRecords[i].City);
				Assert.Equal(weatherRecordDtos[i].Temperature, addedRecords[i].Temperature);
				Assert.Equal(weatherRecordDtos[i].Precipitation, addedRecords[i].Precipitation);
				Assert.Equal(weatherRecordDtos[i].WindSpeed, addedRecords[i].WindSpeed);
				Assert.Equal(weatherRecordDtos[i].Summary, addedRecords[i].Summary);
			}
		}
	}
}