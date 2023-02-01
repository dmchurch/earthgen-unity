using System.Collections.Generic;
using UnityEngine;

namespace Earthgen.planet.grid
{
    public class Corner : ScriptableObject
	{
		public static Corner New(int id) => CreateInstance<Corner>().Init(id);
		public Corner Init(int i)
		{
			id = i;
			return this;
		}

		public int position(Tile t)
		{
			for (int i=0; i<3; i++)
				if (tiles[i] == t)
					return i;
			return -1;
		}
		public int position(Corner n)
		{
			for (int i=0; i<3; i++)
				if (corners[i] == n)
					return i;
			return -1;
		}
		public int position(Edge e)
		{
			for (int i=0; i<3; i++)
				if (edges[i] == e)
					return i;
			return -1;
		}

		public int id;
		public Vector3 v;
		public Tile[] tiles = new Tile[3];
		public Corner[] corners = new Corner[3];
		public Edge[] edges = new Edge[3];

		public Corner nth_corner(int i)
		{
			int k = i < 0 ?
				i%3 + 3 :
				i%3;
			return corners[k];
		}

		public Edge nth_edge(int i)
		{
			int k = i < 0 ?
				i%3 + 3 :
				i%3;
			return edges[k];
		}
	}
}
