using UnityEngine;

public interface IPlayerState
{
	void EnterState(Player player);

	void UpdateState(Player player);

	void ExitState(Player player);

	void HandleTrigger(Player player, Collider other)
	{
		if (BlinkManager.GetInstance() != null && BlinkManager.GetInstance().IsFadingIn)
		{
			return;
		}
		InteractionObject componentInParent = other.GetComponentInParent<InteractionObject>();
		DialogueTrigger componentInParent2 = other.GetComponentInParent<DialogueTrigger>();
		TabLight componentInParent3 = other.GetComponentInParent<TabLight>();
		Transform transform = other.transform.Find("GuardLight");
		if (player.CurrentSelectedItem != null)
		{
			QuestItem componentInChildren = player.CurrentSelectedItem.GetComponentInChildren<QuestItem>();
			if (componentInParent3 != null && componentInChildren != null && transform != null)
			{
				player.InteractObject = componentInParent3;
				player.InTrigger = true;
				return;
			}
		}
		if (componentInParent != null && !player.CanInteract)
		{
			return;
		}
		if (other.GetComponentInParent<Elevator>() != null)
		{
			player.ElevatorInstance = other.GetComponentInParent<Elevator>();
			if (componentInParent != null && player.InteractObject != componentInParent)
			{
				player.InteractObject = componentInParent;
			}
			player.InTrigger = true;
		}
		if ((componentInParent != null || componentInParent2 != null) && transform == null && componentInParent3 == null)
		{
			if (componentInParent != null && player.InteractObject != componentInParent)
			{
				player.InteractObject = componentInParent;
			}
			player.InTrigger = true;
		}
	}
}
