using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

using Earthgen.planet.grid;

namespace Earthgen.planet.terrain
{
	public static class Terrain_generation
    {
        public static void generate_terrain(this Planet p, Terrain_parameters par)
        {
	        p.clear();
	        p.set_grid_size(par.grid_size);
	        p.init_terrain();
	        p._set_variables(par);
			//Debug.Log($"Generating elevation vectors...");
	        p._set_elevation(par);
			//Debug.Log($"Creating sea...");
	        p._create_sea(par);
			//Debug.Log($"Classifying terrain...");
	        p._classify_terrain();
			//Debug.Log($"Setting river directions...");
	        p._set_river_directions();
			//Debug.Log($"Done!");
        }

		static void _set_variables(this Planet p, Terrain_parameters par) {
			p.terrain.var.axis = par.axis;
			p.terrain.var.radius = 40000000;
			p.terrain.var.grid_size = par.grid_size;
		}

		static void _set_elevation(this Planet p, Terrain_parameters par) {
			// can be made concurrent
			var d = _elevation_vectors(par);
			foreach (var t in p.tiles())
				p.terrain.m_tile(t.id).elevation = _elevation_at_point(t.v, d);
			foreach (var c in p.corners())
				p.terrain.m_corner(c.id).elevation = _elevation_at_point(c.v, d);
			p._scale_elevation(par);
		}

		static void _scale_elevation(this Planet p, Terrain_parameters par) {
			float lowest = p.terrain.nth_tile(0).elevation;
			float highest = lowest;
			float scale = 3000;
			foreach (var t in p.terrain.tiles) {
				lowest = Math.Min(lowest, t.elevation);
				highest = Math.Max(highest, t.elevation);
			}
			foreach (var c in p.terrain.corners) {
				lowest = Math.Min(lowest, c.elevation);
				highest = Math.Max(highest, c.elevation);
			}
			highest = Math.Max(1.0f, highest-lowest);
			foreach (var t in p.tiles()) {
				p.terrain.m_tile(t.id).elevation -= lowest;
				p.terrain.m_tile(t.id).elevation *= scale / highest;
			}
			foreach (var c in p.corners()) {
				p.terrain.m_corner(c.id).elevation -= lowest;
				p.terrain.m_corner(c.id).elevation *= scale / highest;
			}
		}

		static Tile lowest_tile (this Planet p) {
			Tile tile = p.nth_tile(0);
			float lowest_elevation = p.terrain.nth_tile(0).elevation;
			foreach (Tile t in p.tiles()) {
				if (p.terrain.nth_tile(t.id).elevation < lowest_elevation) {
					tile = t;
					lowest_elevation = p.terrain.nth_tile(t.id).elevation;
				}
			}
			return tile;
		}

		static void _create_sea(this Planet p, Terrain_parameters par) {
			Tile start_tile = p.lowest_tile();
			float sea_level = p.terrain.nth_tile(start_tile.id).elevation;
			uint water_tile_count = (uint)(par.water_ratio * p.tile_count());
			HashSet<Tile> water_tiles = new();
			SortedSet<(float elevation, Tile tile)> coast_tiles_elevation = new();
			List<bool> coast_tiles = new();
			if (water_tile_count > 0) {
				water_tiles.Add(start_tile);
				coast_tiles.Resize(p.tile_count(), false);
				foreach (Tile i in start_tile.tiles) {
					coast_tiles[i.id] = true;
					var elevation = p.terrain.nth_tile(i.id).elevation;
					coast_tiles_elevation.Add((elevation, i));
				}
				Tile tile;
				void insert_next_tile() {
					tile = coast_tiles_elevation.Min.tile;
					water_tiles.Add(tile);
					coast_tiles[tile.id] = false;
					coast_tiles_elevation.Remove(coast_tiles_elevation.Min);
					foreach (var i in tile.tiles) {
						if (!water_tiles.Contains(i) && !coast_tiles[i.id]) {
							coast_tiles[i.id] = true;
							coast_tiles_elevation.Add((p.terrain.nth_tile(i.id).elevation, i));
						}
					}
				}
				while (water_tiles.Count < water_tile_count) {
					insert_next_tile();
					sea_level = p.terrain.nth_tile(tile.id).elevation;
					while (coast_tiles_elevation.Count > 0 && coast_tiles_elevation.Min.elevation <= sea_level) {
						insert_next_tile();
					}
				}
				if (coast_tiles_elevation.Count > 0)
					sea_level = (sea_level + coast_tiles_elevation.Min.elevation) / 2;
			}
			p.terrain.var.sea_level = sea_level;
			foreach (var t in water_tiles) {
				p.terrain.m_tile(t.id).water.surface = sea_level;
				p.terrain.m_tile(t.id).water.depth = sea_level - p.terrain.nth_tile(t.id).elevation;
			}
		}

