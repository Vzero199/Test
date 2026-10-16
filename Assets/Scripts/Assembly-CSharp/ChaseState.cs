using UnityEngine;

public class ChaseState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
		enemy.IsChaseMode = true;
		enemy.SetTargetRace(Player.GetInstance().gameObject);
		if (!(enemy is VampireEnemy))
		{
			enemy.PlayAnimation("Chase");
		}
	}

	public void UpdateState(Enemy enemy)
	{
		if (enemy is WhisperEnemy whisperEnemy)
		{
			if (!Timer.IsEnabled)
			{
				return;
			}
			if (enemy.MainGoalRoomName != Player.GetInstance().CurrentRoom)
			{
				enemy.ChangeState(new IdleState());
			}
			else if (whisperEnemy.EnemyInViewField())
			{
				whisperEnemy.UpdateSpookyHideTimer();
				if (whisperEnemy.SpookyHideTimer <= 0f)
				{
					enemy.ChangeState(new IdleState());
				}
			}
			else
			{
				whisperEnemy.CheckProximitySound();
				whisperEnemy.SpookyHideTimer = whisperEnemy.SpookyHideTime;
			}
			return;
		}
		enemy.CheckPlayerRoom();
		enemy.UpdateChaseTimer();
		if (!enemy.PlayerInCurrentStage())
		{
			if (enemy.HunterInstinctTimer <= 0f && enemy.ChaseTimer > 0f)
			{
				enemy.HunterInstinctTimer = Random.Range(enemy.HunterInstinctTime.x, enemy.HunterInstinctTime.y);
				enemy.ChangeState(new ShortChaseState());
				return;
			}
		}
		else if (enemy.ChaseTimer <= 0f)
		{
			enemy.ChangeState(new IdleState());
		}
		if (enemy.MainGoal == null)
		{
			enemy.ChangeState(new IdleState());
		}
	}

	public void ExitState(Enemy enemy)
	{
		enemy.IsChaseMode = false;
		enemy.ChaseDirection = 0f;
	}
}
