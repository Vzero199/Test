using UnityEngine;

namespace SmallHedge.SoundManager
{
	public class PlaySoundEnterStereo : StateMachineBehaviour
	{
		[SerializeField]
		private SoundType sound;

		[SerializeField]
		[Range(0f, 1f)]
		private float volume = 1f;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!(animator != null) || !(animator.GetComponent<Enemy>() != null) || !(animator.GetComponent<Enemy>() is WhisperEnemy { SpookyHideTimer: >0f }))
			{
				SoundManager.PlaySoundAtObject(sound, animator.gameObject, volume, 1f, 300f);
			}
		}
	}
}
