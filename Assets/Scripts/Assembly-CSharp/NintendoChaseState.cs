using UnityEngine;

public class NintendoChaseState : INintendoState
{
	private float gapDistance;

	private Player playerInstance;

	public void EnterState(Nintendo nintendo)
	{
		nintendo.GetAnimator().Play("Run");
		gapDistance = nintendo.ChaseDistance + 50f;
		playerInstance = Player.GetInstance();
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
		nintendo.Chase();
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
		nintendo.CheckTriggerForInteract(collider.gameObject);
	}

	public void ExitTriggerState(Nintendo nintendo, Collider collider)
	{
		if (nintendo.ObjectIsPlayer(collider.gameObject))
		{
			Debug.Log(nintendo.PlayerIsObjective());
			Debug.Log(nintendo.CanInteract);
			Debug.Log($"Current Player State Is: {Player.GetInstance().CurrentState}");
			Debug.Log(Player.GetInstance().CurrentState is PlayerHideState);
			Debug.Log($"Current ShowRoflTimer IS: {nintendo.ShowRoflTimer}");
			Debug.Log(nintendo.ShowRoflTimer <= 0f);
		}
		if (nintendo.ObjectIsPlayer(collider.gameObject) && nintendo.GuardCoroutine == null && !nintendo.PlayerIsObjective() && nintendo.CanInteract && !(Player.GetInstance().CurrentState is PlayerHideState) && nintendo.ShowRoflTimer <= 0f)
		{
			nintendo.ChangeState(new NintendoShowRoflState());
		}
	}
}
