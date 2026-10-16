using UnityEngine;

public class PlayerSexModeState : IPlayerState
{
	public void EnterState(Player player)
	{
		Nintendo.HideNintendo();
		if (Random.value <= 0.5f)
		{
			player.PlayAnimation("SecondMebeebSex");
			player.SetEyeLightIntensity(0.32f);
		}
		else
		{
			int layerIndex = 0;
			int stateID = Animator.StringToHash("MebeebSex");
			if (player.GetAnimator().HasState(layerIndex, stateID))
			{
				player.PlayAnimation("MebeebSex");
			}
			else
			{
				player.PlayAnimation("SecondMebeebSex");
				player.SetEyeLightIntensity(0.32f);
			}
		}
		CameraFollow.GetInstance().SetFollowTarget(player.transform.Find("CockCollider"));
		player.ButtonsContainer.SetActive(value: false);
		player.OnDisable();
	}

	public void UpdateState(Player player)
	{
		player.UpdateInteractionTimer();
		player.HandleCurrentIntensity();
		if (player.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName("ClimaxRepeat") && player.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime >= 4f)
		{
			BlinkManager.GetInstance().BlinkWithCustomSpeed(2f, player.gameObject, player.ExitSexMode, allowVerticalMovement: false);
		}
	}

	public void ExitState(Player player)
	{
		player.CurrentHeartNumber--;
		if (player.CurrentHeartNumber > 0)
		{
			CameraFollow.GetInstance().SetFollowTarget(player.transform);
			player.ButtonsContainer.SetActive(value: true);
			player.IsPaused = false;
			player.ManualTriggerCheck();
			SpriteRenderer component = player.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				component.sortingLayerName = "Entity";
				component.sortingOrder = 60;
			}
		}
		else if (player.GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Second"))
		{
			player.GetAnimator().Play("SecondHloop2");
		}
		else
		{
			player.GetAnimator().Play("Hloop2");
		}
	}
}