		private static Terrain.Type _tile_type (this Planet p, Tile t) {
			bool land = false;
			bool water = false;
			foreach (var i in t.tiles) {
				if (p.terrain.nth_tile(i.id).water_depth() > 0) water = true;
				else land = true;
			}
			Terrain.Type type =
				p.terrain.nth_tile(t.id).water_depth() > 0 ?
					Terrain.Type.water :
					Terrain.Type.land;
			if (land && water)
				type |= Terrain.Type.coast;
			return type;
		}

		private static Terrain.Type _corner_type (this Planet p, Corner c) {
			bool land = false;
			bool water = false;
			foreach (var i in c.tiles) {
				if (p.terrain.nth_tile(i.id).water_depth() > 0) water = true;
				else land = true;
			}
			Terrain.Type type =
				land && water ?
					Terrain.Type.coast :
					land ?
					Terrain.Type.land :
					Terrain.Type.water;
			return type;
		}

		private static Terrain.Type _edge_type (this Planet p, Edge e) {
			bool land = false;
			bool water = false;
			foreach (var i in e.tiles) {
				if (p.terrain.nth_tile(i.id).water_depth() > 0) water = true;
					else land = true;
			}
			Terrain.Type type =
				land && water ?
					Terrain.Type.coast :
					land ?
					Terrain.Type.land :
					Terrain.Type.water;
			return type;
		}

		private static void _classify_terrain(this Planet p) {
			foreach (var t in p.tiles())
				p.terrain.m_tile(t.id).type = p._tile_type(t);
			foreach (var c in p.corners())
				p.terrain.m_corner(c.id).type = p._corner_type(c);
			foreach (var e in p.edges())
				p.terrain.m_edge(e.id).type = p._edge_type(e);
		}

		private static void _set_river_directions(this Planet p) {
			SortedSet<(float elevation, Corner corner)> endpoints = new();
			foreach (var c in p.corners())
				if (p.terrain.nth_corner(c.id).is_coast()) {
					p.terrain.m_corner(c.id).distance_to_sea = 0;
					endpoints.Add((p.terrain.nth_corner(c.id).elevation, c));
			}
			//Debug.Log($"Added {endpoints.Count} corners");
			while (endpoints.Count > 0) {
				var first = endpoints.Min;
				Corner c = first.corner;
				foreach (var n in c.corners) {
					ref Terrain_corner ter = ref p.terrain.m_corner(n.id);
					if (ter.is_land() && ter.river_direction == -1) {
						ter.river_direction = n.position(c);
						ter.distance_to_sea = 1 + p.terrain.nth_corner(c.id).distance_to_sea;
						endpoints.Add((ter.elevation, n));
					}
				}
				endpoints.Remove(first);
			}
		}

		private static int byte_array_to_uint(byte[] arr) {
			int n = 0;
			foreach (var b in arr) {
				n <<= 8;
				n += b;
			}
			return n;
		}

		private static List<(Vector3 a, Vector3 b, Vector3 c)> _elevation_vectors (Terrain_parameters par) {
			byte[] seed = Encoding.ASCII.GetBytes(par.seed);
			var hash = new MD5CryptoServiceProvider().ComputeHash(seed);			
			Random.InitState(byte_array_to_uint(hash));
			List<(Vector3, Vector3, Vector3)> d = new();
			for (int i=0; i<par.iterations; i++) {
				var v = (
					point_uniform(Random.value, Random.value),
					point_uniform(Random.value, Random.value),
					point_uniform(Random.value, Random.value));
				d.Add(v);
			}
			return d;
		}

		private static float _elevation_at_point (Vector3 point, List<(Vector3 a, Vector3 b, Vector3 c)> elevation_vectors) {
			float elevation = 0;
			foreach (var i in elevation_vectors) {
				if (
					Vector3.SqrMagnitude(point - i.a) < 2.0 &&
					Vector3.SqrMagnitude(point - i.b) < 2.0 &&
					Vector3.SqrMagnitude(point - i.c) < 2.0) {
					elevation++;
				}
			}
			return elevation;
		}

		private static Vector3 point_uniform (float a, float b) {
			float x = 2*Mathf.PI*a;
			float y = Mathf.Acos(2*b-1)-(0.5f*Mathf.PI);
			return new Vector3(Mathf.Sin(x)*Mathf.Cos(y), Mathf.Sin(y), Mathf.Cos(x)*Mathf.Cos(y));
		}

    }
}