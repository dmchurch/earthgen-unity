using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Earthgen.Statics;

namespace Earthgen.planet.grid
{
	public static class Create_grid
	{
		public static Grid size_n_grid (int size) {
			if (size == 0) {
				return size_0_grid();
			}
			else {
				return _subdivided_grid(size_n_grid(size-1));
			}
		}
		static Grid size_0_grid () {
			Grid grid = new Grid(0);
			float x = -0.525731112119133606f;
			float z = -0.850650808352039932f;
	
			Vector3[] icos_tiles = {
				Vector3(-x, 0, z), Vector3(x, 0, z), Vector3(-x, 0, -z), Vector3(x, 0, -z),
				Vector3(0, z, x), Vector3(0, z, -x), Vector3(0, -z, x), Vector3(0, -z, -x),
				Vector3(z, x, 0), Vector3(-z, x, 0), Vector3(z, -x, 0), Vector3(-z, -x, 0)
			};
	
			int[,] icos_tiles_n = {
				{9, 4, 1, 6, 11}, {4, 8, 10, 6, 0}, {11, 7, 3, 5, 9}, {2, 7, 10, 8, 5},
				{9, 5, 8, 1, 0}, {2, 3, 8, 4, 9}, {0, 1, 10, 7, 11}, {11, 6, 10, 3, 2},
				{5, 3, 10, 1, 4}, {2, 5, 4, 0, 11}, {3, 7, 6, 1, 8}, {7, 2, 9, 0, 6}
			};
	
			foreach (Tile t in grid.tiles) {
				t.v = icos_tiles[t.id];
				for (int k=0; k<5; k++) {
					t.tiles[k] = grid.tiles[icos_tiles_n[t.id,k]];
				}
			}
			for (int i=0; i<5; i++) {
				_add_corner(i, grid, 0, icos_tiles_n[0,(i+4)%5], icos_tiles_n[0,i]);
			}
			for (int i=0; i<5; i++) {
				_add_corner(i+5, grid, 3, icos_tiles_n[3,(i+4)%5], icos_tiles_n[3,i]);
			}
			_add_corner(10,grid,10,1,8);
			_add_corner(11,grid,1,10,6);
			_add_corner(12,grid,6,10,7);
			_add_corner(13,grid,6,7,11);
			_add_corner(14,grid,11,7,2);
			_add_corner(15,grid,11,2,9);
			_add_corner(16,grid,9,2,5);
			_add_corner(17,grid,9,5,4);
			_add_corner(18,grid,4,5,8);
			_add_corner(19,grid,4,8,1);
	
			//_add corners to corners
			foreach (Corner c in grid.corners) {
				for (int k=0; k<3; k++) {
					c.corners[k] = c.tiles[k].corners[(position(c.tiles[k], c)+1)%5];
				}
			}
			//new edges
			int next_edge_id = 0;
			foreach (Tile t in grid.tiles) {
				for (int k=0; k<5; k++) {
					if (t.edges[k] == null) {
						_add_edge(next_edge_id, grid, t.id, icos_tiles_n[t.id,k]);
						next_edge_id++;
					}
				}
			}

			_reorder_tiles(grid);
	
			return grid;
		}

		static Grid _subdivided_grid (Grid prev) {
			Grid grid = new Grid(prev.size + 1, prev.Pentiles.Take(12).ToHashSet());

			int prev_tile_count = prev.tiles.size();
			int prev_corner_count = prev.corners.size();
	
			//old tiles
			for (int i=0; i<prev_tile_count; i++) {
				grid.tiles[i].v = prev.tiles[i].v;
				for (int k=0; k<grid.tiles[i].edge_count; k++) {
					grid.tiles[i].tiles[k] = grid.tiles[prev.tiles[i].corners[k].id+prev_tile_count];
				}
			}
			//old corners become tiles
			for (int i=0; i<prev_corner_count; i++) {
				grid.tiles[i+prev_tile_count].v = prev.corners[i].v;
				for (int k=0; k<3; k++) {
					grid.tiles[i+prev_tile_count].tiles[2*k] = grid.tiles[prev.corners[i].corners[k].id+prev_tile_count];
					grid.tiles[i+prev_tile_count].tiles[2*k+1] = grid.tiles[prev.corners[i].tiles[k].id];
				}
			}
			//new corners
			int next_corner_id = 0;
			foreach (Tile n in prev.tiles) {
				Tile t = grid.tiles[n.id];
				for (int k=0; k<t.edge_count; k++) {
					_add_corner(next_corner_id, grid, t.id, t.tiles[(k+t.edge_count-1)%t.edge_count].id, t.tiles[k].id);
					next_corner_id++;
				}
			}
			//connect corners
			foreach (Corner c in grid.corners) {
				for (int k=0; k<3; k++) {
					c.corners[k] = c.tiles[k].corners[(position(c.tiles[k], c)+1)%(c.tiles[k].edge_count)];
				}
			}
			//new edges
			int next_edge_id = 0;
			foreach (Tile t in grid.tiles) {
				for (int k=0; k<t.edge_count; k++) {
					if (t.edges[k] == null) {
						_add_edge(next_edge_id, grid, t.id, t.tiles[k].id);
						next_edge_id++;
					}
				}
			}

			_reorder_tiles(grid);
	
			return grid;
		}

