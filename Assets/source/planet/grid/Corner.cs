using System;
using UnityEngine;

namespace Earthgen.planet.grid
{
	public class Corner : IComparable<Corner>
	{
		public readonly int id;
		public Vector3 v;
		public readonly Tile[] tiles;
		public readonly Corner[] corners;
		public readonly Edge[] edges;

		public Corner(int id)
		{
			this.id = id;
			tiles = new Tile[3];
			corners = new Corner[3];
			edges = new Edge[3];
		}

		public int position (Tile t) {
			var c = this;
			for (int i=0; i<3; i++)
				if (c.tiles[i] == t)
					return i;
			return -1;
		}
		public int position (Corner n) {
			var c = this;
			for (int i=0; i<3; i++)
				if (c.corners[i] == n)
					return i;
			return -1;
		}
		public int position (Edge e) {
			var c = this;
			for (int i=0; i<3; i++)
				if (c.edges[i] == e)
					return i;
			return -1;
		}

		public Corner nth_corner(int i)
		{
			var c = this;
			int k = i < 0 ?
				i%3 + 3 :
				i%3;
			return c.corners[k];
		}

		public Edge nth_edge(int i)
		{
			var c = this;
			int k = i < 0 ?
				i%3 + 3 :
				i%3;
			return c.edges[k];
		}

		public int CompareTo(Corner other) => id.CompareTo(other.id);
	}
}

namespace Earthgen
{
	using Earthgen.planet.grid;
	public static partial class Statics
	{
		public static Corner Corner(int id) => new(id);
		public static int id (Corner c) => c.id;
		public static Vector3 vector (Corner c) => c.v;
		public static Tile[] tiles (Corner c) => c.tiles;
		public static Corner[] corners (Corner c) => c.corners;
		public static Edge[] edges (Corner c) => c.edges;

		public static int position(Corner c, Tile t) => c.position(t);
		public static int position(Corner c, Corner n) => c.position(n);
		public static int position(Corner c, Edge e) => c.position(e);

		public static Corner nth_corner (Corner c, int i) => c.nth_corner(i);
		public static Edge nth_edge (Corner c, int i) => c.nth_edge(i);

	}
}

