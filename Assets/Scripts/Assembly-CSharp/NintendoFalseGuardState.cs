using UnityEngine;

public class NintendoFalseGuardState : INintendoState
{
	public void EnterState(Nintendo nintendo)
	{
		nintendo.StartFalseGuardEvent();
	}

	public void UpdateState(Nintendo nintendo)
	{
		nintendo.HandleTimers();
	}

	public void ExitState(Nintendo nintendo)
	{
	}

	public void EnterTriggerState(Nintendo nintendo, Collider collider)
	{
		nintendo.CheckTriggerForBetrayl(collider.gameObject);
	}

	public void StayTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void ExitTriggerState(Nintendo nintendo, Collider collider)
	{
	}
}
