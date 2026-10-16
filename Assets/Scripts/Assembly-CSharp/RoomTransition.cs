using System.Collections;
using UnityEngine;

public class RoomTransition : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	private Coroutine fadeCoroutine;

	[SerializeField]
	private float fadeSpeed = 1.5f;

	[SerializeField]
	private float insideAlpha;

	[SerializeField]
	private float defaultAlpha = 0.99f;

	private bool isNightCoroutineActive;

	private bool playerIsInside;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		SetAlpha(defaultAlpha);
	}

	private void Update()
	{
		bool flag = NightManager.GetInstance() != null && NightManager.GetInstance().StopGameCoroutine != null;
		if (flag != isNightCoroutineActive)
		{
			isNightCoroutineActive = flag;
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
				fadeCoroutine = null;
			}
			if (isNightCoroutineActive)
			{
				SetAlpha(insideAlpha);
			}
			else
			{
				SetAlpha(playerIsInside ? insideAlpha : defaultAlpha);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerIsInside = true;
			if (!isNightCoroutineActive)
			{
				FadeToAlpha(insideAlpha);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			playerIsInside = false;
			if (!isNightCoroutineActive)
			{
				FadeToAlpha(defaultAlpha);
			}
		}
	}

	private void FadeToAlpha(float targetAlpha)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
	}

	private IEnumerator FadeCoroutine(float targetAlpha)
	{
		float startAlpha = spriteRenderer.color.a;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * fadeSpeed;
			float t2 = Mathf.SmoothStep(0f, 1f, t);
			float alpha = Mathf.Lerp(startAlpha, targetAlpha, t2);
			SetAlpha(alpha);
			yield return null;
		}
		SetAlpha(targetAlpha);
	}

	private void SetAlpha(float alpha)
	{
		Color color = spriteRenderer.color;
		color.a = alpha;
		spriteRenderer.color = color;
	}
}
