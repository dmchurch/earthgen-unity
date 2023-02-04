using System.Collections;
using System.Collections.Generic;

using Earthgen.planet.grid;
using Earthgen.planet.climate;
using Earthgen.planet.terrain;
using System.Linq;
using System;

namespace Earthgen.planet
{
    [UnityEngine.CreateAssetMenu]
    public class Planet : UnityEngine.ScriptableObject
    {
        public void OnEnable()
        {
            if (grid == null) {
                grid = Grid.size_n_grid(terrain ? terrain.var.grid_size : 0);
            }
            terrain = terrain ? terrain : CreateInstance<Terrain>();
            climate = climate ? climate : CreateInstance<Climate>();
        }

        public Grid grid;
        public Terrain terrain;
        public Climate climate;

        [UnityEngine.ContextMenu("Clear Planet")]
        public void clear()
        {
            this.set_grid_size(0);
            this.clear_terrain();
            this.clear_climate();
        }


    };

	public static class utilityExtensions
	{
		public static void Resize<T>(this List<T> list, int sz, T c = default)
		{
			int cur = list.Count;
			if(sz < cur)
				list.RemoveRange(sz, cur - sz);
			else if(sz > cur)
			{
				if(sz > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
				  list.Capacity = sz;
				list.AddRange(Enumerable.Repeat(c, sz - cur));
			}
		}

        public static T[] Resize<T>(this T[] arr, int sz, T c = default)
        {
            int cur = arr.Length;
            if (sz < cur) {
                Array.Resize(ref arr, sz);
            } else if (sz > cur) {
                Array.Resize(ref arr, sz);
                arr.AsSpan(cur).Fill(c);
            }
            return arr;
        }
	}
}
