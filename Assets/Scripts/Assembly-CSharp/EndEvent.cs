using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class EndEvent : MonoBehaviour
{
	[SerializeField]
	private Image fillImage;

	[SerializeField]
	private Image parentObject;

	[SerializeField]
	private CanvasGroup fillRestartGroup;

	[SerializeField]
	private CanvasGroup worldSpaceGroup;

	[SerializeField]
	private float fillSpeed = 0.2f;

	[SerializeField]
	private PlayableDirector timeline;

	private PlayableAsset timelineAsset;

	private PlayableAsset firstEndTimeline;

	private TimelineManager timelineManager;

	public static bool isGameOver;

	public bool isEnabled = true;

	private static EndEvent instance;

	private Coroutine fadeCoroutine;

	private bool parentVisible;

	private float fadeDuration = 1f;

	private bool isGallery;

	private bool isHoldingEnter;

	private Image buttonHintImage;

	private bool skipTriggered;

	private bool skipBlockedByState;

	public PlayableDirector Timeline
	{
		get
		{
			return timeline;
		}
		set
		{
			timeline = value;
			fillImage.fillAmount = 0f;
		}
	}

	public static EndEvent Instance => instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		isGameOver = false;
		fadeCoroutine = null;
	}

	public static EndEvent GetInstance()
	{
		return instance;
	}

	public void DisableButton()
	{
		isEnabled = false;
	}

	public void EnableButton()
	{
		isEnabled = true;
	}

	private void Start()
	{
		skipTriggered = false;
		skipBlockedByState = false;
		fadeCoroutine = null;
		GameObject gameObject = GameObject.Find("WorldSpaceCanvas");
		worldSpaceGroup = gameObject.GetComponent<CanvasGroup>();
		if (worldSpaceGroup == null)
		{
			worldSpaceGroup = gameObject.AddComponent<CanvasGroup>();
		}
		GameObject gameObject2 = base.transform.Find("skipButtonHint").gameObject;
		if (gameObject2 != null)
		{
			buttonHintImage = gameObject2.GetComponent<Image>();
		}
		isGameOver = false;
		string text = SceneManager.GetActiveScene().name;
		isGallery = text == "Gallery";
		isEnabled = true;
		timelineManager = TimelineManager.GetInstance();
		string path = "Timelines/GameOver";
		timelineAsset = Resources.Load<PlayableAsset>(path);
		string path2 = "Timelines/PreGameOver";
		firstEndTimeline = Resources.Load<PlayableAsset>(path2);
		if (timelineAsset == null)
		{
			Debug.LogError("Not Found Game Over Timeline!");
		}
		if (fillImage != null)
		{
			fillImage.fillAmount = 0f;
		}
		else
		{
			Debug.LogError("Fill Image is not assigned.");
		}
	}

	public void OnEnterPress(InputAction.CallbackContext context)
	{
		if (PauseMenu.IsPaused || Timer.IsTransitioning || BlinkManager.GetInstance().IsFade())
		{
			isHoldingEnter = false;
		}
		else if (context.phase == InputActionPhase.Started)
		{
			isHoldingEnter = true;
		}
		else if (context.phase == InputActionPhase.Canceled)
		{
			isHoldingEnter = false;
		}
	}

	private void Update()
	{
		if (skipBlockedByState)
		{
			isHoldingEnter = false;
			return;
		}
		bool flag = isEnabled && !skipTriggered && (IsTimelinePlaying() || IsHScene());
		if (flag != parentVisible)
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			fadeCoroutine = StartCoroutine(FadeImages(flag));
			parentVisible = flag;
		}
		worldSpaceGroup.alpha = (flag ? 0f : 1f);
		if (!isEnabled || fillImage == null || (!IsTimelinePlaying() && !IsHScene()))
		{
			if (fillImage.fillAmount > 0f)
			{
				fillImage.fillAmount -= fillSpeed * 2f * Time.deltaTime;
				if (fillImage.fillAmount < 0f)
				{
					fillImage.fillAmount = 0f;
				}
			}
		}
		else if (isHoldingEnter && ((IsTimelinePlaying() && timeline.playableGraph.IsValid() && timeline.playableGraph.GetRootPlayable(0).GetSpeed() != 0.0) || IsHScene()))
		{
			fillImage.fillAmount += fillSpeed * Time.deltaTime;
			if (isGameOver)
			{
				fillRestartGroup.alpha = Mathf.Max(0f, fillRestartGroup.alpha - fillSpeed * Time.deltaTime);
			}
			if (fillImage.fillAmount >= 1f)
			{
				fillImage.fillAmount = 1f;
				if (IsHScene())
				{
					OnFullFill(null);
				}
				else if (IsTimelinePlaying())
				{
					OnFullFill(timeline);
				}
				isHoldingEnter = false;
			}
		}
		else if (fillImage.fillAmount > 0f)
		{
			fillImage.fillAmount -= fillSpeed * 2f * Time.deltaTime;
			if (isGameOver)
			{
				fillRestartGroup.alpha = Mathf.Min(1f, fillRestartGroup.alpha + fillSpeed * 2f * Time.deltaTime);
			}
			if (fillImage.fillAmount < 0f)
			{
				fillImage.fillAmount = 0f;
			}
		}
	}

	private IEnumerator FadeImages(bool show)
	{
		float timeElapsed = 0f;
		Color parentColor = parentObject.color;
		Color childColor = buttonHintImage.color;
		float parentStartAlpha = parentColor.a;
		float childStartAlpha = childColor.a;
		float endAlpha = (show ? 1f : 0f);
		if (show)
		{
			if (!parentObject.enabled)
			{
				parentObject.enabled = true;
			}
			if (!buttonHintImage.enabled)
			{
				buttonHintImage.enabled = true;
			}
		}
		while (timeElapsed < fadeDuration)
		{
			float t = timeElapsed / fadeDuration;
			parentColor.a = Mathf.Lerp(parentStartAlpha, endAlpha, t);
			childColor.a = Mathf.Lerp(childStartAlpha, endAlpha, t);
			parentObject.color = parentColor;
			buttonHintImage.color = childColor;
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		parentColor.a = endAlpha;
		childColor.a = endAlpha;
		parentObject.color = parentColor;
		buttonHintImage.color = childColor;
		_ = show;
	}

	private bool IsTimelinePlaying()
	{
		if (timeline != null)
		{
			return timeline.state == PlayState.Playing;
		}
		return false;
	}

	private bool IsHScene()
	{
		if (!(NightManager.GetInstance() != null) || NightManager.GetInstance().StopGameCoroutine == null || TVEnemy.IsTrapped || (!WhiteWomanEnemy.isLoseEvent && WhiteWomanEnemy.IsCharmGame) || (SisSupEnemy.IsTrapped && !SisSupEnemy.isCaptured))
		{
			if (isGallery)
			{
				return GallerySceneViewer.isGalleryScene;
			}
			return false;
		}
		return true;
	}

	public bool IsAnyScene()
	{
		if (!IsTimelinePlaying())
		{
			return IsHScene();
		}
		return true;
	}

	public void StartTimeline()
	{
		isGameOver = true;
		UnblockSkip();
		Timer.IsEnabled = false;
		Debug.Log("Start Game Over");
		Entities[] array = Object.FindObjectsOfType<Entities>();
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i].gameObject);
		}
		InteractionObject[] array2 = Object.FindObjectsOfType<InteractionObject>();
		for (int i = 0; i < array2.Length; i++)
		{
			Object.Destroy(array2[i].gameObject);
		}
		timelineManager.Director.extrapolationMode = DirectorWrapMode.Loop;
		timelineManager.Director.playableAsset = timelineAsset;
		timelineManager.Director.Play();
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/GameOverBreath");
		if (audioClip != null)
		{
			AudioSource audioSource = new GameObject("GameOverAudio").AddComponent<AudioSource>();
			AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
			if (sFXGroup != null)
			{
				audioSource.outputAudioMixerGroup = sFXGroup;
			}
			audioSource.clip = audioClip;
			audioSource.loop = true;
			audioSource.playOnAwake = false;
			audioSource.Play();
		}
		else
		{
			Debug.LogWarning("GameOverBreath audio clip not found in Resources/Audio/");
		}
	}

	private void OnFirstTimelineEnd(PlayableDirector director)
	{
		director.stopped -= OnFirstTimelineEnd;
		BlinkManager.GetInstance().FadeToBlink(base.gameObject, StartLoopedTimeline, allowVerticalMovement: true, 0f, 0f, base.gameObject);
	}

	private void StartLoopedTimeline()
	{
		timelineManager.Director.extrapolationMode = DirectorWrapMode.Loop;
		timelineManager.Director.playableAsset = timelineAsset;
		timelineManager.Director.Play();
	}

	public void RestartScene()
	{
		isGameOver = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void BlockSkip()
	{
		skipBlockedByState = true;
		isEnabled = false;
		skipTriggered = true;
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeImages(show: false));
		parentVisible = false;
		isHoldingEnter = false;
		fillImage.fillAmount = 0f;
	}

	public void UnblockSkip()
	{
		skipTriggered = false;
		skipBlockedByState = false;
		isEnabled = true;
	}

	private void OnFullFill(PlayableDirector activeTimeline)
	{
		if (Player.GetInstance() == null && !isGameOver)
		{
			return;
		}
		fillImage.fillAmount = 0f;
		if (isGallery && GallerySceneViewer.isGalleryScene)
		{
			Animator componentInParent = GallerySceneViewer.instance.GetComponentInParent<Animator>();
			_ = componentInParent.GetCurrentAnimatorStateInfo(0).shortNameHash;
			string stateName = ((!componentInParent.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Second")) ? "Climax" : "SecondClimax");
			componentInParent.Play(stateName);
			return;
		}
		if (isGameOver)
		{
			RestartScene();
			return;
		}
		if (Player.GetInstance().CurrentHeartNumber <= 0 && NightManager.GetInstance() != null)
		{
			if (timelineAsset != null && timelineManager != null)
			{
				BlinkManager.GetInstance().BlinkWithCustomSpeed(1f, Player.GetInstance().gameObject, StartTimeline);
			}
			return;
		}
		Debug.Log($"Timeline Status: {activeTimeline} ");
		if (activeTimeline == null)
		{
			if (Player.GetInstance().CurrentState is PlayerSexModeState)
			{
				Debug.Log("HScene for " + Player.GetInstance().gameObject.name + " Skipped");
				if (!Player.GetInstance().GetAnimator().GetCurrentAnimatorStateInfo(0)
					.IsName("Climax") && !Player.GetInstance().GetAnimator().GetCurrentAnimatorStateInfo(0)
					.IsName("ClimaxRepeat"))
				{
					if (Player.GetInstance().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Second"))
					{
						Player.GetInstance().PlayAnimation("SecondClimax");
					}
					else
					{
						Player.GetInstance().PlayAnimation("Climax");
					}
				}
				return;
			}
			if (Nintendo.GetInstance() != null && Nintendo.GetInstance().CurrentState is NintendoHaveFunState)
			{
				Debug.Log("HScene for " + Nintendo.GetInstance().gameObject.name + " Skipped");
				if (!Nintendo.GetInstance().GetAnimator().GetCurrentAnimatorStateInfo(0)
					.IsName("Climax") && !Nintendo.GetInstance().GetAnimator().GetCurrentAnimatorStateInfo(0)
					.IsName("ClimaxRepeat"))
				{
					Nintendo.GetInstance().GetComponent<Animator>().Play("Climax");
				}
				return;
			}
			Enemy[] array = Object.FindObjectsOfType<Enemy>();
			foreach (Enemy enemy in array)
			{
				if (!(enemy.CurrentState is SexModeState) && (!(enemy is WhiteWomanEnemy) || !WhiteWomanEnemy.isLoseEvent))
				{
					continue;
				}
				if (enemy is NunEnemy)
				{
					Debug.Log("HScene for " + enemy.gameObject.name + " Skipped");
					if (!enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Climax") && !enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ClimaxRepeat"))
					{
						enemy.GetComponent<Animator>().Play("PreClimax");
					}
					break;
				}
				Debug.Log("HScene for " + enemy.gameObject.name + " Skipped");
				if (enemy.GetComponent<Animator>() != null && !enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Climax") && !enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("ClimaxRepeat"))
				{
					Animator component = enemy.GetComponent<Animator>();
					if (!(enemy is WhisperEnemy))
					{
						component.Play("Climax");
						break;
					}
					_ = component.GetCurrentAnimatorStateInfo(0).shortNameHash;
					string stateName2 = ((!component.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Second")) ? "Climax" : "SecondClimax");
					component.Play(stateName2);
				}
				break;
			}
		}
		else
		{
			double num = FindNextMarkerTime(activeTimeline);
			if (num > 0.0)
			{
				activeTimeline.time = num;
				activeTimeline.Evaluate();
				Debug.Log($"Таймлайн пропущен до следующей метки: {num}");
			}
			else
			{
				activeTimeline.time = activeTimeline.duration;
				activeTimeline.Evaluate();
				Debug.Log("Таймлайн пропущен до конца.");
			}
		}
	}

	private double FindNextMarkerTime(PlayableDirector director)
	{
		if (director.playableAsset is TimelineAsset timelineAsset)
		{
			double time = director.time;
			double num = double.MaxValue;
			foreach (TrackAsset outputTrack in timelineAsset.GetOutputTracks())
			{
				foreach (IMarker marker in outputTrack.GetMarkers())
				{
					if (marker.time > time && marker.time < num)
					{
						num = marker.time;
					}
				}
			}
			if (num != double.MaxValue)
			{
				return num;
			}
			return -1.0;
		}
		return -1.0;
	}
}
