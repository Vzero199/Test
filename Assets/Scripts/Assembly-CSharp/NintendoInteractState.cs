using UnityEngine;

public class NintendoInteractState : INintendoState
{
	public void EnterState(Nintendo nintendo)
	{
		nintendo.GetAnimator().Play("Idle");
		nintendo.HideCabin = nintendo.InteractObject as HideThing;
	}

	public void UpdateState(Nintendo nintendo)
	{
		nintendo.HandleTimers();
		nintendo.HandleInteraction();
		if ((nintendo.InteractObject == null || nintendo.InteractObject.InteractionComplete || !nintendo.CanInteract) && !(nintendo.CurrentState is NintendoHideState))
		{
			nintendo.ChangeState(new NintendoIdleState());
			if (nintendo.targetChase != nintendo.MainGoal)
			{
				nintendo.SetTargetRace(nintendo.MainGoal);
			}
		}
	}

	public void ExitState(Nintendo nintendo)
	{
		nintendo.InitializeCurrentRoom();
	}

	public void EnterTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void StayTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void ExitTriggerState(Nintendo nintendo, Collider collider)
	{
		if (collider.gameObject == nintendo.targetChase && HospitalManager.GetInstance().GetInteractionObjectCache(nintendo.targetChase) != null)
		{
			nintendo.InitializeCurrentRoom();
			if (HospitalManager.GetInstance().GetDoorTransit(nintendo.targetChase) != null || nintendo.targetChase.GetComponent<Elevator>() != null)
			{
				nintendo.SetTargetRace(nintendo.MainGoal);
			}
			nintendo.InteractObject.StopInteract();
			nintendo.InteractObject = null;
			nintendo.ChangeState(new NintendoIdleState());
		}
	}
}
