using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FifthDayEvent : MonoBehaviour
{
	[SerializeField]
	private float fadeDuration = 30f;

	[SerializeField]
	private float audioFadeDuration = 15f;

	[SerializeField]
	private FadeOutChildren fadeOutParty;

	public Light2D globalLight;

	public Light2D eyeVision;

	public Light2D auraLight;

	public AudioSource dayBgmSource;

	public AudioSource nightBgmSource;

	private void Start()
	{
		if (fadeOutParty == null)
		{
			Debug.LogError("");
		}
	}

	public void ActivateNighmareEvent()
	{
		StartCoroutine(NighmareEventCoroutine());
	}

	public IEnumerator NighmareEventCoroutine()
	{
		yield return new WaitForSeconds(1.5f);
		fadeOutParty.StartFadeOut(fadeDuration);
		yield return new WaitForSeconds(10f);
		yield return StartCoroutine(CrossFadeAudioAndLightCoroutine());
		yield return new WaitForSeconds(10f);
		bool dialogueWasPlaying = false;
		while (DialogueManager.GetInstance().dialogueIsPlaying || QuestManager.IsNightLoading)
		{
			dialogueWasPlaying = true;
			yield return null;
		}
		if (dialogueWasPlaying)
		{
			yield return new WaitForSeconds(5f);
		}
		Player.GetInstance().ChangeState(new PlayerIdleState());
		Player.GetInstance().enabled = false;
		SoundManager.PlaySound(SoundType.PlayerCrawling);
		Player.GetInstance().GetAnimator().Play("Hero_Crawl_Idle");
		QuestManager.GetInstance().StartNightmare();
	}

	private IEnumerator CrossFadeAudioAndLightCoroutine()
	{
		float startDayVolume = dayBgmSource.volume;
		float startGlobalLightIntensity = globalLight.intensity;
		float startEyeVisionIntensity = eyeVision.intensity;
		float startAuraLightIntensity = auraLight.intensity;
		float targetGlobalLightIntensity = 0.1f;
		float targetEyeVisionIntensity = 0.1f;
		float targetAuraLightIntensity = 0.007f;
		nightBgmSource.volume = 0f;
		if (!nightBgmSource.isPlaying)
		{
			nightBgmSource.Play();
		}
		float timer = 0f;
		bool dialogueTimeAdded = false;
		while (timer < fadeDuration)
		{
			if (DialogueManager.GetInstance().dialogueIsPlaying)
			{
				if (!dialogueTimeAdded)
				{
					dialogueTimeAdded = true;
				}
				yield return null;
				continue;
			}
			if (dialogueTimeAdded)
			{
				dialogueTimeAdded = false;
				yield return new WaitForSeconds(5f);
				continue;
			}
			timer += Time.deltaTime;
			float t = timer / fadeDuration;
			globalLight.intensity = Mathf.Lerp(startGlobalLightIntensity, targetGlobalLightIntensity, t);
			eyeVision.intensity = Mathf.Lerp(startEyeVisionIntensity, targetEyeVisionIntensity, t);
			auraLight.intensity = Mathf.Lerp(startAuraLightIntensity, targetAuraLightIntensity, t);
			float num = Mathf.Clamp01(timer / audioFadeDuration);
			if (num < 0.5f)
			{
				float t2 = num / 0.5f;
				dayBgmSource.volume = Mathf.Lerp(startDayVolume, startDayVolume * 0.5f, t2);
				nightBgmSource.volume = 0f;
			}
			else
			{
				float t3 = (num - 0.5f) / 0.5f;
				dayBgmSource.volume = Mathf.Lerp(startDayVolume * 0.5f, 0f, t3);
				nightBgmSource.volume = Mathf.Lerp(0f, startDayVolume, t3);
			}
			if (num >= 1f)
			{
				dayBgmSource.volume = 0f;
				nightBgmSource.volume = startDayVolume;
			}
			yield return null;
		}
		dayBgmSource.volume = 0f;
		nightBgmSource.volume = startDayVolume;
		globalLight.intensity = targetGlobalLightIntensity;
		eyeVision.intensity = targetEyeVisionIntensity;
		auraLight.intensity = targetAuraLightIntensity;
		dayBgmSource.Stop();
	}
}
