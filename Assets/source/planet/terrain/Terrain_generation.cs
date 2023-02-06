using UnityEngine;
using Earthgen.planet;

using static Earthgen.Statics;
using Earthgen.planet.grid;
using System.Collections.Generic;

namespace Earthgen.planet.terrain
{

	public static class Terrain_generation
	{
		public static void generate_terrain (Planet p, Terrain_parameters par) {
			clear(p);
			set_grid_size(p, par.grid_size);
			init_terrain(p);
			_set_variables(p, par);
			_set_elevation(p, par);
			_create_sea(p, par);
			_classify_terrain(p);
			_set_river_directions(p);
		}

		static void _set_variables (Planet p, Terrain_parameters par) {
			m_terrain(p).var.axis = par.axis;
			m_terrain(p).var.radius = 40000000;
		}

		// Needed to disambiguate from the namespace name
		private static Terrain terrain(Planet p) => p.terrain;

		static void _set_elevation (Planet p, Terrain_parameters par) {
			// can be made concurrent
			var d = _elevation_vectors(par);
			foreach (var t in tiles(p))
				m_tile(m_terrain(p), id(t)).elevation = _elevation_at_point(t.v, d);
			foreach (var c in corners(p))
				m_corner(m_terrain(p), id(c)).elevation = _elevation_at_point(c.v, d);
			_scale_elevation(p, par);
		}

		static void _scale_elevation (Planet p, Terrain_parameters par) {
			float lowest = elevation(nth_tile(terrain(p),0));
			float highest = lowest;
			float scale = 3000;
			foreach (var t in tiles(terrain(p))) {
				lowest = min(lowest, elevation(t));
				highest = max(highest, elevation(t));
			}
			foreach (var c in corners(terrain(p))) {
				lowest = min(lowest, elevation(c));
				highest = max(highest, elevation(c));
			}
			highest = max(1.0f, highest-lowest);
			foreach (var t in tiles(p)) {
				m_tile(m_terrain(p), id(t)).elevation -= lowest;
				m_tile(m_terrain(p), id(t)).elevation *= scale / highest;
			}
			foreach (var c in corners(p)) {
				m_corner(m_terrain(p), id(c)).elevation -= lowest;
				m_corner(m_terrain(p), id(c)).elevation *= scale / highest;
			}
		}

		static Tile lowest_tile (Planet p) {
			Tile tile = nth_tile(p, 0);
			float lowest_elevation = elevation(nth_tile(terrain(p), 0));
			foreach (Tile t in tiles(p)) {
				if (elevation(nth_tile(terrain(p), id(t))) < lowest_elevation) {
					tile = t;
					lowest_elevation = elevation(nth_tile(terrain(p), id(t)));
				}
			}
			return tile;
		}

		static void _create_sea (Planet p, Terrain_parameters par) {
			Tile start_tile = lowest_tile(p);
			float sea_level = elevation(nth_tile(terrain(p), id(start_tile)));
			int water_tile_count = Mathf.RoundToInt(par.water_ratio * tile_count(p));
			HashSet<Tile> water_tiles = new();
			SortedSet<(float first, Tile second)> coast_tiles_elevation = new();
			List<bool> coast_tiles = new();
			if (water_tile_count > 0) {
				water_tiles.insert(start_tile);
				coast_tiles.resize(tile_count(p), false);
				foreach (Tile i in tiles(start_tile)) {
					coast_tiles[id(i)] = true;
					coast_tiles_elevation.insert(make_pair(elevation(nth_tile(terrain(p) ,id(i))), i));
				}
				Tile tile;
				void insert_next_tile() {
					tile = coast_tiles_elevation.begin().second;
					water_tiles.insert(tile);
					coast_tiles[id(tile)] = false;
					coast_tiles_elevation.erase(coast_tiles_elevation.begin());
					foreach (var i in tiles(tile)) {
						if (!water_tiles.Contains(i) && !coast_tiles[id(i)]) {
							coast_tiles[id(i)] = true;
							coast_tiles_elevation.insert(make_pair(elevation(nth_tile(terrain(p), id(i))), i));
						}
					}
				};
				while (water_tiles.size() < water_tile_count) {
					insert_next_tile();
					sea_level = elevation(nth_tile(terrain(p), id(tile)));
					while (coast_tiles_elevation.size() > 0 && coast_tiles_elevation.begin().first <= sea_level) {
						insert_next_tile();
					}
				}
				if (coast_tiles_elevation.size() > 0)
					sea_level = (sea_level + coast_tiles_elevation.begin().first) / 2;
			}
			m_terrain(p).var.sea_level = sea_level;
			foreach (var t in water_tiles) {
				m_tile(m_terrain(p), id(t)).water.surface = sea_level;
				m_tile(m_terrain(p), id(t)).water.depth = sea_level - elevation(nth_tile(terrain(p), id(t)));
			}
		}

