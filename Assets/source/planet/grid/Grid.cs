using System.Collections.Generic;
using System.Linq;
using static Earthgen.Statics;

namespace Earthgen.planet.grid
{
	public class Grid
	{
		public readonly int size;
		public readonly Tile[] tiles;
		public readonly Corner[] corners;
		public readonly Edge[] edges;

		public Grid(int s, HashSet<int> pentileIds = null)
		{
			pentileIds ??= Enumerable.Range(0, 12).ToHashSet();
			size = s;
			var tiles = new List<Tile>(tile_count(size));
			var corners = new List<Corner>(corner_count(size));
			var edges = new List<Edge>(edge_count(size));

			for (int i=0; i<tile_count(size); i++)
				tiles.push_back(Tile(i, pentileIds.Contains(i) ? 5 : 6));
			for (int i=0; i<corner_count(size); i++)
				corners.push_back(Corner(i));
			for (int i=0; i<edge_count(size); i++)
				edges.push_back(Edge(i));

			this.tiles = tiles.ToArray();
			this.corners = corners.ToArray();
			this.edges = edges.ToArray();
		}

		public IEnumerable<int> Pentiles => from t in tiles where t.edge_count == 5 select t.id;
		public IEnumerable<int> Hextiles => from t in tiles where t.edge_count == 6 select t.id;

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
	using Earthgen.planet;
	using Earthgen.planet.grid;
	public static partial class Statics
	{
		public static void set_grid_size(planet.Planet p, int size) => PlanetExtensions.set_grid_size(p, size);

		public static Tile[] tiles (Planet p) => PlanetExtensions.tiles(p);
		public static Corner[] corners (Planet p) => PlanetExtensions.corners(p);
		public static Edge[] edges (Planet p) => PlanetExtensions.edges(p);

		public static Tile nth_tile (Planet p, int n) => PlanetExtensions.nth_tile(p, n);
		public static Corner nth_corner (Planet p, int n) => PlanetExtensions.nth_corner(p, n);
		public static Edge nth_edge (Planet p, int n) => PlanetExtensions.nth_edge(p, n);

		public static int tile_count (Planet p) => PlanetExtensions.tile_count(p);
		public static int corner_count (Planet p) => PlanetExtensions.corner_count(p);
		public static int edge_count (Planet p) => PlanetExtensions.edge_count(p);
	}
}