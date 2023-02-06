using System;
using Earthgen.planet.grid;
using Earthgen.planet.terrain;

using static Earthgen.Statics;

namespace Earthgen.planet.terrain
{
	[Serializable]
	public struct River
	{
		public Corner source;
		public Corner direction;
		public Edge channel;
	}

	public static partial class PlanetExtensions
	{
		// for disambiguation
		private static Terrain terrain(Planet p) => p.terrain;

		public static bool has_river (this Planet p, Edge e) {
			Corner first = nth_corner(e, 0);
			Corner second = nth_corner(e, 1);
			if (river_direction(nth_corner(terrain(p), first.id)) == position(first, second)) return true;
			if (river_direction(nth_corner(terrain(p), second.id)) == position(second, first)) return true;
			return false;
		}

		public static River river (this Planet p, Edge e) {
			River r = default;
			r.channel = e;
			Corner first = nth_corner(e, 0);
			Corner second = nth_corner(e, 1);
			if (river_direction(nth_corner(terrain(p), first.id)) == position(first, second)) {
				r.source = first;
				r.direction = second;
			}
			else if (river_direction(nth_corner(terrain(p), second.id)) == position(second, first)) {
				r.source = second;
				r.direction = first;
			}
			return r;
		}

		public static River river (this Planet p, Corner c) {
			River r;
			r.source = c;
			r.direction = nth_corner(c, river_direction(nth_corner(terrain(p), c.id)));
			r.channel = nth_edge(c, river_direction(nth_corner(terrain(p), c.id)));
			return r;
		}

		public static Corner left_tributary (this Planet p, River r) {
			Corner c = null;
			int pos = position(r.source, r.direction);
			Corner t = nth_corner(r.source, pos+1);
			if (river_direction(nth_corner(terrain(p), t.id)) == position(t, r.source))
				c = t;
			return c;
		}

		public static Corner right_tributary (this Planet p, River r) {
			Corner c = null;
			int pos = position(r.source, r.direction);
			Corner t = nth_corner(r.source, pos-1);
			if (river_direction(nth_corner(terrain(p), t.id)) == position(t, r.source))
				c = t;
			return c;
		}
	}
}

