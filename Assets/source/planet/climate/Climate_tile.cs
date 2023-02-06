using System;
using static Earthgen.Statics;

namespace Earthgen.planet.climate
{
    [Serializable]
	public struct Climate_tile
	{
		public Wind wind;
		public float temperatureInDegreesCelsius;
		public TemperatureFloat temperature 
		{
			get => TemperatureFloat.FromDegreesC(temperatureInDegreesCelsius);
			set => temperatureInDegreesCelsius = value.Celsius;
		}
		public float humidity;
		public float precipitation;

		public float aridity => Climate.aridity(potential_evapotranspiration);
		public float potential_evapotranspiration => saturation_humidity(temperature - humidity);
	}
}

namespace Earthgen
{
	using Earthgen.planet.climate;
	public static partial class Statics
	{
		public static TemperatureFloat temperature (Climate_tile t) => t.temperature;
		public static float humidity (Climate_tile t) => t.humidity;
		public static float aridity (Climate_tile t) {
			return aridity(potential_evapotranspiration(t));
		}
		public static float potential_evapotranspiration (Climate_tile t) {
			return saturation_humidity(temperature(t)) - humidity(t);
		}
		public static float precipitation (Climate_tile t) => t.precipitation;
	}
}
