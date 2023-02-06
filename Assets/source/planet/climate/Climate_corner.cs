using System;

namespace Earthgen.planet.climate
{
    [Serializable]
    public struct Climate_corner
    {
        public float river_flow_increase;
    }
}

namespace Earthgen
{
    public static partial class Statics
    {
        public static float river_flow_increase (planet.climate.Climate_corner c) => c.river_flow_increase;

    }
}
