using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
	private Vector2 moveDirection = Vector2.zero;

	private bool jumpPressed;

	private bool interactPressed;

	private bool submitPressed;

	private bool useItemPressed;

	private bool consolePressed;

	private static InputManager instance;

	private void Awake()
	{
		if (instance != null)
		{
			Debug.LogError("Found more than one Input Manager in the scene.");
		}
		instance = this;
	}

	public static InputManager GetInstance()
	{
		return instance;
	}

	public void MovePressed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			moveDirection = context.ReadValue<Vector2>();
		}
		else if (context.canceled)
		{
			moveDirection = context.ReadValue<Vector2>();
		}
	}

	public void JumpPressed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			jumpPressed = true;
		}
		else if (context.canceled)
		{
			jumpPressed = false;
		}
	}

	public void InteractButtonPressed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			interactPressed = true;
		}
		else if (context.canceled)
		{
			interactPressed = false;
		}
	}

	public void SubmitPressed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			submitPressed = true;
		}
		else if (context.canceled)
		{
			submitPressed = false;
		}
	}

	public Vector2 GetMoveDirection()
	{
		return moveDirection;
	}

	public bool GetJumpPressed()
	{
		bool result = jumpPressed;
		jumpPressed = false;
		return result;
	}

	public bool GetInteractPressed()
	{
		bool result = interactPressed;
		interactPressed = false;
		return result;
	}

	public bool GetSubmitPressed()
	{
		bool result = submitPressed;
		submitPressed = false;
		return result;
	}

	public bool GetUseItemPressed()
	{
		bool result = useItemPressed;
		useItemPressed = false;
		return result;
	}

	public bool GetConsolePressed()
	{
		bool result = consolePressed;
		consolePressed = false;
		return result;
	}

	public void RegisterSubmitPressed()
	{
		submitPressed = false;
	}
}
