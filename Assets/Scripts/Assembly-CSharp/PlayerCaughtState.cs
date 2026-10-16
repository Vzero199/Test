public class PlayerCaughtState : IPlayerState
{
	public void EnterState(Player player)
	{
		player.PlayAnimation("HairCatch");
	}

	public void UpdateState(Player player)
	{
		player.UpdateInteractionTimer();
	}

	public void ExitState(Player player)
	{
	}
}
