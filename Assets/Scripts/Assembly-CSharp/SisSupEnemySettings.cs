using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SisSupEnemySettings", menuName = "EnemySettings/SisSupEnemySettings")]
public class SisSupEnemySettings : BaseEnemySettings
{
	public Sprite newSprite;

	public GameObject hornyBar;

	public Sprite bedNewSprite;

	public GameObject prefabToSpawn;

	public GameObject additionalSpritePrefab;

	public Vector2 spawnOffset;

	public GameObject shinePrefab;

	public GameObject keyObjectPrefab;

	public float hornyTime;

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)> { 
		{
			"Spawn Priority",
			(1f, 10f, initialPriority)
		} };
	}
}
