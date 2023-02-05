using System;

namespace Earthgen.planet.climate
{
	[Serializable]
    public struct Climate_edge
    {
		public static readonly Climate_edge Default = default;
        public float wind_velocity;
        public float river_flow;
    }
}