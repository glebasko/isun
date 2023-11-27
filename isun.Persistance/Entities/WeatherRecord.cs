namespace isun.Persistance.Entities
{
	public class WeatherRecord
	{
		public int Id {  get; set; }
		public string City { get; set; }
		public short Temperature { get; set; }
		public ushort Precipitation { get; set; }
		public ushort WindSpeed { get; set; }
		public string Summary { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

//{
//    "city": "Vilnius",
//  "temperature": -3,
//  "precipitation": 11,
//  "windSpeed": 7,
//  "summary": "Cool"
//}