using System;
using System.Collections.Generic;
using UnityEngine;

namespace Earthgen.planet.climate
{
	public class Climate : ScriptableObject
	{
		public Climate_variables var;
		public List<Season> seasons = new();

		public static float freezing_point () {return 273.15f;}
		public static float temperature_lapse_rate () {return 9.8e-3f;}
		public static float temperature_lapse (float height) {
			return height * temperature_lapse_rate();
		}

		public static float saturation_humidity (float temperature) {
			double c = 4.6e-9;
			double k = 0.05174;
			return (float)(c*Math.Exp(k*temperature));
		}

		public static float aridity (float potential_evapotranspiration) {
			float index_base_temperature = 10 + freezing_point();
			return potential_evapotranspiration / saturation_humidity(index_base_temperature);
		}
	}

	public static partial class PlanetExtensions
	{
		public static void clear_climate (this Planet p) {
			p.climate.seasons.Clear();
			p.climate.var.season_count = 0;
		}

		public static List<Season> seasons (this Planet p) => p.climate.seasons;
		public static Season nth_season (this Planet p, int n) => p.climate.seasons[n];

	}
}
