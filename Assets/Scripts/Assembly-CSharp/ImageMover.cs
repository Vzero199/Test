using UnityEngine;
using UnityEngine.InputSystem;

public class ImageMover : MonoBehaviour
{
	public float moveSpeed = 5f;

	public RectTransform imageRectTransform;

	public Canvas canvas;

	private void Awake()
	{
		if (imageRectTransform == null)
		{
			imageRectTransform = GetComponent<RectTransform>();
		}
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		if (context.performed || context.canceled)
		{
			float y = context.ReadValue<Vector2>().y;
			Debug.Log($"Current Direction: {y}");
			MoveImage(y);
		}
	}

	private void MoveImage(float direction)
	{
		float value = imageRectTransform.anchoredPosition.y + direction * moveSpeed * Time.deltaTime;
		RectTransform component = canvas.GetComponent<RectTransform>();
		float min = (0f - component.rect.height) / 2f + imageRectTransform.rect.height / 2f;
		float max = component.rect.height / 2f - imageRectTransform.rect.height / 2f;
		value = Mathf.Clamp(value, min, max);
		imageRectTransform.anchoredPosition = new Vector2(imageRectTransform.anchoredPosition.x, value);
	}
}
