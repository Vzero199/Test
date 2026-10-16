using System;
using UnityEngine;

public static class ExtensionMethods
{
	public static Vector2 Round(this Vector2 vector2, int decimalPlaces = 2)
	{
		float num = 1f;
		for (int i = 0; i < decimalPlaces; i++)
		{
			num *= 10f;
		}
		return new Vector2(Mathf.Round(vector2.x * num) / num, Mathf.Round(vector2.y * num) / num);
	}

	public static Vector3 Round(this Vector3 vector, int decimalPlaces = 2)
	{
		float num = Mathf.Pow(10f, decimalPlaces);
		return new Vector3(Mathf.Round(vector.x * num) / num, Mathf.Round(vector.y * num) / num, Mathf.Round(vector.z * num) / num);
	}

	public static Transform[] GetChilds(this Transform transform)
	{
		if (transform == null)
		{
			Debug.LogError("Transform is null.");
			return new Transform[0];
		}
		int childCount = transform.childCount;
		Transform[] array = new Transform[childCount];
		for (int i = 0; i < childCount; i++)
		{
			array[i] = transform.GetChild(i);
		}
		return array;
	}

	public static bool HasScript(this GameObject gameObject, string scriptName)
	{
		Type type = Type.GetType(scriptName);
		if (type != null)
		{
			return gameObject.GetComponent(type) != null;
		}
		Debug.LogError("Failed to find the type of script: " + scriptName);
		return false;
	}

	public static Vector3 RoundToNearestHalf(this Vector3 v)
	{
		v.x = Mathf.Round(v.x * 2f) / 2f;
		v.y = Mathf.Round(v.y * 2f) / 2f;
		v.z = Mathf.Round(v.z * 2f) / 2f;
		return v;
	}
}
