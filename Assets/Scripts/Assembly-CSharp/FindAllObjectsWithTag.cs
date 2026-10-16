using System.Collections.Generic;
using UnityEngine;

public class FindAllObjectsWithTag : MonoBehaviour
{
	public static List<GameObject> GetAllObjectsWithTag(string tag)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.CompareTag(tag))
			{
				list.Add(gameObject);
			}
		}
		return list;
	}
}
