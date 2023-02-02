using System;
using UnityEngine;

namespace Earthgen.planet.grid
{
    public class Edge : IComparable<Edge>
	{
		public static Edge New(int id) => new Edge().Init(id);
		public Edge Init(int i)
		{
			id = i;
			return this;
		}

		public int position(Tile t)
		{
			if (tiles[0] == t)
				return 0;
			else if (tiles[1] == t)
				return 1;
			return -1;
		}
		public int position(Corner c)
		{
			if (corners[0] == c)
				return 0;
			else if (corners[1] == c)
				return 1;
			return -1;
		}

		public int sign(Tile t)
		{
			if (tiles[0] == t)
				return 1;
			else if (tiles[1] == t)
				return -1;
			return 0;
		}
		public int sign(Corner c)
		{
			if (corners[0] == c)
				return 1;
			else if (corners[1] == c)
				return -1;
			return 0;
		}

		public int id;
		public Tile[] tiles = new Tile[2];
		public Corner[] corners = new Corner[2];

		public Tile nth_tile(int i)
		{
			return tiles[i];
		}
		public Corner nth_corner(int i)
		{
			return corners[i];
		}

		public int CompareTo(Edge other) => id.CompareTo(other.id);
	}
}
