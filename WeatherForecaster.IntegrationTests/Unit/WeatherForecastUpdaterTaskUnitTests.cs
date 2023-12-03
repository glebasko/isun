using Moq;
using WeatherForecaster.Domain.DTO;
using WeatherForecaster.Domain.Interfaces;
using WeatherForecaster.Domain.Services;
using Xunit;

namespace WeatherForecaster.Tests.Unit
{
    public class WeatherForecastUpdaterTaskUnitTests
    {
        [Fact]
        public async Task Start_ShouldStartTimerAndCallDoWorkAsync()
        {
            var apiServiceMock = new Mock<IWeatherForecastApiService>();
            apiServiceMock.Setup(x => x.GetWeatherRecordAsync(It.IsAny<string>())).Returns(Task.FromResult(new WeatherRecordDto()));

            var repoMock = new Mock<IWeatherRecordRepository>();
            var interval = TimeSpan.FromSeconds(1);
            var cities = new[] { "City1", "City2" };

            var updaterTask = new WeatherForecastUpdaterJob(apiServiceMock.Object, repoMock.Object, interval, cities);

            updaterTask.StartAsync();

            // Wait for at least two ticks
            await Task.Delay(2500);

            apiServiceMock.Verify(x => x.GetWeatherRecordAsync(It.IsAny<string>()), Times.AtLeast(4));
            repoMock.Verify(x => x.AddAsync(It.IsAny<WeatherRecordDto>()), Times.AtLeast(4));

            await updaterTask.StopAsync();
        }

        [Fact]
        public async Task StopAsync_ShouldCancelTaskAndDisposeCancellationTokenSource()
        {
            var apiServiceMock = new Mock<IWeatherForecastApiService>();
            var repositoryMock = new Mock<IWeatherRecordRepository>();
            var interval = TimeSpan.FromSeconds(1);
            var cities = new[] { "City1", "City2" };

            var updaterTask = new WeatherForecastUpdaterJob(apiServiceMock.Object, repositoryMock.Object, interval, cities);

            updaterTask.StartAsync();
            await updaterTask.StopAsync();

            Assert.True(updaterTask.GetTimerTask()?.IsCompleted ?? false);
        }
    }

    internal static class WeatherForecastUpdaterTaskExtensions
    {
        public static Task? GetTimerTask(this WeatherForecastUpdaterJob updaterTask)
        {
            return updaterTask.GetType().GetField(
                "_timerTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(updaterTask) as Task;
        }
    }
}
