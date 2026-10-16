using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class BaseEnemySettings : ScriptableObject
{
	public EnemyType enemyType;

	[Range(150f, 400f)]
	public float currentMoveSpeed = 200f;

	[Range(0f, 5f)]
	public float currentInteractionSpeed = 1f;

	[Range(0f, 5f)]
	public float interactionCooldown = 1f;

	[Range(0.5f, 5f)]
	public float resistanceTime = 2f;

	[Range(100f, 8000f)]
	public float viewDistance = 900f;

	[Range(5f, 50f)]
	public float chaseTime = 20f;

	[Range(5f, 60f)]
	public float hunterInstinctCooldown = 30f;

	[Range(0f, 600f)]
	public float hearDistance = 300f;

	public Vector2 hunterInstinctTime = new Vector2(10f, 15f);

	public Vector3 colliderSize;

	public RuntimeAnimatorController animatorController;

	public Animator animator;

	[Range(0f, 10f)]
	public float initialPriority = 2f;

	private void Start()
	{
		if (animatorController != null)
		{
			animator.runtimeAnimatorController = animatorController;
		}
	}

	public abstract Dictionary<string, (float min, float max, float current)> GetSettings();
}
