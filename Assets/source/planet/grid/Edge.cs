using System;

namespace Earthgen.planet.grid
{
	public class Edge
	{
		public readonly int id;
		public readonly Tile[] tiles;
		public readonly Corner[] corners;

		public Edge(int id)
		{
			this.id = id;
			tiles = new Tile[2];
			corners = new Corner[2];
		}

		public Edge Clone(int? newId = null)
		{
			var newEdge = new Edge(newId ?? id);
			Array.Copy(tiles, newEdge.tiles, tiles.Length);
			Array.Copy(corners, newEdge.corners, corners.Length);
			return newEdge;
		}

		public int position (Tile t) {
			var e = this;
			if (e.tiles[0] == t)
				return 0;
			else if (e.tiles[1] == t)
				return 1;
			return -1;
		}
		public int position (Corner c) {
			var e = this;
			if (e.corners[0] == c)
				return 0;
			else if (e.corners[1] == c)
				return 1;
			return -1;
		}

		public int sign (Tile t) {
			var e = this;
			if (e.tiles[0] == t)
				return 1;
			else if (e.tiles[1] == t)
				return -1;
			return 0;
		}
		public int sign (Corner c) {
			var e = this;
			if (e.corners[0] == c)
				return 1;
			else if (e.corners[1] == c)
				return -1;
			return 0;
		}

	}
}

namespace Earthgen
{
	using Earthgen.planet.grid;
	public static partial class Statics
	{
		public static Edge Edge(int id) => new(id);
		public static int position(Edge e, Tile t) => e.position(t);
		public static int position(Edge e, Corner c) => e.position(c);
		public static int sign(Edge e, Tile t) => e.sign(t);
		public static int sign(Edge e, Corner c) => e.sign(c);

		public static int id (Edge e) => e.id;
		public static Tile[] tiles (Edge e) => e.tiles;
		public static Corner[] corners (Edge e) => e.corners;

		public static Tile nth_tile (Edge e, int i) {
			return e.tiles[i];
		}
		public static Corner nth_corner (Edge e, int i) {
			return e.corners[i];
		}
	}
}

