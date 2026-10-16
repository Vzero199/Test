using System;
using UnityEngine;

[Serializable]
public class ItemSpawnSettings
{
	public enum SpawnCategory
	{
		Normal = 0,
		Special = 1
	}

	public GameObject itemPrefab;

	[Min(0f)]
	public float weight;

	public SpawnCategory spawnCategory;

	[HideInInspector]
	public float normalizedChance;
}
