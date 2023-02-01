using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Earthgen.planet;
using Earthgen.planet.grid;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlanetGenerator : MonoBehaviour
{
    public Planet planet;
    [Range(0,8)]
    public int gridSize;

    [Header("Actions (check to execute)")]
    [Tooltip("Regenerate mesh")]
    public bool meshDirty;
    public bool subdivideGrid;
    public bool resetPlanet;

    // Start is called before the first frame update
    void Start()
    {
        if (!planet) {
            planet = ScriptableObject.CreateInstance<Planet>();
            gridSize = 0;
        }
        meshDirty = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (subdivideGrid) {
            gridSize = planet.grid.size + 1;
            subdivideGrid = false;
        }
        if (gridSize != planet.grid.size) {
            Debug.Log($"Setting planet grid size from {planet.grid.size} to {gridSize}");
            planet.set_grid_size(gridSize);
            meshDirty = true;
        }
        if (resetPlanet) {
            planet.Clear();
            gridSize = planet.grid.size;
            meshDirty = true;
            resetPlanet = false;
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
            planet.grid.GenerateMesh(mesh);
            meshDirty = false;
        }
    }
}
