using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FitTextWidth : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private float padding;

	private RectTransform rectTransform;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	private void Start()
	{
		UpdateWidth();
	}

	public void UpdateWidth()
	{
		if (!(text == null))
		{
			text.ForceMeshUpdate();
			float preferredWidth = text.preferredWidth;
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + padding);
		}
	}
}
