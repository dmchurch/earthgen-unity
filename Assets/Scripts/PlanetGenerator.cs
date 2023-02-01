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
    public bool meshDirty;

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
        if (gridSize != planet.grid.size) {
            Debug.Log($"Setting planet grid size from {planet.grid.size} to {gridSize}");
            planet.set_grid_size(gridSize);
            meshDirty = true;
        }
        if (meshDirty) {
            planet.grid.GenerateMesh(GetComponent<MeshFilter>().mesh);
            meshDirty = false;
        }
    }
}
