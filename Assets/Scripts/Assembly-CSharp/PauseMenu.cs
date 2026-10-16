using System;
using System.Collections;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	[Header("Page Objects")]
	public CanvasGroup pauseMenuGroup;

	public CanvasGroup mainMenuGroup;

	public CanvasGroup restartSureGroup;

	public CanvasGroup tutorialGroup;

	public CanvasGroup quitSureGroup;

	private static bool isPaused = false;

	private bool _shouldForcePause;

	private float fadeDuration = 0.3f;

	[Header("First Selected Options")]
	[SerializeField]
	private GameObject _pauseMenuFirst;

	[SerializeField]
	private GameObject _restartSureMenuFirst;

	[SerializeField]
	private GameObject _quitSureMenuFirst;

	[SerializeField]
	private GameObject _tutorialMenuFirst;

	private bool _isTransitioning;

	[Header("Volume Sliders")]
	[SerializeField]
	private Slider masterSlider;

	[SerializeField]
	private Slider bgmSlider;

	[SerializeField]
	private Slider sfxSlider;

	[SerializeField]
	private Slider uiSlider;

	[Header("Tutorial Blocks")]
	[SerializeField]
	private GameObject itemsBlock;

	[SerializeField]
	private GameObject enemiesBlock;

	[SerializeField]
	private Button itemsButton;

	[SerializeField]
	private Button enemiesButton;

	private bool _isRestarting;

	private static HashSet<AudioSource> pausedAudioSources = new HashSet<AudioSource>();

	public static bool IsPaused => isPaused;

	public static event Action OnPauseTriggered;

	public static void SetPause(bool value)
	{
		isPaused = value;
	}

	private void Start()
	{
		isPaused = false;
		InputSystemUIInputModule inputSystemUIInputModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
		if (inputSystemUIInputModule != null && inputSystemUIInputModule.actionsAsset != null)
		{
			inputSystemUIInputModule.actionsAsset.devices = new InputDevice[2]
			{
				Keyboard.current,
				Mouse.current
			};
			Debug.Log("[Input] UI is limited to keyboard and mouse only");
		}
		SetCanvasGroupVisible(pauseMenuGroup, visible: false);
		if (masterSlider != null)
		{
			masterSlider.value = SoundManager.GetVolume("Master_Volume");
		}
		if (bgmSlider != null)
		{
			bgmSlider.value = SoundManager.GetVolume("BGM_Volume");
		}
		if (sfxSlider != null)
		{
			sfxSlider.value = SoundManager.GetVolume("SFX_Volume");
		}
		if (uiSlider != null)
		{
			uiSlider.value = SoundManager.GetVolume("UI_Volume");
		}
		foreach (InputDevice device in InputSystem.devices)
		{
			Debug.Log($"[InputDevice] {device.displayName} ({device.deviceId}) - {device.description}");
			Debug.Log("Layout: " + device.layout + ", Interface: " + device.description.interfaceName + ", Product: " + device.description.product);
		}
		string text = SceneManager.GetActiveScene().name;
		if (text.Contains("Scene") || text.Contains("Gallery"))
		{
			return;
		}
		bool isNight = text.Contains("Night") || text.Contains("NIght") || text.Contains("FifthDay");
		GameSaveManager.SaveGame(text, isNight);
		AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
		foreach (AudioSource item in array)
		{
			if (!pausedAudioSources.Contains(item))
			{
				pausedAudioSources.Add(item);
			}
		}
		TimelineManager.OnTimelineStarted += OnTimelineStarted;
		InvokeRepeating("TryAutoPause", 0f, 0.5f);
	}

	private void OnTimelineStarted()
	{
		if (isPaused)
		{
			ResumeGame();
		}
	}

	public void ShowItemsTutorial()
	{
		if (!(itemsBlock == null) && !(enemiesBlock == null))
		{
			SoundManager.PlaySound(SoundType.UIClick);
			itemsBlock.SetActive(value: true);
			enemiesBlock.SetActive(value: false);
			if (itemsButton != null)
			{
				itemsButton.interactable = false;
			}
			if (enemiesButton != null)
			{
				enemiesButton.interactable = true;
			}
			EventSystem.current.SetSelectedGameObject(enemiesButton.gameObject);
		}
	}

	public void ShowEnemiesTutorial()
	{
		if (!(itemsBlock == null) && !(enemiesBlock == null))
		{
			SoundManager.PlaySound(SoundType.UIClick);
			itemsBlock.SetActive(value: false);
			enemiesBlock.SetActive(value: true);
			if (itemsButton != null)
			{
				itemsButton.interactable = true;
			}
			if (enemiesButton != null)
			{
				enemiesButton.interactable = false;
			}
			EventSystem.current.SetSelectedGameObject(itemsButton.gameObject);
		}
	}

	public void SetMasterVolume(float volume)
	{
		SoundManager.SetVolume("Master_Volume", volume * 100f);
	}

	public void SetBGMVolume(float volume)
	{
		SoundManager.SetVolume("BGM_Volume", volume * 100f);
	}

	public void SetSFXVolume(float volume)
	{
		SoundManager.SetVolume("SFX_Volume", volume * 100f);
	}

	public void SetUIVolume(float volume)
	{
		SoundManager.SetVolume("UI_Volume", volume * 100f);
	}

	private void OnApplicationQuit()
	{
		string text = SceneManager.GetActiveScene().name;
		if (!text.Contains("Scene") && !text.Contains("Gallery"))
		{
			bool flag = text.Contains("Night") || text.Contains("NIght") || text.Contains("FifthDay");
			Debug.Log($"Приложение закрывается! Сохранение сцены: {text}, isNight: {flag}");
			GameSaveManager.SaveGame(text, flag);
		}
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		if (_isTransitioning || !context.performed)
		{
			return;
		}
		TimelineManager instance = TimelineManager.GetInstance();
		if ((instance != null && instance.IsTimelinePlaying()) || ((Timer.HasLoadedNextScene || Timer.IsTransitioning) && !Timer.HasUpdatedBasementDoor) || (DialogueManager.GetInstance() != null && DialogueManager.GetInstance().dialogueIsPlaying))
		{
			return;
		}
		if (GallerySceneViewerForSuccTE.isGalleryScene)
		{
			GallerySceneViewerForSuccTE.instance.ExitSexMode();
		}
		OnPauseTriggered?.Invoke();
		if (isPaused)
		{
			if (!mainMenuGroup.gameObject.activeInHierarchy)
			{
				if ((!(EndEvent.GetInstance() != null) || !EndEvent.GetInstance().IsAnyScene()) && !QuestManager.IsNightLoading)
				{
					SoundManager.PlaySound(SoundType.UIClick);
					PauseGame();
				}
			}
			else
			{
				ResumeGame();
			}
		}
		else if ((!(EndEvent.GetInstance() != null) || !EndEvent.GetInstance().IsAnyScene()) && !QuestManager.IsNightLoading)
		{
			SoundManager.PlaySound(SoundType.UIClick);
			PauseGame();
		}
	}

	public void PauseGame()
	{
		if ((!(EndEvent.GetInstance() != null) || !EndEvent.GetInstance().IsAnyScene()) && (!(DialogueManager.GetInstance() != null) || !DialogueManager.GetInstance().dialogueIsPlaying))
		{
			EventSystem.current.SetSelectedGameObject(_pauseMenuFirst);
			if (MouseController.Instance != null)
			{
				MouseController.Instance.EnableMouse();
			}
			StartCoroutine(SwitchMenu(restartSureGroup, mainMenuGroup));
			StartCoroutine(SwitchMenu(quitSureGroup, mainMenuGroup));
			StartCoroutine(SwitchMenu(tutorialGroup, mainMenuGroup));
			StartCoroutine(FadeCanvasGroup(pauseMenuGroup, fadeIn: true));
			Time.timeScale = 0f;
			isPaused = true;
			SetAudioPauseState(shouldPause: true);
		}
	}

	public void ResumeGame()
	{
		if (MouseController.Instance != null)
		{
			MouseController.Instance.DisableMouse();
		}
		StartCoroutine(FadeCanvasGroup(pauseMenuGroup, fadeIn: false, () =>
		{
			Time.timeScale = 1f;
		}));
		isPaused = false;
		SetAudioPauseState(shouldPause: false);
		EndSceneController endSceneController = UnityEngine.Object.FindObjectOfType<EndSceneController>();
		if (endSceneController != null)
		{
			endSceneController.NotifyUnpaused();
		}
	}

	public void OnRestartSceneGame()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowConfirmation(restartSureGroup, _restartSureMenuFirst);
	}

	public void OnQuitGame()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowConfirmation(quitSureGroup, _quitSureMenuFirst);
	}

	public void OnTutorial()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowConfirmation(tutorialGroup, _tutorialMenuFirst);
		ShowEnemiesTutorial();
	}

	public void SaveAndQuit()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		string text = SceneManager.GetActiveScene().name;
		if (text.Contains("Scene") || text.Contains("Gallery"))
		{
			ReturnToMainMenu();
			return;
		}
		bool isNight = text.Contains("Night") || text.Contains("NIght") || text.Contains("FifthDay");
		if (text != "Gallery")
		{
			GameSaveManager.SaveGame(text, isNight);
		}
		ReturnToMainMenu();
	}

	public void ReturnToMainMenu()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		StartCoroutine(LoadSceneAndResumeTime("MainMenu"));
	}

	public void RestartScene()
	{
		if (!_isRestarting)
		{
			_isRestarting = true;
			SoundManager.PlaySound(SoundType.UIClick);
			StartCoroutine(LoadSceneAndResumeTime(SceneManager.GetActiveScene().name));
		}
	}

	private IEnumerator LoadSceneAndResumeTime(string sceneName)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		asyncLoad.allowSceneActivation = true;
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		Time.timeScale = 1f;
		_isRestarting = false;
	}

	private void OnDestroy()
	{
		Time.timeScale = 1f;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		_shouldForcePause = !hasFocus;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		_shouldForcePause = pauseStatus;
	}

	private void TryAutoPause()
	{
		if (_shouldForcePause && !isPaused && !QuestManager.IsNightLoading)
		{
			TimelineManager instance = TimelineManager.GetInstance();
			if (!(instance != null) || !instance.IsTimelinePlaying())
			{
				PauseGame();
			}
		}
	}

	private void ShowConfirmation(CanvasGroup targetGroup, GameObject firstSelect)
	{
		StartCoroutine(SwitchMenu(mainMenuGroup, targetGroup));
		EventSystem.current.SetSelectedGameObject(firstSelect);
	}

	public static void RegisterPausableAudio(AudioSource source)
	{
		if (source != null && !pausedAudioSources.Contains(source))
		{
			pausedAudioSources.Add(source);
		}
	}

	public static void UnregisterPausableAudio(AudioSource source)
	{
		if (source != null && pausedAudioSources.Contains(source))
		{
			pausedAudioSources.Remove(source);
		}
	}

	private void SetAudioPauseState(bool shouldPause)
	{
		foreach (AudioSource pausedAudioSource in pausedAudioSources)
		{
			if (!(pausedAudioSource != null) || pausedAudioSource.gameObject.CompareTag("Ambience") || pausedAudioSource.GetComponent<SoundManager>() != null || pausedAudioSource.tag == "TabLight")
			{
				continue;
			}
			if (pausedAudioSource.tag == "enemyNoise")
			{
				if (shouldPause)
				{
					pausedAudioSource.Stop();
				}
				else
				{
					pausedAudioSource.Play();
				}
			}
			pausedAudioSource.mute = shouldPause;
		}
		pausedAudioSources.RemoveWhere((AudioSource source) => source == null);
	}

	private IEnumerator SwitchMenu(CanvasGroup from, CanvasGroup to)
	{
		yield return FadeCanvasGroup(from, fadeIn: false);
		yield return FadeCanvasGroup(to, fadeIn: true);
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn, Action onComplete = null)
	{
		_isTransitioning = true;
		float startAlpha = canvasGroup.alpha;
		float endAlpha = (fadeIn ? 1 : 0);
		float elapsed = 0f;
		if (fadeIn)
		{
			canvasGroup.gameObject.SetActive(value: true);
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
		while (elapsed < fadeDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
		if (!fadeIn)
		{
			canvasGroup.gameObject.SetActive(value: false);
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
		onComplete?.Invoke();
		_isTransitioning = false;
	}

	private void SetCanvasGroupVisible(CanvasGroup canvasGroup, bool visible)
	{
		canvasGroup.alpha = (visible ? 1 : 0);
		canvasGroup.interactable = visible;
		canvasGroup.blocksRaycasts = visible;
		canvasGroup.gameObject.SetActive(visible);
	}
}
