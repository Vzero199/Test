using UnityEngine;

public class IdleState : IEnemyState
{
	private bool hasLookedLeft;

	private bool hasSeenPlayer;

	private float lastTurnTime;

	public void EnterState(Enemy enemy)
	{
		enemy.ChaseDirection = 0f;
		enemy.IsIdle = true;
		enemy.IdleTimer = (enemy.IsChaseMode ? (enemy.IdleTime / 5f) : enemy.IdleTime);
		if (enemy is HoodieCatEnemy)
		{
			enemy.IdleTimer = 2f;
		}
		lastTurnTime = Time.time;
		if (!(enemy is PosterEnemy))
		{
			enemy.PlayAnimation("Idle");
			enemy.FixPixelPosition();
		}
		else if (enemy.name == "SpriteParent")
		{
			enemy.GetAnimator().SetFloat("Speed", 2f);
			enemy.GetAnimator().Play("PhantomAppear");
		}
	}

	public void UpdateState(Enemy enemy)
	{
		enemy.UpdateChaseTimer();
		enemy.HunterInstinctTimerUpdate();
		if (enemy is PosterEnemy)
		{
			AnimatorStateInfo currentAnimatorStateInfo = enemy.GetAnimator().GetCurrentAnimatorStateInfo(0);
			if (enemy.name == "SpriteParent" && currentAnimatorStateInfo.IsName("PhantomAppear") && currentAnimatorStateInfo.normalizedTime >= 1f)
			{
				enemy.GetAnimator().SetFloat("Speed", 1f);
				enemy.ChangeState(new ChaseState());
			}
			return;
		}
		enemy.IdleTimer -= Time.deltaTime;
		if (enemy is WhisperEnemy)
		{
			if ((enemy.IdleTimer <= 0f || enemy.TargetChase == enemy.MainGoal) && !enemy.EnemyInViewField())
			{
				enemy.ChangeState(new ChaseState());
			}
			return;
		}
		if (hasSeenPlayer || enemy.PlayerInViewField() || (enemy.ChaseTimer > 0f && (!(Player.GetInstance().CurrentState is PlayerHideState) || !enemy.PlayerInCurrentRoom())) || enemy is WhisperEnemy)
		{
			if (enemy.ChaseTimer > 0f)
			{
				enemy.ChangeState(new ChaseState());
			}
			else
			{
				enemy.ChangeState(new DetectionState());
			}
		}
		else if (enemy.IdleTimer <= 0f)
		{
			if (enemy.ChaseTimer > 0f && Player.GetInstance().CurrentState is PlayerHideState && enemy.PlayerInCurrentRoom())
			{
				enemy.TimerToLostAnim = 2.5f;
				enemy.IsLostBoy = true;
			}
			enemy.ChangeState(new RoamState());
		}
		if (enemy.PlayerInViewField())
		{
			hasSeenPlayer = true;
			return;
		}
		if (Time.time - lastTurnTime >= enemy.IdleTime / 2f && !hasLookedLeft)
		{
			enemy.FaceRight = !enemy.FaceRight;
			hasLookedLeft = true;
			lastTurnTime = Time.time;
		}
		if (hasLookedLeft && enemy.IdleTimer > 0f && Time.time - lastTurnTime >= enemy.IdleTime / 2f)
		{
			enemy.FaceRight = !enemy.FaceRight;
			lastTurnTime = Time.time;
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
		hasLookedLeft = false;
		hasSeenPlayer = false;
	}
}
