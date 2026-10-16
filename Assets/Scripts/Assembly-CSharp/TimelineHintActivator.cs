using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class TimelineHintActivator : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector timeline;

	[SerializeField]
	private CanvasGroup hintCanvasGroup;

	[SerializeField]
	private string HintType;

	private bool hintShown;

	private bool isHiding;

	private Coroutine fadeCoroutine;

	private void Awake()
	{
		if (GlobalSaveSystem.GetProgress(HintType))
		{
			hintCanvasGroup.gameObject.SetActive(value: false);
			return;
		}
		timeline.stopped += OnTimelineEnd;
		PauseMenu.OnPauseTriggered += OnPauseExternally;
	}

	private void OnDestroy()
	{
		if (timeline != null)
		{
			timeline.stopped -= OnTimelineEnd;
		}
		PauseMenu.OnPauseTriggered -= OnPauseExternally;
	}

	private void OnPauseExternally()
	{
		if (hintShown && !isHiding)
		{
			Hide();
		}
	}

	private void OnTimelineEnd(PlayableDirector director)
	{
		if (!hintShown)
		{
			hintCanvasGroup.gameObject.SetActive(value: true);
			Time.timeScale = 0f;
			fadeCoroutine = StartCoroutine(FadeCanvasGroup(hintCanvasGroup, 0f, 1f, 0.5f));
			hintShown = true;
			isHiding = false;
			GlobalSaveSystem.SetProgress(HintType, value: true);
			timeline.stopped -= OnTimelineEnd;
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
		isHiding = true;
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeOutAndDisable());
	}

	private IEnumerator FadeOutAndDisable()
	{
		yield return FadeCanvasGroup(hintCanvasGroup, 1f, 0f, 0.5f);
		hintCanvasGroup.gameObject.SetActive(value: false);
		GlobalSaveSystem.SetProgress(HintType, value: true);
		if (!PauseMenu.IsPaused)
		{
			Time.timeScale = 1f;
		}
		hintShown = false;
		isHiding = false;
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			time += Time.unscaledDeltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
	}
}
