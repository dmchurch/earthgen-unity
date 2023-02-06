using System;
using System.Collections.Generic;

using static Earthgen.Statics;

namespace Earthgen.planet.climate
{
	[Serializable]
	public class Climate
	{
		public Climate_variables var;
		public List<Season> seasons = new();

		public void clear_climate () {
			var.season_count = 0;
			seasons.Clear();
		}

		public static TemperatureFloat freezing_point () => TemperatureFloat.FromKelvins(273.15f);
		public static TemperatureFloat temperature_lapse_rate () => TemperatureFloat.FromKelvins(9.8e-3f);
		public static TemperatureFloat temperature_lapse (float height) {
			return height * temperature_lapse_rate();
		}

		public static float saturation_humidity (TemperatureFloat temperature) {
			double c = 4.6e-9;
			double k = 0.05174;
			return (float)(c*exp(k*temperature.Kelvins));
		}

		public static float aridity (float potential_evapotranspiration) {
			TemperatureFloat index_base_temperature = 10 + freezing_point();
			return potential_evapotranspiration / saturation_humidity(index_base_temperature);
		}
	}

	public static partial class PlanetExtensions
	{
		public static void clear_climate(this Planet p)
		{
			m_climate(p).clear_climate();
		}

		public static Climate climate (this Planet p) => p.climate;
		public static ref Climate m_climate (this Planet p) => ref p.climate;

		public static List<Season> seasons (this Planet p) => climate(p).seasons;
		public static Season nth_season (this Planet p, int n) => climate(p).seasons[n];
		public static Season m_season (this Planet p, int n) => m_climate(p).seasons[n];
	}

}

namespace Earthgen
{
	using Earthgen.planet;
	using Earthgen.planet.climate;
	public static partial class Statics
	{
		public static void clear_climate(Planet p) => PlanetExtensions.clear_climate(p);

		public static Climate climate (Planet p) => p.climate;
		public static ref Climate m_climate (Planet p) => ref p.climate;

		public static List<Season> seasons (Planet p) => climate(p).seasons;
		public static Season nth_season (Planet p, int n) => climate(p).seasons[n];
		public static Season m_season (Planet p, int n) => m_climate(p).seasons[n];

		public static TemperatureFloat freezing_point() => Climate.freezing_point();
		public static TemperatureFloat temperature_lapse_rate() => Climate.temperature_lapse_rate();
		public static TemperatureFloat temperature_lapse(float f) => Climate.temperature_lapse(f);
		public static float saturation_humidity(TemperatureFloat f) => Climate.saturation_humidity(f);
		public static float aridity(float f) =>  Climate.aridity(f);
	}
}