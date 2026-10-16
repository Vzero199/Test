using System.Collections;
using System.IO;
using System.Linq;
using SmallHedge.SoundManager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private Toggle fullscreenToggle;

	[Header("Menu Objects")]
	[SerializeField]
	private GameObject _mainMenuCanvasGO;

	[SerializeField]
	private GameObject _playMenuCanvasGO;

	[SerializeField]
	private GameObject _extraMenuCanvasGO;

	[SerializeField]
	private GameObject _newGameSureMenuCanvasGO;

	[SerializeField]
	private GameObject _sceneSureMenuCanvasGO;

	[SerializeField]
	private GameObject _settingsMenuCanvasGO;

	[SerializeField]
	private GameObject _gameSettingsMenuCanvasGO;

	[SerializeField]
	private GameObject _soundSettingsMenuCanvasGO;

	[SerializeField]
	private GameObject _controlSettingsMenuCanvasGO;

	[SerializeField]
	private GameObject _customNightMenuCanvasGO;

	[SerializeField]
	private GameObject _creditsMenuCanvasGO;

	[Header("First Selected Options")]
	[SerializeField]
	private GameObject _mainMenuFirst;

	[SerializeField]
	private GameObject _playMenuFirst;

	[SerializeField]
	private GameObject _extraMenuFirst;

	[SerializeField]
	private GameObject _newGameSureMenuFirst;

	[SerializeField]
	private GameObject _sceneSureMenuFirst;

	[SerializeField]
	private GameObject _gameSettingsMenuFirst;

	[SerializeField]
	private GameObject _soundSettingsMenuFirst;

	[SerializeField]
	private GameObject _controlSettingsMenuFirst;

	[SerializeField]
	private GameObject _customNightMenuFirst;

	[SerializeField]
	private GameObject _creditsMenuFirst;

	[Header("Language Buttons")]
	[SerializeField]
	private Button _japaneseButton;

	[SerializeField]
	private Button _englishButton;

	[SerializeField]
	private Button _chineseButton;

	[SerializeField]
	private Button _russianButton;

	[Header("Difficult Buttons")]
	[SerializeField]
	private Button practiceButton;

	[SerializeField]
	private Button easyButton;

	[SerializeField]
	private Button normalButton;

	[SerializeField]
	private Button hardButton;

	private Difficulty currentDifficulty;

	[Header("Switch Settings")]
	[SerializeField]
	private GameObject _generalSettings;

	[SerializeField]
	private GameObject _itemsSettings;

	[SerializeField]
	private GameObject _playerSettings;

	[SerializeField]
	private TextMeshProUGUI _currentSwitchText;

	[Header("Sliders")]
	public Slider maxEnemiesSlider;

	public Slider maxItemsSlider;

	public Slider timeMultiplirSlider;

	public Slider itemSpawnChanceSlider;

	[Header("Player Sliders")]
	public Slider playerMoveSpeedSlider;

	public Slider playerMoveSpeedBoostSlider;

	public Slider playerInteractionSpeedSlider;

	public Slider playerEnduranceTimeSlider;

	public Slider playerElevatorCooldownSlider;

	public Slider playerBatteryPropSlider;

	[Header("Group Spawners")]
	public Transform slidersParent;

	public Transform itemsSlidersParent;

	[Header("Stand By")]
	public GameObject toggleObject;

	[Header("Other Options")]
	public BaseEnemySettings currentEnemySettings;

	[Header("Settings Button")]
	public Button generalPropButton;

	public Button itemsPropButton;

	public Button playerPropButton;

	public GameObject sliderPrefab;

	[Header("Volume Sliders")]
	[SerializeField]
	private Slider masterSlider;

	[SerializeField]
	private Slider bgmSlider;

	[SerializeField]
	private Slider sfxSlider;

	[SerializeField]
	private Slider uiSlider;

	public TextMeshProUGUI enemyCountText;

	public Transform togglesParent;

	public ItemSpawnSettings[] itemSpawnSettings;

	private GameObject toggleSetter;

	[SerializeField]
	private float fadeDuration = 0.5f;

	private int chosenSceneNumber;

	private void Awake()
	{
		GameSettings.LoadSettings();
	}

	private void Start()
	{
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
		if (_currentSwitchText == null)
		{
			Debug.LogWarning("Object Switch text not Set!");
		}
		if (_generalSettings == null)
		{
			Debug.LogWarning("Object Switch text not Set!");
		}
		if (_itemsSettings == null)
		{
			Debug.LogWarning("Object Switch text not Set!");
		}
		_mainMenuCanvasGO.SetActive(value: true);
		_gameSettingsMenuCanvasGO.SetActive(value: false);
		if (maxEnemiesSlider != null)
		{
			maxEnemiesSlider.maxValue = 0f;
		}
		EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
		OpenMainMenu();
		if (fullscreenToggle != null)
		{
			fullscreenToggle.isOn = Screen.fullScreen;
			fullscreenToggle.onValueChanged.AddListener(ToggleFullscreen);
		}
		if (PlayerPrefs.HasKey("GameDifficulty"))
		{
			currentDifficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty");
		}
		else
		{
			currentDifficulty = Difficulty.Normal;
		}
		UpdateDifficultyUI();
		switch (GameSettings.CurrentLanguage)
		{
		case "RU":
			OnRussianLanguage();
			break;
		case "EN":
			OnEnglishLanguage();
			break;
		case "CN":
			OnChineseLanguage();
			break;
		case "JP":
			OnJapaneseLanguage();
			break;
		default:
			Debug.LogWarning("Неизвестный язык: " + GameSettings.CurrentLanguage);
			break;
		}
	}

	public void ChoosePracticeDifficulty()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		SetDifficulty(Difficulty.Practice);
	}

	public void ChooseEasyDifficulty()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		SetDifficulty(Difficulty.Easy);
	}

	public void ChooseNormalDifficulty()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		SetDifficulty(Difficulty.Normal);
	}

	public void ChooseHardDifficulty()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		SetDifficulty(Difficulty.Hard);
	}

	private void SetDifficulty(Difficulty difficulty)
	{
		currentDifficulty = difficulty;
		PlayerPrefs.SetInt("GameDifficulty", (int)currentDifficulty);
		PlayerPrefs.Save();
		UpdateDifficultyUI();
	}

	private void UpdateDifficultyUI()
	{
		practiceButton.interactable = currentDifficulty != Difficulty.Practice;
		easyButton.interactable = currentDifficulty != Difficulty.Easy;
		normalButton.interactable = currentDifficulty != Difficulty.Normal;
		hardButton.interactable = currentDifficulty != Difficulty.Hard;
	}

	private void ToggleFullscreen(bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
	}

	private void ToggleEasyMode(bool isEasyMode)
	{
		PlayerPrefs.SetInt("EasyMode", isEasyMode ? 1 : 0);
		PlayerPrefs.Save();
	}

	private void OnLanguageChanged(string language)
	{
		GameSettings.CurrentLanguage = language;
		Debug.Log("Язык изменён на: " + GameSettings.CurrentLanguage);
	}

	private void SetMenuActive(GameObject menu, bool active)
	{
		StartCoroutine(FadeCanvas(menu, active));
	}

	private IEnumerator FadeCanvas(GameObject menu, bool active)
	{
		EventSystem.current.sendNavigationEvents = false;
		CanvasGroup canvasGroup = menu.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = menu.AddComponent<CanvasGroup>();
		}
		menu.SetActive(value: true);
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		float startAlpha = canvasGroup.alpha;
		float endAlpha = (active ? 1f : 0f);
		float elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
		canvasGroup.interactable = active;
		canvasGroup.blocksRaycasts = active;
		if (!active)
		{
			menu.SetActive(value: false);
		}
		EventSystem.current.sendNavigationEvents = true;
	}

	public void ShowMainMenu()
	{
		SetMenuActive(_mainMenuCanvasGO, active: true);
	}

	public void HideMainMenu()
	{
		SetMenuActive(_mainMenuCanvasGO, active: false);
	}

	public void ShowPlayMenu()
	{
		SetMenuActive(_playMenuCanvasGO, active: true);
	}

	public void HidePlayMenu()
	{
		SetMenuActive(_playMenuCanvasGO, active: false);
	}

	public void ShowExtraMenu()
	{
		SetMenuActive(_extraMenuCanvasGO, active: true);
	}

	public void HideExtraMenu()
	{
		SetMenuActive(_extraMenuCanvasGO, active: false);
	}

	public void ShowNewGameSureMenu()
	{
		SetMenuActive(_newGameSureMenuCanvasGO, active: true);
	}

	public void HideNewGameSureMenu()
	{
		SetMenuActive(_newGameSureMenuCanvasGO, active: false);
	}

	public void ShowCreditsMenu()
	{
		SetMenuActive(_creditsMenuCanvasGO, active: true);
	}

	public void HideCreditsMenu()
	{
		SetMenuActive(_creditsMenuCanvasGO, active: false);
	}

	public void ShowSceneSureMenu()
	{
		SetMenuActive(_sceneSureMenuCanvasGO, active: true);
	}

	public void HideSceneSureMenu()
	{
		SetMenuActive(_sceneSureMenuCanvasGO, active: false);
	}

	public void ShowSettingsMenu()
	{
		SetMenuActive(_settingsMenuCanvasGO, active: true);
	}

	public void HideSettingsMenu()
	{
		SetMenuActive(_settingsMenuCanvasGO, active: false);
	}

	public void ShowGameSettingsMenu()
	{
		SetMenuActive(_gameSettingsMenuCanvasGO, active: true);
	}

	public void HideGameSettingsMenu()
	{
		SetMenuActive(_gameSettingsMenuCanvasGO, active: false);
	}

	public void ShowSoundSettingsMenu()
	{
		SetMenuActive(_soundSettingsMenuCanvasGO, active: true);
	}

	public void HideSoundSettingsMenu()
	{
		SetMenuActive(_soundSettingsMenuCanvasGO, active: false);
	}

	public void ShowControlSettingsMenu()
	{
		SetMenuActive(_controlSettingsMenuCanvasGO, active: true);
	}

	public void HideControlSettingsMenu()
	{
		SetMenuActive(_controlSettingsMenuCanvasGO, active: false);
	}

	public void ShowCustomNightMenu()
	{
		SetMenuActive(_customNightMenuCanvasGO, active: true);
	}

	public void HideCustomNightMenu()
	{
		SetMenuActive(_customNightMenuCanvasGO, active: false);
	}

	public void ChooseJapaneseLanguage()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OnJapaneseLanguage();
		EventSystem.current.SetSelectedGameObject(_japaneseButton.gameObject);
		OnLanguageChanged("JP");
	}

	public void ChooseEnglishLanguage()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OnEnglishLanguage();
		EventSystem.current.SetSelectedGameObject(_englishButton.gameObject);
		OnLanguageChanged("EN");
	}

	public void ChooseChineseLanguage()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OnChineseLanguage();
		EventSystem.current.SetSelectedGameObject(_chineseButton.gameObject);
		OnLanguageChanged("CN");
	}

	public void ChooseRussianLanguage()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OnRussianLanguage();
		EventSystem.current.SetSelectedGameObject(_russianButton.gameObject);
		OnLanguageChanged("RU");
	}

	private void OnRussianLanguage()
	{
		_japaneseButton.interactable = true;
		_englishButton.interactable = true;
		_chineseButton.interactable = true;
		_russianButton.interactable = false;
	}

	private void OnEnglishLanguage()
	{
		_japaneseButton.interactable = true;
		_englishButton.interactable = false;
		_chineseButton.interactable = true;
		_russianButton.interactable = true;
	}

	private void OnChineseLanguage()
	{
		_japaneseButton.interactable = true;
		_englishButton.interactable = true;
		_chineseButton.interactable = false;
		_russianButton.interactable = true;
	}

	private void OnJapaneseLanguage()
	{
		_japaneseButton.interactable = false;
		_englishButton.interactable = true;
		_chineseButton.interactable = true;
		_russianButton.interactable = true;
	}

	private void HideAllMenus()
	{
		HideMainMenu();
		HidePlayMenu();
		HideExtraMenu();
		HideNewGameSureMenu();
		HideSceneSureMenu();
		HideSettingsMenu();
		HideGameSettingsMenu();
		HideSoundSettingsMenu();
		HideControlSettingsMenu();
		HideCustomNightMenu();
	}

	public void PlayGame()
	{
		HideAllMenus();
		SoundManager.PlaySound(SoundType.UIClick);
		MainMenuBackgroundController mainMenuBackgroundController = Object.FindObjectOfType<MainMenuBackgroundController>();
		if (mainMenuBackgroundController != null)
		{
			AudioTimeManager.SaveCicadasTime(mainMenuBackgroundController.audioSource);
		}
		GameSaveManager.DeleteSave();
		GlobalSaveSystem.ResetKarmaProgress();
		SceneManager.LoadSceneAsync("FirstDay", LoadSceneMode.Single);
	}

	public void LoadGallery()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		SceneManager.LoadSceneAsync("Gallery", LoadSceneMode.Single);
		HideAllMenus();
	}

	public void LoadEndlessNight()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		SceneManager.LoadSceneAsync("EndlessNight", LoadSceneMode.Single);
		HideAllMenus();
	}

	public void OnContinueGame()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		if (File.Exists(GameSaveManager.SavePath))
		{
			if (GameSaveManager.LoadGame())
			{
				HideAllMenus();
			}
			else
			{
				Debug.Log("Сохранение несовместимо или повреждено.");
			}
		}
		else
		{
			Debug.Log("Нет сохранённых данных!");
		}
	}

	public void ExitGame()
	{
		HideAllMenus();
		Debug.Log("Exit Game");
		Application.Quit();
	}

	public void ChoseSceneNumber(int sceneNumber)
	{
		if (sceneNumber >= 0 && sceneNumber < SceneManager.sceneCountInBuildSettings)
		{
			OpenSceneSureMenuHandle();
			chosenSceneNumber = sceneNumber;
		}
		else
		{
			Debug.LogError("Ошибка: номер сцены вне допустимого диапазона!");
		}
	}

	public void ConfirmChoise()
	{
		HideAllMenus();
		SoundManager.PlaySound(SoundType.UIClick);
		SceneManager.LoadSceneAsync(chosenSceneNumber, LoadSceneMode.Single);
	}

	private BaseEnemySettings FindEnemyByName(string name)
	{
		return NightManager.enemiesInNightProp.FirstOrDefault((BaseEnemySettings enemy) => enemy.name == name);
	}

	public void OpenPlayMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowPlayMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_playMenuFirst);
	}

	public void OpenPlayMenuHandlBack()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowMainMenu();
		HidePlayMenu();
		EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
	}

	public void OpenNewGameSureMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		HidePlayMenu();
		ShowNewGameSureMenu();
		EventSystem.current.SetSelectedGameObject(_newGameSureMenuFirst);
	}

	public void OpenNewGameSureMenuHandleBack()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowPlayMenu();
		HideNewGameSureMenu();
		EventSystem.current.SetSelectedGameObject(_playMenuFirst);
	}

	public void OpenExtraMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowExtraMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_extraMenuFirst);
	}

	public void OpenExtraMenuHandleBack()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		HideExtraMenu();
		ShowMainMenu();
		EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
	}

	public void OpenGameSettingsMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowSettingsMenu();
		ShowGameSettingsMenu();
		HideSoundSettingsMenu();
		HideControlSettingsMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_gameSettingsMenuFirst);
	}

	public void OpenSoundSettingsMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowSettingsMenu();
		ShowSoundSettingsMenu();
		HideGameSettingsMenu();
		HideControlSettingsMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_soundSettingsMenuFirst);
	}

	public void OpenControlSettingsMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowSettingsMenu();
		ShowControlSettingsMenu();
		HideGameSettingsMenu();
		HideSoundSettingsMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_controlSettingsMenuFirst);
	}

	public void OpenGameSettingsMenuHandleBack()
	{
		ShowMainMenu();
		HideSettingsMenu();
		HideGameSettingsMenu();
		HideSoundSettingsMenu();
		HideControlSettingsMenu();
		EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
	}

	public void OpenCreditsMenu()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowCreditsMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_creditsMenuFirst);
	}

	public void OpenCreditsMenuBack()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowMainMenu();
		HideCreditsMenu();
		EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
	}

	public void OpenSceneSureMenuHandle()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowSceneSureMenu();
		HideCustomNightMenu();
		EventSystem.current.SetSelectedGameObject(_sceneSureMenuFirst);
	}

	public void OpenSceneSureMenuHandleBack()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowCustomNightMenu();
		HideSceneSureMenu();
		EventSystem.current.SetSelectedGameObject(_customNightMenuFirst);
	}

	private void OpenMainMenu()
	{
		ShowMainMenu();
		HidePlayMenu();
		HideExtraMenu();
		HideNewGameSureMenu();
		HideSceneSureMenu();
		HideSettingsMenu();
		HideGameSettingsMenu();
		HideSoundSettingsMenu();
		HideControlSettingsMenu();
		HideCustomNightMenu();
		HideCreditsMenu();
		EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
	}

	private void OpenCustumNightHandle()
	{
		ShowCustomNightMenu();
		HideGameSettingsMenu();
		HideMainMenu();
		HideExtraMenu();
		EventSystem.current.SetSelectedGameObject(_customNightMenuFirst);
	}

	public void OpenCustumNightHandleBack()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		ShowExtraMenu();
		HideCustomNightMenu();
		HideGameSettingsMenu();
		HideMainMenu();
		EventSystem.current.SetSelectedGameObject(_extraMenuFirst);
	}

	public void OnMaxEnemiesChanged(float value)
	{
		NightManager.maxEnemiesProp = Mathf.RoundToInt(value);
	}

	public void OnItemChanceChanged(float value)
	{
		NightManager.chanceToItemSpawnProp = value;
	}

	public void OnMaxItemsChanged(float value)
	{
		NightManager.maxItemsProp = Mathf.RoundToInt(value);
	}

	public void OnSettingsPress()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OpenGameSettingsMenuHandle();
	}

	public void OnSettingsBackPress()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OpenMainMenu();
	}

	public void OnCustomNightPress()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OpenCustumNightHandle();
	}

	public void OnCustomNightBackPress()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		OpenMainMenu();
	}

	public void OnFullResetPress()
	{
		HideAllMenus();
		SoundManager.PlaySound(SoundType.UIClick);
		GlobalSaveSystem.ResetProgress();
		GameSaveManager.DeleteSave();
		SceneManager.LoadScene("IntroScene");
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

	public void OpenFullGallery()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		GlobalSaveSystem.SetAllProgressTrue();
	}
}
