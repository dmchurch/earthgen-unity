using Earthgen.planet;
using Earthgen.planet.climate;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Grid = Earthgen.planet.grid.Grid;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PlanetRenderer : MonoBehaviour
{
    public Planet planet;
    public Mesh mesh;
    public Material[] materials;

    public const int MaxTiles = 65536 / 7; // six hex corners and the center

    public Tile tile;
    public Vector3 tileVector;
    public List<Vector2> tilePolygon;
    public double latitude;
    public double longitude;
    public double north;
    public double area;
    public Terrain_tile terrainTile;
    public Climate_tile climateTile;

    private static Dictionary<int, int[]> TileRenderOrderings = new();
    private int[] TileRenderOrder => GetTileRenderOrder(planet.grid.size);

    private static int[] GetTileRenderOrder(int gridSize)
    {
        if (!TileRenderOrderings.TryGetValue(gridSize, out var ordering) || true) {
            if (Grid.tile_count(gridSize) <= MaxTiles) {
                ordering = Enumerable.Range(0, Grid.tile_count(gridSize)).ToArray();
            } else {
                var grid = Grid.size_n_grid(gridSize);
                ordering = new int[grid.tiles.Length];
                PopulateRenderOrdering(ordering, grid, 0, ordering.Length, MaxTiles);
            }
            TileRenderOrderings[gridSize] = ordering;
        }
        return ordering;
    }

    private static int PopulateRenderOrdering(int[] ordering, Grid grid, int startIndex, int count, int tilesPerSegment)
    {
        HashSet<int> addedTiles = new(ordering.Length);
        HashSet<int> seenTiles = new(ordering.Length);
        Queue<int> tilesToAdd = new();

        // start with whatever is at the end of the current range, becausee that's a boundary cell
        tilesToAdd.Enqueue(ordering[startIndex + count - 1]);

        if (startIndex > 0) {
            addedTiles.UnionWith(ordering[..startIndex]);
        }
        if (startIndex + count < ordering.Length) {
            addedTiles.UnionWith(ordering[(startIndex + count)..]);
        }
        seenTiles.UnionWith(addedTiles);

        for (int i = 0; i < count; i++) {
            int nextTile = tilesToAdd.Dequeue();
            ordering[i + startIndex] = nextTile;
            addedTiles.Add(nextTile);
            if (i % tilesPerSegment == 0) {
                // We just started a new segment, clear the todo list
                tilesToAdd.Clear();
                seenTiles.Clear();
                seenTiles.UnionWith(addedTiles);
            }
            foreach (var neighbor in grid.tiles[nextTile].tiles) {
                if (!seenTiles.Contains(neighbor.id)) {
                    tilesToAdd.Enqueue(neighbor.id);
                    seenTiles.Add(neighbor.id);
                }
            }
        }

        return tilesToAdd.Count > 0 ? tilesToAdd.Dequeue() : -1;
    }

    public int firstTile;
    public int tileCount;

    public ArraySegment<int> TileIds => new (TileRenderOrder,firstTile,tileCount);
    public TileAccessor Tiles => new(this);

    public struct TileAccessor : IReadOnlyList<Tile>
    {
        private PlanetRenderer self;

        public TileAccessor(PlanetRenderer self) => this.self = self;

        public Tile this[int index]
        {
            get
            {
                Debug.Log($"Fetching firstTile {self.firstTile} + index {index} = {self.firstTile+index} (of {self.tileCount} / {self.TileRenderOrder.Length})");
                return self.planet.nth_tile(self.TileRenderOrder[self.firstTile + index]);
            }
        }

        public Tile this[Index index] => this[index.GetOffset(self.tileCount)];

        public int Count => self.tileCount;

        public IEnumerator<Tile> GetEnumerator()
        {
            var planet = self.planet;
            var order = self.TileRenderOrder;
            return (from i in Enumerable.Range(self.firstTile, self.tileCount)
                    select planet.nth_tile(order[i])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private PlanetGenerator.MeshParameters meshParameters;
    private bool doElevation = false;
    private float radius = 40000000;

    private Vector3 scaleElevation(Vector3 v, float elevation) => doElevation ? v * (1 + elevation / radius * meshParameters.elevationScale) : v;
    private Vector3 scaleElevation(Vector3 v, Tile elevationSource, float? seaLevel = null) => doElevation ? scaleElevation(v, seaLevel ?? elevationSource.terrain(planet).elevation) : v;
    private Vector3 scaleElevation(Vector3 v, Corner elevationSource, float? seaLevel = null) => doElevation ? scaleElevation(v, seaLevel ?? elevationSource.terrain(planet).elevation) : v;

    public void GenerateMesh(PlanetGenerator.MeshParameters meshParameters)
    {
        this.meshParameters = meshParameters;
        doElevation = meshParameters.modelElevation && (planet.terrain.tiles?.Length ?? 0) >= planet.tile_count();
        if (!mesh) mesh = new();
        mesh.Clear();
        mesh.name = $"{gameObject.name} [Mesh]";
        var renderOrder = TileRenderOrder;
        //mesh.indexFormat = planet.grid.size > 5 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.subMeshCount = 3; // Land, sea, rivers
        radius = planet.radius() > 0 ? (float)planet.radius() : 40000000; // Set a reasonable planet-radius
        float seaLevel = (float)planet.sea_level();
        int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));

        var vertices = new List<Vector3>(); //[corners.Count + tiles.Count];
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        List<int> triangles = GenerateSubmesh();
        //Debug.Log($"Generated mesh with {triangles.Count} triangles and {vertices.Count} vertices");
        int firstSeaVertex = vertices.Count;
        List<int> seaTriangles = GenerateSubmesh(firstSeaVertex, seaLevel);
        int firstRiverVertex = vertices.Count;
        List<int> riverLines = doElevation ? GenerateRivers(firstRiverVertex) : new();

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0, true, 0);
        mesh.SetTriangles(seaTriangles, 1, true, firstSeaVertex);
        mesh.SetIndices(riverLines, MeshTopology.Lines, 2, true, firstRiverVertex);

        UpdateComponents();

        int AddVertex(Vector3 pos, Vector3 normal, Vector2 uv)
        {
            int idx = vertices.Count;
            vertices.Add(pos);
            normals.Add(normal);
            uvs.Add(uv);
            return idx;
        }

        List<int> GenerateRivers(int baseVertex = 0)
        {
            var lines = new List<int>();
            var corner_vertices = new Dictionary<Corner, int>();

            int CornerVertex(Corner c)
            {
                if (!corner_vertices.TryGetValue(c, out int idx)) {
                    idx = AddVertex(scaleElevation(c.v, c), c.v, new Vector2(c.terrain(planet).distance_to_sea, c.terrain(planet).elevation));
                    corner_vertices[c] = idx;
                }
                return idx;
            }

            if (planet.terrain.edges == null) return lines;

            HashSet<Edge> edges = (from t in Tiles from e in t.edges select e).ToHashSet();

            foreach (var e in edges) {
                if (!planet.has_river(e)) continue;
                River r = planet.river(e);
                lines.Add(CornerVertex(r.source) - baseVertex);
                lines.Add(CornerVertex(r.direction) - baseVertex);
            }

            return lines;
        }

        List<int> GenerateSubmesh(int baseVertex = 0, float? seaLevel = null)
        {
            var triangles = new List<int>();
            Vector3 AddTriangle(int c1, int c2, int c3)
            {
                triangles.AddRange(new[] { c1 - baseVertex, c2 - baseVertex, c3 - baseVertex });
                var tplane = new Plane(vertices[c1], vertices[c2], vertices[c3]);
                return tplane.normal;
            }

            int maxTile = Math.Min(firstTile + MaxTiles, planet.tile_count());
            foreach (Tile t in Tiles) {
                int x = t.id % tilesPerSide;
                int y = t.id / tilesPerSide;
                Vector2 uvCenter = new((x + 0.5f) / tilesPerSide, (y + 0.5f) / tilesPerSide);
                Terrain_tile ter = t.terrain(planet);
                tile = t;
                tileVector = t.v;
                tilePolygon = t.polygon(Quaternion.identity);
                latitude = planet.latitude(t.v);
                longitude = planet.longitude(t.v);
                north = planet.north(t);
                area = planet.area(t);
                terrainTile = ter;
                climateTile = planet.season_count() > 0 ? t.climate(planet.nth_season(0)) : default;
                if (seaLevel != null && ter.is_land() && !ter.is_water() && !ter.has_coast()) continue;
                if (seaLevel == null && ter.is_water() && !ter.is_land() && !ter.has_coast()) continue;
                //Vector3 v = Vector3.zero;
                Vector3 norm = Vector3.zero;
                float angle = 0;
                float aDelta = Mathf.PI * 2 / t.edge_count;
                int centerVertex = AddVertex(scaleElevation(t.v, t, seaLevel), t.v, uvCenter);
                int[] cornerVertices = new int[t.edge_count];
                for (int j = 0; j < t.edge_count; j++) {
                    cornerVertices[j] = AddVertex(
                        scaleElevation(t.nth_corner(j).v, t.nth_corner(j), seaLevel),
                        t.v,
                        uvCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) / tilesPerSide / 2);
                    angle += aDelta;
                }
                for (int j = 0; j < t.edge_count; j++) {
                    //v += t.corners[j].v;
                    norm += AddTriangle(
                        cornerVertices[j], // this corner
                        cornerVertices[(j + 1) % t.edge_count], // next corner
                        centerVertex // center
                    );
                }
                norm = norm.normalized; // average all plane normals to get the tile normal
                normals[centerVertex] = norm;
                foreach (var idx in cornerVertices) {
                    normals[idx] = norm; // set all normals for the tile to the same value
                }
                //vertices[centerVertex] = t.v; // v / t.edge_count;
            }

            return triangles;
        }

    }

    public void UpdateComponents()
    {
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterials = materials;
    }
}
