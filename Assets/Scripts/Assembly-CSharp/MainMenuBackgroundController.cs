using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBackgroundController : MonoBehaviour
{
	[SerializeField]
	private Sprite nightSprite;

	[SerializeField]
	private Sprite daySprite;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image eyesBlink;

	[SerializeField]
	private GameObject eyesAnimator;

	[SerializeField]
	private AudioClip dayAmbince;

	[SerializeField]
	private AudioClip nightAmbince;

	[SerializeField]
	private ParticleSystem dayParticles;

	public AudioSource audioSource;

	[SerializeField]
	private ScrollRect blinkEyes;

	[SerializeField]
	private ScrollRect secondBlinkEyes;

	[SerializeField]
	private float blinkDuration = 0.4f;

	[SerializeField]
	private float minBlinkInterval = 8f;

	[SerializeField]
	private float maxBlinkInterval = 15f;

	private Animator backgroundAnimator;

	private void Start()
	{
		backgroundAnimator = background.GetComponent<Animator>();
		SetBackgroundSprite();
		StartCoroutine(MoveBlinkEyes());
	}

	public void SetBackgroundSprite()
	{
		if (background == null)
		{
			return;
		}
		if (GameSaveManager.IsNightSaveExists())
		{
			if (audioSource.clip != nightAmbince)
			{
				audioSource.clip = nightAmbince;
			}
			background.sprite = nightSprite;
			eyesBlink.color = Color.black;
			if (eyesAnimator != null)
			{
				eyesAnimator.SetActive(value: true);
			}
			DisableAnimator();
		}
		else
		{
			background.sprite = daySprite;
			EnableAnimator();
			if (dayParticles != null)
			{
				ParticleSystem.MainModule main = dayParticles.main;
				Color color = main.startColor.color;
				color.a = 0.7803922f;
				main.startColor = color;
			}
		}
		StartCoroutine(PlayAudioAfterDelay());
	}

	private IEnumerator PlayAudioAfterDelay()
	{
		yield return new WaitForSeconds(1.5f);
		audioSource.volume = 0f;
		audioSource.Play();
		float targetVolume = 0.017f;
		float fadeDuration = 8f;
		float elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeDuration);
			yield return null;
		}
		audioSource.volume = targetVolume;
	}

	private void DisableAnimator()
	{
		if (backgroundAnimator != null)
		{
			backgroundAnimator.enabled = false;
		}
	}

	private void EnableAnimator()
	{
		if (backgroundAnimator != null)
		{
			backgroundAnimator.enabled = true;
		}
	}

	private IEnumerator MoveBlinkEyes()
	{
		while (true)
		{
			float seconds = Random.Range(minBlinkInterval, maxBlinkInterval);
			yield return new WaitForSeconds(seconds);
			StartCoroutine(MoveToPosition(blinkEyes.content.anchoredPosition.y - blinkEyes.content.rect.height, blinkEyes));
			yield return StartCoroutine(MoveToPosition(secondBlinkEyes.content.anchoredPosition.y - secondBlinkEyes.content.rect.height, secondBlinkEyes));
			StartCoroutine(MoveToPosition(blinkEyes.content.anchoredPosition.y + blinkEyes.content.rect.height, blinkEyes));
			yield return StartCoroutine(MoveToPosition(secondBlinkEyes.content.anchoredPosition.y + secondBlinkEyes.content.rect.height, secondBlinkEyes));
		}
	}

	private IEnumerator MoveToPosition(float targetY, ScrollRect scrollRect)
	{
		float elapsedTime = 0f;
		float duration = blinkDuration;
		Vector2 startPosition = scrollRect.content.anchoredPosition;
		Vector2 targetPosition = new Vector2(startPosition.x, targetY);
		while (elapsedTime < duration)
		{
			scrollRect.content.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		scrollRect.content.anchoredPosition = targetPosition;
	}
}
