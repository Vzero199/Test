using SmallHedge.SoundManager;
using UnityEngine;

public class TimeBoostItem : ItemToUse
{
	private const float boostedMultiplier = 10f;

	private const float boostDuration = 3f;

	public override void UseItem()
	{
		base.UseItem();
		if (Timer.IsEnabled)
		{
			Timer timer = Object.FindObjectOfType<Timer>();
			if (timer != null)
			{
				SoundManager.PlaySound(SoundType.UseClock);
				timer.BoostTime(10f, 3f);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
