using SmallHedge.SoundManager;
using UnityEngine;

public class VendingMachine : InteractionObject
{
	[SerializeField]
	private GameObject givingItem;

	public override bool CheckConditions()
	{
		if (Player.GetInstance().CanInteract && (Player.GetInstance().CurrentSelectedItem == null || Player.GetInstance().CurrentSelectedItem.GetComponentInChildren<Item>() == null) && CheckPlayerWallet())
		{
			return base.CheckConditions();
		}
		return false;
	}

	private bool CheckPlayerWallet()
	{
		return int.Parse(Player.GetInstance().wallet.text) > 0;
	}

	protected override bool CheckInteractButton()
	{
		if (player.IsActionButton)
		{
			return CheckConditions();
		}
		return false;
	}

	protected override void InteractComplete()
	{
		InteractionManager.Instance.StartInteractionCooldown(player);
		SoundManager.PlaySoundAtPosition(SoundType.VendingMachineBuy, base.transform.position);
		int num = int.Parse(Player.GetInstance().wallet.text);
		Player.GetInstance().wallet.text = (num - 100).ToString();
		Player.GetInstance().SetItemInSelectedCell(givingItem);
		base.InteractComplete();
	}
}