		static Terrain_tile.Type _tile_type (Planet p, Tile t) {
			bool land = false;
			bool water = false;
			foreach (var i in tiles(t)) {
				if (water_depth(nth_tile(terrain(p) ,id(i))) > 0) water = true;
				else land = true;
			}
			Terrain_tile.Type type =
				water_depth(nth_tile(terrain(p) ,id(t))) > 0 ?
					Terrain_tile.Type.Water :
					Terrain_tile.Type.Land;
			if (land && water)
				type |= Terrain_tile.Type.Coast;
			return type;
		}

		static Terrain_corner.Type _corner_type (Planet p, Corner c) {
			bool land = false;
			bool water = false;
			foreach (var i in tiles(c)) {
				if (water_depth(nth_tile(terrain(p) ,id(i))) > 0) water = true;
				else land = true;
			}
			Terrain_corner.Type type =
				land && water ?
					Terrain_corner.Type.Coast :
					land ?
						Terrain_corner.Type.Land :
						Terrain_corner.Type.Water;
			return type;
		}

		static Terrain_edge.Type _edge_type (Planet p, Edge e) {
			bool land = false;
			bool water = false;
			foreach (var i in tiles(e)) {
				if (water_depth(nth_tile(terrain(p) ,id(i))) > 0) water = true;
					else land = true;
			}
			Terrain_edge.Type type =
				land && water ?
					Terrain_edge.Type.Coast :
					land ?
						Terrain_edge.Type.Land :
						Terrain_edge.Type.Water;
			return type;
		}

		static void _classify_terrain (Planet p) {
			foreach (var t in tiles(p))
				m_tile(m_terrain(p), id(t)).type = _tile_type(p, t);
			foreach (var c in corners(p))
				m_corner(m_terrain(p), id(c)).type = _corner_type(p, c);
			foreach (var e in edges(p))
				m_edge(m_terrain(p), id(e)).type = _edge_type(p, e);
		}

		static void _set_river_directions (Planet p) {
			SortedSet<(float first, Corner second)> endpoints = new();
			foreach (var c in corners(p))
				if (is_coast(nth_corner(terrain(p), id(c)))) {
					m_corner(m_terrain(p), id(c)).distance_to_sea = 0;
					endpoints.insert(make_pair(elevation(nth_corner(terrain(p), id(c))), c));
		}
			while (endpoints.size() > 0) {
			var first = endpoints.begin();
			Corner c = first.second;
			foreach (var n in corners(c)) {
				Terrain_corner ter = m_corner(m_terrain(p), id(n));
					if (is_land(ter) && ter.river_direction == -1) {
					ter.river_direction = position(n, c);
						ter.distance_to_sea = 1 + distance_to_sea(nth_corner(terrain(p), id(c)));
						endpoints.insert(make_pair(elevation(ter), n));
					}
				}
				endpoints.erase(first);			
			}
		}

		static int byte_array_to_int(byte[] s) {
			uint n = 0;
			for (int i=0; i<s.Length; i++) {
				n *= 256;
				n += s[i];
			}
			return (int)n;
		}

		static List<Vector3[]> _elevation_vectors (Terrain_parameters par) {
			Random.InitState(byte_array_to_int(md5(par.seed)));
			List <Vector3[]> d = new();
			for (int i=0; i<par.iterations; i++) {
				Vector3[] v = {
					Random.onUnitSphere,
					Random.onUnitSphere,
					Random.onUnitSphere};
				d.push_back(v);
			}
			return d;
		}

		static float _elevation_at_point (Vector3 point, List<Vector3[]> elevation_vectors) {
			float elevation = 0;
			foreach (var i in elevation_vectors) {
				if (
					squared_distance(point, i[0]) < 2.0 &&
					squared_distance(point, i[1]) < 2.0 &&
					squared_distance(point, i[2]) < 2.0) {
					elevation++;
				}
			}
			return elevation;
		}
	}

	public static partial class PlanetExtensions
	{
		public static void generate_terrain(this Planet p, Terrain_parameters par) => Terrain_generation.generate_terrain(p, par);
	}
}

namespace Earthgen
{
	using Earthgen.planet.terrain;
	public static partial class Statics
	{
		public static void generate_terrain(planet.Planet p, Terrain_parameters par) => planet.terrain.PlanetExtensions.generate_terrain(p, par);
	}
}

