using UnityEngine;

public class DetectionState : IEnemyState
{
	private float detectionTimer;

	public void EnterState(Enemy enemy)
	{
		enemy.ChaseTimer = enemy.ChaseTime;
		enemy.PlayAnimation("Idle");
		detectionTimer = 1f;
		if (enemy is HoodieCatEnemy)
		{
			detectionTimer = 2f;
		}
		enemy.ChaseDirection = 0f;
		enemy.IsIdle = true;
		if (!(enemy is HoodieCatEnemy))
		{
			enemy.CreateCommonEffect("DetectionTriggerEffect");
		}
	}

	public void UpdateState(Enemy enemy)
	{
		detectionTimer -= Time.deltaTime;
		if (detectionTimer <= 0f)
		{
			enemy.ChangeState(new ChaseState());
		}
	}

	public void ExitState(Enemy enemy)
	{
		enemy.IsIdle = false;
		if (!(enemy is HoodieCatEnemy))
		{
			enemy.RemoveChildByName("DetectionTriggerEffect");
		}
	}
}
