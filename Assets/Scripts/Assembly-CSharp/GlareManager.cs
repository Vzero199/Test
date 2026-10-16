using UnityEngine;

public class GlareManager : MonoBehaviour
{
	private GlareObject[] objectsToGlare;

	private void Awake()
	{
		objectsToGlare = FindAllGlareObjects();
		InitializeDoorsInRoom(objectsToGlare);
	}

	private GlareObject[] FindAllGlareObjects()
	{
		return Object.FindObjectsOfType<GlareObject>();
	}

	private void InitializeDoorsInRoom(GlareObject[] objectsToGlare)
	{
		if (objectsToGlare == null || objectsToGlare.Length == 0)
		{
			Debug.LogError("objectsToGlare не инициализирован или пустой!");
			return;
		}
		foreach (GlareObject glareObject in objectsToGlare)
		{
			if (glareObject != null)
			{
				GameObject obj = new GameObject(glareObject.name + " Glare");
				obj.transform.SetParent(glareObject.transform);
				obj.transform.localPosition = new Vector3(0f, 0f - glareObject.transform.localScale.y, 0f);
				obj.transform.localScale = Vector3.one;
				SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
				SpriteRenderer component = glareObject.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					spriteRenderer.sprite = component.sprite;
					spriteRenderer.flipY = true;
				}
				else
				{
					Debug.LogWarning("SpriteRenderer не найден на объекте " + glareObject.name);
				}
			}
		}
	}
}
