using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MebeebEnemySettings", menuName = "EnemySettings/MebeebEnemySettings")]
public class MebeebEnemySettings : BaseEnemySettings
{
	[Serializable]
	public struct BearPoolByDifficulty
	{
		public Difficulty difficulty;

		public GameObject[] bearPool;
	}

	public BearPoolByDifficulty[] bearPools;

	public Vector3 spawnOffset;

	public GameObject[] GetBearPoolForDifficulty(Difficulty difficulty)
	{
		BearPoolByDifficulty[] array = bearPools;
		for (int i = 0; i < array.Length; i++)
		{
			BearPoolByDifficulty bearPoolByDifficulty = array[i];
			if (bearPoolByDifficulty.difficulty == difficulty)
			{
				return bearPoolByDifficulty.bearPool;
			}
		}
		Debug.LogWarning($"BearPool для сложности {difficulty} не найден, будет использован пустой массив");
		return new GameObject[0];
	}

	public override Dictionary<string, (float min, float max, float current)> GetSettings()
	{
		return new Dictionary<string, (float, float, float)>();
	}
}
