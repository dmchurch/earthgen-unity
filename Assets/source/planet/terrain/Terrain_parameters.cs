using System;
using UnityEngine;

namespace Earthgen.planet.terrain
{
	[Serializable]
    public struct Terrain_parameters : IEquatable<Terrain_parameters>
	{
		public static readonly Terrain_parameters Default = new Terrain_parameters().set_default();

		[Range(0, 10), Tooltip("Number of subdivisions of icosahedron")]
		public int grid_size;
		public Vector3 axis;
		[Tooltip("Any text string")]
		public string seed;
		[Range(1,5000), Tooltip("Number of times to raise elevations")]
		public int iterations;
		[Range(0, 1), Tooltip("Percentage of surface to cover with water")]
		public double water_ratio;

		public Terrain_parameters set_default() {
			grid_size = 6;
			axis = Terrain.default_axis();
			iterations = 1000;
			water_ratio = 0.65;
			return this;
		}

		public void correct_values () {
			grid_size = Math.Clamp(grid_size, 0, 10);

			if (axis == Vector3.zero)
				axis = new Vector3(0,0,1);
			else {
				axis = axis.normalized;
			}

			iterations = Math.Max(0, iterations);

			water_ratio = Math.Clamp(water_ratio, 0, 1);
		}

		public override bool Equals(object obj) => obj is Terrain_parameters parameters && Equals(parameters);
		public bool Equals(Terrain_parameters other) => grid_size == other.grid_size && axis.Equals(other.axis) && seed == other.seed && iterations == other.iterations && water_ratio == other.water_ratio;
		public override int GetHashCode() => HashCode.Combine(grid_size, axis, seed, iterations, water_ratio);

		public static bool operator ==(Terrain_parameters left, Terrain_parameters right) => left.Equals(right);
		public static bool operator !=(Terrain_parameters left, Terrain_parameters right) => !(left == right);
	}
}