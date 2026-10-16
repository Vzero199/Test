using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class ToggleUIVisibility : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup targetGroup;

	[SerializeField]
	private float fadeDuration = 0.3f;

	[SerializeField]
	private PlayableDirector timeline;

	[SerializeField]
	private bool isVisible;

	private bool isFading;

	private bool wasHiddenByTimeline;

	private void OnEnable()
	{
		if (timeline != null)
		{
			timeline.played += OnTimelineStarted;
			timeline.stopped += OnTimelineStopped;
		}
	}

	private void OnDisable()
	{
		if (timeline != null)
		{
			timeline.played -= OnTimelineStarted;
			timeline.stopped -= OnTimelineStopped;
		}
	}

	public void OnToggle(InputAction.CallbackContext context)
	{
		if (context.performed && Time.timeScale != 0f && (!(timeline != null) || timeline.state != PlayState.Playing) && !isFading)
		{
			if (isVisible)
			{
				StartCoroutine(FadeOut());
			}
			else
			{
				StartCoroutine(FadeIn());
			}
		}
	}

	private IEnumerator FadeIn()
	{
		isFading = true;
		targetGroup.interactable = true;
		targetGroup.blocksRaycasts = true;
		float time = 0f;
		while (time < fadeDuration)
		{
			time += Time.unscaledDeltaTime;
			targetGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
			yield return null;
		}
		targetGroup.alpha = 1f;
		isVisible = true;
		isFading = false;
	}

	private IEnumerator FadeOut()
	{
		isFading = true;
		float time = 0f;
		while (time < fadeDuration)
		{
			time += Time.unscaledDeltaTime;
			targetGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
			yield return null;
		}
		targetGroup.alpha = 0f;
		targetGroup.interactable = false;
		targetGroup.blocksRaycasts = false;
		isVisible = false;
		isFading = false;
	}

	private void OnTimelineStarted(PlayableDirector director)
	{
		if (isVisible && !isFading)
		{
			wasHiddenByTimeline = true;
			StartCoroutine(FadeOut());
		}
	}

	private void OnTimelineStopped(PlayableDirector director)
	{
		if (wasHiddenByTimeline && !isVisible && !isFading)
		{
			wasHiddenByTimeline = false;
			StartCoroutine(FadeIn());
		}
	}
}
