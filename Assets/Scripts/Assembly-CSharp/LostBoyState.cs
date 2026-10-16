using UnityEngine;

public class LostBoyState : IEnemyState
{
	private float lastTurnTime;

	public void EnterState(Enemy enemy)
	{
		enemy.ChaseTimer = 0f;
		enemy.PlayAnimation("LostBoy");
		enemy.ChaseDirection = 0f;
		enemy.IsIdle = true;
		enemy.IsLostBoy = false;
	}

	public void UpdateState(Enemy enemy)
	{
		AnimatorStateInfo currentAnimatorStateInfo = enemy.GetAnimator().GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("LostBoy") && currentAnimatorStateInfo.normalizedTime >= 1f)
		{
			enemy.ChangeState(new RoamState());
		}
		if ((enemy.PlayerInViewField() && (!(Player.GetInstance().CurrentState is PlayerHideState) || !enemy.PlayerInCurrentRoom())) || enemy is WhisperEnemy)
		{
			enemy.ChangeState(new DetectionState());
		}
		if (enemy.IsPlayerStepInsideHearZone())
		{
			enemy.HunterInstinctTimer = Random.Range(enemy.HunterInstinctTime.x, enemy.HunterInstinctTime.y);
			enemy.ChangeState(new ShortChaseState());
		}
	}

	public void ExitState(Enemy enemy)
	{
		enemy.IsIdle = false;
	}
}
