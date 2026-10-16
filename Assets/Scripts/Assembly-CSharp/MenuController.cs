using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
	public Button[] menuButtons;

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
		if (menuButtons.Length != 0)
		{
			SelectButton(menuButtons[currentButtonIndex]);
		}
		for (int i = 0; i < menuButtons.Length; i++)
		{
			int index = i;
			EventTrigger eventTrigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();
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

	public void OnNavigate(InputAction.CallbackContext context)
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
			StartCoroutine(PressButton(menuButtons[currentButtonIndex]));
		}
	}

	private void Navigate(int direction)
	{
		EventSystem.current.SetSelectedGameObject(null);
		currentButtonIndex = (currentButtonIndex + direction + menuButtons.Length) % menuButtons.Length;
		SelectButton(menuButtons[currentButtonIndex]);
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
		SelectButton(menuButtons[currentButtonIndex]);
	}

	private void OnPointerExitButton()
	{
		isCursorOverButton = false;
	}
}
