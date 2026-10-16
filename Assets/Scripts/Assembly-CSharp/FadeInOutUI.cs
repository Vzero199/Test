using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOutUI : MonoBehaviour
{
	public float fadeDuration = 2f;

	public float visibleDuration = 2f;

	private Image image;

	private TextMeshProUGUI text;

	private CanvasGroup canvasGroup;

	private void Awake()
	{
		image = GetComponent<Image>();
		text = GetComponentInChildren<TextMeshProUGUI>();
		canvasGroup = base.gameObject.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		canvasGroup.alpha = 0f;
	}

	private void OnEnable()
	{
		StartCoroutine(FadeRoutine());
	}

	private IEnumerator FadeRoutine()
	{
		yield return Fade(0f, 1f, fadeDuration);
		yield return new WaitForSeconds(visibleDuration);
		yield return Fade(1f, 0f, fadeDuration);
	}

	private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
	}
}
