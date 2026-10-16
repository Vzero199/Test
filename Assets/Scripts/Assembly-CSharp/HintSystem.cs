using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class HintSystem : MonoBehaviour
{
	[Serializable]
	public class Hint
	{
		public string hintName;

		public CanvasGroup canvasGroup;

		public HintType type;

		public float displayDuration;

		[Header("Time-based Settings")]
		public float activationDelay;

		public bool useRealtimeForDelay = true;

		[Header("Room-based Settings")]
		public string requiredRoom;

		[Header("Event-based Settings")]
		public string requiredEvent;

		[Header("Timeline-based Settings")]
		public PlayableDirector timeline;

		[NonSerialized]
		public bool isShown;

		[NonSerialized]
		public bool isHiding;

		[NonSerialized]
		public Coroutine currentCoroutine;
	}

	public enum HintType
	{
		TimeBased = 0,
		RoomBased = 1,
		EventBased = 2,
		TimelineBased = 3,
		Manual = 4
	}

	[Header("Hint Settings")]
	[SerializeField]
	private Hint[] hints;

	[Header("Global Settings")]
	[SerializeField]
	private float fadeDuration = 0.5f;

	[SerializeField]
	private bool useTimeScale = true;

	[SerializeField]
	private bool debugMode;

	private Dictionary<string, Hint> hintDictionary;

	private Hint currentActiveHint;

	private void Awake()
	{
		InitializeHints();
		SubscribeToEvents();
	}

	private void InitializeHints()
	{
		hintDictionary = new Dictionary<string, Hint>();
		Hint[] array = hints;
		foreach (Hint hint in array)
		{
			if (GlobalSaveSystem.GetProgress("Hint_" + hint.hintName + "_Notified"))
			{
				if (hint.canvasGroup != null)
				{
					hint.canvasGroup.gameObject.SetActive(value: false);
					hint.canvasGroup.alpha = 0f;
				}
				hint.isShown = true;
				continue;
			}
			hintDictionary[hint.hintName] = hint;
			switch (hint.type)
			{
			case HintType.TimeBased:
				if (hint.activationDelay > 0f)
				{
					StartCoroutine(DelayedShowHint(hint));
				}
				else if (debugMode)
				{
					Debug.LogWarning("TimeBased hint " + hint.hintName + " has activationDelay = 0");
				}
				break;
			case HintType.TimelineBased:
				if (hint.timeline != null)
				{
					hint.timeline.stopped += (PlayableDirector director) =>
					{
						OnTimelineEnd(director, hint);
					};
				}
				else if (debugMode)
				{
					Debug.LogWarning("TimelineBased hint " + hint.hintName + " has no timeline");
				}
				break;
			}
		}
	}

	private void SubscribeToEvents()
	{
		Player.OnRoomChanged += CheckRoomForHint;
		PauseMenu.OnPauseTriggered += OnPauseExternally;
	}

	private void OnDestroy()
	{
		Player.OnRoomChanged -= CheckRoomForHint;
		PauseMenu.OnPauseTriggered -= OnPauseExternally;
		Hint[] array = hints;
		foreach (Hint hint in array)
		{
			if (hint.type == HintType.TimelineBased && hint.timeline != null)
			{
				hint.timeline.stopped -= (PlayableDirector director) =>
				{
					OnTimelineEnd(director, hint);
				};
			}
		}
	}

	private void OnPauseExternally()
	{
		if (currentActiveHint != null && !currentActiveHint.isHiding)
		{
			HideHint(currentActiveHint);
		}
	}

	private void CheckRoomForHint(string currentRoom)
	{
		Hint[] array = hints;
		foreach (Hint hint in array)
		{
			if (hint.type == HintType.RoomBased && hint.requiredRoom == currentRoom && !hint.isShown && !hint.isHiding && !GlobalSaveSystem.GetProgress("Hint_" + hint.hintName + "_Notified"))
			{
				ShowHint(hint);
			}
		}
	}

	private void OnTimelineEnd(PlayableDirector director, Hint hint)
	{
		if (!hint.isShown && !GlobalSaveSystem.GetProgress("Hint_" + hint.hintName + "_Notified"))
		{
			ShowHint(hint);
		}
	}

	public void ActivateHint(string hintName)
	{
		if (hintDictionary.TryGetValue(hintName, out var value) && (value.type == HintType.EventBased || value.type == HintType.Manual) && !value.isShown && !GlobalSaveSystem.GetProgress("Hint_" + value.hintName + "_Notified"))
		{
			ShowHint(value);
		}
	}

	public void ActivateHintByEvent(string eventName)
	{
		Hint[] array = hints;
		foreach (Hint hint in array)
		{
			if (hint.type == HintType.EventBased && hint.requiredEvent == eventName && !hint.isShown && !hint.isHiding && !GlobalSaveSystem.GetProgress("Hint_" + hint.hintName + "_Notified"))
			{
				ShowHint(hint);
				break;
			}
		}
	}

	private IEnumerator DelayedShowHint(Hint hint)
	{
		if (hint.useRealtimeForDelay)
		{
			yield return new WaitForSecondsRealtime(hint.activationDelay);
		}
		else
		{
			yield return new WaitForSeconds(hint.activationDelay);
		}
		if (!hint.isShown && !hint.isHiding && !GlobalSaveSystem.GetProgress("Hint_" + hint.hintName + "_Notified"))
		{
			ShowHint(hint);
		}
	}

	private bool CanShowHint(Hint hint)
	{
		if (currentActiveHint != null)
		{
			if (debugMode)
			{
				Debug.Log("Cannot show hint '" + hint.hintName + "' because hint '" + currentActiveHint.hintName + "' is already active");
			}
			return false;
		}
		if (hint.isShown || hint.isHiding)
		{
			return false;
		}
		if (GlobalSaveSystem.GetProgress("Hint_" + hint.hintName + "_Notified"))
		{
			return false;
		}
		return true;
	}

	private void ShowHint(Hint hint)
	{
		if (CanShowHint(hint))
		{
			if (debugMode)
			{
				Debug.Log("=== SHOW HINT: " + hint.hintName + " ===");
			}
			if (currentActiveHint != null)
			{
				ForceHideCurrentHint();
			}
			currentActiveHint = hint;
			hint.canvasGroup.gameObject.SetActive(value: true);
			if (useTimeScale)
			{
				Time.timeScale = 0f;
			}
			if (hint.currentCoroutine != null)
			{
				StopCoroutine(hint.currentCoroutine);
			}
			hint.currentCoroutine = StartCoroutine(FadeCanvasGroup(hint.canvasGroup, 0f, 1f, fadeDuration));
			hint.isShown = true;
			hint.isHiding = false;
			GlobalSaveSystem.SetProgress("Hint_" + hint.hintName + "_Notified", value: true);
			if (hint.displayDuration > 0f)
			{
				StartCoroutine(AutoHideHint(hint));
			}
		}
	}

	private void ForceHideCurrentHint()
	{
		if (currentActiveHint != null)
		{
			if (debugMode)
			{
				Debug.Log("Force hiding hint '" + currentActiveHint.hintName + "'");
			}
			if (currentActiveHint.currentCoroutine != null)
			{
				StopCoroutine(currentActiveHint.currentCoroutine);
			}
			currentActiveHint.canvasGroup.gameObject.SetActive(value: false);
			currentActiveHint.isShown = false;
			currentActiveHint.isHiding = false;
			currentActiveHint.canvasGroup.alpha = 0f;
			currentActiveHint = null;
		}
	}

	private IEnumerator AutoHideHint(Hint hint)
	{
		if (hint.useRealtimeForDelay)
		{
			yield return new WaitForSecondsRealtime(hint.displayDuration);
		}
		else
		{
			yield return new WaitForSeconds(hint.displayDuration);
		}
		if (hint.isShown && !hint.isHiding)
		{
			HideHint(hint);
		}
	}

	public void HideHint(InputAction.CallbackContext context)
	{
		if (context.performed && currentActiveHint != null && !currentActiveHint.isHiding)
		{
			HideHint(currentActiveHint);
		}
	}

	public void HideHint(string hintName)
	{
		if (hintDictionary.TryGetValue(hintName, out var value))
		{
			HideHint(value);
		}
	}

	private void HideHint(Hint hint)
	{
		if (hint.isShown && !hint.isHiding)
		{
			if (debugMode)
			{
				Debug.Log("Hide Hint: " + hint.hintName);
			}
			hint.isHiding = true;
			if (hint.currentCoroutine != null)
			{
				StopCoroutine(hint.currentCoroutine);
			}
			if (useTimeScale && !PauseMenu.IsPaused)
			{
				Time.timeScale = 1f;
			}
			hint.currentCoroutine = StartCoroutine(FadeOutAndDisable(hint));
		}
	}

	private IEnumerator FadeOutAndDisable(Hint hint)
	{
		yield return FadeCanvasGroup(hint.canvasGroup, 1f, 0f, fadeDuration);
		hint.canvasGroup.gameObject.SetActive(value: false);
		hint.isShown = false;
		hint.isHiding = false;
		if (currentActiveHint == hint)
		{
			currentActiveHint = null;
		}
		GlobalSaveSystem.SetProgress("Hint_" + hint.hintName + "_Notified", value: true);
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

	public bool IsHintShown(string hintName)
	{
		if (hintDictionary.TryGetValue(hintName, out var value))
		{
			return value.isShown;
		}
		return false;
	}

	public bool IsAnyHintActive()
	{
		return currentActiveHint != null;
	}

	public string GetCurrentActiveHint()
	{
		return currentActiveHint?.hintName;
	}

	public void ResetHint(string hintName)
	{
		if (hintDictionary.TryGetValue(hintName, out var value))
		{
			GlobalSaveSystem.SetProgress("Hint_" + value.hintName + "_Notified", value: false);
			value.isShown = false;
			value.isHiding = false;
			if (currentActiveHint == value)
			{
				currentActiveHint = null;
			}
			if (value.currentCoroutine != null)
			{
				StopCoroutine(value.currentCoroutine);
				value.currentCoroutine = null;
			}
			value.canvasGroup.gameObject.SetActive(value: false);
			value.canvasGroup.alpha = 0f;
			if (value.type == HintType.TimeBased && value.activationDelay > 0f)
			{
				StartCoroutine(DelayedShowHint(value));
			}
		}
	}
}
