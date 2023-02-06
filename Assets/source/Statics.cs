using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Earthgen
{
    public static partial class Statics
	{
		public static readonly RadianFloat pi = new(Mathf.PI);

		public static float min(float a, float b) => Math.Min(a, b);
		public static float max(float a, float b) => Math.Max(a, b);
		public static int pow(int x, int y) => (int)Math.Pow(x, y);
		public static float pow(float x, float y) => Mathf.Pow(x, y);
		public static float pow(double x, double y) => (float)Math.Pow(x, y);
		public static float sin(RadianFloat f) => Mathf.Sin(f);
		public static float cos(RadianFloat f) => Mathf.Cos(f);
		public static RadianFloat asin(float f) => new(Mathf.Asin(f));
		public static RadianFloat acos(float f) => new(Mathf.Acos(f));
		public static RadianFloat atan2(float y, float x) => new(Mathf.Atan2(y, x));

		public static Quaternion Quaternion() => UnityEngine.Quaternion.identity;
		public static Quaternion Quaternion(Vector3 from, Vector3 to) => UnityEngine.Quaternion.FromToRotation(from, to);
		public static Quaternion Quaternion(Vector3 axis, float angle) => UnityEngine.Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
		public static Quaternion conjugate(Quaternion q) => UnityEngine.Quaternion.Inverse(q);

		public static Vector3 normal(Vector3 v) => v.normalized;
		public static Vector3 Vector3(float x, float y, float z) => new(x, y, z);
		public static bool zero(Vector3 v) => v == UnityEngine.Vector3.zero;
		public static DegreeFloat angle(Vector3 from, Vector3 to) => new(UnityEngine.Vector3.Angle(from, to));
		public static float dot_product(Vector3 lhs, Vector3 rhs) => UnityEngine.Vector3.Dot(lhs, rhs);
		public static float distance(Vector3 from, Vector3 to) => (to - from).magnitude;
		public static float squared_distance(Vector3 from, Vector3 to) => (to - from).magnitude;

		public static Vector2 Vector2(float x, float y) => new(x, y);

		public static (T first, U second) make_pair<T, U>(T first, U second) => (first, second);
		public static void insert<T>(this ISet<T> set, T value) => set.Add(value);
		public static T begin<T>(this SortedSet<T> set) => set.Min;
		public static void push_back<T>(this List<T> list, T value) => list.Add(value);
		public static void erase<T>(this ICollection<T> collection, T value) => collection.Remove(value);
		public static void resize<T>(this List<T> list, int count, T defaultValue)
		{
			if (count > list.Count) {
				list.AddRange(Enumerable.Repeat(defaultValue, count - list.Count));
			} else if (count < list.Count) {
				list.RemoveRange(count, list.Count - count);
			}
		}
		public static int size<T>(this T[] array) => array.Length;
		public static int size<T>(this ICollection<T> c) => c.Count;
		public static int length(this string s) => s.Length;

		public static byte[] md5(string s) => MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(s));
	}

	public interface AngleFloat
	{
		public float Radians { get; }
		public float Degrees { get;}
	}
	public readonly struct RadianFloat : AngleFloat {
		private readonly float angle;
		public float Radians => angle;
		public float Degrees => angle * Mathf.Deg2Rad;
		public RadianFloat(float angleInRadians) => angle = angleInRadians;
		public static implicit operator RadianFloat(float f) => new(f);
		public static implicit operator RadianFloat(double d) => new((float)d);
		public static implicit operator float(RadianFloat r) => r.angle;
		public static implicit operator RadianFloat(DegreeFloat d) => new(d.Radians);

		public static RadianFloat operator +(RadianFloat a, RadianFloat b) => new(a.angle + b.angle);
		public static RadianFloat operator -(RadianFloat a, RadianFloat b) => new(a.angle - b.angle);
		public static RadianFloat operator +(RadianFloat a, DegreeFloat b) => new(a.angle + b.Radians);
		public static RadianFloat operator -(RadianFloat a, DegreeFloat b) => new(a.angle - b.Radians);
		public static RadianFloat operator *(RadianFloat a, float b) => new(a.angle * b);
		public static RadianFloat operator /(RadianFloat a, float b) => new(a.angle / b);
	}

	public readonly struct DegreeFloat : AngleFloat
	{
		private readonly float angle;
		public float Radians => angle * Mathf.Rad2Deg;
		public float Degrees => angle;
		public DegreeFloat(float angleInDegrees) => angle = angleInDegrees;
		public static implicit operator DegreeFloat(float f) => new(f);
		public static implicit operator DegreeFloat(double d) => new((float)d);
		public static implicit operator float(DegreeFloat d) => d.angle;
		public static implicit operator DegreeFloat(RadianFloat r) => new(r.Degrees);

		public static DegreeFloat operator +(DegreeFloat a, DegreeFloat b) => new(a.angle + b.angle);
		public static DegreeFloat operator -(DegreeFloat a, DegreeFloat b) => new(a.angle - b.angle);
		public static DegreeFloat operator +(DegreeFloat a, RadianFloat b) => new(a.angle + b.Degrees);
		public static DegreeFloat operator -(DegreeFloat a, RadianFloat b) => new(a.angle - b.Degrees);
		public static DegreeFloat operator *(DegreeFloat a, float b) => new(a.angle * b);
		public static DegreeFloat operator /(DegreeFloat a, float b) => new(a.angle / b);
	}
}
