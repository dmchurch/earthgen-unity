using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Earthgen.planet.terrain;
using Earthgen.planet.climate;

namespace Earthgen.unity
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(GeneratedPlanetData))]
    public class PlanetGenerator : MonoBehaviour
    {
        public Terrain_parameters terrainParameters = Terrain_parameters.Default;
        public Climate_parameters climateParameters = Climate_parameters.Default;

        public bool terrainDirty = true;
        public bool climateDirty = true;

        [SerializeField]
        private bool saveGeneratedData = false;
        public bool SaveGeneratedData
        {
            get => saveGeneratedData;
            set => (saveGeneratedData, GetComponent<GeneratedPlanetData>().hideFlags)
                = (value, value ? HideFlags.None : HideFlags.NotEditable | HideFlags.DontSave);
        }

        public GeneratedPlanetData Data => GetComponent<GeneratedPlanetData>();

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        }

        [ContextMenu("Generate Terrain")]
        public void GenerateTerrain()
        {
            Debug.Log($"Generating Terrain");
            Data.planet.generate_terrain(terrainParameters);
            terrainDirty = false;
            climateDirty = true;
        }

        [ContextMenu("Generate Climate")]
        public void GenerateClimate()
        {
            Debug.Log($"Generating Climate");
            Data.planet.generate_climate(climateParameters);
            climateDirty = false;
        }
    }
}
