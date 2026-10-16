using UnityEngine;

public class NintendoIdleState : INintendoState
{
	private float idleTimer;

	private float showRoflDistance;

	private Player playerInstance;

	public void EnterState(Nintendo nintendo)
	{
		idleTimer = 0.5f;
		nintendo.GetAnimator().Play("Idle");
		nintendo.ChaseDirection = 0f;
		showRoflDistance = nintendo.ChaseDistance + 50f;
		playerInstance = Player.GetInstance();
	}

	public void UpdateState(Nintendo nintendo)
	{
		float num = Mathf.Abs(playerInstance.transform.position.x - nintendo.transform.position.x);
		nintendo.HandleTimers();
		if (num >= 50f && num < showRoflDistance && nintendo.TargetIsPlayer() && !Player.GetInstance().IsMoving && !(Player.GetInstance().CurrentState is PlayerHideState) && nintendo.CheckPlayerAttention() && nintendo.ShowRoflTimer <= 0f)
		{
			nintendo.ChangeState(new NintendoShowRoflState());
		}
		if (nintendo.TargetIsPlayer() && Player.GetInstance().CurrentState is PlayerHideState)
		{
			nintendo.ChangeState(new NintendoFalseGuardState());
		}
		if (nintendo.TargetIsPlayer() && nintendo.PlayerIsFucked())
		{
			nintendo.HideAway();
		}
		if (nintendo.TargetIsPlayer() && Elevator.GetTriggered() && !nintendo.EventTriggered)
		{
			nintendo.StartLiftAnim();
		}
		idleTimer -= Time.deltaTime;
		if (idleTimer <= 0f && !nintendo.isReachedMainGoal)
		{
			nintendo.ChangeState(new NintendoChaseState());
		}
	}

	public void ExitState(Nintendo nintendo)
	{
	}

	public void EnterTriggerState(Nintendo nintendo, Collider collider)
	{
		nintendo.CheckTriggerForInteract(collider.gameObject);
	}

	public void StayTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void ExitTriggerState(Nintendo nintendo, Collider collider)
	{
	}
}
