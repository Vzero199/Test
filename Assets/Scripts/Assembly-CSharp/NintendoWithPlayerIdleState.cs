using UnityEngine;

public class NintendoWithPlayerIdleState : INintendoState
{
	private float gapDistance;

	private Player playerInstance;

	public void EnterState(Nintendo nintendo)
	{
		nintendo.ShowWantedItem();
		nintendo.CheckForHint();
		gapDistance = nintendo.ChaseDistance + 50f;
		playerInstance = Player.GetInstance();
		nintendo.GetAnimator().Play("Idle");
		nintendo.ChaseDirection = 0f;
	}

	public void UpdateState(Nintendo nintendo)
	{
		float num = Mathf.Abs(playerInstance.transform.position.x - nintendo.transform.position.x);
		nintendo.HandleTimers();
		if (num >= 50f && num < gapDistance && nintendo.TargetIsPlayer() && !Player.GetInstance().IsMoving && !(Player.GetInstance().CurrentState is PlayerHideState) && nintendo.CheckPlayerAttention() && nintendo.ShowRoflTimer <= 0f)
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
		if (num >= gapDistance)
		{
			nintendo.ChangeState(new NintendoChaseState());
		}
	}

	public void ExitState(Nintendo nintendo)
	{
		nintendo.HideWantedItem();
		nintendo.isReachedMainGoal = false;
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
