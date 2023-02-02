using Earthgen.planet.grid;

namespace Earthgen.planet.terrain
{
    public readonly struct River
    {
        public readonly Corner source;
        public readonly Corner direction;
        public readonly Edge channel;

        public River(Corner source, Corner direction, Edge channel)
        {
            this.source = source;
            this.direction = direction;
            this.channel = channel;
        }
    }

    public static partial class PlanetExtensions
    {
        public static bool has_river(this Planet p, Edge e)
        {
            Corner first = e.nth_corner(0);
            Corner second = e.nth_corner(1);
	        if (p.terrain.nth_corner(first.id).river_direction == first.position(second)) return true;
	        if (p.terrain.nth_corner(second.id).river_direction == second.position(first)) return true;
            return false;
        }
        public static River river(this Planet p, Edge e)
        {
	        Corner first = e.nth_corner(0);
	        Corner second = e.nth_corner(1);
	        if (p.terrain.nth_corner(first.id).river_direction == first.position(second)) {
                return new River(first, second, e);
	        }
	        else if (p.terrain.nth_corner(second.id).river_direction == second.position(first)) {
                return new River(second, first, e);
	        }
	        return new River(null, null, e);
        }
        public static River river(this Planet p, Corner c)
        {
	        var source = c;
	        var direction = c.nth_corner(p.terrain.nth_corner(c.id).river_direction);
	        var channel = c.nth_edge(p.terrain.nth_corner(c.id).river_direction);
	        return new River(source, direction, channel);
        }
        public static Corner left_tributary(this Planet p, River r)
        {
	        Corner c = null;
	        int pos = r.source.position(r.direction);
	        Corner t = r.source.nth_corner(pos+1);
	        if (p.terrain.nth_corner(t.id).river_direction == t.position(r.source))
		        c = t;
	        return c;
        }
        public static Corner right_tributary(this Planet p, River r)
        {
	        Corner c = null;
	        int pos = r.source.position(r.direction);
	        Corner t = r.source.nth_corner(pos-1);
	        if (p.terrain.nth_corner(t.id).river_direction == t.position(r.source))
		        c = t;
	        return c;
        }
    }
}