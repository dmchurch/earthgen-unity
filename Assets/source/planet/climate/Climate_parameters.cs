using System;

using static Earthgen.Statics;

namespace Earthgen.planet.climate
{
    [Serializable]
	public struct Climate_parameters
	{
		public static readonly Climate_parameters Default = new()
		{
			seasons = 1,
			axial_tilt = AngleFloat.FromRadians(0.4f),
			error_tolerance = 0.01f,
		};

		public int seasons;
		public float axialTiltInDegrees;
		public AngleFloat axial_tilt
		{
			get => AngleFloat.FromDegrees(axialTiltInDegrees);
			set => axialTiltInDegrees = value.Degrees;
		}
		public float error_tolerance;

		public void set_default() => this = Default;

		public void correct_values()
		{
			seasons = Math.Max(seasons, 1);
			axial_tilt = AngleFloat.FromRadians(Math.Clamp(axial_tilt.Radians, 0, pi.Radians/2));
			error_tolerance = Math.Clamp(error_tolerance, 0.001f, 1.0f);
		}
	}
}
