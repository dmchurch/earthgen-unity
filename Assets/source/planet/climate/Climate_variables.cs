
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
        public static int season_count(this Planet p) => p.climate.var.season_count;
    }
}