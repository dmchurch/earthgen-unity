using System;

namespace Earthgen.planet.climate
{
	[Serializable]
	public struct Climate_tile
	{
		public static readonly Climate_tile Default = default;
		public Wind wind;
		public float temperature;
		public float humidity;
		public float precipitation;

		public float aridity () {
			return Climate.aridity(potential_evapotranspiration());
		}
		public float potential_evapotranspiration () {
			return Climate.saturation_humidity(temperature) - humidity;
		}
	}


}