using UnityEngine;

namespace SmallHedge.SoundManager
{
	public class PlaySoundLoopMono : StateMachineBehaviour
	{
		[SerializeField]
		private SoundType sound;

		[SerializeField]
		[Range(0f, 1f)]
		private float volume = 1f;

		[SerializeField]
		private float delay = 0.5f;

		private float lastNormalizedTime = -1f;

		private float timeSinceLastSound;

		private bool soundPlayedThisCycle;

		public float Delay => delay;

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			float normalizedTime = stateInfo.normalizedTime;
			if (Mathf.Floor(normalizedTime) > Mathf.Floor(lastNormalizedTime))
			{
				timeSinceLastSound = 0f;
				soundPlayedThisCycle = false;
			}
			timeSinceLastSound += Time.deltaTime;
			if (timeSinceLastSound >= delay && !soundPlayedThisCycle)
			{
				SoundManager.PlaySound(sound, null, volume);
				Entities component = animator.GetComponent<Entities>();
				if (component != null)
				{
					SoundLoopBus.RaiseLoopSound(component);
				}
				soundPlayedThisCycle = true;
			}
			lastNormalizedTime = normalizedTime;
		}
	}
}
