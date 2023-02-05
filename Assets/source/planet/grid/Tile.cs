using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Earthgen.planet.grid
{
    public class Tile : IComparable<Tile>
	{
		public static Tile New(int id, int edge_count) => new Tile().Init(id, edge_count);
		public Tile Init(int i, int e)
		{
			id = i;
			edge_count = e;
			tiles = tiles.Resize(edge_count, null);
			corners = corners.Resize(edge_count, null);
			edges = edges.Resize(edge_count, null);
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
		public Tile[] tiles = new Tile[0];
		public Corner[] corners = new Corner[0];
		public Edge[] edges = new Edge[0];

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
//			Debug.Log($"getting reference rotation for tile {id} with v {this.v} and rotation {d}, rotated {v}");
			Quaternion h = Quaternion.identity;
			if (v.x != 0 || v.z != 0) { // If this tile is not at one of the poles;
				if (v.z != 0) { // if it has any forward/back component;
					// rotate around to put the tile on the -x semicircle
					h = Quaternion.FromToRotation(new Vector3(v.x, 0, v.z).normalized, new Vector3(-1,0,0));
				}
				else if (v.x > 0) { // if it is on the +x semicircle;
					// rotate 180 degrees around the poles to put it on -x
					h = Quaternion.AngleAxis(180, new Vector3(0,1,0));
				}
			}
			// at this point, the rotated vector h*d is either one of the poles or on the -x semicircle;
			// the z component is 0
			Quaternion q = Quaternion.identity;
			if (v.x == 0 && v.z == 0) { // If this tile is at one of the poles;
				if (v.y < 0) { // If it's the south pole;
					// rotate it around the x axis to put it at the north pole
					q = Quaternion.AngleAxis(180, new Vector3(1,0,0));
				}
			}
			else { // Otherwise it's on the -x semicircle;
				// So use quaternions to put it directly at the north pole
				q = Quaternion.FromToRotation(h*v, new Vector3(0,1,0));
			}
			return q*h*d;
		}

        public List<Vector2> polygon(Quaternion d)
		{
			List<Vector2> p = new();
			Quaternion q = reference_rotation(d);
			for (int i=0; i<edge_count; i++) {
				Vector3 c = q * nth_corner(i).v;
				p.Add(new Vector2(c.x, c.z));
			}
			//Debug.Log($"polygon for tile {id} with v {v} at rotation {d}: {string.Join(", ", p)}");
			return p;
		}

		public int CompareTo(Tile other) => id.CompareTo(other.id);
	}
}
