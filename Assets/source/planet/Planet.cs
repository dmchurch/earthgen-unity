using System;

using Earthgen.planet.grid;
using Earthgen.planet.terrain;
using Earthgen.planet.climate;

using static Earthgen.Statics;

namespace Earthgen.planet
{
	[Serializable]
	public class Planet : UnityEngine.ISerializationCallbackReceiver
	{
		public Grid grid;
		public Terrain terrain;
		public Climate climate;

		public Planet()
		{
			grid = size_n_grid(0);
		}

		public void clear() {
			var p = this;
			set_grid_size(p, 0);
			clear_terrain(p);
			clear_climate(p);
		}

		public void OnAfterDeserialize()
		{
			grid = size_n_grid(terrain.var.grid_size);
		}

		public void OnBeforeSerialize() { }
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

