using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NintendoEntitySettings", menuName = "EnemySettings/NintendoEntitySettings")]
public class NintendoEntitySettings : BaseEnemySettings
{
	[Range(0f, 5f)]
	public float interactionBoostTime = 1f;

	[Range(100f, 300f)]
	public float chaseDistance = 200f;

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
			}
		};
	}
}
