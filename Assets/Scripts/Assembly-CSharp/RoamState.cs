using UnityEngine;

public class RoamState : IEnemyState
{
	private float roamTimer;

	public void EnterState(Enemy enemy)
	{
		if (enemy.HunterInstinctCooldownTimer <= 0f)
		{
			enemy.HunterInstinctCooldownTimer = enemy.HunterInstinctCooldown;
		}
		enemy.TargetChase = null;
		roamTimer = Random.Range(10f, 15f);
		enemy.ChaseDirection = 0f;
		if (!(enemy is VampireEnemy))
		{
			enemy.PlayAnimation("Roam");
		}
		enemy.RandomRoam();
	}

	public void UpdateState(Enemy enemy)
	{
		enemy.UpdateLostTimer();
		enemy.UpdateChaseTimer();
		if (enemy.IsLostBoy && enemy.TimerToLostAnim <= 0f)
		{
			enemy.ChangeState(new LostBoyState());
		}
		if (!enemy.PlayerInCurrentStage())
		{
			enemy.HunterInstinctCooldownTimerUpdate();
		}
		if (enemy.PlayerInViewField())
		{
			enemy.ChangeState(new DetectionState());
			return;
		}
		roamTimer -= Time.deltaTime;
		if (enemy.HunterInstinctTimer > 0f && !enemy.PlayerInCurrentStage())
		{
			enemy.ChangeState(new ShortChaseState());
		}
		else
		{
			enemy.HunterInstinctTimer = 0f;
		}
		HandleShortChaseTimer(enemy);
		if (roamTimer <= 0f)
		{
			enemy.ChangeState(new IdleState());
		}
		if (enemy.IsPlayerStepInsideHearZone())
		{
			enemy.HunterInstinctTimer = Random.Range(enemy.HunterInstinctTime.x, enemy.HunterInstinctTime.y);
			enemy.ChangeState(new ShortChaseState());
		}
	}

	public void ExitState(Enemy enemy)
	{
	}

	private static void HandleShortChaseTimer(Enemy enemy)
	{
		if (!enemy.PlayerInCurrentStage() && enemy.HunterInstinctCooldownTimer <= 0f)
		{
			enemy.HunterInstinctTimer = Random.Range(enemy.HunterInstinctTime.x, enemy.HunterInstinctTime.y);
			enemy.ChangeState(new ShortChaseState());
		}
	}
}
