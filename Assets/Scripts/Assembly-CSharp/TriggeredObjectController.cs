using UnityEngine;

public class TriggeredObjectController : MonoBehaviour
{
	private GameObject spawnedObject;

	public void Initialize(GameObject spawnPrefab)
	{
		spawnedObject = spawnPrefab;
	}

	public void ShowHair()
	{
		if (spawnedObject != null)
		{
			spawnedObject.SetActive(value: true);
			LineRenderer[] componentsInChildren = spawnedObject.GetComponentsInChildren<LineRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}
}
