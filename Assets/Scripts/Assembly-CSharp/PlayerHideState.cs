using UnityEngine;

public class PlayerHideState : IPlayerState
{
	private Renderer renderer;

	public void EnterState(Player player)
	{
		player.GetAnimator().enabled = false;
		renderer = player.GetComponent<SpriteRenderer>();
		renderer.enabled = false;
		player.FlashlightOff();
		player.BatteryDrainDisable();
		if (player.HideCabin.name == "OpenBox")
		{
			SpriteRenderer component = player.HideCabin.GetComponent<SpriteRenderer>();
			Sprite sprite = Resources.Load<Sprite>("Sprites/CloseBox");
			component.sprite = sprite;
		}
		else if (player.HideCabin.transform.childCount <= 0 && !(player.HideCabin.name == "CurtainsHide"))
		{
			SpriteRenderer component2 = player.HideCabin.GetComponent<SpriteRenderer>();
			Sprite sprite2 = Resources.Load<Sprite>("Sprites/CabinClose");
			component2.sprite = sprite2;
		}
	}

	public void UpdateState(Player player)
	{
		if (!player.IsPaused && !BlinkManager.GetInstance().IsFadingIn)
		{
			player.UpdateInteractionTimer();
			player.HandleCurrentIntensity();
			player.HandleInteractions();
		}
	}

	public void ExitState(Player player)
	{
		renderer.enabled = true;
		if (player.HideCabin != null)
		{
			if (player.HideCabin.name == "OpenBox")
			{
				SpriteRenderer component = player.HideCabin.GetComponent<SpriteRenderer>();
				Sprite sprite = Resources.Load<Sprite>("Sprites/OpenBox");
				component.sprite = sprite;
			}
			else if (player.HideCabin.transform.childCount <= 0 && !(player.HideCabin.name == "CurtainsHide"))
			{
				SpriteRenderer component2 = player.HideCabin.GetComponent<SpriteRenderer>();
				Sprite sprite2 = Resources.Load<Sprite>("Sprites/CabinOpen");
				component2.sprite = sprite2;
			}
		}
		player.GetAnimator().enabled = true;
		renderer.enabled = true;
	}
}
