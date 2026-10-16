public class PlayerInteractState : IPlayerState
{
	public void EnterState(Player player)
	{
		if (player.TotalSpeedModifier <= player.GetCrawlSpeedThreshold())
		{
			player.PlayAnimation("Hero_Crawl_Idle");
			if (player.IsFlashLightOn)
			{
				player.FlashlightOff();
			}
		}
		else if (player.IsNightSprites)
		{
			player.PlayAnimation("Idle_Night");
		}
		else
		{
			player.PlayAnimation("Idle_Day");
		}
	}

	public void UpdateState(Player player)
	{
		if (player.IsPaused || BlinkManager.GetInstance().IsFadingIn)
		{
			return;
		}
		player.UpdateInteractionTimer();
		player.HandleCurrentIntensity();
		if (!(DialogueManager.GetInstance() != null) || !DialogueManager.GetInstance().dialogueIsPlaying)
		{
			player.HandleInteractions();
			if (player.InteractObject == null || !player.CanInteract || !player.IsActionButton || player.InteractableObjects.Count == 0)
			{
				player.ChangeState(new PlayerIdleState());
				player.BreakInteract();
			}
		}
	}

	public void ExitState(Player player)
	{
		player.SetInteracting(value: false);
	}
}
