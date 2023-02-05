using System.Collections.Generic;
using static Earthgen.Statics;

namespace Earthgen.planet.grid
{
	public class Grid
	{
		public readonly int size;
		public readonly Tile[] tiles;
		public readonly Corner[] corners;
		public readonly Edge[] edges;

		public Grid(int s)
		{
			size = s;
			var tiles = new List<Tile>(tile_count(size));
			var corners = new List<Corner>(corner_count(size));
			var edges = new List<Edge>(edge_count(size));

			for (int i=0; i<tile_count(size); i++)
				tiles.push_back(Tile(i, i<12 ? 5 : 6));
			for (int i=0; i<corner_count(size); i++)
				corners.push_back(Corner(i));
			for (int i=0; i<edge_count(size); i++)
				edges.push_back(Edge(i));

			this.tiles = tiles.ToArray();
			this.corners = corners.ToArray();
			this.edges = edges.ToArray();
		}

		public static int tile_count (int size) {return 10*pow(3,size)+2;}
		public static int corner_count (int size) {return 20*pow(3,size);}
		public static int edge_count (int size) {return 30*pow(3,size);}
	}

	public static class PlanetExtensions
	{
		public static void set_grid_size (this Planet p, int size) {
			p.grid = size_n_grid(size);
		}

		public static Tile[] tiles (this Planet p) => p.grid.tiles;
		public static Corner[] corners (this Planet p) => p.grid.corners;
		public static Edge[] edges (this Planet p) => p.grid.edges;

		public static Tile nth_tile (this Planet p, int n) => p.grid.tiles[n];
		public static Corner nth_corner (this Planet p, int n) => p.grid.corners[n];
		public static Edge nth_edge (this Planet p, int n) => p.grid.edges[n];

		public static int tile_count (this Planet p) => p.grid.tiles.Length;
		public static int corner_count (this Planet p) => p.grid.corners.Length;
		public static int edge_count (this Planet p) => p.grid.edges.Length;
	}
}

namespace Earthgen
{
	public static partial class Statics
	{
		public static void set_grid_size(planet.Planet p, int size) => planet.grid.PlanetExtensions.set_grid_size(p, size);
	}
}