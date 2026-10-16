public class ShortChaseState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
		enemy.IsChaseMode = true;
		if (!enemy.PlayerInCurrentRoom() || !(Player.GetInstance().CurrentState is PlayerHideState))
		{
			enemy.SetTargetRace(Player.GetInstance().gameObject);
			if (!(enemy is VampireEnemy))
			{
				enemy.PlayAnimation("Chase");
			}
		}
		else
		{
			enemy.TargetChase = null;
			enemy.TimerToLostAnim = 2.5f;
			enemy.IsLostBoy = true;
			enemy.ChangeState(new RoamState());
		}
	}

	public void UpdateState(Enemy enemy)
	{
		enemy.UpdateChaseTimer();
		enemy.HunterInstinctTimerUpdate();
		if (enemy.PlayerInViewField() || (enemy.ChaseTimer > 0f && enemy.PlayerInCurrentRoom()))
		{
			if (enemy.ChaseTimer > 0f)
			{
				enemy.ChangeState(new ChaseState());
			}
			else
			{
				enemy.ChangeState(new DetectionState());
			}
			return;
		}
		if (enemy.HunterInstinctTimer <= 0f && enemy.ChaseTimer <= 0f)
		{
			enemy.ChangeState(new IdleState());
		}
		if (enemy.IsPlayerStepInsideHearZone())
		{
			enemy.TimerToLostAnim = 2.5f;
		}
	}

	public void ExitState(Enemy enemy)
	{
		enemy.IsChaseMode = false;
		enemy.ChaseDirection = 0f;
		enemy.TargetChase = null;
	}
}
