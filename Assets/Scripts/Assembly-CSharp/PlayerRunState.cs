public class PlayerRunState : IPlayerState
{
	public void EnterState(Player player)
	{
		player.PlayAnimation("Run");
		player.AddSpeedModifier("MoveSpeedBoost", player.MoveSpeedBoost);
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
			if (player.PressInteractButton())
			{
				player.ChangeState(new PlayerInteractState());
			}
			else if (!player.IsMoving)
			{
				player.ChangeState(new PlayerIdleState());
			}
		}
	}

	public void ExitState(Player player)
	{
		player.RemoveSpeedModifier("MoveSpeedBoost");
		player.FixPixelPosition();
	}
}
