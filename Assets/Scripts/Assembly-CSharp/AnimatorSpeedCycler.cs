using System.Collections;
using UnityEngine;

public class AnimatorSpeedCycler : MonoBehaviour
{
	[SerializeField]
	private Animator targetAnimator;

	[SerializeField]
	private float[] speeds = new float[3] { 1f, 1.5f, 2f };

	private int currentIndex;

	private Coroutine cycleCoroutine;

	private AudioSource mainSoundSource;

	private void OnEnable()
	{
		cycleCoroutine = StartCoroutine(CycleSpeed());
	}

	private void OnDisable()
	{
		if (cycleCoroutine != null)
		{
			StopCoroutine(cycleCoroutine);
			cycleCoroutine = null;
		}
		if (targetAnimator != null)
		{
			targetAnimator.speed = 1f;
		}
		if (mainSoundSource != null)
		{
			mainSoundSource.pitch = 1f;
		}
		GallerySpeedController.CurrentSpeed = 1f;
	}

	private IEnumerator CycleSpeed()
	{
		yield return new WaitForSeconds(5f);
		while (true)
		{
			float num = speeds[currentIndex];
			if (targetAnimator != null)
			{
				targetAnimator.speed = num;
			}
			GallerySpeedController.CurrentSpeed = num;
			currentIndex = (currentIndex + 1) % speeds.Length;
			yield return new WaitForSeconds(5f);
		}
	}

	public void RegisterAudioSource(AudioSource source)
	{
		mainSoundSource = source;
	}
}
