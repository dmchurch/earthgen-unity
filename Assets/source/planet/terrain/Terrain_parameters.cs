﻿using System;
using UnityEngine;

using static Earthgen.Statics;

namespace Earthgen.planet.terrain
{
    [Serializable]
	public struct Terrain_parameters
	{
		public static readonly Terrain_parameters Default = new()
		{
			grid_size = 6,
			axis = Terrain.default_axis(),
			iterations = 1000,
			water_ratio = 0.65f,
		};

		[Range(0,10)]
		public int grid_size;
		public Vector3 axis;
		public string seed;
		public int iterations;
		[Range(0,1)]
		public float water_ratio;

		public void set_default() => this = Default;
		public void correct_values()
		{
			grid_size = Mathf.Clamp(grid_size, 0, 10);

			axis = zero(axis) ? Terrain.default_axis() : normal(axis);
			iterations = Mathf.Max(iterations, 0);
			water_ratio = Mathf.Clamp(water_ratio, 0, 1);
		}
	}

}
