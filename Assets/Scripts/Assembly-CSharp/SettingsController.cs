using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
	public Button[] settingsButtons;

	private int currentButtonIndex;

	private bool isCursorOverButton;

	private PlayerInput playerInput;

	private InputAction navigateAction;

	private InputAction selectAction;

	private void Awake()
	{
		playerInput = GetComponent<PlayerInput>();
		navigateAction = playerInput.actions["Navigate"];
		selectAction = playerInput.actions["Submit"];
	}

	private void OnEnable()
	{
		navigateAction.performed += OnNavigate;
		selectAction.performed += OnSubmit;
		if (settingsButtons.Length != 0)
		{
			SelectButton(settingsButtons[currentButtonIndex]);
		}
		for (int i = 0; i < settingsButtons.Length; i++)
		{
			int index = i;
			EventTrigger eventTrigger = settingsButtons[i].gameObject.AddComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((BaseEventData eventData) =>
			{
				OnPointerEnterButton(index);
			});
			eventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback.AddListener((BaseEventData eventData) =>
			{
				OnPointerExitButton();
			});
			eventTrigger.triggers.Add(entry2);
		}
	}

	private void OnDisable()
	{
		navigateAction.performed -= OnNavigate;
		selectAction.performed -= OnSubmit;
	}

	private void OnNavigate(InputAction.CallbackContext context)
	{
		if (!isCursorOverButton)
		{
			Vector2 vector = context.ReadValue<Vector2>();
			if (vector.y > 0f)
			{
				Navigate(-1);
			}
			else if (vector.y < 0f)
			{
				Navigate(1);
			}
		}
	}

	private void OnSubmit(InputAction.CallbackContext context)
	{
		if (!isCursorOverButton)
		{
			StartCoroutine(PressButton(settingsButtons[currentButtonIndex]));
		}
	}

	private void Navigate(int direction)
	{
		EventSystem.current.SetSelectedGameObject(null);
		currentButtonIndex = (currentButtonIndex + direction + settingsButtons.Length) % settingsButtons.Length;
		SelectButton(settingsButtons[currentButtonIndex]);
	}

	private void SelectButton(Button button)
	{
		EventSystem.current.SetSelectedGameObject(button.gameObject);
		button.Select();
	}

	private IEnumerator PressButton(Button button)
	{
		button.OnPointerDown(new PointerEventData(EventSystem.current));
		yield return new WaitForSeconds(0.1f);
		button.OnPointerUp(new PointerEventData(EventSystem.current));
		button.onClick.Invoke();
	}

	private void OnPointerEnterButton(int index)
	{
		isCursorOverButton = true;
		currentButtonIndex = index;
		SelectButton(settingsButtons[currentButtonIndex]);
	}

	private void OnPointerExitButton()
	{
		isCursorOverButton = false;
	}
}
