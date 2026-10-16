using System.Collections;
using UnityEngine;

public class RoomAudioMuffler : MonoBehaviour
{
	[SerializeField]
	private AudioSource source;

	[SerializeField]
	[Range(0f, 1f)]
	private float muffledVolume;

	[SerializeField]
	private float fadeSpeed = 1f;

	private float originalVolume;

	private Coroutine fadeCoroutine;

	private void Start()
	{
		if (source == null)
		{
			source = GetComponent<AudioSource>();
			if (source == null)
			{
				Debug.LogError("RoomAudioMuffler: AudioSource не найден на объекте " + base.gameObject.name);
				base.enabled = false;
				return;
			}
		}
		originalVolume = source.volume;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			FadeToVolume(muffledVolume);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			FadeToVolume(originalVolume);
		}
	}

	private void FadeToVolume(float targetVolume)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCoroutine(targetVolume));
	}

	private IEnumerator FadeCoroutine(float targetVolume)
	{
		float startVolume = source.volume;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * fadeSpeed;
			float t2 = Mathf.SmoothStep(0f, 1f, t);
			float volume = Mathf.Lerp(startVolume, targetVolume, t2);
			source.volume = volume;
			yield return null;
		}
		source.volume = targetVolume;
		fadeCoroutine = null;
	}
}
