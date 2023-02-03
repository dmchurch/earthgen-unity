using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlanetGenerator : MonoBehaviour
{
    public Planet planet;
    [Range(0,10)]
    public int gridSize;
    [Range(1, 10000)]
    public float elevationScale = 1;
    private float oldElevationScale = 1;
    public bool modelElevation = true;
    public Vector3 rotationAxis;
    public Terrain_parameters terrainParameters = Terrain_parameters.Default;
    private Terrain_parameters oldTerrainParameters = Terrain_parameters.Default;
    public bool regenerateAutomatically = false;

    [Header("Actions (check to execute)")]
    [Tooltip("Regenerate mesh")]
    public bool meshDirty;
    public bool subdivideGrid;
    public bool resetPlanet;
    public bool resetAxis;
    public bool generateTerrain;
    public bool generateClimate;

    // Start is called before the first frame update
    void Start()
    {
        if (!planet) {
            planet = ScriptableObject.CreateInstance<Planet>();
        }
        gridSize = planet.grid.size;
        rotationAxis = planet.terrain.var.axis;
        meshDirty = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (subdivideGrid) {
            gridSize = planet.grid.size + 1;
            subdivideGrid = false;
        }
        if (planet && gridSize != planet.grid.size) {
            Debug.Log($"Setting planet grid size from {planet.grid.size} to {gridSize}");
            planet.set_grid_size(gridSize);
            meshDirty = true;
        }
        if (elevationScale != oldElevationScale) {
            oldElevationScale = elevationScale;
            meshDirty = true;
        }
        if (resetAxis) {
            resetAxis = false;
        }
        if (resetPlanet) {
            if (!planet) planet = ScriptableObject.CreateInstance<Planet>();
            planet.clear();
            gridSize = planet.grid.size;
            meshDirty = true;
            resetPlanet = false;
        }
        if (regenerateAutomatically && terrainParameters != oldTerrainParameters) {
            generateTerrain = true;
        }
        if (generateTerrain) {
            generateTerrain = false;
            gridSize = terrainParameters.grid_size;
            rotationAxis = terrainParameters.axis;
            planet.generate_terrain(terrainParameters);
            meshDirty = true;
            oldTerrainParameters = terrainParameters;
            transform.rotation = planet.rotation();
        }
        if (meshDirty) {
            var meshFilter = GetComponent<MeshFilter>();
            Mesh mesh;
            if (Application.isPlaying) {
                mesh = meshFilter.mesh;
            } else {
                mesh = meshFilter.sharedMesh;
                if (!mesh) {
                    mesh = meshFilter.sharedMesh = new Mesh();
                    mesh.name = gameObject.name;
                }
            }
            GenerateMesh(mesh);
            meshDirty = false;
        }
    }

    [UnityEngine.ContextMenu("Generate Mesh")]
	public void GenerateMeshCommand()
	{
		GenerateMesh(GetComponent<MeshFilter>().mesh);
	}

    private bool doElevation = false;
    private float radius = 40000000;

    private Vector3 scaleElevation(Vector3 v, float elevation) => doElevation ? v * (1 + elevation / radius * elevationScale) : v;
    private Vector3 scaleElevation(Vector3 v, Tile elevationSource, float? seaLevel = null) => doElevation ? scaleElevation(v, seaLevel ?? elevationSource.terrain(planet).elevation) : v;
    private Vector3 scaleElevation(Vector3 v, Corner elevationSource, float? seaLevel = null) => doElevation ? scaleElevation(v, seaLevel ?? elevationSource.terrain(planet).elevation) : v;

    public void GenerateMesh(Mesh mesh)
    {
        doElevation = modelElevation && (planet.terrain.tiles?.Length ?? 0) >= planet.tile_count();
        mesh.Clear();
        mesh.indexFormat = planet.grid.size > 5 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.subMeshCount = 3; // Land, sea, rivers
        radius = planet.radius() > 0 ? (float)planet.radius() : 40000000; // Set a reasonable planet-radius
        float seaLevel = (float)planet.sea_level();
        int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));

        var vertices = new List<Vector3>(); //[corners.Count + tiles.Count];
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        List<int> triangles = GenerateSubmesh();
        List<int> seaTriangles = GenerateSubmesh(seaLevel);
        List<int> riverLines = GenerateRivers();

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(seaTriangles, 1);
        mesh.SetIndices(riverLines, MeshTopology.Lines, 2);

        int AddVertex(Vector3 pos, Vector3 normal, Vector2 uv)
        {
            int idx = vertices.Count;
            vertices.Add(pos);
            normals.Add(normal);
            uvs.Add(uv);
            return idx;
        }

        List<int> GenerateRivers()
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

            for (int i = 0; i < planet.edge_count(); i++) {
                Edge e = planet.nth_edge(i);
                if (!planet.has_river(e)) continue;
                River r = planet.river(e);
                lines.Add(CornerVertex(r.source));
                lines.Add(CornerVertex(r.direction));
            }

            return lines;
        }

        List<int> GenerateSubmesh(float? seaLevel = null)
        {
            var triangles = new List<int>();
            Vector3 AddTriangle(int c1, int c2, int c3)
            {
                triangles.AddRange(new[] { c1, c2, c3 });
                var tplane = new Plane(vertices[c1], vertices[c2], vertices[c3]);
                return tplane.normal;
            }

            for (int i = 0; i < planet.tile_count(); i++) {
                int x = i % tilesPerSide;
                int y = i / tilesPerSide;
                Vector2 uvCenter = new((x + 0.5f) / tilesPerSide, (y + 0.5f) / tilesPerSide);
                Tile t = planet.nth_tile(i);
                Terrain_tile ter = t.terrain(planet);
                if (seaLevel != null && ter.is_land() && !ter.is_water() && !ter.has_coast()) continue;
                if (seaLevel == null && ter.is_water() && !ter.is_land() && !ter.has_coast()) continue;
                //Vector3 v = Vector3.zero;
                Vector3 norm = Vector3.zero;
                float angle = 0;
                float aDelta = Mathf.PI * 2 / t.edge_count;
                int centerVertex = AddVertex(scaleElevation(t.v, t, seaLevel), t.v, uvCenter);
                int[] cornerVertices = new int[t.edge_count];
                for (int j = 0; j < t.edge_count; j++) {
                    cornerVertices[j] = AddVertex(scaleElevation(t.nth_corner(j).v, t.nth_corner(j), seaLevel), t.v, uvCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) / tilesPerSide / 2);
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

}
