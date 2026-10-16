using SmallHedge.SoundManager;

public class CoffeeItem : ItemToUse
{
	public override void UseItem()
	{
		base.UseItem();
		Player instance = Player.GetInstance();
		if (base.ItemName == "coffee")
		{
			instance.enduranceSM.RestoreFull();
			instance.enduranceSM.PauseConsumption(10f);
			SoundManager.PlaySound(SoundType.Coffee);
		}
		else if (base.ItemName == "green pill")
		{
			instance.enduranceSM.ApplyTemporaryBoost();
			SoundManager.PlaySound(SoundType.GreenPills);
		}
	}
}
