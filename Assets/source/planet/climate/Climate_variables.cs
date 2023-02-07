using System;

namespace Earthgen.planet.climate
{
    [Serializable]
    public struct Climate_variables
    {
        public int season_count;
    }

    public static partial class PlanetExtensions
    {
        public static int season_count (this Planet p) => climate(p).var.season_count;
    }
}

namespace Earthgen
{
    public static partial class Statics
    {
        public static int season_count (planet.Planet p) => planet.climate.PlanetExtensions.season_count(p);

    }
}
