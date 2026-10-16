using UnityEngine;

namespace SmallHedge.SoundManager
{
	public class PlaySoundLoopStereo : StateMachineBehaviour
	{
		[Header("Основные настройки")]
		[SerializeField]
		private SoundType sound;

		[SerializeField]
		[Range(0f, 1f)]
		private float volume = 1f;

		[SerializeField]
		private float delay;

		public bool isActive = true;

		[Header("Режим: 3 звука каждые 2 цикла")]
		[SerializeField]
		private bool useThreeSoundsPerTwoCycles;

		private float lastNormalizedTime = -1f;

		private float timeSinceLastSound;

		private bool soundPlayedThisCycle;

		private int soundIndex;

		private readonly float interval = 0.66f;

		private float nextTriggerTime;

		public float Delay => delay;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (useThreeSoundsPerTwoCycles)
			{
				soundIndex = 0;
				nextTriggerTime = delay;
				lastNormalizedTime = stateInfo.normalizedTime;
			}
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (isActive && !EndEvent.GetInstance().IsAnyScene())
			{
				if (useThreeSoundsPerTwoCycles)
				{
					HandleRepeatingThreeSounds(animator, stateInfo);
				}
				else
				{
					HandleDefaultSound(animator, stateInfo);
				}
			}
		}

		private void HandleDefaultSound(Animator animator, AnimatorStateInfo stateInfo)
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
				GameObject gameObject = animator.gameObject;
				SoundManager.PlaySoundAtObject(sound, gameObject, volume, 1f, 600f);
				Entities component = gameObject.GetComponent<Entities>();
				if (component != null)
				{
					SoundLoopBus.RaiseLoopSound(component);
				}
				soundPlayedThisCycle = true;
			}
			lastNormalizedTime = normalizedTime;
		}

		private void HandleRepeatingThreeSounds(Animator animator, AnimatorStateInfo stateInfo)
		{
			float normalizedTime = stateInfo.normalizedTime;
			float num = lastNormalizedTime;
			if (normalizedTime - num > 0.1f)
			{
				int num2 = Mathf.FloorToInt(normalizedTime / (interval * 2f));
				soundIndex = num2 * 3 % 3;
				_ = interval;
				nextTriggerTime = (float)num2 * interval * 2f + delay;
				while (nextTriggerTime <= normalizedTime && soundIndex < 3)
				{
					nextTriggerTime += interval;
					soundIndex++;
				}
				if (soundIndex >= 3)
				{
					soundIndex = 0;
					nextTriggerTime = (float)(num2 + 1) * interval * 2f + delay;
				}
			}
			else
			{
				while (normalizedTime >= nextTriggerTime && soundIndex < 3)
				{
					GameObject gameObject = animator.gameObject;
					SoundManager.PlaySoundAtObject(sound, gameObject, volume, 1f, 600f);
					Entities component = gameObject.GetComponent<Entities>();
					if (component != null)
					{
						SoundLoopBus.RaiseLoopSound(component);
					}
					soundIndex++;
					nextTriggerTime += interval;
				}
				if (soundIndex >= 3)
				{
					soundIndex = 0;
					nextTriggerTime += delay;
				}
			}
			lastNormalizedTime = normalizedTime;
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			soundIndex = 0;
			nextTriggerTime = delay;
		}
	}
}
