using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TVEnemySettings", menuName = "EnemySettings/TVEnemySettings")]
public class TVEnemySettings : BaseEnemySettings
{
	public GameObject spawnPrefab;

	public RuntimeAnimatorController newAnimatorController;

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)> { 
		{
			"Spawn Priority",
			(1f, 10f, initialPriority)
		} };
	}
}
