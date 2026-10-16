using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFadeIn : MonoBehaviour
{
	[SerializeField]
	private float fadeDuration = 3f;

	private CanvasGroup canvasGroup;

	private Coroutine fadeCoroutine;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		canvasGroup.alpha = 0f;
		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
			yield return null;
		}
		canvasGroup.alpha = 1f;
		fadeCoroutine = null;
	}
}
