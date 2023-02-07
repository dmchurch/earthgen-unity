using System;
using Earthgen.planet;
using Earthgen.planet.grid;
using UnityEngine;
using static Earthgen.Statics;

namespace Earthgen.planet.terrain
{
	[Serializable]
	public struct Terrain
	{
		public Terrain_variables var;
		public Terrain_tile[] tiles;
		public Terrain_corner[] corners;
		public Terrain_edge[] edges;

		public void clear_terrain()
		{
			tiles = null;
			corners = null;
			edges = null;
		}

		public void init_terrain(Planet p)
		{
			tiles = new Terrain_tile[tile_count(p)];
			corners = new Terrain_corner[corner_count(p)];
			edges = new Terrain_edge[edge_count(p)];
		}

		public static AngleFloat latitude (Vector3 v) {
			return asin(v.z);
		}
		public static AngleFloat longitude (Vector3 v) {
			if (v.x == 0 && v.y == 0)
				return default;
			return atan2(v.y, v.x);
		}

		public static Vector3 default_axis () => Vector3(0,0,1);
	}

	public static class GridExtensions
	{
		public static Terrain_tile terrain(this Tile t, Planet p) => p.terrain.tiles[t.id];
		public static Terrain_corner terrain(this Corner c, Planet p) => p.terrain.corners[c.id];
		public static Terrain_edge terrain(this Edge e, Planet p) => p.terrain.edges[e.id];
	}

	public static partial class PlanetExtensions
	{
		public static void clear_terrain(this Planet p) => p.terrain.clear_terrain();
		public static void init_terrain(this Planet p) => p.terrain.init_terrain(p);

		public static AngleFloat latitude (this Planet p, Vector3 v) {
			return pi/2 - angle(axis(p), v);
		}

		public static AngleFloat longitude (this Planet p, Vector3 v) {
			Vector3 u = rotation_to_default(p) * v;
			return Terrain.longitude(u);
		}

		public static AngleFloat north (this Planet p, Tile t) {
			Vector3 v = reference_rotation(t, rotation_to_default(p)) * vector(nth_tile(t, 0));
			return pi-atan2(v.y, v.x);
		}

		public static float unit_area (this Planet p, Tile t) {
			float a = 0.0f;
			for (int k=0; k<edge_count(t); k++) {
				AngleFloat angle = acos(dot_product(normal(vector(t) - vector(nth_corner(t,k))), normal(vector(t) - vector(nth_corner(t,k+1)))));
				a += 0.5f * sin(angle) * distance(vector(t), vector(nth_corner(t,k))) * distance(vector(t), vector(nth_corner(t,k+1)));
				/*
				 *	double base = length(corner(t,k)->v - corner(t,k+1)->v);
				 *	double height = length(((corner(t,k)->v + corner(t,k+1)->v) * 0.5) - t->v);
				 *	a += 0.5 * base * height;
				 */
			}
			return a;
		}
		public static double area (this Planet p, Tile t) {
			return unit_area(p, t) * pow(radius(p), 2.0);
		}

		public static double length (this Planet p, Edge e) {
			return distance(vector(nth_corner(e,0)), vector(nth_corner(e,1))) * radius(p);
		}

		public static AngleFloat angular_velocity (this Planet p) {
			/* currently locked at 24 hours */
			return 2.0 * pi / (24 * 60 * 60);
		}

		public static double coriolis_coefficient (this Planet p, AngleFloat latitude) {
			return 2.0 * angular_velocity(p).Radians * sin(latitude);
		}

		public static Quaternion rotation (this Planet p) {
			return Quaternion(default_axis(), axis(p));
		}
		public static Quaternion rotation_to_default (this Planet p) {
			return conjugate(rotation(p));
		}
	}
}

namespace Earthgen
{
	using planet.terrain;
	public static partial class Statics
	{
		public static void clear_terrain(Planet p) => PlanetExtensions.clear_terrain(p);
		public static void init_terrain(Planet p) => PlanetExtensions.init_terrain(p);

		public static AngleFloat latitude(Vector3 v) => Terrain.latitude(v);
		public static AngleFloat longitude(Vector3 v) => Terrain.longitude(v);

		public static AngleFloat latitude (Planet p, Vector3 v) => PlanetExtensions.latitude(p, v);
		public static AngleFloat longitude (Planet p, Vector3 v) => PlanetExtensions.longitude(p, v);
		public static AngleFloat north (Planet p, Tile t) => PlanetExtensions.north(p, t);
		public static float area (Planet p, Tile t) => (float)PlanetExtensions.area(p, t);
		public static float length (Planet p, Edge e) => (float)PlanetExtensions.length(p, e);
		public static AngleFloat angular_velocity (Planet p) => PlanetExtensions.angular_velocity(p);
		public static double coriolis_coefficient (Planet p, AngleFloat latitude) => PlanetExtensions.coriolis_coefficient(p, latitude);
		public static Quaternion rotation (Planet p) => PlanetExtensions.rotation(p);
		public static Quaternion rotation_to_default (Planet p) => PlanetExtensions.rotation_to_default(p);

		public static Vector3 default_axis() => Terrain.default_axis();

		public static Terrain terrain(Planet p) => p.terrain;
		public static ref Terrain m_terrain(Planet p) => ref p.terrain;

		public static Terrain_tile[] tiles (Terrain t) => t.tiles;
		public static Terrain_corner[] corners (Terrain t) => t.corners;
		public static Terrain_edge[] edges (Terrain t) => t.edges;

		public static Terrain_tile nth_tile (Terrain t, int n) => t.tiles[n];
		public static Terrain_corner nth_corner (Terrain t, int n) => t.corners[n];
		public static Terrain_edge nth_edge (Terrain t, int n) => t.edges[n];

		public static ref Terrain_tile m_tile (Terrain t, int n) => ref t.tiles[n];
		public static ref Terrain_corner m_corner (Terrain t, int n) => ref t.corners[n];
		public static ref Terrain_edge m_edge (Terrain t, int n) => ref t.edges[n];
	}
}


