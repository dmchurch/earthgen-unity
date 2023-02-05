using Earthgen.planet.grid;
using Earthgen.planet.climate;
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

    public static class GridExtensions
    {
        public static Climate_tile climate(this Tile t, Season s)
        {
            if (s.tiles?.Length > t.id) {
                return s.nth_tile(t.id);
            }
            else {
                return Climate_tile.Default;
            }
        }

        public static Climate_corner climate(this Corner c, Season s) => s.corners.Length > c.id ? s.nth_corner(c.id) : Climate_corner.Default;
        public static Climate_edge climate(this Edge e, Season s) => s.edges.Length > e.id ? s.nth_edge(e.id) : Climate_edge.Default;

        public static ref Climate_tile m_climate(this Tile t, Season s) => ref s.m_tile(t.id);
        public static ref Climate_corner m_climate(this Corner c, Season s) => ref s.m_corner(c.id);
        public static ref Climate_edge m_climate(this Edge e, Season s) => ref s.m_edge(e.id);
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
