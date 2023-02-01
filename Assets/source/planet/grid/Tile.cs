using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Earthgen.planet.grid
{
    public class Tile : ScriptableObject
	{
		public static Tile New(int id, int edge_count) => CreateInstance<Tile>().Init(id, edge_count);
		public Tile Init(int i, int e)
		{
			id = i;
			edge_count = e;
			tiles.Resize(edge_count, null);
			corners.Resize(edge_count, null);
			edges.Resize(edge_count, null);
			return this;
		}

		public int position(Tile n)
		{
			for (int i=0; i<edge_count; i++)
				if (tiles[i] == n)
					return i;
			return -1;
		}
        public int position(Corner c)
		{
			for (int i=0; i<edge_count; i++)
				if (corners[i] == c)
					return i;
			return -1;
		}
		public int position(Edge e)
		{
			for (int i=0; i<edge_count; i++)
				if (edges[i] == e)
					return i;
			return -1;
		}

		public int id;
		public int edge_count;
		public Vector3 v;
		public List<Tile> tiles = new();
		public List<Corner> corners = new();
		public List<Edge> edges = new();

		public Tile nth_tile(int n)
		{
			int k = n < 0 ?
				n % edge_count + edge_count :
				n % edge_count;
			return tiles[k];
		}

		public Corner nth_corner(int n)
		{
			int k = n < 0 ?
				n % edge_count + edge_count :
				n % edge_count;
			return corners[k];
		}

		public Edge nth_edge(int n)
		{
			int k = n < 0 ?
				n % edge_count + edge_count :
				n % edge_count;
			return edges[k];
		}

        public Quaternion reference_rotation(Quaternion d)
		{
			Vector3 v = d * this.v;
			Quaternion h = Quaternion.identity;
			if (v.x != 0 || v.y != 0) {
				if (v.y != 0) h = Quaternion.FromToRotation(new Vector3(v.x, v.y, 0).normalized, new Vector3(-1,0,0));
				else if (v.x > 0) h = Quaternion.AngleAxis(Mathf.PI, new Vector3(0,0,1));
			}
			Quaternion q = Quaternion.identity;
			if (v.x == 0 && v.y == 0) {
				if (v.z < 0) q = Quaternion.AngleAxis(Mathf.PI, new Vector3(1,0,0));
			}
			else {
				q = Quaternion.FromToRotation(h*v, new Vector3(0,0,1));
			}
			return q*h*d;
		}

        public List<Vector2> polygon(Quaternion d)
		{
			List<Vector2> p = new();
			Quaternion q = reference_rotation(d);
			for (int i=0; i<edge_count; i++) {
				Vector3 c = q * nth_corner(i).v;
				p.Add(new Vector2(c.x, c.y));
			}
			return p;
		}
	}


	public static class ListExtensions
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
	}
}
