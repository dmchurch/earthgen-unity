using System;
using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using Earthgen.planet.climate;
using static Earthgen.Statics;

namespace Earthgen.render
{
	[Serializable]
	public struct Planet_colours
	{
		public Color[] tiles;

		public enum Mode
		{
			Topography,
			Vegetation,
			Temperature,
			Aridity,
			Humidity,
			Precipitation,
		}

		public Mode TOPOGRAPHY => Mode.Topography;
		public Mode VEGETATION => Mode.Vegetation;
		public Mode TEMPERATURE => Mode.Temperature;
		public Mode ARIDITY => Mode.Aridity;
		public Mode HUMIDITY => Mode.Humidity;
		public Mode PRECIPITATION => Mode.Precipitation;

		public void clear_colours()
		{
			tiles = null;
		}

		public void init_colours (Planet p) {
			Array.Resize(ref tiles, tile_count(p));
		}

		public void set_colours (Planet p, Mode mode) {
			var c = this;
			if (mode == c.TOPOGRAPHY)
				colour_topography(p);
		}

		public void set_colours (Planet p, Season s, Mode mode) {
			var c = this;
			if (s != null) {
				if (mode == c.VEGETATION)
					colour_vegetation(p, s);
				if (mode == c.TEMPERATURE)
					colour_temperature(p, s);
				else if (mode == c.ARIDITY)
					colour_aridity(p, s);
				else if (mode == c.HUMIDITY)
					colour_humidity(p, s);
				else if (mode == c.PRECIPITATION)
					colour_precipitation(p, s);
			}
			set_colours(p, mode);
		}

		public void colour_topography (Planet p) {
			var c = this;
			Color water_deep = Colour(0.0, 0.0, 0.25);
			Color water = Colour(0.0, 0.12, 0.5);
			Color water_shallow = Colour(0.0, 0.4, 0.6);

			Color[] land = {
				Colour(0.0, 0.4, 0.0),
				Colour(0.0, 0.7, 0.0),
				Colour(1.0, 1.0, 0.0),
				Colour(1.0, 0.5, 0.0),
				Colour(0.7, 0.0, 0.0),
				Colour(0.1, 0.1, 0.1)};
			float[] land_limits = {-500, 0, 500, 1000, 1500, 2000, 2500};
			foreach (Tile t in tiles(p)) {
				Terrain_tile ter = nth_tile(terrain(p), id(t));
				double elev = elevation(ter) - sea_level(p);
				if (is_water(ter)) {
					if (elev < -1000) {
						c.tiles[id(t)] = water_deep;
					}
					else if (elev < -500) {
						double d = (elev+500)/(-500);
						c.tiles[id(t)] = interpolate(water, water_deep, d);
					}
					else {
						double d = elev/(-500);
						c.tiles[id(t)] = interpolate(water_shallow, water, d);
					}
				}
				else {
					c.tiles[id(t)] = land[5];
					for (int i=0; i<5; i++) {
						if (elev <= land_limits[i+1]) {
							double d = max(0.0, min(1.0, (elev - land_limits[i]) / (land_limits[i+1] - land_limits[i])));
							c.tiles[id(t)] = interpolate(land[i], land[i+1], d);
							break;
						}
					}
				}
			}
		}

		public void colour_vegetation (Planet p, Season s) {
			var c = this;
			Color snow = Colour(1.0, 1.0, 1.0);
			Color water_deep = Colour(0.05, 0.05, 0.20);
			Color water_shallow = Colour(0.04, 0.22, 0.42);
			Color land_low = Colour(0.95, 0.81, 0.53);
			Color land_high = Colour(0.1, 0.1, 0.1);
			Color vegetation = Colour(0.176, 0.32, 0.05);

			foreach (Tile t in tiles(p)) {
				if (is_water(nth_tile(terrain(p) ,id(t)))) {
					double d = min(1.0f, water_depth(nth_tile(terrain(p), id(t)))/400);
					c.tiles[id(t)] = interpolate(water_shallow, water_deep, d);
				}
				else {
					var climate = nth_tile(s, id(t));
					if (temperature(climate) <= freezing_point())
						c.tiles[id(t)] = snow;
					else {
						double d = min(1.0, (elevation(nth_tile(terrain(p), id(t))) - sea_level(p))/2500);
						Color ground = interpolate(land_low, land_high, d);
						double v = min(1.0f, aridity(climate)/1.5f);
						c.tiles[id(t)] = interpolate(vegetation, ground, v);
					}
				}
			}
		}

