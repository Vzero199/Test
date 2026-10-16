using UnityEngine;

public class FindInactiveObjects : MonoBehaviour
{
	public static GameObject FindObjectWithTagIncludingInactive(string tag)
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.CompareTag(tag))
			{
				return gameObject;
			}
		}
		return null;
	}
}
