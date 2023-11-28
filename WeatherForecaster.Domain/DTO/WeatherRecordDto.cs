namespace WeatherForecaster.Domain.DTO
{
	public class WeatherRecordDto
	{
        public string City { get; set; }
        public short Temperature { get; set; }
        public ushort Precipitation { get; set; }
        public ushort WindSpeed { get; set; }
        public string Summary { get; set; }
    }
}