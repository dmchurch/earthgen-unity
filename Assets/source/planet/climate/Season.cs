using System;
using System.Collections.Generic;
using UnityEngine;

namespace Earthgen.planet.climate
{
	[Serializable]
    public class Season
    {
        public Climate_tile[] tiles;
        public Climate_corner[] corners;
        public Climate_edge[] edges;

        public Climate_tile nth_tile(int n) => tiles[n];
        public Climate_corner nth_corner(int n) => corners[n];
        public Climate_edge nth_edge(int n) => edges[n];

        public ref Climate_tile m_tile(int n) => ref tiles[n];
        public ref Climate_corner m_corner(int n) => ref corners[n];
        public ref Climate_edge m_edge(int n) => ref edges[n];
    }
}