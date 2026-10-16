using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseScroll : MonoBehaviour
{
	public ScrollRect scrollRect;

	public float scrollSpeed = 0.5f;

	public float keyScrollSpeed = 0.3f;

	private float keyboardScrollInput;

	private float targetScrollPosition;

	private float scrollVelocity;

	private float smoothTime = 0.1f;

	[SerializeField]
	private bool startFromBottom = true;

	[Header("Arrow Hint (Optional)")]
	[SerializeField]
	private CanvasGroup arrowHint;

	[SerializeField]
	private float fadeSpeed = 2f;

	[SerializeField]
	private float scrollThreshold = 50f;

	[SerializeField]
	[Range(0f, 1f)]
	private float minAlphaWhileBlinking = 0.3f;

	private float accumulatedScroll;

	private bool hintHidden;

	private bool fadingIn = true;

	[SerializeField]
	private float holdBrightDuration = 0.5f;

	[SerializeField]
	private float holdDimDuration = 0.2f;

	private float holdTimer;

	private bool isHolding;

	private void Start()
	{
		if (startFromBottom)
		{
			scrollRect.verticalNormalizedPosition = 0f;
			targetScrollPosition = 0f;
		}
		else
		{
			scrollRect.verticalNormalizedPosition = 1f;
			targetScrollPosition = 1f;
		}
		if (arrowHint != null)
		{
			arrowHint.alpha = minAlphaWhileBlinking;
			arrowHint.gameObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		float num = 0f;
		Vector2 vector = Mouse.current.delta.ReadValue();
		if (vector.y != 0f)
		{
			float num2 = vector.y * scrollSpeed * Time.deltaTime;
			num += Mathf.Abs(num2);
			targetScrollPosition = Mathf.Clamp01(targetScrollPosition + num2);
		}
		if (keyboardScrollInput != 0f)
		{
			float num3 = keyboardScrollInput * keyScrollSpeed * Time.deltaTime;
			num += Mathf.Abs(num3);
			targetScrollPosition = Mathf.Clamp01(targetScrollPosition + num3);
		}
		scrollRect.verticalNormalizedPosition = Mathf.SmoothDamp(scrollRect.verticalNormalizedPosition, targetScrollPosition, ref scrollVelocity, smoothTime);
		if (!hintHidden && arrowHint != null)
		{
			float height = scrollRect.content.rect.height;
			float height2 = scrollRect.viewport.rect.height;
			float num4 = height - height2;
			float num5 = num * num4;
			accumulatedScroll += num5;
			if (accumulatedScroll >= scrollThreshold)
			{
				arrowHint.alpha = 0f;
				arrowHint.gameObject.SetActive(value: false);
				hintHidden = true;
			}
			else
			{
				BlinkArrow();
			}
		}
	}

	private void BlinkArrow()
	{
		if (arrowHint == null)
		{
			return;
		}
		float num = (fadingIn ? 1f : minAlphaWhileBlinking);
		float num2 = (fadingIn ? holdBrightDuration : holdDimDuration);
		if (isHolding)
		{
			holdTimer += Time.deltaTime;
			if (holdTimer >= num2)
			{
				holdTimer = 0f;
				isHolding = false;
				fadingIn = !fadingIn;
			}
		}
		else
		{
			arrowHint.alpha = Mathf.MoveTowards(arrowHint.alpha, num, fadeSpeed * Time.deltaTime);
			if (Mathf.Abs(arrowHint.alpha - num) < 0.01f)
			{
				arrowHint.alpha = num;
				isHolding = true;
			}
		}
	}

	public void OnScrollInput(InputAction.CallbackContext context)
	{
		keyboardScrollInput = context.ReadValue<Vector2>().y;
	}
}
