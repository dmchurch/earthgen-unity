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
		public static readonly AngleFloat pi = AngleFloat.FromRadians(Mathf.PI);

		public static float min(float a, float b) => Math.Min(a, b);
		public static float max(float a, float b) => Math.Max(a, b);
		public static float abs(float f) => Math.Abs(f);
		public static int pow(int x, int y) => (int)Math.Pow(x, y);
		public static float pow(float x, float y) => Mathf.Pow(x, y);
		public static float pow(double x, double y) => (float)Math.Pow(x, y);
		public static float exp(float f) => Mathf.Exp(f);
		public static float exp(double d) => (float)Math.Exp(d);
		public static float sin(AngleFloat f) => Mathf.Sin(f.Radians);
		public static float cos(AngleFloat f) => Mathf.Cos(f.Radians);
		public static AngleFloat asin(float f) => AngleFloat.FromRadians(Mathf.Asin(f));
		public static AngleFloat acos(float f) => AngleFloat.FromRadians(Mathf.Acos(f));
		public static AngleFloat atan2(float y, float x) => AngleFloat.FromRadians(Mathf.Atan2(y, x));
		public static AngleFloat atan2(double y, double x) => AngleFloat.FromRadians((float)Math.Atan2(y, x));

		public static Quaternion Quaternion() => UnityEngine.Quaternion.identity;
		public static Quaternion Quaternion(Vector3 from, Vector3 to) => UnityEngine.Quaternion.FromToRotation(from, to);
		public static Quaternion Quaternion(Vector3 axis, AngleFloat angle) => UnityEngine.Quaternion.AngleAxis(angle.Degrees, axis);
		public static Quaternion conjugate(Quaternion q) => UnityEngine.Quaternion.Inverse(q);

		public static Vector3 normal(Vector3 v) => v.normalized;
		public static Vector3 Vector3(float x, float y, float z) => new(x, y, z);
		public static bool zero(Vector3 v) => v == UnityEngine.Vector3.zero;
		public static AngleFloat angle(Vector3 from, Vector3 to) => AngleFloat.FromDegrees(UnityEngine.Vector3.Angle(from, to));
		public static float dot_product(Vector3 lhs, Vector3 rhs) => UnityEngine.Vector3.Dot(lhs, rhs);
		public static float distance(Vector3 from, Vector3 to) => (to - from).magnitude;
		public static float squared_distance(Vector3 from, Vector3 to) => (to - from).magnitude;

		public static Vector2 Vector2(float x, float y) => new(x, y);
		public static Vector2 Vector2(double x, double y) => new((float)x, (float)y);
		public static float length(Vector2 v) => v.magnitude;
		public static AngleFloat angle(Vector2 v) => atan2(v.y, v.x);
		public static Quaternion rotation_matrix(AngleFloat angle) => Quaternion(Vector3(0,0,1), angle);

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
}
