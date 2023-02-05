using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using Earthgen.planet.climate;
using System;
using Grid = Earthgen.planet.grid.Grid;
using Earthgen.render;

[ExecuteInEditMode]
[RequireComponent(typeof(GeneratedPlanetData))]
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

    public Climate_parameters climateParameters = Climate_parameters.Default;
    private Climate_parameters oldClimateParameters = Climate_parameters.Default;
    public bool regenerateAutomatically = false;

    [Header("Precalculated data")]
    public bool savePrecalculatedData = false;
    public bool saveSceneHierarchy = false;
    private bool oldSaveHierarchy = false;
    private GeneratedPlanetData generatedData = null;

    [Header("Actions (check to execute)")]
    public bool resetPlanet;
    public bool generateTerrain;
    public bool generateClimate;

    public bool instantiateRenderers;
    public bool regenerateMeshes;
    public bool regenerateTextures;
    public bool updatePrecalculatedData;

    private PlanetRenderer[] renderers = new PlanetRenderer[0];

    private ref Planet planet => ref generatedData.planet;
    private ref Texture2D tileTexture => ref generatedData.tileTexture;
    private ref Planet_colours planetColours => ref generatedData.planetColours;

    void Awake()
    {
        generatedData = GetComponent<GeneratedPlanetData>();
        renderers = GetComponentsInChildren<PlanetRenderer>();
        Debug.Log($"Starting PlanetGenerator, {renderers.Length} renderers detected, generatedData = {generatedData}");
        if (generatedData) {
            Debug.Log($"Loading precalculated data: {planet.tile_count()} tiles, {planet.terrain.tiles?.Length ?? 0} terrain tiles, {planet.season_count()} seasons)");
        }

        oldMeshParameters = meshParameters;
        oldTextureParameters = textureParameters;
        oldTerrainParameters = terrainParameters;
        oldClimateParameters = climateParameters;
        oldSaveHierarchy = saveSceneHierarchy;

        if (!generatedData) {
            Debug.Log($"No precalculated data, creating new and resetting planet");
            generatedData = gameObject.AddComponent<GeneratedPlanetData>();
            resetPlanet = true;
            regenerateMeshes = true;
            regenerateTextures = true;
        }

        instantiateRenderers = true;

        generatedData.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        if (!generatedData) {
            Awake();
        } else if (!planet || !tileTexture || planetColours == null) {
            generatedData.Awake();
        }
        if (materials[0].mainTexture != tileTexture) {
            materials[0].mainTexture = tileTexture;
            materials[1].mainTexture = tileTexture;
        }
        if (meshParameters != oldMeshParameters) {
            regenerateMeshes = true;
        }
        if (textureParameters != oldTextureParameters) {
            regenerateTextures = true;
        }
        generatedData.hideFlags = savePrecalculatedData ? HideFlags.None : (HideFlags.NotEditable | HideFlags.DontSave);
        if (saveSceneHierarchy != oldSaveHierarchy) {
            foreach (var r in renderers) {
                r.gameObject.hideFlags = saveSceneHierarchy ? HideFlags.None : (HideFlags.DontSaveInEditor | HideFlags.NotEditable);
            }
            oldSaveHierarchy = saveSceneHierarchy;
        }
        if (resetPlanet) {
            Debug.Log($"Resetting planet");
            planet.name = $"{gameObject.name} [Planet data]";
            planet.clear();
            instantiateRenderers = true;
            regenerateMeshes = true;
            regenerateTextures = true;
            updatePrecalculatedData = true;
            resetPlanet = false;
        }
        if (regenerateAutomatically && terrainParameters != oldTerrainParameters) {
            generateTerrain = true;
            generateClimate = true;
        } else if (regenerateAutomatically && climateParameters != oldClimateParameters) {
            generateClimate = true;
        }
        if (generateClimate && (planet.terrain.tiles?.Length ?? 0) < planet.tile_count()) {
            generateTerrain = true;
        }
        if (generateTerrain) {
            generateTerrain = false;
            planet.generate_terrain(terrainParameters);
            instantiateRenderers = true;
            regenerateMeshes = true;
            regenerateTextures = true;
            oldTerrainParameters = terrainParameters;
        } else if (regenerateMeshes && planet.grid.size != terrainParameters.grid_size) {
            planet.set_grid_size(terrainParameters.grid_size);
            instantiateRenderers = true;
        }
        if (generateClimate) {
            generateClimate = false;
            planet.generate_climate(climateParameters);
            oldClimateParameters = climateParameters;
        }
        if (instantiateRenderers) {
            InstantiateRenderers();
        }
        if (regenerateMeshes) {
            foreach (var renderer in GetComponentsInChildren<PlanetRenderer>() /*renderers*/) {
                renderer.GenerateMesh(meshParameters);
            }
            oldMeshParameters = meshParameters;
            updatePrecalculatedData = true;
            regenerateMeshes = false;
        }
        if (regenerateTextures) {
            oldTextureParameters = textureParameters;
            regenerateTextures = false;
            if (!tileTexture) {
                tileTexture = new Texture2D(2048, 2048);
                tileTexture.name = $"{gameObject.name} [Tile Texture]";
                updatePrecalculatedData = true;
            }
            GenerateTextures();
            materials[0].mainTexture = tileTexture;
            materials[1].mainTexture = tileTexture;
        }
        if (updatePrecalculatedData) {
            updatePrecalculatedData = false;
        }
    }

    private void InstantiateRenderers(bool removeUnused = true)
    {
        int tileCount = Grid.tile_count(terrainParameters.grid_size);
        int renderersNeeded = Mathf.CeilToInt((float)tileCount / 1);// PlanetRenderer.MaxTiles);

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
            renderer.firstTile = i; // * PlanetRenderer.MaxTiles;
            renderer.tileCount = 1; //Math.Min(PlanetRenderer.MaxTiles, tileCount - renderer.firstTile);
            renderer.planet = planet;
            renderer.materials = materials;
            if (generatedData && (generatedData.meshes?.Length ?? 0) > i && generatedData.meshes[i]) {
                renderer.mesh = generatedData.meshes[i];
                renderer.UpdateComponents();
            }
        }
        instantiateRenderers = false;
        updatePrecalculatedData = true;
    }

    public void GenerateTextures()
    {
        tileTexture.Reinitialize(2048, 2048, TextureFormat.RGB24, false);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        int tilesPerSide = Mathf.CeilToInt(Mathf.Sqrt(planet.tile_count()));
        float pixelsPerTile = 2048f / tilesPerSide;
        Color[] colors = new Color[Mathf.CeilToInt((pixelsPerTile + 1) * (pixelsPerTile + 1))];
        var colorSpan = colors.AsSpan();

        planetColours.init_colours(planet);
        if (planet.season_count() > 0) {
            Season season = planet.nth_season(Math.Clamp(Mathf.FloorToInt(textureParameters.yearProgress * planet.season_count()),
                                                         0, planet.season_count() - 1));
            planetColours.set_colours(planet, season, textureParameters.colorMode);
        } else {
            planetColours.set_colours(planet, textureParameters.colorMode);
        }

        for (int i = 0; i < planet.tile_count(); i++) {
            int x = i % tilesPerSide;
            int y = i / tilesPerSide;
            int uMin = Mathf.RoundToInt(x * pixelsPerTile);
            int vMin = Mathf.RoundToInt(y * pixelsPerTile);
            int uMax = Mathf.RoundToInt((x + 1) * pixelsPerTile);
            int vMax = Mathf.RoundToInt((y + 1) * pixelsPerTile);

            Color color = planetColours.tiles[i];

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
        public Planet_colours.Mode colorMode;
        [Range(0,1)]
        public float yearProgress;

        public static readonly TextureParameters Default = new()
        {
            colorMode = Planet_colours.Mode.Vegetation,
            yearProgress = 0,
        };

        #region Boilerplate (Equals, GetHashCode, ==, !=)
        public override bool Equals(object obj) => obj is TextureParameters parameters && Equals(parameters);
        public bool Equals(TextureParameters other) => colorMode == other.colorMode && yearProgress == other.yearProgress;
        public override int GetHashCode() => HashCode.Combine(colorMode, yearProgress);

        public static bool operator ==(TextureParameters left, TextureParameters right) => left.Equals(right);
        public static bool operator !=(TextureParameters left, TextureParameters right) => !(left == right);
        #endregion
    }

}
