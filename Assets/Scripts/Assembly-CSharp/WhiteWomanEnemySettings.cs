using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WhiteWomanEnemySettings", menuName = "EnemySettings/WhiteWomanEnemySettings")]
public class WhiteWomanEnemySettings : BaseEnemySettings
{
	public RectTransform largeBar;

	public RectTransform charmGamePanel;

	public RectTransform smallBar;

	public RectTransform circle;

	public ProgressBar penaltyBar;

	[Range(2f, 15f)]
	public float charmGameTimer = 5f;

	public GameObject enemyPrefabWithAnimator;

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)> { 
		{
			"Elevator Game Timer",
			(2f, 15f, charmGameTimer)
		} };
	}
}
