using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NunEnemySettings", menuName = "EnemySettings/NunEnemySettings")]
public class NunEnemySettings : BaseEnemySettings
{
	public ProgressBar captureBar;

	[Range(20f, 100f)]
	public float captureTime;

	[Range(0f, 0.5f)]
	public float initialSlowdownPercent;

	[Range(0f, 0.5f)]
	public float additionalSlowdownPercent;

	public RuntimeAnimatorController nunCatchAnimatorController;

	public GameObject prefab;

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)>
		{
			{
				"Spawn Priority",
				(1f, 10f, initialPriority)
			},
			{
				"Capture Time",
				(20f, 100f, captureTime)
			},
			{
				"SlowDown Factor 1",
				(0f, 0.5f, initialSlowdownPercent)
			},
			{
				"SlowDown Factor 2",
				(0f, 0.5f, additionalSlowdownPercent)
			}
		};
	}
}
