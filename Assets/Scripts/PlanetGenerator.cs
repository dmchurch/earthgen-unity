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
    [RequireComponent(typeof(GeneratedPlanetData), typeof(RenderedPlanetData))]
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
            SaveGeneratedData = SaveGeneratedData;
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
            meshDirty = false;
            throw new NotImplementedException();
        }

        public void GenerateTextures()
        {
            Debug.Log($"Generating Textures");
            textureDirty = false;
            throw new NotImplementedException();
        }

    }
}
