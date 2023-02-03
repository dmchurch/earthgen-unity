using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Earthgen.planet.climate.Climate;

namespace Earthgen.planet.climate
{
	public static class Climate_generation
	{
		public static void generate_climate(this Planet planet, Climate_parameters par)
		{
			planet.clear_climate();
			planet.terrain.var.axial_tilt = par.axial_tilt;
			planet.climate.var.season_count = par.seasons;
			Debug.Log("Seasons:");
			for (int i=0; i<par.seasons; i++) {
				Debug.Log(i);
				generate_season(planet, par, (float)i/par.seasons);
			}
			Debug.Log("done");
		}

		private static void copy_season (Climate_generation_season from, Season to) {
			for (int i=0; i<from.tiles.Length; i++) {
				to.tiles[i].temperature = from.tiles[i].temperature;
				to.tiles[i].humidity = from.tiles[i].humidity;
				to.tiles[i].precipitation = from.tiles[i].precipitation;
			}
			for (int i=0; i<from.edges.Length; i++) {
				to.edges[i].wind_velocity = from.edges[i].wind_velocity;
			}
		}

		public static void generate_season (this Planet planet, Climate_parameters par, float time_of_year) {
			Climate_generation_season season = new();
			season.tiles = new Climate_generation_tile[planet.tile_count()];
			season.corners = new Climate_generation_corner[planet.corner_count()];
			season.edges = new Climate_generation_edge[planet.edge_count()];

			season.var.time_of_year = time_of_year;
			season.var.solar_equator = (float)planet.axial_tilt() * Mathf.Sin(2.0f*Mathf.PI*time_of_year);
			season.tropical_equator = 0.67f*season.var.solar_equator;
	
			_set_temperature(planet, par, season);
			_set_wind(planet, par, season);
			_set_humidity(planet, par, season);
		//	_set_river_flow(planet, par, season);
	
			Season s = new();
			s.tiles = new Climate_tile[planet.tile_count()];
			s.corners = new Climate_corner[planet.corner_count()];
			s.edges = new Climate_edge[planet.edge_count()];
			copy_season(season, s);
			planet.climate.seasons.Add(s);
		}

		private static void _set_temperature (Planet planet, Climate_parameters _, Climate_generation_season season) {
			float temperature_at_latitude(float latitude) {
				return freezing_point() - 25 + 50*Mathf.Cos(latitude);
			};
	
			foreach (var t in planet.tiles()) {
				float temperature = temperature_at_latitude(season.tropical_equator - (float)planet.latitude(t.v));
				if (planet.terrain.nth_tile(t.id).is_land()) {
					if (planet.terrain.nth_tile(t.id).elevation > planet.sea_level())
						temperature -= temperature_lapse(planet.terrain.nth_tile(t.id).elevation - (float)planet.sea_level());
				}
				else {
					temperature = 0.3f*temperature + 0.7f*temperature_at_latitude((float)planet.latitude(t.v));
				}
				season.tiles[t.id].temperature = temperature;
			}
		}

		// Too annoying to convert all these to Unity notation, just making some stand-ins
		private static float angle(Vector2 v) => Mathf.Atan2(v.y, v.x);
		private static Vector2Rotation rotation_matrix(double angle) => new((float)angle);

		private struct Vector2Rotation
		{
			private Quaternion q;
			public Vector2Rotation(float angle) => q = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
			public static Vector2 operator *(Vector2Rotation rot, Vector2 v) => rot.q * v;
			public static List<Vector2> operator *(Vector2Rotation rot, IEnumerable<Vector2> vv)
				=> (from v in vv select (Vector2)(rot.q * v)).ToList();
		}

