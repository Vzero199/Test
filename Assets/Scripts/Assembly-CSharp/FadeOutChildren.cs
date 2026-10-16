using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutChildren : MonoBehaviour
{
	[Header("Настройки исчезновения")]
	[SerializeField]
	private bool destroyAfterFade;

	private List<SpriteRenderer> allSpriteRenderers = new List<SpriteRenderer>();

	private Coroutine fadeCoroutine;

	private void Start()
	{
		GetAllSpriteRenderersRecursive(base.transform);
	}

	public void StartFadeOut(float fadeDuration)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeOutCoroutine(fadeDuration));
	}

	private IEnumerator FadeOutCoroutine(float customDuration = -1f)
	{
		float duration = ((customDuration > 0f) ? customDuration : 10f);
		float timer = 0f;
		Dictionary<SpriteRenderer, float> initialAlphas = new Dictionary<SpriteRenderer, float>();
		foreach (SpriteRenderer allSpriteRenderer in allSpriteRenderers)
		{
			if (allSpriteRenderer != null)
			{
				initialAlphas[allSpriteRenderer] = allSpriteRenderer.color.a;
			}
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float t = timer / duration;
			foreach (SpriteRenderer allSpriteRenderer2 in allSpriteRenderers)
			{
				if (allSpriteRenderer2 != null)
				{
					Color color = allSpriteRenderer2.color;
					color.a = Mathf.Lerp(initialAlphas[allSpriteRenderer2], 0f, t);
					allSpriteRenderer2.color = color;
				}
			}
			yield return null;
		}
		foreach (SpriteRenderer allSpriteRenderer3 in allSpriteRenderers)
		{
			if (allSpriteRenderer3 != null)
			{
				Color color2 = allSpriteRenderer3.color;
				color2.a = 0f;
				allSpriteRenderer3.color = color2;
			}
		}
		if (destroyAfterFade)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void ResetAlpha()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
		foreach (SpriteRenderer allSpriteRenderer in allSpriteRenderers)
		{
			if (allSpriteRenderer != null)
			{
				Color color = allSpriteRenderer.color;
				color.a = 1f;
				allSpriteRenderer.color = color;
			}
		}
	}

	private void GetAllSpriteRenderersRecursive(Transform parent)
	{
		foreach (Transform item in parent)
		{
			SpriteRenderer component = item.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				allSpriteRenderers.Add(component);
			}
			if (item.childCount > 0)
			{
				GetAllSpriteRenderersRecursive(item);
			}
		}
	}

	public void RefreshSpriteRenderers()
	{
		allSpriteRenderers.Clear();
		GetAllSpriteRenderersRecursive(base.transform);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position, Vector3.one);
	}
}
