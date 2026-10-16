using UnityEngine;

public class NintendoShowRoflState : INintendoState
{
	public void EnterState(Nintendo nintendo)
	{
		nintendo.StartRoflEvent();
	}

	public void UpdateState(Nintendo nintendo)
	{
		nintendo.HandleTimers();
		if (!nintendo.CheckPlayerAttention())
		{
			if (nintendo.TargetIsPlayer())
			{
				nintendo.ChangeState(new NintendoWithPlayerIdleState());
			}
			else
			{
				nintendo.RoflRunAwayBoost();
			}
		}
	}

	public void ExitState(Nintendo nintendo)
	{
		nintendo.InterraptRoflEvent();
	}

	public void EnterTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void StayTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void ExitTriggerState(Nintendo nintendo, Collider collider)
	{
	}
}
