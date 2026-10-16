using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextOverflowController : MonoBehaviour
{
	[SerializeField]
	private RectTransform containerRect;

	[SerializeField]
	private TextMeshProUGUI dynamicText;

	[SerializeField]
	private RectTransform pinnedRect;

	[SerializeField]
	private float pinnedOffsetY;

	private RectTransform textRect;

	private void Awake()
	{
		if (dynamicText != null)
		{
			textRect = dynamicText.GetComponent<RectTransform>();
		}
	}

	private void Update()
	{
		if (!(dynamicText == null) && !(containerRect == null) && !(pinnedRect == null) && !(textRect == null))
		{
			dynamicText.ForceMeshUpdate();
			float preferredHeight = dynamicText.preferredHeight;
			float height = containerRect.rect.height;
			Vector2 zero = Vector2.zero;
			if (preferredHeight <= height)
			{
				textRect.anchoredPosition = zero;
			}
			else
			{
				float num = preferredHeight - height;
				textRect.anchoredPosition = new Vector2(zero.x, zero.y + num);
			}
			float y = textRect.anchoredPosition.y;
			Vector2 anchoredPosition = pinnedRect.anchoredPosition;
			anchoredPosition.y = y - pinnedOffsetY;
			pinnedRect.anchoredPosition = anchoredPosition;
		}
	}
}
