using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using System;
using Grid = Earthgen.planet.grid.Grid;

[ExecuteInEditMode]
public class PlanetGenerator : MonoBehaviour
{
    public Material[] materials;
    public PlanetRenderer rendererPrefab;

    public MeshParameters meshParameters = MeshParameters.Default;
    private MeshParameters oldMeshParameters = MeshParameters.Default;

    public TextureParameters textureParameters = TextureParameters.Default;
    private TextureParameters oldTextureParameters = TextureParameters.Default;

    public Terrain_parameters terrainParameters = Terrain_parameters.Default;
    private Terrain_parameters oldTerrainParameters = Terrain_parameters.Default;
    public bool regenerateAutomatically = false;

    [Header("Precalculated data")]
    public bool savePrecalculatedData = false;
    public GeneratedData precalculatedData = null;

    [Header("Actions (check to execute)")]
    public bool resetPlanet;
    public bool generateTerrain;
    public bool generateClimate;

    public bool instantiateRenderers;
    public bool regenerateMeshes;
    public bool regenerateTextures;

    private PlanetRenderer[] renderers = new PlanetRenderer[0];

    private Planet planet;
    private Texture2D tileTexture;
    private Mesh[] meshes;

    // Start is called before the first frame update
    void Start()
    {
        renderers = GetComponentsInChildren<PlanetRenderer>();

        if (precalculatedData) {
            planet = precalculatedData.planet;
            tileTexture = precalculatedData.tileTexture;
        } else {
            planet = new();
            tileTexture = new(2048, 2048);
            resetPlanet = true;
            instantiateRenderers = true;
            regenerateMeshes = true;
            regenerateTextures = true;
        }

        //if (!planet) {
        //    planet = ScriptableObject.CreateInstance<Planet>();
        //}
        //meshDirty = true;
        //textureDirty = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (meshParameters != oldMeshParameters) {
            regenerateMeshes = true;
        }
        if (textureParameters != oldTextureParameters) {
            regenerateTextures = true;
        }
        if (resetPlanet) {
            if (!planet) planet = ScriptableObject.CreateInstance<Planet>();
            planet.name = $"{gameObject.name} [Planet data]";
            planet.clear();
            instantiateRenderers = true;
            regenerateMeshes = true;
            regenerateTextures = true;
            resetPlanet = false;
        }
        if (regenerateAutomatically && terrainParameters != oldTerrainParameters) {
            generateTerrain = true;
        }
        if (generateTerrain) {
            generateTerrain = false;
            planet.generate_terrain(terrainParameters);
            instantiateRenderers = true;
            regenerateMeshes = true;
            regenerateTextures = true;
            oldTerrainParameters = terrainParameters;
            transform.rotation = planet.rotation();
        } else if (regenerateMeshes && planet.grid.size != terrainParameters.grid_size) {
            planet.set_grid_size(terrainParameters.grid_size);
            instantiateRenderers = true;
        }
        if (instantiateRenderers) {
            InstantiateRenderers();
        }
        if (regenerateMeshes) {
            foreach (var renderer in renderers) {
                renderer.GenerateMesh(meshParameters);
            }
            oldMeshParameters = meshParameters;
            regenerateMeshes = false;
        }
        if (regenerateTextures) {
            oldTextureParameters = textureParameters;
            regenerateTextures = false;
            if (!tileTexture) {
                tileTexture = new Texture2D(2048, 2048);
                tileTexture.name = $"gameObject.name [Tile Texture]";
            }
            GenerateTextures();
            materials[0].mainTexture = tileTexture;
            materials[1].mainTexture = tileTexture;
        }
    }

    private void InstantiateRenderers(bool removeUnused = true)
    {
        int tileCount = Grid.tile_count(terrainParameters.grid_size);
        int renderersNeeded = Mathf.CeilToInt((float)tileCount / PlanetRenderer.MaxTiles);

        renderers = GetComponentsInChildren<PlanetRenderer>();

        for (int i = renderersNeeded; i < renderers.Length; i++) {
            if (Application.isPlaying) {
                Destroy(renderers[i].gameObject);
            } else {
                DestroyImmediate(renderers[i].gameObject);
            }
        }
        if (renderers.Length != renderersNeeded) {
            Array.Resize(ref renderers, renderersNeeded);
        }
        for (int i = 0; i < renderersNeeded; i++) {
            var renderer = renderers[i];
            if (!renderer) {
                renderer = Instantiate(rendererPrefab, transform);
                renderer.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.NotEditable;
                renderers[i] = renderer;
            }
            renderer.gameObject.name = $"{name} [Renderer {i}]";
            renderer.firstTile = i * PlanetRenderer.MaxTiles;
            renderer.planet = planet;
            renderer.materials = materials;
        }
        instantiateRenderers = false;
    }

    public void GenerateTextures()
    {
        tileTexture.Reinitialize(2048, 2048, TextureFormat.RGB24, false);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
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

    public class GeneratedData : ScriptableObject
    {
        public Planet planet;
        public Texture2D tileTexture;
        public Mesh[] meshes;
    }
}
