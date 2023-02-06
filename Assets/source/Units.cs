using System;
using UnityEngine;

namespace Earthgen
{
	public readonly struct AngleFloat : IComparable<AngleFloat>
	{
		private readonly float angle;
		public float Radians => angle;
		public float Degrees => angle * Mathf.Rad2Deg;
		private AngleFloat(float angleInRadians) => angle = angleInRadians;
		public static explicit operator AngleFloat(float f) => new(f);
		public static explicit operator float(AngleFloat r) => r.angle;
		internal static AngleFloat FromRadians(float v) => new(v);
		internal static AngleFloat FromDegrees(float v) => new(v * Mathf.Deg2Rad);

		public int CompareTo(AngleFloat other) => angle.CompareTo(other.angle);

		public static AngleFloat operator +(AngleFloat a, AngleFloat b) => new(a.angle + b.angle);
		public static AngleFloat operator -(AngleFloat a, AngleFloat b) => new(a.angle - b.angle);
		public static AngleFloat operator *(double a, AngleFloat b) => new(b.angle * (float)a);
		public static AngleFloat operator *(AngleFloat a, float b) => new(a.angle * b);
		public static AngleFloat operator /(AngleFloat a, double b) => new(a.angle / (float)b);
		public static float operator /(AngleFloat a, AngleFloat b) => a.angle / b.angle;
		public static bool operator <(AngleFloat left, AngleFloat right) => left.CompareTo(right) < 0;
		public static bool operator <=(AngleFloat left, AngleFloat right) => left.CompareTo(right) <= 0;
		public static bool operator >(AngleFloat left, AngleFloat right) => left.CompareTo(right) > 0;
		public static bool operator >=(AngleFloat left, AngleFloat right) => left.CompareTo(right) >= 0;
	}


	public readonly struct TemperatureFloat
	{
		private readonly float temperature;
		public float Kelvins => temperature;
		public float Celsius => temperature - 273.15f;

		public static explicit operator TemperatureFloat(float f) => new(f);
		public static explicit operator float(TemperatureFloat k) => k.temperature;
		private TemperatureFloat(float temperatureInKelvins) => temperature = temperatureInKelvins;

		public static TemperatureFloat FromKelvins(float k) => new(k);
		public static TemperatureFloat FromDegreesC(float c) => new(c + 273.15f);

		public static TemperatureFloat operator +(TemperatureFloat a, TemperatureFloat b) => new(a.temperature + b.temperature);
		public static TemperatureFloat operator -(TemperatureFloat a, TemperatureFloat b) => new(a.temperature - b.temperature);
		public static TemperatureFloat operator +(TemperatureFloat a, float b) => new(a.temperature + b);
		public static TemperatureFloat operator -(TemperatureFloat a, float b) => new(a.temperature - b);
		public static TemperatureFloat operator +(float a, TemperatureFloat b) => new(a + b.temperature);
		public static TemperatureFloat operator *(TemperatureFloat a, float b) => new(a.temperature * b);
		public static TemperatureFloat operator *(double a, TemperatureFloat b) => new((float)a * b.temperature);
		public static TemperatureFloat operator /(TemperatureFloat a, float b) => new(a.temperature / b);
	}

}
