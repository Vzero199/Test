using System;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector timeline;

	[SerializeField]
	private EndEvent SkipUI;

	private static TimelineManager instance;

	private Canvas overlayCanvas;

	private Canvas worldspaceCanvas;

	private bool firstTimelinePlayed;

	public bool FirstTimelinePlayed => firstTimelinePlayed;

	public PlayableAsset Timeline
	{
		get
		{
			if (!(timeline != null))
			{
				return null;
			}
			return timeline.playableAsset;
		}
		set
		{
			if (timeline != null)
			{
				timeline.playableAsset = value;
				OnTimelineStarted?.Invoke();
				timeline.Play();
				if (SkipUI != null)
				{
					SkipUI.Timeline = timeline;
				}
				else
				{
					Debug.LogError("SkipUI Not Set");
				}
			}
			else
			{
				Debug.LogError("PlayableDirector не установлен!");
			}
		}
	}

	public PlayableDirector Director => timeline;

	public static event Action OnTimelineStarted;

	public static TimelineManager GetInstance()
	{
		if (instance == null)
		{
			instance = UnityEngine.Object.FindObjectOfType<TimelineManager>();
		}
		return instance;
	}

	private void Start()
	{
		GameObject gameObject = GameObject.Find("WorldSpaceCanvas");
		if (gameObject != null)
		{
			worldspaceCanvas = gameObject.GetComponent<Canvas>();
		}
		GameObject gameObject2 = GameObject.Find("Canvas");
		if (gameObject2 != null)
		{
			overlayCanvas = gameObject2.GetComponent<Canvas>();
		}
		if (timeline == null)
		{
			timeline = GetComponent<PlayableDirector>();
		}
		if (timeline == null)
		{
			Debug.LogError("PlayableDirector Not Set");
		}
		if (SkipUI == null)
		{
			SkipUI = UnityEngine.Object.FindObjectOfType<EndEvent>();
		}
		if (SkipUI == null)
		{
			Debug.LogError("SkipUI Not Found");
		}
		if (timeline != null)
		{
			timeline.stopped += OnTimelineFinished;
		}
		if (timeline != null)
		{
			timeline.played += OnTimelineStartedCallback;
			timeline.stopped += OnTimelineFinished;
		}
	}

	private void OnTimelineStartedCallback(PlayableDirector director)
	{
		OnTimelineStarted?.Invoke();
	}

	public bool IsTimelinePlaying()
	{
		if (timeline != null)
		{
			return timeline.state == PlayState.Playing;
		}
		return false;
	}

	private void Update()
	{
		if (worldspaceCanvas != null)
		{
			worldspaceCanvas.gameObject.SetActive(!IsTimelinePlaying());
		}
	}

	private void OnTimelineFinished(PlayableDirector director)
	{
		if (!firstTimelinePlayed && director.playOnAwake)
		{
			firstTimelinePlayed = true;
			director.playOnAwake = false;
		}
		if (timeline != null)
		{
			timeline.playableAsset = null;
		}
	}

	private void OnDestroy()
	{
		if (timeline != null)
		{
			timeline.stopped -= OnTimelineFinished;
		}
	}
}
