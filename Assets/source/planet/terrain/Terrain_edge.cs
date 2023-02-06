using Earthgen.planet.terrain;
using System;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_edge
    {
        public static readonly Terrain_edge Default = new() { type = Type.Land };
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
    public static partial class Statics
    {
        public static bool is_land(this Terrain_edge e) => e.is_land;
        public static bool is_water(this Terrain_edge e) => e.is_water;
        public static bool is_coast(this Terrain_edge e) => e.is_coast;
    }
}