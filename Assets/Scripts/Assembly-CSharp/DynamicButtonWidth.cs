using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DynamicButtonWidth : MonoBehaviour
{
	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private float minWidth = 480f;

	[SerializeField]
	private float padding = 20f;

	[SerializeField]
	private float minFontSize = 12f;

	[SerializeField]
	private float maxFontSize = 100f;

	[SerializeField]
	private float fontSizeTolerance = 0.5f;

	private RectTransform rectTransform;

	private Coroutine updateCoroutine;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		if (text == null)
		{
			Debug.LogError("Text component is not assigned.");
			base.enabled = false;
		}
		else
		{
			text.enableAutoSizing = false;
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
		}
	}

	private void OnDestroy()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
	}

	private void OnTextChanged(Object changed)
	{
		if (changed == text)
		{
			if (updateCoroutine != null)
			{
				StopCoroutine(updateCoroutine);
			}
			updateCoroutine = StartCoroutine(DelayedUpdateLayout());
		}
	}

	private IEnumerator DelayedUpdateLayout()
	{
		yield return new WaitForEndOfFrame();
		float b = text.preferredWidth + padding;
		float size = Mathf.Max(minWidth, b);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		FitFontSizeToHeight();
		updateCoroutine = null;
	}

	private void FitFontSizeToHeight()
	{
		float height = rectTransform.rect.height;
		float num = minFontSize;
		float num2 = maxFontSize;
		float fontSize = num;
		while (num2 - num > fontSizeTolerance)
		{
			float num3 = (num + num2) / 2f;
			text.fontSize = num3;
			text.ForceMeshUpdate();
			if (text.textBounds.size.y > height)
			{
				num2 = num3;
				continue;
			}
			fontSize = num3;
			num = num3;
		}
		text.fontSize = fontSize;
	}
}
