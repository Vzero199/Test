using UnityEngine;

public class FadeAway : MonoBehaviour
{
	[SerializeField]
	private float fadeTime = 1f;

	[SerializeField]
	private bool destroyOnFadeComplete = true;

	private SpriteRenderer spriteRenderer;

	private float currentFadeTime;

	private Color originalColor;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			Debug.LogError("FadeAway: SpriteRenderer не найден на объекте " + base.gameObject.name);
			base.enabled = false;
		}
		else
		{
			originalColor = spriteRenderer.color;
			currentFadeTime = fadeTime;
		}
	}

	private void Update()
	{
		if (spriteRenderer == null)
		{
			return;
		}
		if (NightManager.GetInstance() != null && NightManager.GetInstance().StopGameCoroutine != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		currentFadeTime -= Time.deltaTime;
		if (currentFadeTime > 0f)
		{
			float a = currentFadeTime / fadeTime;
			Color color = originalColor;
			color.a = a;
			spriteRenderer.color = color;
		}
		else if (destroyOnFadeComplete)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			base.enabled = false;
		}
	}

	public void StartFading(float time)
	{
		fadeTime = time;
		currentFadeTime = fadeTime;
		base.enabled = true;
	}

	public void FadeImmediately()
	{
		if (spriteRenderer != null)
		{
			Color color = originalColor;
			color.a = 0f;
			spriteRenderer.color = color;
		}
		if (destroyOnFadeComplete)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			base.enabled = false;
		}
	}
}
