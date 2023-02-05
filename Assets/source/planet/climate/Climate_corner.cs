using System;

namespace Earthgen.planet.climate
{
	[Serializable]
    public struct Climate_corner
    {
		public static readonly Climate_corner Default = default;
        public float river_flow_increase;
    }
}