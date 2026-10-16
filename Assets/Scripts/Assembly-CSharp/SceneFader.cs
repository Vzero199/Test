using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
	public Image fadeImage;

	public float fadeDuration = 2f;

	public float delayBeforeFade = 2f;

	private void Awake()
	{
		fadeImage.color = new Color(0f, 0f, 0f, 1f);
	}

	private void Start()
	{
		StartCoroutine(DelayBeforeFadeOut());
	}

	private IEnumerator DelayBeforeFadeOut()
	{
		yield return new WaitForSeconds(delayBeforeFade);
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		float elapsedTime = 0f;
		Color fadeColor = fadeImage.color;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			fadeColor.a = Mathf.Clamp01(1f - elapsedTime / fadeDuration);
			fadeImage.color = fadeColor;
			yield return null;
		}
		fadeColor.a = 0f;
		fadeImage.color = fadeColor;
	}
}
