using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SisEnemySettings", menuName = "EnemySettings/SisEnemySettings")]
public class SisEnemySettings : BaseEnemySettings
{
	public bool isAngry;

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)>
		{
			{
				"Move Speed",
				(150f, 400f, currentMoveSpeed)
			},
			{
				"Interaction Speed",
				(0f, 5f, currentInteractionSpeed)
			},
			{
				"View Distance",
				(100f, 8000f, viewDistance)
			},
			{
				"Spawn Priority",
				(1f, 10f, initialPriority)
			}
		};
	}
}
