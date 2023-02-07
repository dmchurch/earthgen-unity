using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Earthgen.planet.terrain;
using Earthgen.planet.climate;

using Random = UnityEngine.Random;

namespace Earthgen.unity
{
    [ExecuteInEditMode]
    public class PlanetGenerator : MonoBehaviour
    {
        public Terrain_parameters terrainParameters = Terrain_parameters.Default;
        public Climate_parameters climateParameters = Climate_parameters.Default;
        public MeshParameters meshParameters = MeshParameters.Default;
        public TextureParameters textureParameters = TextureParameters.Default;

        public bool autoGenerate = false;
        public bool autoRender = false;

        public enum SeedSource
        {
            Specified,
            Random,
            [InspectorName("GameObject")]
            GameObject,
        };
        public SeedSource seedSource;

        public bool terrainDirty = true;
        public bool climateDirty = true;
        public bool meshDirty = true;
        public bool textureDirty = true;

        [SerializeField]
        private bool saveGeneratedData = false;
        public bool SaveGeneratedData
        {
            get => saveGeneratedData;
            set => (saveGeneratedData, Data.hideFlags)
                = (value, value ? HideFlags.None : HideFlags.NotEditable | HideFlags.DontSave);
        }

        [SerializeField]
        private bool saveRenderedObjects = false;
        public bool SaveRenderedObjects
        {
            get => saveRenderedObjects;
            set => (saveRenderedObjects, Render.hideFlags)
                = (value, value ? HideFlags.None : HideFlags.NotEditable | HideFlags.DontSave);
        }

        public GeneratedPlanetData Data => GetComponent<GeneratedPlanetData>();
        public RenderedPlanetData Render => GetComponent<RenderedPlanetData>();

        void Awake()
        {
            if (!Data) gameObject.AddComponent<GeneratedPlanetData>();
            if (!Render) gameObject.AddComponent<RenderedPlanetData>();
            SaveGeneratedData = SaveGeneratedData;
            SaveRenderedObjects = SaveRenderedObjects;
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void AutoRender()
        {
            if (autoGenerate && terrainDirty) {
                GenerateTerrain();
            }
            if (autoGenerate && climateDirty) {
                GenerateClimate();
            }
            if (autoRender && meshDirty) {
                GenerateMeshes();
            }
            if (autoRender && textureDirty) {
                GenerateTextures();
            }
            if (autoRender && !meshDirty && !textureDirty) {
                InstantiateRenderers();
            }
        }

        public static string RandomSeed()
        {
            char[] consonants = "bcdfghjklmnpqrstvwxyz".ToCharArray();
            char[] vowels = "aeiouy".ToCharArray();
            string s = vowels[Random.Range(0, vowels.Length)].ToString().ToUpper();
            while (s.Length < 8) {
                var array = s.Length % 2 == 0 ? vowels : consonants;
                s += array[Random.Range(0, array.Length)];
            }
            return s;
        }

        [ContextMenu("Generate Terrain")]
        public void GenerateTerrain()
        {
            if (seedSource == SeedSource.Random) {
                terrainParameters.seed = RandomSeed();
                Debug.Log($"Generated random seed {terrainParameters.seed}");
            } else if (seedSource == SeedSource.GameObject && terrainParameters.seed != gameObject.name) {
                terrainParameters.seed = gameObject.name;
                Debug.Log($"Set seed {terrainParameters.seed}");
            }
            Debug.Log($"Generating terrain for {DescribeParameters(terrainParameters)}");
            Data.planet.generate_terrain(terrainParameters);
            if (terrainParameters.grid_size != Data.terrainParameters.grid_size) {
                // if grid size changed, climate MUST be regenned before texture
                textureDirty = false;
            }
            // Record generation parameters
            Data.terrainParameters = terrainParameters;
            terrainDirty = false;
            climateDirty = true;
            meshDirty = true;
        }

        public void InstantiateRenderers()
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            var meshes = Render.meshes;
            int rendererCount = renderers.Length;
            var materials = textureParameters.materials;

            if (rendererCount < meshes.Length) {
                Array.Resize(ref renderers, meshes.Length);
                for (; rendererCount < meshes.Length; rendererCount++) {
                    var child = new GameObject(
                        $"{gameObject.name} [Renderer {rendererCount}]",
                        new[] { typeof(MeshRenderer), typeof(MeshFilter) });
                    child.transform.parent = transform;
                    renderers[rendererCount] = child.GetComponent<MeshRenderer>();
                }
            } else {
                for (; rendererCount > meshes.Length; rendererCount--) {
                    if (Application.isPlaying) {
                        Destroy(renderers[rendererCount - 1].gameObject);
                    } else {
                        DestroyImmediate(renderers[rendererCount - 1].gameObject);
                    }
                }
                Array.Resize(ref renderers, meshes.Length);
            }

            for (int i = 0; i < rendererCount; i++) {
                var renderer = renderers[i];
                var filter = renderer.GetComponent<MeshFilter>();
                if (meshes.Length > i) {
                    filter.sharedMesh = meshes[i];
                }
                // convert from Earthgen z-up to unity y-up
                renderer.transform.rotation = Quaternion.FromToRotation(new(0, 0, 1), Vector3.up);
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.gameObject.name = $"{gameObject.name} [Renderer{(rendererCount == 1 ? "" : " "+i)}]";
                renderer.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
            }
        } 

        public static string DescribeParameters(Terrain_parameters par)
        {
            return $"grid size {par.grid_size}, seed \"{par.seed}\", iterations {par.iterations}";
        }

        [ContextMenu("Generate Climate")]
        public void GenerateClimate()
        {
            Debug.Log($"Generating Climate");
            Data.planet.generate_climate(climateParameters);
            Data.climateParameters = climateParameters;
            climateDirty = false;
            textureDirty = true;
        }

        public void GenerateMeshes()
        {
            Debug.Log($"Generating Meshes");
            Render.RenderMeshes(meshParameters, gameObject.name);
            meshDirty = false;
        }

        public void GenerateTextures()
        {
            Debug.Log($"Generating Textures");
            Render.RenderTextures(textureParameters, gameObject.name);
            if (textureParameters.materials.Length > 0) {
                textureParameters.materials[0].mainTexture = Render.tileTexture;
            }
            textureDirty = false;
        }

    }
}
