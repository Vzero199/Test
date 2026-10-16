using UnityEngine;

public class InteractState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
		if (enemy is SisEnemy)
		{
			enemy.PlayAnimation("Idle");
		}
		if (enemy.InteractObject != null)
		{
			enemy.InteractObject.Interact(enemy.gameObject);
			enemy.ChaseDirection = 0f;
		}
	}

	public void UpdateState(Enemy enemy)
	{
		enemy.UpdateChaseTimer();
		enemy.InteractObject.Interact(enemy.gameObject);
		if (enemy.InteractObject == null || enemy.InteractObject.InteractionComplete || !enemy.CanInteract)
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
		enemy.InteractObject?.StopInteract();
		enemy.InitializeCurrentRoom();
		enemy.InteractObject = null;
	}
}