		static void _reorder_tiles(Grid grid)
		{
			// Order the grid tiles in such a way as to make it easy to break the tiles down into smaller
			// and smaller contiguous regions, for rendering purposes. To do so, take an arbitrary tile and
			// add neighbors in a breadth-first search until the set has at least half the tiles available.
			// Then repeat, choosing to subdivide the largest segment starting from an edge (defined as: one
			// of the tiles with the fewest neighbors among the active set). Repeat until all segments have been
			// subdivided to size 1.

			Queue<ArraySegment<Tile>> segments = new();
			segments.Enqueue(new(grid.tiles));

			while (segments.TryDequeue(out var segment)) {
				if (segment.Count <= 2) {
					// 1 or 2 tiles are already properly split, just ignore and discard this segment
					continue;
				}
				HashSet<Tile> tiles = segment.ToHashSet();
				Tile startingTile = bestStartingTile(tiles);
				Queue<Tile> tilesToAdd = new();
				tilesToAdd.Enqueue(startingTile);

				int idx = 0;
				int limit = (segment.Count + 1) / 2; // put this many tiles in the first half-segment

				while (idx < limit) {
					if (tilesToAdd.Count == 0) {
						//Debug.LogWarning("No tiles in add queue, but still tiles in segment??");
						// This happens sometimes, but it doesn't really hurt anything.
						// Add another tile to continue the division from those remaining
						tilesToAdd.Enqueue(bestStartingTile(tiles));
					}
					var tile = tilesToAdd.Dequeue();
					if (!tiles.Contains(tile)) continue;
					tiles.Remove(tile);
					segment[idx++] = tile;
					foreach (var neighbor in tile.tiles) {
						tilesToAdd.Enqueue(neighbor);
					}
				}
				foreach (var tile in tiles) {
					segment[idx++] = tile;
				}
				segments.Enqueue(segment.Slice(0, limit));
				segments.Enqueue(segment.Slice(limit));
			}
			// Once all the segments are gone, grid.tiles is now in proper order (but the ids are not).

			// Now we have to fix all the tiles, which is tricky because of all the crosslinks; we have
			// to make each Tile, Corner, and Edge anew and then fix all the links
			Dictionary<Tile, Tile> tileMapping = new();
			Dictionary<Corner, Corner> cornerMapping = new();
			Dictionary<Edge, Edge> edgeMapping = new();
			int id = 0;
			foreach(var tile in grid.tiles) {
				tileMapping[tile] = grid.tiles[id] = tile.Clone(id);
				id++;
			}
			foreach (var corner in grid.corners) {
				cornerMapping[corner] = grid.corners[corner.id] = corner.Clone();
			}
			foreach (var edge in grid.edges) {
				edgeMapping[edge] = grid.edges[edge.id] = edge.Clone();
			}

			foreach (var tile in grid.tiles) {
				for (int i = 0; i < tile.edge_count; i++) {
					tile.tiles[i] = tileMapping[tile.tiles[i]];
					tile.corners[i] = cornerMapping[tile.corners[i]];
					tile.edges[i] = edgeMapping[tile.edges[i]];
				}
			}
			foreach (var corner in grid.corners) {
				for (int i = 0; i < 3; i++) {
					corner.tiles[i] = tileMapping[corner.tiles[i]];
					corner.corners[i] = cornerMapping[corner.corners[i]];
					corner.edges[i] = edgeMapping[corner.edges[i]];
				}
			}
			foreach (var edge in grid.edges) {
				for (int i = 0; i < 2; i++) {
					edge.tiles[i] = tileMapping[edge.tiles[i]];
					edge.corners[i] = cornerMapping[edge.corners[i]];
				}
			}

			static Tile bestStartingTile(HashSet<Tile> tiles)
			{
				int minActiveNeighbors = 7;
				Tile startingTile = null;
				foreach (var t in tiles) {
					int activeNeighbors = (from neighbor in t.tiles where tiles.Contains(neighbor) select 1).Count();
					if (activeNeighbors < minActiveNeighbors) {
						minActiveNeighbors = activeNeighbors;
						startingTile = t;
					}
				}

				return startingTile;
			}
		}

		static void _add_corner (int id, Grid grid, int t1, int t2, int t3) {
			Corner c = grid.corners[id];
			Tile[] t = {grid.tiles[t1], grid.tiles[t2], grid.tiles[t3]};
			Vector3 v = t[0].v + t[1].v + t[2].v;
			c.v = normal(v);
			for (int i=0; i<3; i++) {
				t[i].corners[position(t[i], t[(i+2)%3])] = c;
				c.tiles[i] = t[i];
			}
		}
		static void _add_edge (int id, Grid grid, int t1, int t2) {
			Edge e = grid.edges[id];
			Tile[] t = {grid.tiles[t1], grid.tiles[t2]};
			Corner[] c = {
				grid.corners[t[0].corners[position(t[0], t[1])].id],
				grid.corners[t[0].corners[(position(t[0], t[1])+1)%t[0].edge_count].id]};
			for (int i=0; i<2; i++) {
				t[i].edges[position(t[i], t[(i+1)%2])] = e;
				e.tiles[i] = t[i];
				c[i].edges[position(c[i], c[(i+1)%2])] = e;
				e.corners[i] = c[i];
			}
		}
	}

}

namespace Earthgen
{
	public static partial class Statics
	{
		public static planet.grid.Grid size_n_grid(int size) => planet.grid.Create_grid.size_n_grid(size);
	}
}

