using UnityEngine;

namespace Earthgen.planet.grid
{
    public partial class Grid
    {
        public static Grid size_n_grid(int size)
        {
            if (size == 0) {
                return size_0_grid();
            }
            else {
                return _subdivided_grid(size_n_grid(size-1));
            }
        }
        public static Grid size_0_grid()
        {
			Grid grid = Grid.New(0);
			float x = -0.525731112119133606f;
			float z = -0.850650808352039932f;
	
			Vector3[] icos_tiles = new[] {
				new Vector3(-x, 0, z), new Vector3(x, 0, z), new Vector3(-x, 0, -z), new Vector3(x, 0, -z),
                new Vector3(0, z, x), new Vector3(0, z, -x), new Vector3(0, -z, x), new Vector3(0, -z, -x),
                new Vector3(z, x, 0), new Vector3(-z, x, 0), new Vector3(z, -x, 0), new Vector3(-z, -x, 0)
			};
	
			int[,] icos_tiles_n = new int[12,5] {
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
				grid._add_corner(i, 0, icos_tiles_n[0,(i+4)%5], icos_tiles_n[0,i]);
			}
			for (int i=0; i<5; i++) {
				grid._add_corner(i+5, 3, icos_tiles_n[3,(i+4)%5], icos_tiles_n[3,i]);
			}
			grid._add_corner(10,10,1,8);
			grid._add_corner(11,1,10,6);
			grid._add_corner(12,6,10,7);
			grid._add_corner(13,6,7,11);
			grid._add_corner(14,11,7,2);
			grid._add_corner(15,11,2,9);
			grid._add_corner(16,9,2,5);
			grid._add_corner(17,9,5,4);
			grid._add_corner(18,4,5,8);
			grid._add_corner(19,4,8,1);
	
			//_add corners to corners
			foreach (Corner c in grid.corners) {
				for (int k=0; k<3; k++) {
					c.corners[k] = c.tiles[k].corners[(c.tiles[k].position(c)+1)%5];
				}
			}
			//new edges
			int next_edge_id = 0;
			foreach (Tile t in grid.tiles) {
				for (int k=0; k<5; k++) {
					if (t.edges[k] == null) {
						grid._add_edge(next_edge_id, t.id, icos_tiles_n[t.id,k]);
						next_edge_id++;
					}
				}
			}
			return grid;
        }
        static Grid _subdivided_grid(Grid prev)
        {
            Grid grid = Grid.New(prev.size + 1);

            int prev_tile_count = prev.tiles.Count;
            int prev_corner_count = prev.corners.Count;

            //old tiles
            for (int i = 0; i < prev_tile_count; i++) {
                grid.tiles[i].v = prev.tiles[i].v;
                for (int k = 0; k < grid.tiles[i].edge_count; k++) {
                    grid.tiles[i].tiles[k] = grid.tiles[prev.tiles[i].corners[k].id + prev_tile_count];
                }
            }
            //old corners become tiles
            for (int i = 0; i < prev_corner_count; i++) {
                grid.tiles[i + prev_tile_count].v = prev.corners[i].v;
                for (int k = 0; k < 3; k++) {
                    grid.tiles[i + prev_tile_count].tiles[2 * k] = grid.tiles[prev.corners[i].corners[k].id + prev_tile_count];
                    grid.tiles[i + prev_tile_count].tiles[2 * k + 1] = grid.tiles[prev.corners[i].tiles[k].id];
                }
            }
            //new corners
            int next_corner_id = 0;
            foreach (Tile n in prev.tiles) {
                Tile t = grid.tiles[n.id];
                for (int k = 0; k < t.edge_count; k++) {
                    grid._add_corner(next_corner_id, t.id, t.tiles[(k + t.edge_count - 1) % t.edge_count].id, t.tiles[k].id);
                    next_corner_id++;
                }
            }
            //connect corners
            foreach (Corner c in grid.corners) {
                for (int k = 0; k < 3; k++) {
                    c.corners[k] = c.tiles[k].corners[(c.tiles[k].position(c) + 1) % (c.tiles[k].edge_count)];
                }
            }
            //new edges
            int next_edge_id = 0;
            foreach (Tile t in grid.tiles) {
                for (int k = 0; k < t.edge_count; k++) {
                    if (t.edges[k] == null) {
                        grid._add_edge(next_edge_id, t.id, t.tiles[k].id);
                        next_edge_id++;
                    }
                }
            }

            return grid;
        }

        void _add_corner(int id, int t1, int t2, int t3)
        {
            Corner c = corners[id];
            Tile[] t = new Tile[3] {tiles[t1], tiles[t2], tiles[t3]};
            Vector3 v = t[0].v + t[1].v + t[2].v;
            c.v = v.normalized;
            for (int i = 0; i < 3; i++) {
                t[i].corners[t[i].position(t[(i + 2) % 3])] = c;
                c.tiles[i] = t[i];
            }
        }

        void _add_edge(int id, int t1, int t2)
        {
	        Edge e = edges[id];
	        Tile[] t = new Tile[2] {tiles[t1], tiles[t2]};
            Corner[] c = new Corner[2] {
		        corners[t[0].corners[t[0].position(t[1])].id],
		        corners[t[0].corners[(t[0].position(t[1])+1)%t[0].edge_count].id]};
	        for (int i=0; i<2; i++) {
		        t[i].edges[t[i].position(t[(i+1)%2])] = e;
		        e.tiles[i] = t[i];
		        c[i].edges[c[i].position(c[(i+1)%2])] = e;
		        e.corners[i] = c[i];
	        }
        }
    }
}
