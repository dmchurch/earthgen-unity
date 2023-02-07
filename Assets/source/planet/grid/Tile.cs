using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Earthgen.Statics;

namespace Earthgen.planet.grid
{
	public class Tile : IComparable<Tile>
	{
		public readonly int id;
		public readonly int edge_count;
		public Vector3 v;
		public readonly Tile[] tiles;
		public readonly Corner[] corners;
		public readonly Edge[] edges;

		public Tile(int id, int edge_count)
		{
			this.id = id;
			this.edge_count = edge_count;
			tiles = new Tile[edge_count];
			corners = new Corner[edge_count];
			edges = new Edge[edge_count];
		}

		public Tile Clone(int? newId = null)
		{
			var newTile = new Tile(newId ?? id, edge_count);
			newTile.v = v;
			v = Vector3(-10,-10,-10); // poison this value, REMOVE THIS LINE LATER
			Array.Copy(tiles, newTile.tiles, edge_count);
			Array.Copy(corners, newTile.corners, edge_count);
			Array.Copy(edges, newTile.edges, edge_count);
			return newTile;
		}

		public int position (Tile n) {
			var t = this;
			for (int i=0; i<t.edge_count; i++)
				if (t.tiles[i] == n)
					return i;
			return -1;
		}

		public int position (Corner c) {
			var t = this;
			for (int i=0; i<t.edge_count; i++)
				if (t.corners[i] == c)
					return i;
			return -1;
		}

		public int position (Edge e) {
			var t = this;
			for (int i=0; i<t.edge_count; i++)
				if (t.edges[i] == e)
					return i;
			return -1;
		}

		public Tile nth_tile (int n) {
			var t = this;
			int k = n < 0 ?
				n % edge_count(t) + edge_count(t) :
				n % edge_count(t);
			return t.tiles[k];
		}

		public Corner nth_corner (int n) {
			var t = this;
			int k = n < 0 ?
				n % edge_count(t) + edge_count(t) :
				n % edge_count(t);
			return t.corners[k];
		}

		public Edge nth_edge (int n) {
			var t = this;
			int k = n < 0 ?
				n % edge_count(t) + edge_count(t) :
				n % edge_count(t);
			return t.edges[k];
		}

		public Quaternion reference_rotation (Quaternion d) {
			var t = this;
			Vector3 v = d * vector(t);
			Quaternion h = Quaternion();
			if (v.x != 0 || v.y != 0) {
				if (v.y != 0) h = Quaternion(normal(Vector3(v.x, v.y, 0)), Vector3(-1,0,0));
				else if (v.x > 0) h = Quaternion(Vector3(0,0,1), pi);
			}
			Quaternion q = Quaternion();
			if (v.x == 0 && v.y == 0) {
				if (v.z < 0) q = Quaternion(Vector3(1,0,0), pi);
			}
			else {
				q = Quaternion(h*v, Vector3(0,0,1));
			}
			return q*h*d;
		}

		public Polygon polygon (Quaternion d) {
			var t = this;
			Polygon p = new();
			Quaternion q = reference_rotation(t, d);
			for (int i=0; i<edge_count(t); i++) {
				Vector3 c = q * vector(nth_corner(t, i));
				p.push_back(Vector2(c.x, c.y));
			}
			return p;
		}

		public class Polygon : List<Vector2>
		{
			public static Polygon operator *(Quaternion lhs, Polygon rhs)
			{
				var p = new Polygon();
				p.AddRange(from v in rhs select (Vector2)(lhs * v));
				return p;
			}
		}

		private static Quaternion reference_rotation(Tile t, Quaternion d) => t.reference_rotation(d);
		private static Corner nth_corner(Tile t, int i) => t.nth_corner(i);
		public int CompareTo(Tile other) => id.CompareTo(other.id);
	}
}

namespace Earthgen
{
	using Earthgen.planet.grid;
	using System.Collections.Generic;

	public static partial class Statics
	{
		public static Tile Tile(int id, int edge_count) => new(id, edge_count);
		public static int position(Tile t, Tile n) => t.position(n);
		public static int position(Tile t, Corner c) => t.position(c);
		public static int position(Tile t, Edge e) => t.position(e);

		public static int id(Tile t) => t.id;
		public static int edge_count(Tile t) => t.edge_count;
		public static Vector3 vector(Tile t) => t.v;

		public static Tile[] tiles(Tile t) => t.tiles;
		public static Corner[] corners(Tile t) => t.corners;
		public static Edge[] edges(Tile t) => t.edges;

		public static Tile nth_tile (Tile t, int n) => t.nth_tile(n);
		public static Corner nth_corner (Tile t, int n) => t.nth_corner(n);
		public static Edge nth_edge (Tile t, int n) => t.nth_edge(n);

		public static Quaternion reference_rotation(Tile t, Quaternion d) => t.reference_rotation(d);
		public static Tile.Polygon polygon(Tile t, Quaternion d) => t.polygon(d);
	}

}
