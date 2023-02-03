using System;
using UnityEngine;

namespace Earthgen.planet.climate
{
	[Serializable]
	public struct Climate_parameters : IEquatable<Climate_parameters>
	{
		public static readonly Climate_parameters Default = new()
		{
			seasons = 1,
			axial_tilt = 0.4,
			error_tolerance = 0.01f,
		};
		
		public void set_default()
		{
			this = Default;
		}

		public void correct_values()
		{
			seasons = Math.Max(1, seasons);
			axial_tilt = Math.Clamp(axial_tilt, 0, Math.PI / 2);
			error_tolerance = Math.Clamp(error_tolerance, 0.001f, 1.0f);
		}

		public int seasons;
		[Range(0, Mathf.PI / 2)]
		public double axial_tilt;
		[Range(0.001f, 1)]
		public float error_tolerance;

        #region Boilerplate (Equals, GetHashCode, ==, !=)
        public override bool Equals(object obj) => obj is Climate_parameters parameters && Equals(parameters);
		public bool Equals(Climate_parameters other) => seasons == other.seasons && axial_tilt == other.axial_tilt && error_tolerance == other.error_tolerance;
		public override int GetHashCode() => HashCode.Combine(seasons, axial_tilt, error_tolerance);

		public static bool operator ==(Climate_parameters left, Climate_parameters right) => left.Equals(right);
		public static bool operator !=(Climate_parameters left, Climate_parameters right) => !(left == right);
        #endregion
    }
}
