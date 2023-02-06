using UnityEngine;
using System;

namespace Earthgen.unity
{
    [Serializable]
    public struct MeshParameters
    {
        public static MeshParameters Default = new()
        {
            modelTopography = true,
            elevationScale = 1,
        };
        [Tooltip("When unchecked, all planets with the same grid size will share the same mesh")]
        public bool modelTopography;

        [Tooltip("Planet topography will be exaggerated when this is > 1, requires \"Model Topography\" to be enabled")]
        [Range(1, 10000)]
        public float elevationScale;
    }
}
