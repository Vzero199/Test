using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HintUIController : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private Coroutine fadeCoroutine;

	private bool hintShown;

	private bool isHiding;

	[SerializeField]
	private InputActionReference hideHintAction;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			Debug.LogError("CanvasGroup not found on Hint UI.");
		}
		PauseMenu.OnPauseTriggered += OnPauseExternally;
	}

	private void OnEnable()
	{
		if (hideHintAction != null)
		{
			hideHintAction.action.performed += HideHint;
			hideHintAction.action.Enable();
		}
	}

	private void OnDestroy()
	{
		PauseMenu.OnPauseTriggered -= OnPauseExternally;
	}

	private void OnPauseExternally()
	{
		if (hintShown && !isHiding)
		{
			Hide();
		}
	}

	private void OnDisable()
	{
		if (hideHintAction != null)
		{
			hideHintAction.action.performed -= HideHint;
			hideHintAction.action.Disable();
		}
	}

	public void HideHint(InputAction.CallbackContext context)
	{
		if (context.performed && hintShown && !isHiding)
		{
			Hide();
		}
	}

	private void Hide()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		isHiding = true;
		fadeCoroutine = StartCoroutine(FadeOutAndDisable());
	}

	private IEnumerator FadeOutAndDisable()
	{
		float duration = 0.3f;
		float time = 0f;
		float startAlpha = canvasGroup.alpha;
		while (time < duration)
		{
			canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
			time += Time.unscaledDeltaTime;
			yield return null;
		}
		canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		if (!PauseMenu.IsPaused)
		{
			Time.timeScale = 1f;
		}
		hintShown = false;
		isHiding = false;
	}

	public void SetHintShown(bool value)
	{
		hintShown = value;
	}
}
