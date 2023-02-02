using System;
using UnityEngine;

namespace Earthgen.planet.grid
{
	public partial class Grid
    {
		public static Grid New(int s)
		{
			Grid grid = new();
			grid.Init(s);
			return grid;
		}
		public void Init(int s)
		{
			size = s;
			tiles = tiles.Resize(tile_count(size), null);
			corners = corners.Resize(corner_count(size), null);
			edges = edges.Resize(edge_count(size), null);
            for (int i=0; i<tile_count(size); i++)
				tiles[i] = Tile.New(i, i<12 ? 5 : 6);
            for (int i=0; i<corner_count(size); i++)
				corners[i] = Corner.New(i);
            for (int i=0; i<edge_count(size); i++)
				edges[i] = Edge.New(i);
        }

        public int size;

		public Tile[] tiles = new Tile[0];
		public Corner[] corners = new Corner[0];
		public Edge[] edges = new Edge[0];
		public Mesh mesh;

		public static int tile_count(int size) => 10 * (int)Math.Pow(3, size) + 2;
		public static int corner_count(int size) => 20 * (int)Math.Pow(3, size);
		public static int edge_count(int size) => 30 * (int)Math.Pow(3, size);
	}
	public static class PlanetExtensions
	{
		public static void set_grid_size(this Planet p, int size)
		{
			p.grid = Grid.size_n_grid(size);
		}
		public static Tile[] tiles(this Planet p) => p.grid.tiles;
		public static Corner[] corners(this Planet p) => p.grid.corners;
		public static Edge[] edges(this Planet p) => p.grid.edges;

		public static Tile nth_tile(this Planet p, int n) => p.grid.tiles[n];
		public static Corner nth_corner(this Planet p, int n) => p.grid.corners[n];
		public static Edge nth_edge(this Planet p, int n) => p.grid.edges[n];

		public static int tile_count(this Planet p) => p.grid.tiles.Length;
		public static int corner_count(this Planet p) => p.grid.corners.Length;
		public static int edge_count(this Planet p) => p.grid.edges.Length;
    }
}
