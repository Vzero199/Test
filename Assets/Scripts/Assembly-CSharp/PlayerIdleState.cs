using UnityEngine;

public class PlayerIdleState : IPlayerState
{
	public void EnterState(Player player)
	{
		Animator component = player.GetComponent<Animator>();
		player.IsMoving = false;
		if (player.TotalSpeedModifier <= player.GetCrawlSpeedThreshold())
		{
			player.PlayAnimation("Hero_Crawl_Idle");
			if (player.IsFlashLightOn)
			{
				player.FlashlightOff();
			}
			player.AlignToRoomLine();
		}
		else if (component != null && component.GetCurrentAnimatorStateInfo(0).IsName("RunLoop"))
		{
			player.PlayAnimation("Idle_Night_AfterRun");
			player.AlignToRoomLine();
		}
		else
		{
			if (player.IsNightSprites)
			{
				player.PlayAnimation("Idle_Night");
			}
			else
			{
				player.PlayAnimation("Idle_Day");
			}
			player.AlignToRoomLine();
		}
	}

	public void UpdateState(Player player)
	{
		player.FixPixelPosition();
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
			if (player.IsMoving && !(player.CurrentState is PlayerRunState))
			{
				player.ChangeState(new PlayerWalkState());
			}
		}
	}

	public void ExitState(Player player)
	{
	}
}
