using System;

namespace Earthgen.planet.climate
{
    [Serializable]
    public class Season
    {
        public Climate_tile[] tiles;
        public Climate_corner[] corners;
        public Climate_edge[] edges;
    }
}

namespace Earthgen
{
    using planet.climate;
    public static partial class Statics
    {
        public static Climate_tile[] tiles (Season s) => s.tiles;
        public static Climate_corner[] corners (Season s) => s.corners;
        public static Climate_edge[] edges (Season s) => s.edges;

        public static Climate_tile nth_tile (Season s, int n) => s.tiles[n];
        public static Climate_corner nth_corner (Season s, int n) => s.corners[n];
        public static Climate_edge nth_edge (Season s, int n) => s.edges[n];

        public static ref Climate_tile m_tile (Season s, int n) => ref s.tiles[n];
        public static ref Climate_corner m_corner (Season s, int n) => ref s.corners[n];
        public static ref Climate_edge m_edge (Season s, int n) => ref s.edges[n];

    }
}
