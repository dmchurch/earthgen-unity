using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Earthgen.planet.grid
{
	public partial class Grid : ScriptableObject
    {
		public static Grid New(int s)
		{
			Grid grid = CreateInstance<Grid>();
			grid.Init(s);
			return grid;
		}
		public void Init(int s)
		{
			size = s;
            for (int i=0; i<tile_count(size); i++)
                tiles.Add(Tile.New(i, i<12 ? 5 : 6));
            for (int i=0; i<corner_count(size); i++)
                corners.Add(Corner.New(i));
            for (int i=0; i<edge_count(size); i++)
                edges.Add(Edge.New(i));
        }

        public int size;

		public List<Tile> tiles = new();
		public List<Corner> corners = new();
		public List<Edge> edges = new();

		public static int tile_count(int size) => 10 * (int)Math.Pow(3, size) + 2;
		public static int corner_count(int size) => 20 * (int)Math.Pow(3, size);
		public static int edge_count(int size) => 30 * (int)Math.Pow(3, size);
	};

	public static class PlanetExtensions
	{
		public static void set_grid_size(this Planet p, int size)
		{
			p.grid = Grid.size_n_grid(size);
		}

		public static List<Tile> tiles(this Planet p) => p.grid.tiles;
		public static List<Corner> corners(this Planet p) => p.grid.corners;
		public static List<Edge> edges(this Planet p) => p.grid.edges;

		public static Tile nth_tile(this Planet p, int n) => p.grid.tiles[n];
		public static Corner nth_corner(this Planet p, int n) => p.grid.corners[n];
		public static Edge nth_edge(this Planet p, int n) => p.grid.edges[n];

		public static int tile_count(this Planet p) => p.grid.tiles.Count;
		public static int corner_count(this Planet p) => p.grid.corners.Count;
		public static int edge_count(this Planet p) => p.grid.edges.Count;
    }
}
