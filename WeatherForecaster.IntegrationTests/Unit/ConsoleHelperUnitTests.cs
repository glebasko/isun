using WeatherForecaster.ConsoleUI;
using Xunit;

namespace WeatherForecaster.Tests.Unit
{
	public class ConsoleHelperUnitTests
	{
		private readonly string[] _apiCities =
		[
			"Vilnius",
			"Klaipėda",
			"Riga",
			"Tallinn",
			"Rome",
			"Berlin",
			"Paris",
			"Tunis",
			"Oslo",
			"N'Djamena"
		];
		
		[Fact]
		public void ProcessAndValidateCmdArgs_ShouldFailWithEmptyArgs()
		{
			bool result = ConsoleHelper.ProcessAndValidateCmdArgs([], _apiCities, out string[] outputArgs);

			Assert.False(result);
		}

		[Theory]
		[InlineData
		(	
			"Vilnius",
			"Klaipėda",
			"00000" //should cause failure
		)]
		public void ProcessAndValidateCmdArgs_ShouldFailWithInvalidCities(params string[] args)
		{
			bool result = ConsoleHelper.ProcessAndValidateCmdArgs(args, _apiCities, out string[] outputArgs);

			Assert.False(result);
		}

		[Theory]
		[InlineData
		(
			"Vilnius",
			"Klaipėda,",
			"Riga" 
		)]
		public void ProcessAndValidateCmdArgs_ShouldPassAndReturnCorrectCities(params string[] args)
		{
			bool result = ConsoleHelper.ProcessAndValidateCmdArgs(args, _apiCities, out string[] outputArgs);

			Assert.True(result);
			Assert.Equal(3, outputArgs.Length);
		}
	}
}