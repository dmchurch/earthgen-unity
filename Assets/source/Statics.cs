using System;
using System.Collections.Generic;
using UnityEngine;

namespace Earthgen
{
    public static partial class Statics
	{
		public const float pi = Mathf.PI;

		public static int pow(int x, int y) => (int)Math.Pow(x, y);
		public static Quaternion Quaternion() => UnityEngine.Quaternion.identity;
		public static Quaternion Quaternion(Vector3 from, Vector3 to) => UnityEngine.Quaternion.FromToRotation(from, to);
		public static Quaternion Quaternion(Vector3 axis, float angle) => UnityEngine.Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);

		public static Vector3 normal(Vector3 v) => v.normalized;
		public static Vector3 Vector3(float x, float y, float z) => new(x, y, z);

		public static Vector2 Vector2(float x, float y) => new(x, y);

		public static void push_back<T>(this List<T> list, T value) => list.Add(value);
		public static int size<T>(this T[] array) => array.Length;
		public static int size<T>(this List<T> list) => list.Count;
	}
}
