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
    public void GenerateMesh(Mesh mesh)
    {
        bool doElevation = modelElevation && (planet.terrain.tiles?.Length ?? 0) >= planet.tile_count();
        mesh.Clear();
        mesh.indexFormat = planet.grid.size > 5 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.subMeshCount = 2; // Land, water
        float radius = planet.radius() > 0 ? (float)planet.radius() : 40000000;
        float seaLevel = (float)planet.sea_level();
        int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));

        Vector3 scaleElevation(Vector3 v, float elevation) => v * (1 + elevation / radius * elevationScale);

        var vertices = new List<Vector3>(); //[corners.Count + tiles.Count];
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        List<int> triangles = GenerateSubmesh();
        List<int> seaTriangles = GenerateSubmesh(seaLevel);

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        if (doElevation) mesh.SetTriangles(seaTriangles, 1);

        List<int> GenerateSubmesh(float? elevationOverride = null)
        {
            var triangles = new List<int>();

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
            for (int i = 0; i < planet.tile_count(); i++) {
                int x = i % tilesPerSide;
                int y = i / tilesPerSide;
                Vector2 uvCenter = new((x + 0.5f) / tilesPerSide, (y + 0.5f) / tilesPerSide);
                Tile t = planet.nth_tile(i);
                if (doElevation && elevationOverride != null && planet.terrain.nth_tile(i).is_land()) continue;
                Vector3 v = Vector3.zero;
                float angle = 0;
                float aDelta = Mathf.PI * 2 / t.edge_count;
                int centerVertex = AddVertex(scaleElevation(t.v, doElevation ? elevationOverride ?? planet.terrain.nth_tile(t.id).elevation : 0), t.v, uvCenter, false);
                for (int j = 0; j < t.edge_count; j++) {
                    v += t.corners[j].v;
                    float nextAngle = angle + aDelta;
                    AddVertex(scaleElevation(t.nth_corner(j).v, doElevation ? elevationOverride ?? planet.terrain.nth_corner(t.nth_corner(j).id).elevation : 0), t.v, uvCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) / tilesPerSide / 2); // this corner
                    AddVertex(scaleElevation(t.nth_corner(j + 1).v, doElevation ? elevationOverride ?? planet.terrain.nth_corner(t.nth_corner(j + 1).id).elevation : 0), t.v, uvCenter + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) / tilesPerSide / 2); // next corner
                    triangles.Add(centerVertex); // center
                    angle = nextAngle;
                }
                //vertices[centerVertex] = t.v; // v / t.edge_count;
            }

            return triangles;
        }
    }

}
