using System.Collections.Generic;
using UnityEngine;

public class NurseSpawner : MonoBehaviour
{
	public Sprite newSprite;

	public ProgressBar hornyBar;

	public Sprite bedNewSprite;

	public GameObject prefabToSpawn;

	public GameObject additionalSpritePrefab;

	public Vector2 spawnOffset = new Vector2(104f, 69f);

	public BaseEnemySettings Settings;

	private SpriteRenderer spriteRenderer;

	private SpriteRenderer bedSpriteRenderer;

	private Sprite originalSprite;

	private Sprite originalBedSprite;

	private GameObject additionalSpriteInstance;

	private GameObject prefabInstance;

	private SisSupEnemy prefabScript;

	private List<GameObject> bedChildObjects = new List<GameObject>();

	public void Initialize()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		originalSprite = spriteRenderer.sprite;
		Transform transform = base.transform.Find("Bed");
		if (transform != null)
		{
			bedSpriteRenderer = transform.GetComponent<SpriteRenderer>();
			originalBedSprite = bedSpriteRenderer.sprite;
			DisableBedChildren(transform);
		}
		else
		{
			Debug.LogError("Объект 'Bed' не найден среди дочерних объектов.");
		}
		ChangeSprites();
		SpawnPrefab();
		SpawnAdditionalSprite();
	}

	public void SetPrefabFields(float moveSpeed, float interactionSpeed, float interactionCooldown, float resistanceTime, float viewDistance, GameObject shinePrefab, GameObject keyObjectPrefab, float hornyTime)
	{
		prefabScript.CurrentMoveSpeed = moveSpeed;
		prefabScript.CurrentInteractionSpeed = interactionSpeed;
		prefabScript.InteractionCooldown = interactionCooldown;
		prefabScript.resistanceTime = resistanceTime;
		prefabScript.ViewDistance = viewDistance;
		prefabScript.ShinePrefab = shinePrefab;
		prefabScript.KeyObjectPrefab = keyObjectPrefab;
		prefabScript.HornyTime = hornyTime;
	}

	private void ChangeSprites()
	{
		spriteRenderer.sprite = newSprite;
		if (bedSpriteRenderer != null)
		{
			bedSpriteRenderer.sprite = bedNewSprite;
		}
	}

	private void SpawnPrefab()
	{
		if (bedSpriteRenderer != null)
		{
			Vector3 position = bedSpriteRenderer.transform.position + (Vector3)spawnOffset;
			prefabInstance = Object.Instantiate(prefabToSpawn, position, Quaternion.identity, bedSpriteRenderer.transform);
			prefabScript = prefabInstance.GetComponent<SisSupEnemy>();
			prefabScript.HornyBar = hornyBar;
			prefabScript.Settings = Settings;
		}
	}

	private void SpawnAdditionalSprite()
	{
		if (bedSpriteRenderer != null)
		{
			Vector3 position = bedSpriteRenderer.transform.position;
			additionalSpriteInstance = Object.Instantiate(additionalSpritePrefab, position, Quaternion.identity, bedSpriteRenderer.transform);
			SpriteRenderer component = additionalSpriteInstance.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				component.sortingOrder = 57;
			}
		}
	}

	private void DisableBedChildren(Transform bedTransform)
	{
		foreach (Transform item in bedTransform)
		{
			if (item.gameObject.activeSelf)
			{
				bedChildObjects.Add(item.gameObject);
				item.gameObject.SetActive(value: false);
			}
		}
	}

	private void EnableBedChildren()
	{
		foreach (GameObject bedChildObject in bedChildObjects)
		{
			bedChildObject.SetActive(value: true);
		}
	}

	private void OnDestroy()
	{
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = originalSprite;
		}
		if (bedSpriteRenderer != null)
		{
			bedSpriteRenderer.sprite = originalBedSprite;
		}
		if (additionalSpriteInstance != null)
		{
			Object.Destroy(additionalSpriteInstance);
		}
		if (prefabInstance != null)
		{
			Object.Destroy(prefabInstance);
		}
		EnableBedChildren();
	}
}
