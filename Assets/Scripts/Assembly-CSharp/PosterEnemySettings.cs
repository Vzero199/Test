using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PosterEnemySettings", menuName = "EnemySettings/PosterEnemySettings")]
public class PosterEnemySettings : BaseEnemySettings
{
	[Range(2f, 20f)]
	public float disappearanceDelay = 11f;

	public float respawnTime;

	public bool useRoomBasedSpawn;

	public bool shouldUseBaseUpdate;

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
				"Light Resistance",
				(0.5f, 5f, resistanceTime)
			},
			{
				"Spawn Priority",
				(1f, 10f, initialPriority)
			},
			{
				"Disappearance Delay",
				(2f, 20f, disappearanceDelay)
			}
		};
	}
}
