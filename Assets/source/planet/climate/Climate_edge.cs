using System;

namespace Earthgen.planet.climate
{
    [Serializable]
    public struct Climate_edge
    {
        public float wind_velocity;
        public float river_flow;
    }
}

namespace Earthgen
{
    using planet.climate;
    public static partial class Statics
    {
        public static float wind_velocity (Climate_edge e) => e.wind_velocity;
        public static float river_flow (Climate_edge e) => e.river_flow;
    }
}
