using UnityEngine;
using System;
using Earthgen.render;

namespace Earthgen.unity
{
    [Serializable]
    public struct TextureParameters
    {
        public static TextureParameters Default = new()
        {
        };

        public Material[] materials;

        // British spelling in honor of the original author :)
        public Planet_colours.Mode colourScheme;

        [Range(0, 1)]
        public float timeOfYear;
    }
}