		private static Vector2 _default_pressure_gradient_force (double tropical_equator, double latitude) {
			double pressure_derivate = 0.0;
			double pressure_deviation = 20.0 / 15000;
			if (latitude > tropical_equator) {
				double c = 3.0 * Math.PI / (Math.PI / 2.0 - tropical_equator);
				pressure_derivate = pressure_deviation * Math.Sin(c * (latitude - tropical_equator));
			}
			else {
				double c = 3.0 * Math.PI / (Math.PI / 2.0 + tropical_equator);
				pressure_derivate = pressure_deviation * Math.Sin(c * (latitude - tropical_equator));
			}
			if (latitude < tropical_equator + (Math.PI/2.0 - tropical_equator)/3.0 && latitude > tropical_equator - (Math.PI/2.0 + tropical_equator)/3.0) {
				pressure_derivate = pressure_derivate / 3.0;
			}
			return new Vector2(-(float)pressure_derivate, 0);
		}

		private static Wind _prevailing_wind (Vector2 pressure_gradient_force, double coriolis_coefficient, double friction_coefficient) {
			double angle_offset = Math.Atan2(coriolis_coefficient, friction_coefficient);
			double speed = pressure_gradient_force.magnitude / new Vector2((float)coriolis_coefficient, (float)friction_coefficient).magnitude;
			Vector2 v = rotation_matrix(angle(pressure_gradient_force) - angle_offset) * new Vector2(1, 0);
			Wind w;
			w.speed = (float)speed;
			w.direction = Mathf.Atan2(v.y, v.x);
			return w;
		}

		private static Wind _default_wind (Planet p, int i, double tropical_equator) {
			Vector2 pressure_force = _default_pressure_gradient_force(tropical_equator, p.latitude(p.nth_tile(i).v));
			double coriolis_coeff = p.coriolis_coefficient(p.latitude(p.nth_tile(i).v));
			double friction = p.terrain.nth_tile(i).is_land() ? 0.000045 : 0.000045;
			return _prevailing_wind(pressure_force, coriolis_coeff, friction);
		}

		private static void _set_wind (Planet planet, Climate_parameters _, Climate_generation_season season) {
			foreach (var t in planet.tiles()) {
				season.tiles[t.id].wind = _default_wind(planet, t.id, season.tropical_equator);
				season.tiles[t.id].wind.direction += (float)planet.north(t);
			}
			foreach (var t in planet.tiles()) {
				//tile shape in 2d, rotated according to wind direction
				List<Vector2> corners =
					rotation_matrix(planet.north(t) - season.tiles[t.id].wind.direction) * t.polygon(planet.rotation_to_default());

				int e = t.edge_count;
				for (int k=0; k<e; k++) {
					int direction = t.nth_edge(k).sign(t);
					if (corners[k].x + corners[(k+1)%e].x < 0) direction *= -1;
					season.edges[t.nth_edge(k).id].wind_velocity -=
						0.5f * direction
						* season.tiles[t.id].wind.speed
						* Math.Abs(corners[k].y - corners[(k+1)%e].y)
						/ (corners[k] - corners[(k+1)%e]).magnitude;
				}
			}
		}

		private static float _air_flow_volume (Planet planet, Edge e, float wind_velocity) {
			float atmosphere_volume_scale = 100;
			float flow = (float)planet.length(e) * wind_velocity * atmosphere_volume_scale;
			if (flow < 0.0) flow *= -1;
			return flow;
		}

		private static float _incoming_wind (Planet planet, Climate_generation_season season, int i) {
			float sum = 0;
			Tile t = planet.nth_tile(i);
			foreach (var e in t.edges) {
				if (e.sign(t) * season.edges[e.id].wind_velocity > 0) {
					sum +=
						Mathf.Abs(season.edges[e.id].wind_velocity)
						* (float)planet.length(e);
				}
			}
			return sum;
		}

		private static float _outgoing_wind (Planet planet, Climate_generation_season season, int i) {
			float sum = 0;
			Tile t = planet.nth_tile(i);
			foreach (var e in t.edges) {
				if (e.sign(t) * season.edges[e.id].wind_velocity < 0) {
					sum +=
						Mathf.Abs(season.edges[e.id].wind_velocity)
						* (float)planet.length(e);
				}
			}
			return sum;
		}

