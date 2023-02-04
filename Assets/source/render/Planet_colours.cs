using System;
using UnityEngine;
using Earthgen.planet;
using Earthgen.planet.grid;
using Earthgen.planet.climate;
using Earthgen.planet.terrain;

namespace Earthgen.render
{
	public class Planet_colours
	{
		public Color[] tiles = new Color[0];

		public void clear_colours()
		{
			tiles = new Color[0];
		}
		public enum Mode
		{
            Topography,
            Vegetation,
            Temperature,
            Aridity,
            Humidity,
            Precipitation,
        };
		public void init_colours(Planet p) {
			Array.Resize(ref tiles, p.tile_count());
		}

		public void set_colours (Planet p, Mode mode) {
			if (mode == Mode.Topography)
				colour_topography(p);
		}

		public void set_colours (Planet p, Season s, Mode mode) {
			if (s != null) {
				if (mode == Mode.Vegetation)
					colour_vegetation(p, s);
				if (mode == Mode.Temperature)
					colour_temperature(p, s);
				else if (mode == Mode.Aridity)
					colour_aridity(p, s);
				else if (mode == Mode.Humidity)
					colour_humidity(p, s);
				else if (mode == Mode.Precipitation)
					colour_precipitation(p, s);
			}
			set_colours(p, mode);
		}

		// translation aids
		private static Color Colour(double r, double g, double b) => new((float)r, (float)g, (float)b);
		private static Color interpolate(Color a, Color b, double t) => Color.Lerp(a, b, (float)t);

		public void colour_topography (Planet p) {
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

			double[] land_limits = {-500, 0, 500, 1000, 1500, 2000, 2500};
			foreach (Tile t in p.tiles()) {
				Terrain_tile ter = t.terrain(p);
				double elev = ter.elevation - p.sea_level();
				if (ter.is_water()) {
					if (elev < -1000) {
						tiles[t.id] = water_deep;
					}
					else if (elev < -500) {
						double d = (elev+500)/(-500);
						tiles[t.id] = interpolate(water, water_deep, d);
					}
					else {
						double d = elev/(-500);
						tiles[t.id] = interpolate(water_shallow, water, d);
					}
				}
				else {
					tiles[t.id] = land[5];
					for (int i=0; i<5; i++) {
						if (elev <= land_limits[i+1]) {
							double d = Math.Clamp((elev - land_limits[i]) / (land_limits[i+1] - land_limits[i]), 0, 1);
							tiles[t.id] = interpolate(land[i], land[i+1], d);
							break;
						}
					}
				}
			}
		}

		public void colour_vegetation (Planet p, Season s) {
			Color snow = Colour(1.0, 1.0, 1.0);
			Color water_deep = Colour(0.05, 0.05, 0.20);
			Color water_shallow = Colour(0.04, 0.22, 0.42);
			Color land_low = Colour(0.95, 0.81, 0.53);
			Color land_high = Colour(0.1, 0.1, 0.1);
			Color vegetation = Colour(0.176, 0.32, 0.05);

			foreach (Tile t in p.tiles()) {
				if (p.terrain.nth_tile(t.id).is_water()) {
					double d = Math.Min(1.0f, p.terrain.nth_tile(t.id).water_depth()/400);
					tiles[t.id] = interpolate(water_shallow, water_deep, d);
				}
				else {
					var climate = s.nth_tile(t.id);
					if (climate.temperature <= Climate.freezing_point())
						tiles[t.id] = snow;
					else {
						double d = Math.Min(1.0, (p.terrain.nth_tile(t.id).elevation - p.sea_level())/2500);
						Color ground = interpolate(land_low, land_high, d);
						double v = Math.Min(1.0, climate.aridity()/1.5f);
						tiles[t.id] = interpolate(vegetation, ground, v);
					}
				}
			}
		}

		public void colour_temperature (Planet p, Season s) {
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

			foreach (Tile t in p.tiles()) {
				float temp = s.nth_tile(t.id).temperature - Climate.freezing_point();
				if (temp <= limits[0])
					tiles[t.id] = col[0];
				else if (temp >= limits[7])
					tiles[t.id] = col[7];
				else {
					for (int i=0; i<7; i++) {
						if (temp >= limits[i] && temp < limits[i+1]) {
							double d = (temp - limits[i]) / (limits[i+1] - limits[i]);
							tiles[t.id] = interpolate(col[i], col[i+1], d);
							break;
						}
					}
				}
			}
		}

		public void colour_aridity (Planet p, Season s) {
			Color water = Colour(1.0, 1.0, 1.0);

			Color[] col = {
				Colour(1.0, 0.0, 0.0),
				Colour(1.0, 1.0, 0.0),
				Colour(0.0, 1.0, 0.0),
				Colour(0.0, 0.5, 0.0)};
		
			float[] limits = {2.0f, 1.0f, 0.5f, 0.0f};

			foreach (Tile t in p.tiles()) {
				if (p.terrain.nth_tile(t.id).is_water())
					tiles[t.id] = water;
				else {
					float ar = s.nth_tile(t.id).aridity();
					tiles[t.id] = col[3];
					for (int i=1; i<4; i++) {
						if (ar > limits[i]) {
							double d = Math.Min(1.0f, (ar - limits[i]) / (limits[i-1] - limits[i]));
							tiles[t.id] = interpolate(col[i], col[i-1], d);
							break;
						}
					}
				}
			}
		}

		public void colour_humidity (Planet p, Season s) {
			Color water = Colour(1.0, 1.0, 1.0);
			Color land_dry = Colour(1.0, 1.0, 0.5);
			Color land_mid = Colour(1.0, 1.0, 0.0);
			Color land_humid = Colour(0.0, 0.7, 0.0);
	
			foreach (Tile t in p.tiles()) {
				double h = s.nth_tile(t.id).humidity / Climate.saturation_humidity(s.nth_tile(t.id).temperature);
				if (p.terrain.nth_tile(t.id).is_water()) {
					tiles[t.id] = water;
				}
				else {
					if (h <= 0.5) {
						double d = h / 0.5;
						tiles[t.id] = interpolate(land_dry, land_mid, d);
					}
					else {
						double d = (h-0.5)/0.5;
						tiles[t.id] = interpolate(land_mid, land_humid, d);
					}
				}
			}
		}

		public void colour_precipitation (Planet p, Season s) {
			Color water = Colour(1.0, 1.0, 1.0);
			Color dry = Colour(1.0, 1.0, 0.5);
			Color medium = Colour(0.0, 1.0, 0.0);
			Color wet = Colour(0.0, 0.0, 1.0);

			foreach (Tile t in p.tiles()) {
				double high = 7e-8;
				double low = high/10;
				if (p.terrain.nth_tile(t.id).is_water())
					tiles[t.id] = water;
				else {
					float prec = s.nth_tile(t.id).precipitation;
					if (prec < low) {
						double d = prec / low;
						tiles[t.id] = interpolate(dry, medium, d);
					}
					else {
						double d = Math.Min(1.0, (prec - low) / (high - low));
						tiles[t.id] = interpolate(medium, wet, d);
					}
				}
			}
		}

	}

}
