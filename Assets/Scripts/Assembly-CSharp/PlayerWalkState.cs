using UnityEngine;

public class PlayerWalkState : IPlayerState
{
	public void EnterState(Player player)
	{
		Animator component = player.GetComponent<Animator>();
		if (player.TotalSpeedModifier <= player.GetCrawlSpeedThreshold())
		{
			player.PlayAnimation("Hero_Crawl");
			if (player.IsFlashLightOn)
			{
				player.FlashlightOff();
			}
		}
		else if (component != null && component.GetCurrentAnimatorStateInfo(0).IsName("RunLoop"))
		{
			player.PlayAnimation("Walk_Night_AfterRun");
		}
		else if (player.IsNightSprites)
		{
			player.PlayAnimation("Walk_Night");
		}
		else
		{
			player.PlayAnimation("Walk_Day");
		}
	}

	public void UpdateState(Player player)
	{
		if (!player.IsPaused && !BlinkManager.GetInstance().IsFadingIn)
		{
			player.UpdateInteractionTimer();
			player.HandleCurrentIntensity();
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
		player.FixPixelPosition();
	}
}
