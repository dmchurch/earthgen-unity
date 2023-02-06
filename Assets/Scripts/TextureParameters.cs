using UnityEngine;
using System;

namespace Earthgen.unity
{
    [Serializable]
    public struct TextureParameters
    {
        public static TextureParameters Default = new()
        {
        };
        [Range(0, 1)]
        public float timeOfYear;
    }
}
