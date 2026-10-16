using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonTextColorController : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public Color normalColor = Color.white;

	public Color highlightedColor = Color.yellow;

	public Color pressedColor = Color.green;

	public Color disabledColor = Color.gray;

	private Button button;

	private TextMeshProUGUI text;

	private void Awake()
	{
		button = GetComponent<Button>();
		text = GetComponentInChildren<TextMeshProUGUI>();
		if (text != null)
		{
			text.fontMaterial = new Material(text.fontMaterial);
		}
		if (button != null && text != null)
		{
			button.onClick.AddListener(OnClick);
		}
	}

	private void OnEnable()
	{
		UpdateButtonColor();
	}

	private void UpdateButtonColor()
	{
		if (!(button == null) && !(text == null))
		{
			if (button.interactable)
			{
				ApplyColor(normalColor);
			}
			else
			{
				ApplyColor(disabledColor);
			}
		}
	}

	private void Update()
	{
		if (EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			ApplyColor(highlightedColor);
		}
	}

	private void OnClick()
	{
		ApplyColor(pressedColor);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ApplyColor(highlightedColor);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnSelect(BaseEventData eventData)
	{
		ApplyColor(highlightedColor);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UpdateButtonColor();
	}

	private void ApplyColor(Color color)
	{
		if (!(text == null))
		{
			text.color = color;
		}
	}
}
