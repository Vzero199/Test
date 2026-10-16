using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VampireEnemySettings", menuName = "EnemySettings/VampireEnemySettings")]
public class VampireEnemySettings : BaseEnemySettings
{
	public float BatsMoveSpeed = 300f;

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)>
		{
			{
				"Woman Move Speed",
				(150f, 400f, currentMoveSpeed)
			},
			{
				"Bats Move Speed",
				(200f, 800f, BatsMoveSpeed)
			},
			{
				"View Distance",
				(100f, 8000f, viewDistance)
			},
			{
				"Interaction Speed",
				(0f, 5f, currentInteractionSpeed)
			},
			{
				"Spawn Priority",
				(1f, 10f, initialPriority)
			},
			{
				"Light Resistance",
				(0.5f, 5f, resistanceTime)
			}
		};
	}
}
