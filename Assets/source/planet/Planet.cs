using System;

using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using Earthgen.planet.climate;

using static Earthgen.Statics;

namespace Earthgen.planet
{
	[Serializable]
	public class Planet
	{
		public Grid grid;
		public Terrain terrain;
		public Climate climate;

		public Planet()
		{
			grid = size_n_grid(0);
			terrain = new Terrain();
			climate = new Climate();
		}

		public void clear() {
			var p = this;
			set_grid_size(p, 0);
			clear_terrain(p);
			clear_climate(p);
		}
	}
}

namespace Earthgen
{
	using Earthgen.planet;
	public static partial class Statics
	{
		public static void clear(Planet p) => p.clear();
	}
}

