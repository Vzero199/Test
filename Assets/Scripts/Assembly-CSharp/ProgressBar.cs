using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	private float maxValue = 1f;

	private Image progressBarImage;

	private CanvasGroup canvasGroup;

	[SerializeField]
	private Sprite emptyBarSprite;

	[SerializeField]
	private Sprite fullBarSprite;

	private Image rollbackImage;

	public void Awake()
	{
		canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		Image component = GetComponent<Image>();
		if (component != null)
		{
			canvasGroup.alpha = component.color.a;
			Color color = component.color;
			component.enabled = false;
			GameObject obj = new GameObject(emptyBarSprite.name);
			obj.transform.SetParent(base.transform);
			Image image = obj.AddComponent<Image>();
			image.sprite = emptyBarSprite;
			image.preserveAspect = true;
			image.color = color;
			obj.transform.localRotation = Quaternion.identity;
			RectTransform component2 = obj.GetComponent<RectTransform>();
			component2.anchorMin = Vector2.zero;
			component2.anchorMax = Vector2.one;
			component2.localScale = Vector3.one;
			component2.sizeDelta = Vector2.zero;
			component2.anchoredPosition = Vector3.zero;
			GameObject gameObject = new GameObject(fullBarSprite.name);
			gameObject.transform.SetParent(base.transform);
			progressBarImage = gameObject.AddComponent<Image>();
			progressBarImage.sprite = fullBarSprite;
			progressBarImage.type = Image.Type.Filled;
			progressBarImage.fillMethod = Image.FillMethod.Horizontal;
			progressBarImage.preserveAspect = true;
			progressBarImage.color = color;
			gameObject.AddComponent<Mask>().showMaskGraphic = true;
			GameObject gameObject2 = new GameObject("RollbackZone");
			gameObject2.transform.SetParent(gameObject.transform);
			rollbackImage = gameObject2.AddComponent<Image>();
			rollbackImage.sprite = fullBarSprite;
			rollbackImage.type = Image.Type.Filled;
			rollbackImage.fillMethod = Image.FillMethod.Horizontal;
			rollbackImage.fillOrigin = 1;
			rollbackImage.color = Color.blue;
			rollbackImage.preserveAspect = true;
			RectTransform component3 = gameObject2.GetComponent<RectTransform>();
			component3.anchorMin = Vector2.zero;
			component3.anchorMax = Vector2.one;
			component3.localScale = Vector3.one;
			component3.sizeDelta = Vector2.zero;
			component3.anchoredPosition = Vector3.zero;
			rollbackImage.fillAmount = 0f;
			RectTransform component4 = gameObject.GetComponent<RectTransform>();
			component4.anchorMin = Vector2.zero;
			component4.anchorMax = Vector2.one;
			component4.localScale = Vector3.one;
			component4.sizeDelta = Vector2.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			component4.anchoredPosition = Vector3.zero;
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Debug.LogError("Image component not found on this GameObject.");
		}
	}

	public void SetRollback(float amount)
	{
		if (rollbackImage != null)
		{
			rollbackImage.fillAmount = Mathf.Clamp01(amount);
		}
	}

	public void UpdateProgress(float barValue)
	{
		if (!(progressBarImage == null))
		{
			float fillAmount = barValue / maxValue;
			progressBarImage.fillAmount = fillAmount;
		}
	}

	public void ShowBar()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		StartCoroutine(FadeInBar());
	}

	private IEnumerator FadeInBar()
	{
		float duration = 1f;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	public IEnumerator FadeOutBar()
	{
		float duration = 1f;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			canvasGroup.alpha = Mathf.Clamp01(1f - elapsedTime / duration);
			yield return null;
		}
		canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
	}

	public float GetProgress()
	{
		return progressBarImage.fillAmount;
	}
}
