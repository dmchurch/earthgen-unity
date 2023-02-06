using System;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_corner
    {
        public static readonly Terrain_corner Default = new()
        {
            elevation = 0,
            river_direction = -1,
            distance_to_sea = -1,
            type = Type.Land,
        };
        public float elevation;
        public int river_direction;
        public int distance_to_sea;
        public Type type;

        public enum Type
        {
            Land = 1,
            Water = 2,
            Coast = 4,
        }

        public bool is_land => type == Type.Land;
        public bool is_water => type == Type.Water;
        public bool is_coast => type == Type.Coast;
    }

}


namespace Earthgen
{
    using Earthgen.planet.terrain;
    public static partial class Statics
    {
        public static bool is_land (Terrain_corner c) => c.is_land;
        public static bool is_water (Terrain_corner c) => c.is_water;
        public static bool is_coast (Terrain_corner c) => c.is_coast;

        public static float elevation (Terrain_corner c) => c.elevation;
        public static int river_direction (Terrain_corner c) => c.river_direction;
        public static int distance_to_sea (Terrain_corner c) => c.distance_to_sea;

    }
}