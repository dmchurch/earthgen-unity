using Earthgen.planet.grid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Earthgen.planet.terrain
{
    public partial class Terrain : ScriptableObject
    {
        public Terrain_variables var;
        public Terrain_tile[] tiles;
        public Terrain_corner[] corners;
        public Terrain_edge[] edges;

        public float MaxElevation => (from t in tiles select t.elevation).Concat(from c in corners select c.elevation).Max();
        public float MinElevation => (from t in tiles select t.elevation).Concat(from c in corners select c.elevation).Min();

        public static double latitude(Vector3 v) => Mathf.Asin(v.y);
        public static double longitude(Vector3 v) => v.x == 0 && v.z == 0 ? 0 : Mathf.Atan2(v.z, v.x);

        public static Vector3 default_axis() => Vector3.up;

        public Terrain_tile nth_tile(int n) => tiles[n];
        public Terrain_corner nth_corner(int n) => corners[n];
        public Terrain_edge nth_edge(int n) => edges[n];

        public ref Terrain_tile m_tile(int n) => ref tiles[n];
        public ref Terrain_corner m_corner(int n) => ref corners[n];
        public ref Terrain_edge m_edge(int n) => ref edges[n];


        [Flags]
        public enum Type
        {
            land = 1,
            water = 2,
            coast = 4,
        }
    }

    public static class GridExtensions
    {
        public static Terrain_tile terrain(this Tile t, Planet p)
        {
            if (p.terrain.tiles?.Length > t.id) {
                return p.terrain.nth_tile(t.id);
            }
            else {
                return Terrain_tile.Default;
            }
        }

        public static Terrain_corner terrain(this Corner c, Planet p) => p.terrain.corners.Length > c.id ? p.terrain.nth_corner(c.id) : Terrain_corner.Default;
        public static Terrain_edge terrain(this Edge e, Planet p) => p.terrain.edges.Length > e.id ? p.terrain.nth_edge(e.id) : Terrain_edge.Default;

        public static ref Terrain_tile m_terrain(this Tile t, Planet p) => ref p.terrain.m_tile(t.id);
        public static ref Terrain_corner m_terrain(this Corner c, Planet p) => ref p.terrain.m_corner(c.id);
        public static ref Terrain_edge m_terrain(this Edge e, Planet p) => ref p.terrain.m_edge(e.id);
    }

    public static partial class PlanetExtensions
    {
        public static void clear_terrain(this Planet p)
        {
            p.terrain.tiles = new Terrain_tile[0];
            p.terrain.corners = new Terrain_corner[0];
            p.terrain.edges = new Terrain_edge[0];
        }
        public static void init_terrain(this Planet p)
        {
            p.clear_terrain();
            p.terrain.tiles = p.terrain.tiles.Resize(p.tile_count(), Terrain_tile.Default);
            p.terrain.corners = p.terrain.corners.Resize(p.corner_count(), Terrain_corner.Default);
            p.terrain.edges = p.terrain.edges.Resize(p.edge_count(), Terrain_edge.Default);
        }

        public static double latitude (this Planet p, Vector3 v) => Mathf.PI / 2 - Vector3.Angle(p.axis(), v);
        public static double longitude (this Planet p, Vector3 v) => Terrain.longitude(p.rotation_to_default() * v);

        // angle from corner 0 to north
        public static double north(this Planet p, Tile t)
        {
            Vector3 v = t.reference_rotation(p.rotation_to_default()) * t.nth_tile(0).v;
            return Mathf.PI - Mathf.Atan2(v.y, v.x);
        }

        public static double area (this Planet p, Tile t)
        {
            double a = 0.0;
	        for (int k=0; k<t.edge_count; k++) {
		        float angle = Mathf.Acos(Vector3.Dot((t.v - t.nth_corner(k).v).normalized, (t.v - t.nth_corner(k+1).v).normalized));
		        a += 0.5 * Mathf.Sin(angle) * Vector3.Distance(t.v, t.nth_corner(k).v) * Vector3.Distance(t.v, t.nth_corner(k+1).v);
		        /*
		         *	double base = length(corner(t,k)->v - corner(t,k+1)->v);
		         *	double height = length(((corner(t,k)->v + corner(t,k+1)->v) * 0.5) - t->v);
		         *	a += 0.5 * base * height;
		         */
	        }
	        return a * Math.Pow(p.radius(), 2.0);
        }
        public static double length (this Planet p, Edge e) => Vector3.Distance(e.nth_corner(0).v, e.nth_corner(1).v) * p.radius();

        private static double angular_velocity(this Planet p)
        {
	        /* currently locked at 24 hours */
	        return 2.0 * Math.PI / (24 * 60 * 60);
        }

        public static double coriolis_coefficient (this Planet p, double latitude) => 2.0 * p.angular_velocity() * Math.Sin(latitude);

        public static Quaternion rotation (this Planet p) => Quaternion.FromToRotation(Terrain.default_axis(), p.axis());
        // rotation to bring planet axis into default position
        public static Quaternion rotation_to_default (this Planet p) => Quaternion.FromToRotation(p.axis(), Terrain.default_axis());
    }
}
