using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using System;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlanetGenerator : MonoBehaviour
{
    public Planet planet;
    public Mesh mesh;
    public Texture2D tileTexture;

    public MeshParameters meshParameters = MeshParameters.Default;
    private MeshParameters oldMeshParameters = MeshParameters.Default;

    public TextureParameters textureParameters = TextureParameters.Default;
    private TextureParameters oldTextureParameters = TextureParameters.Default;

    public Terrain_parameters terrainParameters = Terrain_parameters.Default;
    private Terrain_parameters oldTerrainParameters = Terrain_parameters.Default;
    public bool regenerateAutomatically = false;

    [Header("Actions (check to execute)")]
    [Tooltip("Regenerate mesh")]
    public bool meshDirty;
    [Tooltip("Regenerate texture")]
    public bool textureDirty;
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
        meshDirty = true;
        textureDirty = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (meshParameters != oldMeshParameters) {
            meshDirty = true;
        }
        if (textureParameters != oldTextureParameters) {
            textureDirty = true;
        }
        if (resetAxis) {
            resetAxis = false;
        }
        if (resetPlanet) {
            if (!planet) planet = ScriptableObject.CreateInstance<Planet>();
            planet.clear();
            meshDirty = true;
            resetPlanet = false;
        }
        if (regenerateAutomatically && terrainParameters != oldTerrainParameters) {
            generateTerrain = true;
        }
        if (generateTerrain) {
            generateTerrain = false;
            planet.generate_terrain(terrainParameters);
            meshDirty = true;
            textureDirty = true;
            oldTerrainParameters = terrainParameters;
            transform.rotation = planet.rotation();
        }
        if (meshDirty) {
            oldMeshParameters = meshParameters;
            var meshFilter = GetComponent<MeshFilter>();
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
        if (textureDirty) {
            oldTextureParameters = textureParameters;
            textureDirty = false;
            if (!tileTexture) {
                tileTexture = new Texture2D(2048, 2048);
                tileTexture.name = gameObject.name;
            }
            GenerateTextures();
            var renderer = GetComponent<MeshRenderer>();
            Material[] materials;
            if (Application.isPlaying) {
                materials = renderer.materials;
            } else {
                materials = renderer.sharedMaterials;
            }
            materials[0].mainTexture = tileTexture;
            materials[1].mainTexture = tileTexture;
        }
    }

    [UnityEngine.ContextMenu("Generate Mesh")]
	public void GenerateMeshCommand()
	{
		GenerateMesh(GetComponent<MeshFilter>().mesh);
	}

    private bool doElevation = false;
    private float radius = 40000000;

    private Vector3 scaleElevation(Vector3 v, float elevation) => doElevation ? v * (1 + elevation / radius * meshParameters.elevationScale) : v;
    private Vector3 scaleElevation(Vector3 v, Tile elevationSource, float? seaLevel = null) => doElevation ? scaleElevation(v, seaLevel ?? elevationSource.terrain(planet).elevation) : v;
    private Vector3 scaleElevation(Vector3 v, Corner elevationSource, float? seaLevel = null) => doElevation ? scaleElevation(v, seaLevel ?? elevationSource.terrain(planet).elevation) : v;

    public void GenerateTextures()
    {
        tileTexture.Reinitialize(2048, 2048, TextureFormat.RGB24, false);
        int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));
        float pixelsPerTile = 2048f / tilesPerSide;
        Color[] colors = new Color[Mathf.CeilToInt((pixelsPerTile + 1) * (pixelsPerTile + 1))];
        var colorSpan = colors.AsSpan();

	    Color water_deep = new(0.0f, 0.0f, 0.25f);
	    Color water = new(0.0f, 0.12f, 0.5f);
	    Color water_shallow = new(0.0f, 0.4f, 0.6f);

        var land = new (float limit, Color color)[]
        {
		    (-500, new(0.0f, 0.4f, 0.0f)),
		    (0, new(0.0f, 0.7f, 0.0f)),
		    (1000, new(1.0f, 1.0f, 0.0f)),
		    (1500, new(1.0f, 0.5f, 0.0f)),
		    (2000, new(0.7f, 0.0f, 0.0f)),
		    (2500, new(0.1f, 0.1f, 0.1f)),
        };

        for (int i = 0; i < planet.tile_count(); i++) {
            int x = i % tilesPerSide;
            int y = i / tilesPerSide;
            int uMin = Mathf.RoundToInt(x * pixelsPerTile);
            int vMin = Mathf.RoundToInt(y * pixelsPerTile);
            int uMax = Mathf.RoundToInt((x + 1) * pixelsPerTile);
            int vMax = Mathf.RoundToInt((y + 1) * pixelsPerTile);

            Tile t = planet.nth_tile(i);
            Terrain_tile ter = t.terrain(planet);
            float elev = ter.elevation - (float)planet.sea_level();
            Color color;
            if (ter.is_water()) {
                if (elev < -1000) {
                    color = water_deep;
                } else if (elev < -500) {
                    float d = (elev + 500) / (-500);
                    color = Color.Lerp(water, water_deep, d);
                } else {
                    float d = elev/-500;
                    color = Color.Lerp(water_shallow, water, d);
                }
            } else {
                color = land[5].color;
                for (int j = 0; j < 5; j++) {
                    if (elev <= land[j+1].limit) {
                        float d = (elev - land[j].limit) / (land[j+1].limit / land[j].limit);
                        color = Color.Lerp(land[j].color, land[j+1].color, d);
                        break;
                    }
                }
            }

            colorSpan.Fill(color);

            tileTexture.SetPixels(uMin, vMin, uMax - uMin, vMax - vMin, colors);
        }

        tileTexture.Apply();
    }

    public void GenerateMesh(Mesh mesh)
    {
        doElevation = meshParameters.modelElevation && (planet.terrain.tiles?.Length ?? 0) >= planet.tile_count();
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

    [Serializable]
    public struct MeshParameters : IEquatable<MeshParameters>
    {
        [Range(1, 10000)]
        public float elevationScale;
        public bool modelElevation;

        public static readonly MeshParameters Default = new()
        {
            elevationScale = 1,
            modelElevation = true,
        };

        #region Boilerplate (Equals, GetHashCode, ==, !=)
        public override bool Equals(object obj) => obj is MeshParameters parameters && Equals(parameters);
        public bool Equals(MeshParameters other) => elevationScale == other.elevationScale && modelElevation == other.modelElevation;
        public override int GetHashCode() => HashCode.Combine(elevationScale, modelElevation);

        public static bool operator ==(MeshParameters left, MeshParameters right) => left.Equals(right);
        public static bool operator !=(MeshParameters left, MeshParameters right) => !(left == right);
        #endregion
    }

    [Serializable]
    public struct TextureParameters : IEquatable<TextureParameters>
    {
        public enum ColorMode
        {
            Topography,
            Vegetation,
            Temperature,
            Aridity,
            Humidity,
            Precipitation,
        };
        public ColorMode colorMode;
        public static readonly TextureParameters Default = new()
        {

        };

        #region Boilerplate (Equals, GetHashCode, ==, !=)
        public override bool Equals(object obj) => obj is TextureParameters parameters && Equals(parameters);
        public bool Equals(TextureParameters other) => colorMode == other.colorMode;
        public override int GetHashCode() => HashCode.Combine(colorMode);

        public static bool operator ==(TextureParameters left, TextureParameters right) => left.Equals(right);
        public static bool operator !=(TextureParameters left, TextureParameters right) => !(left == right);
        #endregion
    }
}