		private static float _incoming_humidity (Planet planet, Climate_generation_season season, int i) {
			float humidity = 0;
			Tile t = planet.nth_tile(i);
			for (int k=0; k<t.edge_count; k++) {
				Edge e = t.nth_edge(k);
				if (e.sign(t) * season.edges[e.id].wind_velocity > 0) {
					humidity +=
						season.tiles[t.nth_tile(k).id].humidity
						* Math.Abs(season.edges[e.id].wind_velocity)
						* (float)planet.length(e);
				}
			}
			return humidity;
		}

		private static float _humidity_change (float first, float second) {
			float near_zero = 1.0e-15f;
			if (first < near_zero) {
				if (second > near_zero) return 1;
				else return 0;
			}
			return 1.0f - first/second;
		}

		private static void _iterate_humidity (Planet planet, Climate_parameters par, Climate_generation_season season) {
			var humidity = new float[planet.tile_count()];
			var precipitation = new float[planet.tile_count()];
	
			float delta = 1;
			while (delta > par.error_tolerance) {
		//		std::cout << "delta: " << delta << "\n";
				for (int i=0; i<planet.tile_count(); i++) {
					precipitation[i] = 0;
					if (planet.terrain.nth_tile(i).is_land()) {
						humidity[i] = 0;
						precipitation[i] = 0;
						float incoming_wind = _incoming_wind(planet, season, i);
						float outgoing_wind = _outgoing_wind(planet, season, i);
						if (incoming_wind > 0.0) {
							float convection = outgoing_wind - incoming_wind;
							float incoming_humidity = _incoming_humidity(planet, season, i);
							// less humidity when incoming wind is less than outgoing
							float density = convection > 0 ?
								incoming_humidity / (incoming_wind + convection) :
								incoming_humidity / incoming_wind;
							float saturation = saturation_humidity(season.tiles[i].temperature);
							// limit to saturation humidity
							humidity[i] = Math.Min(saturation, density);
							if (saturation < density)
								precipitation[i] += (density - saturation) * incoming_wind;
							// increase humidity when outgoing wind is less than incoming
							if (convection < 0) {
								float convective = humidity[i] * (-convection / incoming_wind);
								if (humidity[i] + convective > saturation)
									precipitation[i] += (humidity[i] + convective - saturation) * (-convection);
								humidity[i] = Math.Min(saturation, humidity[i] + convective);
							}
						}
						// scale by constant and area
						precipitation[i] *= 3 / (float)planet.area(planet.nth_tile(i));
					}
					else
						humidity[i] = season.tiles[i].humidity;
				}
				float largest_change = 0;
				for (int i=0; i<planet.tile_count(); i++) {
					largest_change = Math.Max(largest_change, _humidity_change(season.tiles[i].humidity, humidity[i]));
				}
				delta = largest_change;
				for (int i=0; i<planet.tile_count(); i++) {
					season.tiles[i].humidity = humidity[i];
					season.tiles[i].precipitation = precipitation[i];
				}
			}
		}

		private static void _set_humidity (Planet planet, Climate_parameters par, Climate_generation_season season) {
			foreach (var t in planet.tiles()) {
				float humidity = 0;
				if (planet.terrain.nth_tile(t.id).is_water()) {
					humidity = saturation_humidity(season.tiles[t.id].temperature);
				}
				season.tiles[t.id].humidity = humidity;
			}
			_iterate_humidity(planet, par, season);
		}

		private static int _lowest_corner (Planet p, Tile t) {
			return 0;
		}

		private static void _set_river_flow (Planet planet, Climate_parameters par, Climate_generation_season season) {
			/*
			foreach (var t in planet.tiles()) {
				lowest_corner(t).river_flow_increase += t.precipitation;
			}
			std::multimap<int, Corner> river_sources;
			for c : corners {
				if distance_to_sea(c) > 0
					river_sources.insert(-distance_to_sea(c), c);
			}
			for c : river_sources {
				float flow = 0;
				flow += climate_generation_corner(c.id).river_flow_increase
				if corner has river behind it..
				flow += edge.river_flow;
				climate_generation_edge(river_direction(c)).river_flow = flow;
			}
			*/
		}

	}
}