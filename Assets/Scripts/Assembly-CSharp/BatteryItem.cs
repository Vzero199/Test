using SmallHedge.SoundManager;

public class BatteryItem : ItemToUse
{
	public override void UseItem()
	{
		base.UseItem();
		Player.GetInstance().RestoreBattery();
		SoundManager.PlaySound(SoundType.BatteryUse);
	}
}
