using UnityEngine;

using Earthgen.planet;
using Earthgen.render;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeneratedPlanetData : MonoBehaviour
{
    public Planet planet;
    public Texture2D tileTexture;
    public Mesh[] meshes;
    public Planet_colours planetColours;

    public void Awake()
    {
        if (!planet) {
            Debug.Log($"Creating Planet instance");
            planet = ScriptableObject.CreateInstance<Planet>();
        }

        if (!tileTexture) {
            Debug.Log($"Creating tileTexture");
            tileTexture = new(2048, 2048);
        }

        if (meshes == null) {
            Debug.Log($"Fetching meshes");
            meshes = (from r in GetComponentsInChildren<PlanetRenderer>() select r.mesh).ToArray();
        }
        if (planetColours == null) {
            Debug.Log($"Creating planetColours");
            planetColours = new();
        }
    }

    public void SaveMeshes()
    {
        Debug.Log($"Saving meshes");
        meshes = (from r in GetComponentsInChildren<PlanetRenderer>() select r.mesh).ToArray();
    }
    public GeneratedPlanetData Copy() => Instantiate(this);
    public override bool Equals(object obj) => Equals(obj as GeneratedPlanetData);
    public bool Equals(GeneratedPlanetData other) => other is not null && base.Equals(other) && EqualityComparer<Planet>.Default.Equals(planet, other.planet) && EqualityComparer<Texture2D>.Default.Equals(tileTexture, other.tileTexture) && EqualityComparer<Mesh[]>.Default.Equals(meshes, other.meshes) && EqualityComparer<Planet_colours>.Default.Equals(planetColours, other.planetColours);
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), planet, tileTexture, meshes, planetColours);

    public static bool operator ==(GeneratedPlanetData left, GeneratedPlanetData right) => EqualityComparer<GeneratedPlanetData>.Default.Equals(left, right);
    public static bool operator !=(GeneratedPlanetData left, GeneratedPlanetData right) => !(left == right);
}
