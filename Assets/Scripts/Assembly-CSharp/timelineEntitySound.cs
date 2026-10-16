using SmallHedge.SoundManager;
using UnityEngine;

public class timelineEntitySound : MonoBehaviour
{
	public void OnHeroStepSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.HeroWalking, base.gameObject);
	}
}
