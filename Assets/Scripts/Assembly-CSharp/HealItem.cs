using SmallHedge.SoundManager;

public class HealItem : ItemToUse
{
	public override void UseItem()
	{
		base.UseItem();
		Player.GetInstance().CurrentHeartNumber++;
		if (base.ItemName == "red pill")
		{
			SoundManager.PlaySound(SoundType.RedPills);
		}
		else if (base.ItemName == "onigiri")
		{
			SoundManager.PlaySound(SoundType.onigiri);
		}
	}
}
