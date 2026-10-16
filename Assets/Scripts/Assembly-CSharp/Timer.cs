using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
	private float elapsedTime = -10f;

	[SerializeField]
	[Range(0.1f, 8f)]
	public static float timeMultiplier = 1f;

	private static bool isEnabled = true;

	private Coroutine boostCoroutine;

	private const float maxTime = 360f;

	private static bool hasLoadedNextScene = false;

	[SerializeField]
	private bool IsEndlessTime;

	private Image screenFader;

	private Image nightEndImage;

	private static bool isTransitioning = false;

	[SerializeField]
	private GameObject demoEndObject;

	private static bool hasUpdatedBasementDoor = false;

	private BlinkManager blinkManager => BlinkManager.GetInstance();

	public static float MaxTime => 360f;

	public static bool HasLoadedNextScene => hasLoadedNextScene;

	public static bool IsTimeBoostActive { get; private set; }

	public static bool IsTransitioning => isTransitioning;

	public static bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			if (value != isEnabled)
			{
				Debug.Log($"Timer is enabled: {value}");
				isEnabled = value;
			}
		}
	}

	public float ElapsedTime => elapsedTime;

	public static bool HasUpdatedBasementDoor => hasUpdatedBasementDoor;

	private void Start()
	{
		isTransitioning = false;
		boostCoroutine = null;
		isEnabled = true;
		hasLoadedNextScene = false;
		IsTimeBoostActive = false;
		timeMultiplier = 1f;
		if (SceneManager.GetActiveScene().name == "FifthNight")
		{
			elapsedTime = -5f;
		}
		GameObject gameObject = GameObject.Find("EndFader");
		if (gameObject != null)
		{
			screenFader = gameObject.GetComponent<Image>();
		}
		GameObject gameObject2 = GameObject.Find("NightEndImage");
		if (gameObject2 != null)
		{
			nightEndImage = gameObject2.GetComponent<Image>();
		}
	}

	private void OnDestroy()
	{
		hasLoadedNextScene = false;
		isTransitioning = false;
		hasUpdatedBasementDoor = false;
		timeMultiplier = 1f;
		IsTimeBoostActive = false;
		boostCoroutine = null;
	}

	private void Update()
	{
		if (!IsEndlessTime && elapsedTime >= 360f && !hasLoadedNextScene)
		{
			hasLoadedNextScene = true;
			LoadNextScene();
		}
		if (isEnabled && (!(Player.GetInstance() != null) || Player.GetInstance().CurrentHeartNumber > 0) && !EndEvent.isGameOver)
		{
			elapsedTime += Time.deltaTime * timeMultiplier;
		}
	}

	private void LoadNextScene()
	{
		string text = SceneManager.GetActiveScene().name;
		DestroyGameObjects();
		switch (text)
		{
		case "FirstNight":
			GlobalSaveSystem.SetProgress("FirstNightIsComplete", value: true);
			break;
		case "FirstNIght":
			GlobalSaveSystem.SetProgress("FirstNightIsComplete", value: true);
			break;
		case "SecondNight":
			GlobalSaveSystem.SetProgress("SecondNightIsComplete", value: true);
			break;
		case "ThirdNight":
			GlobalSaveSystem.SetProgress("ThirdNightIsComplete", value: true);
			break;
		case "FourthNight":
			GlobalSaveSystem.SetProgress("FourthNightIsComplete", value: true);
			break;
		case "FifthNight":
			GlobalSaveSystem.SetProgress("FifthNightIsComplete", value: true);
			if (!hasUpdatedBasementDoor)
			{
				UpdateBasementDoor();
				hasUpdatedBasementDoor = true;
			}
			ReplaceMapNightSprite();
			PlayBasementTimeline();
			DisableBGM();
			return;
		}
		int num = SceneManager.GetActiveScene().buildIndex + 1;
		if (num < SceneManager.sceneCountInBuildSettings)
		{
			StartCoroutine(LoadSceneWithTransition(num));
		}
		else
		{
			Debug.LogWarning("Следующая сцена отсутствует в Build Settings!");
		}
	}

	private void DisableBGM()
	{
		Object.Destroy(GameObject.Find("BGM"));
	}

	private void PlayBasementTimeline()
	{
		PlayableAsset playableAsset = Resources.Load<PlayableAsset>("Timelines/GoToBasement");
		if (playableAsset == null)
		{
			Debug.LogError("Не найден таймлайн GoToBasement в Resources/Timelines.");
			return;
		}
		TimelineManager instance = TimelineManager.GetInstance();
		if (instance == null)
		{
			Debug.LogError("TimelineManager не найден в сцене.");
		}
		else
		{
			instance.Timeline = playableAsset;
		}
	}

	private IEnumerator ShowDemoEndObject()
	{
		if (nightEndImage == null)
		{
			Debug.LogError("NightEndImage не найден.");
			yield break;
		}
		SetAudioPauseState(shouldPause: true);
		Time.timeScale = 0f;
		if (screenFader != null)
		{
			yield return StartCoroutine(FadeImageAlpha(screenFader, 0f, 1f, 2f));
		}
		SoundManager.PlaySound(SoundType.Chime);
		yield return StartCoroutine(FadeImageAlpha(nightEndImage, 0f, 1f, 1f));
		yield return new WaitForSecondsRealtime(1f);
		yield return StartCoroutine(FadeImageAlpha(nightEndImage, 1f, 0f, 1f));
		yield return new WaitForSecondsRealtime(1f);
		if (demoEndObject == null)
		{
			Debug.LogError("DemoEndObject не найден на сцене.");
			yield break;
		}
		CanvasGroup canvasGroup = demoEndObject.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			Debug.LogError("CanvasGroup не найден на DemoEndObject.");
			yield break;
		}
		demoEndObject.SetActive(value: true);
		float duration = 1f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		GameObject gameObject = GameObject.Find("demoBack");
		if (gameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
		else
		{
			Debug.LogWarning("demoBack не найден в сцене.");
		}
	}

	private void ReplaceMapNightSprite()
	{
		GameObject gameObject = GameObject.Find("mapNight");
		if (gameObject != null)
		{
			Sprite sprite = Resources.Load<Sprite>("Sprites/MapNightPodval");
			if (sprite != null)
			{
				Image component = gameObject.GetComponent<Image>();
				if (component != null)
				{
					component.sprite = sprite;
				}
				else
				{
					Debug.LogWarning("Объект mapNight не содержит компонента Image.");
				}
			}
			else
			{
				Debug.LogWarning("Не удалось загрузить спрайт из ресурсов: Sprites/MapNightPodval.");
			}
		}
		else
		{
			Debug.LogWarning("Объект mapNight не найден.");
		}
	}

	private IEnumerator LoadSceneWithTransition(int nextSceneIndex)
	{
		try
		{
			isTransitioning = true;
			SetAudioPauseState(shouldPause: true);
			Time.timeScale = 0f;
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Single);
			asyncLoad.allowSceneActivation = false;
			if (screenFader == null)
			{
				Debug.LogWarning("ScreenFader не найден или не содержит компонент Image.");
			}
			if (nightEndImage == null)
			{
				Debug.LogWarning("NightEndImage не найден или не содержит компонент Image.");
			}
			if (EndEvent.GetInstance() != null)
			{
				EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: false);
			}
			if (screenFader != null)
			{
				yield return StartCoroutine(FadeImageAlpha(screenFader, 0f, 1f, 2f));
			}
			yield return new WaitForSecondsRealtime(1f);
			if (nightEndImage != null)
			{
				SoundManager.PlaySound(SoundType.Chime);
				yield return StartCoroutine(FadeImageAlpha(nightEndImage, 0f, 1f, 1f));
				yield return new WaitForSecondsRealtime(1f);
				yield return StartCoroutine(FadeImageAlpha(nightEndImage, 1f, 0f, 1f));
				yield return new WaitForSecondsRealtime(1f);
			}
			Time.timeScale = 1f;
			asyncLoad.allowSceneActivation = true;
			while (!asyncLoad.isDone)
			{
				yield return null;
			}
		}
		finally
		{
			SetAudioPauseState(shouldPause: false);
			isTransitioning = false;
		}
	}

	private IEnumerator FadeImageAlpha(Image image, float from, float to, float duration)
	{
		float elapsed = 0f;
		Color color = image.color;
		while (elapsed <= duration)
		{
			float t = Mathf.Clamp01(elapsed / duration);
			color.a = Mathf.Lerp(from, to, t);
			image.color = color;
			elapsed += Time.unscaledDeltaTime;
			yield return new WaitForSecondsRealtime(0f);
		}
		color.a = to;
		image.color = color;
	}

	private void DestroyGameObjects()
	{
		Enemy[] array = Object.FindObjectsOfType<Enemy>();
		foreach (Enemy enemy in array)
		{
			if (enemy is MebeebEnemy || enemy is SisSupEnemy || enemy is PosterEnemy || enemy is TVEnemy || enemy is WhiteWomanEnemy)
			{
				Object.Destroy(enemy);
				continue;
			}
			Object.Destroy(enemy);
			enemy.gameObject.AddComponent<FadeAway>().StartFading(1f);
		}
		Nintendo nintendo = Object.FindObjectOfType<Nintendo>();
		if (nintendo != null)
		{
			nintendo.ChangeState(new NintendoIdleState());
			Object.Destroy(nintendo);
		}
		NightManager nightManager = Object.FindObjectOfType<NightManager>();
		if (nightManager != null)
		{
			Object.Destroy(nightManager);
		}
	}

	private void UpdateBasementDoor()
	{
		GameObject gameObject = GameObject.Find("BasementDoor");
		if (gameObject != null)
		{
			Sprite sprite = Resources.Load<Sprite>("Sprites/SecretDoorOpen");
			if (sprite != null)
			{
				SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					component.sprite = sprite;
				}
			}
			else
			{
				Debug.LogWarning("Не удалось загрузить спрайт из ресурсов: Sprites/SecretDoorOpen.");
			}
			QuestShine componentInChildren = gameObject.GetComponentInChildren<QuestShine>();
			if (componentInChildren != null)
			{
				componentInChildren.KeyItemName = string.Empty;
			}
			else
			{
				Debug.LogWarning("Компонент ShineItem не найден на объекте Basement.");
			}
		}
		else
		{
			Debug.LogWarning("Объект BasementDoor не найден.");
		}
	}

	public void BoostTime(float boostedMultiplier, float duration)
	{
		timeMultiplier = 1f;
		if (boostCoroutine != null)
		{
			StopCoroutine(boostCoroutine);
		}
		boostCoroutine = StartCoroutine(BoostTimeCoroutine(boostedMultiplier, duration));
	}

	private IEnumerator BoostTimeCoroutine(float boostedMultiplier, float duration)
	{
		float originalMultiplier = timeMultiplier;
		timeMultiplier = boostedMultiplier;
		IsTimeBoostActive = true;
		try
		{
			yield return new WaitForSeconds(duration);
		}
		finally
		{
			Timer timer = this;
			NightManager instance = NightManager.GetInstance();
			if (instance != null)
			{
				instance.AddHour();
			}
			timeMultiplier = originalMultiplier;
			IsTimeBoostActive = false;
			timer.boostCoroutine = null;
		}
	}

	private void SetAudioPauseState(bool shouldPause)
	{
		AudioSource[] array = Object.FindObjectsOfType<AudioSource>();
		foreach (AudioSource audioSource in array)
		{
			if (!(audioSource.GetComponent<SoundManager>() != null))
			{
				audioSource.mute = shouldPause;
			}
		}
	}
}
