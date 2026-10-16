using UnityEngine;

public class StunState : IEnemyState
{
	private float stunTimer;

	public void EnterState(Enemy enemy)
	{
		if (enemy is SisEnemy sisEnemy)
		{
			sisEnemy.SetActiveFlashlight(active: false);
		}
		enemy.PlayAnimation("Stun");
		stunTimer = 3f;
		enemy.CreateStunEffect();
	}

	public void UpdateState(Enemy enemy)
	{
		stunTimer -= Time.deltaTime;
		if (stunTimer <= 0f)
		{
			enemy.ChangeState(new RoamState());
		}
	}

	public void ExitState(Enemy enemy)
	{
		if (enemy is VampireEnemy vampireEnemy)
		{
			vampireEnemy.PlayAnimation("Metamorphosis");
		}
		if (enemy is SisEnemy sisEnemy)
		{
			sisEnemy.SetActiveFlashlight(active: true);
		}
		enemy.StartBlind();
	}
}