		public void colour_temperature (Planet p, Season s) {
			var c = this;
			Color[] col = {
				Colour(1.0, 1.0, 1.0),
				Colour(0.7, 0, 0.5),
				Colour(0, 0, 0.5),
				Colour(0, 0, 1.0),
				Colour(0, 1.0, 1.0),
				Colour(1.0, 1.0, 0),
				Colour(1.0, 0.1, 0),
				Colour(0.45, 0, 0)};
			float[] limits = {-50, -35, -20, -10, 0, 10, 20, 30};

			foreach (Tile t in tiles(p)) {
				float temp = temperature(nth_tile(s, id(t))).Celsius;
				if (temp <= limits[0])
					c.tiles[id(t)] = col[0];
				else if (temp >= limits[7])
					c.tiles[id(t)] = col[7];
				else {
					for (int i=0; i<7; i++) {
						if (temp >= limits[i] && temp < limits[i+1]) {
							double d = (temp - limits[i]) / (limits[i+1] - limits[i]);
							c.tiles[id(t)] = interpolate(col[i], col[i+1], d);
							break;
						}
					}
				}
			}
		}

		public void colour_aridity (Planet p, Season s) {
			var c = this;
			Color water = Colour(1.0, 1.0, 1.0);

			Color[] col = {
				Colour(1.0, 0.0, 0.0),
				Colour(1.0, 1.0, 0.0),
				Colour(0.0, 1.0, 0.0),
				Colour(0.0, 0.5, 0.0)};
		
			float[] limits = {2.0f, 1.0f, 0.5f, 0.0f};

			foreach (Tile t in tiles(p)) {
				if (is_water(nth_tile(terrain(p) ,id(t))))
					c.tiles[id(t)] = water;
				else {
					float ar = aridity(nth_tile(s, id(t)));
					c.tiles[id(t)] = col[3];
					for (int i=1; i<4; i++) {
						if (ar > limits[i]) {
							double d = min(1.0f, (ar - limits[i]) / (limits[i-1] - limits[i]));
							c.tiles[id(t)] = interpolate(col[i], col[i-1], d);
							break;
						}
					}
				}
			}
		}

		public void colour_humidity (Planet p, Season s) {
			var c = this;
			Color water = Colour(1.0, 1.0, 1.0);
			Color land_dry = Colour(1.0, 1.0, 0.5);
			Color land_mid = Colour(1.0, 1.0, 0.0);
			Color land_humid = Colour(0.0, 0.7, 0.0);
	
			foreach (Tile t in tiles(p)) {
				double h = humidity(nth_tile(s, id(t))) / saturation_humidity(temperature(nth_tile(s, id(t))));
				if (is_water(nth_tile(terrain(p), id(t)))) {
					c.tiles[id(t)] = water;
				}
				else {
					if (h <= 0.5) {
						double d = h / 0.5;
						c.tiles[id(t)] = interpolate(land_dry, land_mid, d);
					}
					else {
						double d = (h-0.5)/0.5;
						c.tiles[id(t)] = interpolate(land_mid, land_humid, d);
					}
				}
			}
		}

		public void colour_precipitation (Planet p, Season s) {
			var c = this;
			Color water = Colour(1.0, 1.0, 1.0);
			Color dry = Colour(1.0, 1.0, 0.5);
			Color medium = Colour(0.0, 1.0, 0.0);
			Color wet = Colour(0.0, 0.0, 1.0);

			foreach (Tile t in tiles(p)) {
				double high = 7e-8;
				double low = high/10;
				if (is_water(nth_tile(terrain(p), id(t))))
					c.tiles[id(t)] = water;
				else {
					float prec = precipitation(nth_tile(s, id(t)));
					if (prec < low) {
						double d = prec / low;
						c.tiles[id(t)] = interpolate(dry, medium, d);
					}
					else {
						double d = min(1.0, (prec - low) / (high - low));
						c.tiles[id(t)] = interpolate(medium, wet, d);
					}
				}
			}
		}
	}

}

namespace Earthgen
{
	using render;
	public static partial class Statics
	{
		public static void clear_colours (Planet_colours c) => c.clear_colours();
		public static void init_colours (Planet_colours c, Planet p) => c.init_colours(p);
		public static void set_colours (Planet_colours c, Planet p, Planet_colours.Mode mode) => c.set_colours(p, mode);
		public static void set_colours (Planet_colours c, Planet p, Season s, Planet_colours.Mode mode) => c.set_colours(p, s, mode);
		public static void colour_topography (Planet_colours c, Planet p) => c.colour_topography(p);
		public static void colour_vegetation (Planet_colours c, Planet p, Season s) => c.colour_vegetation(p, s);
		public static void colour_temperature (Planet_colours c, Planet p, Season s) => c.colour_temperature(p, s);
		public static void colour_aridity (Planet_colours c, Planet p, Season s) => c.colour_aridity(p, s);
		public static void colour_humidity (Planet_colours c, Planet p, Season s) => c.colour_humidity(p, s);
		public static void colour_precipitation (Planet_colours c, Planet p, Season s) => c.colour_precipitation(p, s);
	}
}
