using Earthgen.planet;
using System;
using UnityEngine;

namespace Earthgen.planet.terrain
{
    [Serializable]
    public struct Terrain_variables
    {
        public int grid_size;
        public Vector3 axis;
		public float axialTiltInDegrees;
		public AngleFloat axial_tilt
		{
			get => AngleFloat.FromDegrees(axialTiltInDegrees);
			set => axialTiltInDegrees = value.Degrees;
		}
        public double radius;
        public double sea_level;
    }

    public static partial class PlanetExtensions
    {
        public static Vector3 axis (this Planet p) => p.terrain.var.axis;
        public static AngleFloat axial_tilt (this Planet p) => p.terrain.var.axial_tilt;
        public static double radius (this Planet p) => p.terrain.var.radius;
        public static double sea_level (this Planet p) => p.terrain.var.sea_level;
    }
}

namespace Earthgen
{
    public static partial class Statics {
        public static Vector3 axis (Planet p) => planet.terrain.PlanetExtensions.axis(p);
        public static AngleFloat axial_tilt (Planet p) => planet.terrain.PlanetExtensions.axial_tilt(p);
        public static float radius (Planet p) => (float)planet.terrain.PlanetExtensions.radius(p);
        public static float sea_level (Planet p) => (float)planet.terrain.PlanetExtensions.sea_level(p);
    }
}
