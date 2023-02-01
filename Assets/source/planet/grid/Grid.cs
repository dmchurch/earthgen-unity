using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		public Mesh mesh;

		public static int tile_count(int size) => 10 * (int)Math.Pow(3, size) + 2;
		public static int corner_count(int size) => 20 * (int)Math.Pow(3, size);
		public static int edge_count(int size) => 30 * (int)Math.Pow(3, size);

        [UnityEngine.ContextMenu("Generate Mesh")]
        public void GenerateMesh(Mesh useMesh = null)
        {
			if (useMesh) mesh = useMesh;
			if (!mesh) mesh = new Mesh();
			mesh.Clear();
			mesh.subMeshCount = 1;
			var vertices = new List<Vector3>(); //[corners.Count + tiles.Count];
			var normals = new List<Vector3>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();
			int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(tiles.Count));
			int AddVertex(Vector3 pos, Vector3 normal, Vector2 uv, bool addToTriangle = true)
			{
				int idx = vertices.Count;
				if (addToTriangle)
					triangles.Add(idx);
				vertices.Add(pos);
				normals.Add(normal);
				uvs.Add(uv);
				return idx;
			}
			for (int i = 0; i < tiles.Count; i++) {
				int x = i % tilesPerSide;
				int y = i / tilesPerSide;
				Vector2 uvCenter = new((x + 0.5f) / tilesPerSide, (y + 0.5f) / tilesPerSide);
				Tile t = tiles[i];
				Vector3 v = Vector3.zero;
				float angle = 0;
				float aDelta = Mathf.PI * 2 / t.edge_count;
				int centerVertex = AddVertex(Vector3.zero, t.v, uvCenter, false);
				for (int j = 0; j < t.edge_count; j++) {
					v += t.corners[j].v;
					float nextAngle = angle + aDelta;
					AddVertex(t.nth_corner(j).v, t.v, uvCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) / tilesPerSide / 2); // this corner
					AddVertex(t.nth_corner(j + 1).v, t.v, uvCenter + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) / tilesPerSide / 2); // next corner
					triangles.Add(centerVertex); // center
					angle = nextAngle;
				}
				vertices[centerVertex] = v / t.edge_count;
			}
			mesh.SetVertices(vertices);
			mesh.SetNormals(normals);
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(triangles, 0);
        }

		[UnityEngine.ContextMenu("Subdivide Grid")]
		public void SubdivideGrid()
		{
			Grid grid = _subdivided_grid(this);
			tiles = grid.tiles;
			corners = grid.corners;
			edges = grid.edges;
			size = grid.size;
			if (mesh) DestroyImmediate(mesh);
			mesh = null;
			DestroyImmediate(grid);
		}
	}
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
