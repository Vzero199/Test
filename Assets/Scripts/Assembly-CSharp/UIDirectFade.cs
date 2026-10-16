using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDirectFade : MonoBehaviour
{
	[SerializeField]
	private float fadeDuration = 1f;

	private List<Graphic> graphics = new List<Graphic>();

	private float elapsed;

	private void Awake()
	{
		GetComponentsInChildren(includeInactive: true, graphics);
	}

	private void OnEnable()
	{
		elapsed = 0f;
		foreach (Graphic graphic in graphics)
		{
			graphic.canvasRenderer.SetAlpha(0f);
		}
		StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			float alpha = Mathf.Clamp01(elapsed / fadeDuration);
			foreach (Graphic graphic in graphics)
			{
				graphic.canvasRenderer.SetAlpha(alpha);
			}
			yield return null;
		}
	}
}
