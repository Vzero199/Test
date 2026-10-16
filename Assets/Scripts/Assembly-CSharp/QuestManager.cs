using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmallHedge.SoundManager;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
	private struct NightSettings
	{
		public int MaxEnemies;

		public int ItemSpawnChance;

		public int MaxItems;

		public EnemyType[] EnemyTypes;

		public NightSettings(int maxEnemies, int itemSpawnChance, int maxItems, EnemyType[] enemyTypes)
		{
			MaxEnemies = maxEnemies;
			ItemSpawnChance = itemSpawnChance;
			MaxItems = maxItems;
			EnemyTypes = enemyTypes;
		}
	}

	[SerializeField]
	private GameObject endSceneTimeline;

	[SerializeField]
	private CanvasGroup blackScreen;

	[SerializeField]
	private CanvasGroup textImage;

	private const float fadeDuration = 2f;

	private const float delayBetweenFades = 1f;

	private static QuestManager instance;

	public float fadeSpeed = 1f;

	private Dictionary<string, (bool isActive, float targetAlpha)> questTargetStates = new Dictionary<string, (bool, float)>();

	private Animator _nurseAnimator;

	private string _lastAnimationName;

	private float _lastNormalizedTime;

	private static readonly EnemyType[] BaseEnemies = new EnemyType[4]
	{
		EnemyType.SisEnemy,
		EnemyType.SisSupEnemy,
		EnemyType.PosterEnemy,
		EnemyType.Nintendo
	};

	private static readonly EnemyType[] SecondNightEnemies = new EnemyType[4]
	{
		EnemyType.WhisperEnemy,
		EnemyType.HoodieCatEnemy,
		EnemyType.TVEnemy,
		EnemyType.WhiteWomanEnemy
	};

	private static readonly EnemyType[] ThirdNightEnemies = new EnemyType[3]
	{
		EnemyType.NunEnemy,
		EnemyType.VampireEnemy,
		EnemyType.MebeebEnemy
	};

	private static readonly EnemyType[] FourthNightEnemies = new EnemyType[0];

	private static readonly EnemyType[] SecondNight = BaseEnemies.Concat(SecondNightEnemies).ToArray();

	private static readonly EnemyType[] ThirdNight = SecondNight.Concat(ThirdNightEnemies).ToArray();

	private static readonly EnemyType[] FourthNight = ThirdNight.Concat(FourthNightEnemies).ToArray();

	private static readonly NightSettings[] NightConfigs = new NightSettings[5]
	{
		new NightSettings(3, 70, 30, BaseEnemies),
		new NightSettings(3, 65, 30, SecondNight),
		new NightSettings(4, 60, 30, ThirdNight),
		new NightSettings(4, 55, 30, FourthNight),
		new NightSettings(5, 50, 30, FourthNight)
	};

	private BlinkManager blinkManager => BlinkManager.GetInstance();

	private TimelineManager timelineManager => TimelineManager.GetInstance();

	public static bool IsNightLoading { get; set; } = false;

	public static QuestManager GetInstance()
	{
		if (instance == null)
		{
			instance = UnityEngine.Object.FindObjectOfType<QuestManager>();
			if (instance == null)
			{
				instance = new GameObject("QuestManager").AddComponent<QuestManager>();
			}
		}
		return instance;
	}

	public void RunMethod(string methodName)
	{
		MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
		if (method != null)
		{
			method.Invoke(this, null);
		}
		else
		{
			Debug.LogWarning("QuestManager: Метод " + methodName + " не найден.");
		}
	}

	private void LoadAndPlayTimeline(string timelineName)
	{
		string text = "Timelines/" + timelineName;
		PlayableAsset playableAsset = Resources.Load<PlayableAsset>(text);
		if (playableAsset != null && timelineManager != null && timelineManager.Director != null)
		{
			timelineManager.Director.playableAsset = playableAsset;
			timelineManager.Director.Play();
		}
		else
		{
			Debug.LogError("Не удалось загрузить Timeline: " + text);
		}
	}

	private void StartFirstDayOldmanTimeline()
	{
		SignalDialogueTrigger.SetDialogueByName("1Day/startMainQuest2P");
		LoadAndPlayTimeline("MainQuest2PDialogue");
	}

	private void FirstDaySisterOutTimeline()
	{
		QuestShine.SetDialogueToNPC("1Day/startMainQuest1P", "nurseSakimu", "mainTalk");
		LoadAndPlayTimeline("FirstDaySisterOut");
		FindAndToggleQuestTask("questTask5", toggle: false);
		FindAndToggleQuestTask("questTask6", toggle: true);
	}

	private void FirstDayNin()
	{
		QuestShine.SetDialogueToNPC("1Day/sideQuestLittleSecrets2", "nurseSakimu", "sideTalk");
		FindAndToggleQuestTask("questTask4", toggle: true);
	}

	private void FirstDayBetrayNin()
	{
		QuestShine.SetDialogueToNPC("Extra/TellSecretNintendo1", "NinPlay", "extraTalk");
		GlobalSaveSystem.SetProgress("NintendoKarma", value: false);
		FindAndToggleQuestTask("questTask4", toggle: false);
	}

	private void SetExtraTalkForAone()
	{
		QuestShine.SetDialogueToNPC("Extra/extraTalkAone2", "nurseAone", "extraTalk");
	}

	private void SetExtraTalkForVamp()
	{
		QuestShine.SetDialogueToNPC("Extra/extraTalkVampire1", "VampQuest", "extraTalk");
	}

	private void FirstDayNinSecondTalk()
	{
		QuestShine.SetDialogueToNPC("Extra/TellSecretNintendo2", "NinPlay", "extraTalk");
	}

	private void FirstDayKeepNinSecret()
	{
		QuestShine.SetDialogueToNPC("Extra/DontTellSecretNintendo", "NinPlay", "extraTalk");
		FindAndToggleQuestTask("questTask4", toggle: false);
	}

	private void SecondDayLostGirlShow()
	{
		GameObject gameObject = GameObject.Find("WhiteGirl2");
		if (gameObject != null)
		{
			gameObject.transform.position = new Vector3(6745f, 3591f, 0f);
		}
	}

	private void SecondDayLostGirlStartQuest()
	{
		GameObject whiteGirl = GameObject.Find("WhiteGirl");
		blinkManager.FadeToBlink(base.gameObject, () =>
		{
			whiteGirl.transform.position = new Vector3(2533f, 289f, 0f);
		}, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		FindAndToggleQuestTask("questTask1", toggle: true);
	}

	private void SecondDayLostGirlCompleteQuest()
	{
		FindAndToggleQuestTask("questTask1", toggle: false);
	}

	private void SecondDayIMeanNicePussyStartQuest()
	{
		FindAndToggleQuestTask("questTask2", toggle: true);
	}

	private void SecondDayIMeanNicePussyProgressQuest()
	{
		FindAndToggleQuestTask("questTask2", toggle: false);
		SoundManager.PlaySound(SoundType.TakeItem);
		FindAndToggleQuestTask("questTask3", toggle: true);
	}

	private void SecondDayIMeanNicePussyPlaceCatFood()
	{
		GameObject obj = GameObject.Find("cat_food");
		SoundManager.PlaySound(SoundType.catMeow);
		obj.transform.position = new Vector3(4125f, 1241f, 0f);
	}

	private void SecondDayIMeanNicePussyCompleteQuest()
	{
		FindAndToggleQuestTask("questTask3", toggle: false);
		QuestShine.SetDialogueToNPC("2Day/afterCompleteSideQuest2IMeanNicePussy", "HoodieCat", "extraTalk");
	}

	private void SecondDayMainQuest()
	{
		FindAndToggleQuestTask("questTask4", toggle: true);
		FindAndToggleQuestTask("questTask5", toggle: false);
	}

	private void ThirdDayMainQuestStart()
	{
		GameObject.Find("questClothes").transform.position = new Vector3(6346f, 6480f, 0f);
		GameObject.Find("questNurse2").transform.position = new Vector3(6346f, 6480f, 0f);
		FindAndToggleQuestTask("questTask1", toggle: true);
		SoundManager.PlaySound(SoundType.DoorOpen);
		FindAndToggleQuestTask("questTask7", toggle: false);
	}

	private void ThirdDayMainQuestComplete()
	{
		FindAndToggleQuestTask("questTask1", toggle: false);
		FindAndToggleQuestTask("questTask2", toggle: true);
		SoundManager.PlaySound(SoundType.TakeItem);
		QuestShine.SetDialogueToNPC("3Day/NeedSomeSleep", "HeroSleepBed", "sleepTalk");
	}

	private void ThirdDayFindLostPlushieStartQuest()
	{
		QuestShine.SetDialogueToNPC("3Day/ongoing1PSideQuest1LostBear", "questNurse3", "sideTalk");
		FindAndToggleQuestTask("questTask3", toggle: true);
	}

	private void ThirdDayFindLostPlushieOngoingQuest()
	{
		QuestShine.SetDialogueToNPC("3Day/ongoing1PSideQuest1LostBear", "questNurse3", "");
		FindAndToggleQuestTask("questTask3", toggle: false);
		FindAndToggleQuestTask("questTask4", toggle: true);
	}

	private void ThirdDayFindLostPlushieOngoingProgressQuest()
	{
		FindAndToggleQuestTask("questTask3", toggle: false);
		FindAndToggleQuestTask("questTask4", toggle: false);
		FindAndToggleQuestTask("questTask5", toggle: true);
		SetTriggerForObject("BoyBear", "ChangeAnim");
		SoundManager.PlaySound(SoundType.TakeItem);
		QuestShine.SetDialogueToNPC("3Day/completeSideQuest1LostBear", "DollDay", "sideTalk");
	}

	private void ThirdDayFindLostPlushieOngoingCompleteQuest()
	{
		FindAndToggleQuestTask("questTask5", toggle: false);
		SetTriggerForObject("DollDay", "isFun");
	}

	private void ThirdDayCottonCandyQuest()
	{
		LoadAndPlayTimeline("ThirdDayCottonCandy");
	}

	private void SetCottonCandyShort()
	{
		QuestShine.SetDialogueToNPC("3Day/startSideQuestCottonCandyShort", "SirasakiTalk", "sideTalk");
	}

	private void FourthDayMainQuestStart()
	{
		blinkManager.FadeToBlink(base.gameObject, ClearPentagram, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		FindAndToggleQuestTask("questTask1", toggle: true);
		FindAndToggleQuestTask("questTask4", toggle: false);
	}

	private void ClearPentagram()
	{
		GameObject.Find("pentagramOnTheWall").transform.position = new Vector3(16407f, 9528f, 0f);
		SoundManager.PlaySound(SoundType.TheManWhoErasedHisName);
	}

	private void FourthDayBloodStepShort()
	{
		QuestShine.SetDialogueToNPC("4Day/startSideQuest1BloodStepShort", "VampQuest", "sideTalk");
	}

	private void FourthDayJointPrayerShort()
	{
		QuestShine.SetDialogueToNPC("4Day/startSideQuest2JointPrayerShort", "NunDay", "sideTalk");
	}

	private void FourthDayBloodStepStart()
	{
		FindAndToggleQuestTask("questTask2", toggle: true);
	}

	private void FourthDayBloodStepProgress()
	{
		FindAndToggleQuestTask("questTask2", toggle: false);
		FindAndToggleQuestTask("questTask3", toggle: true);
	}

	private void FourthDayBloodStepComplete()
	{
		FindAndToggleQuestTask("questTask3", toggle: false);
	}

	private void FourthDayJointPrayer()
	{
		SetDialogueForObject("SignalReceiver", "4Day/completeSideQuest2JointPrayer");
		LoadAndPlayTimeline("FourthDayJointPrayerTask");
	}

	private void FifthNightMainTimelineWithTrueEnd()
	{
		LoadAndPlayTimeline("FifthNightMainTimelineWithTrueEnd");
	}

	private void SetTriggerForObject(string objectName, string triggerName)
	{
		GameObject gameObject = GameObject.Find(objectName);
		if (triggerName == null)
		{
			Debug.LogError("Failed To Find Object: " + objectName);
			return;
		}
		Animator component = gameObject.GetComponent<Animator>();
		if (component == null)
		{
			Debug.LogError("Object: " + objectName + " No Have Animator Component");
		}
		else
		{
			component.SetTrigger(triggerName);
		}
	}

	private void findStuffQuest()
	{
		string[] array = new string[3] { "questShine1", "questShine2", "questShine3" };
		GameObject[] array2 = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = GameObject.Find(array[i]);
			if (array2[i] == null)
			{
				Debug.LogWarning("Объект с именем " + array[i] + " не найден на сцене.");
			}
		}
		array2 = array2.Where((GameObject obj) => obj != null).ToArray();
		if (array2.Length == 0)
		{
			Debug.LogWarning("Нет доступных объектов questShine для перемещения.");
			return;
		}
		List<Vector3> list = new Vector3[3]
		{
			new Vector3(903f, 3588f, 0f),
			new Vector3(523f, 3601f, 0f),
			new Vector3(136f, 3572f, 0f)
		}.ToList();
		Shuffle(list);
		for (int num = 0; num < array2.Length; num++)
		{
			if (num < list.Count)
			{
				array2[num].transform.position = list[num];
			}
			else
			{
				Debug.LogWarning("Не хватает уникальных позиций для всех найденных объектов questShine.");
			}
		}
		FindAndToggleQuestTask("questTask6", toggle: false);
		FindAndToggleQuestTask("questTask1", toggle: true);
	}

	private void Shuffle<T>(List<T> list)
	{
		System.Random random = new System.Random();
		int count = list.Count;
		while (count > 1)
		{
			int index = random.Next(count--);
			T value = list[count];
			list[count] = list[index];
			list[index] = value;
		}
	}

	private void FirstDayQuestComplete()
	{
		FindAndToggleQuestTask("questTask2", toggle: false);
		FindAndToggleQuestTask("questTask3", toggle: true);
	}

	private void FifthDayQuest()
	{
		FindAndToggleQuestTask("questTask1", toggle: false);
		FindAndToggleQuestTask("questTask2", toggle: true);
	}

	public (bool isActive, float targetAlpha)? GetQuestTargetState(string taskName)
	{
		if (questTargetStates.TryGetValue(taskName, out (bool, float) value))
		{
			return value;
		}
		return null;
	}

	public void FindAndToggleQuestTask(string taskName, bool toggle)
	{
		float item = (toggle ? 1f : 0f);
		questTargetStates[taskName] = (toggle, item);
		GameObject gameObject = GameObject.Find("Canvas");
		if (gameObject == null)
		{
			Debug.LogError("Canvas not found in scene.");
			return;
		}
		Transform transform = gameObject.transform.Find("TaskUI/TaskPanel/TaskGroup/" + taskName);
		if (transform != null)
		{
			GameObject gameObject2 = transform.gameObject;
			if (toggle)
			{
				gameObject2.SetActive(value: true);
				_ = transform.parent;
				transform.SetSiblingIndex(1);
				RectTransform component = gameObject2.GetComponent<RectTransform>();
				StartCoroutine(AnimateToNewLayoutPosition(component));
			}
			TMP_Text component2 = gameObject2.GetComponent<TMP_Text>();
			if (component2 == null)
			{
				Debug.LogError("TMP_Text not found on object: " + taskName);
			}
			else
			{
				StartCoroutine(FadeTMPAlpha(component2, toggle ? 1f : 0f, gameObject2, toggle));
			}
		}
		else
		{
			Debug.LogError("Failed To Find Quest Task By Name: " + taskName);
		}
	}

	private IEnumerator AnimateToNewLayoutPosition(RectTransform rect, float duration = 0.25f)
	{
		Vector2 startPos = rect.anchoredPosition;
		yield return new WaitForEndOfFrame();
		Vector2 targetPos = rect.anchoredPosition;
		float time = 0f;
		rect.anchoredPosition = startPos;
		while (time < duration)
		{
			rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / duration);
			time += Time.deltaTime;
			yield return null;
		}
		rect.anchoredPosition = targetPos;
	}

	private IEnumerator FadeTMPAlpha(TMP_Text text, float targetAlpha, GameObject objToToggle, bool toggle)
	{
		Color textColor = text.color;
		Image[] images = objToToggle.GetComponentsInChildren<Image>(includeInactive: true);
		Color[] array = new Color[images.Length];
		for (int i = 0; i < images.Length; i++)
		{
			array[i] = images[i].color;
		}
		while (!Mathf.Approximately(textColor.a, targetAlpha))
		{
			textColor.a = Mathf.MoveTowards(textColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
			text.color = textColor;
			for (int j = 0; j < images.Length; j++)
			{
				Color color = images[j].color;
				color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
				images[j].color = color;
			}
			yield return null;
		}
		if (!toggle)
		{
			objToToggle.SetActive(value: false);
			Transform obj = objToToggle.transform;
			Transform parent = obj.parent;
			obj.SetSiblingIndex(parent.childCount - 1);
		}
	}

	private void TrueEndScene()
	{
		StartCoroutine(BedEndingScene());
	}

	private IEnumerator BedEndingScene()
	{
		GlobalSaveSystem.SetProgress("TrueEndIsReceive", value: true);
		GameObject gameObject = GameObject.Find("BGM");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		GameObject gameObject2 = GameObject.Find("AudioSounds");
		if (gameObject2 != null)
		{
			UnityEngine.Object.Destroy(gameObject2);
		}
		GameObject gameObject3 = GameObject.Find("TorchAmbience");
		if (gameObject3 != null)
		{
			UnityEngine.Object.Destroy(gameObject3);
		}
		GameObject gameObject4 = GameObject.Find("poofbed");
		if (gameObject4 != null)
		{
			gameObject4.GetComponent<Animator>().Play("Poof");
		}
		CameraFollow cameraFollow = UnityEngine.Object.FindObjectOfType<CameraFollow>();
		if (cameraFollow != null)
		{
			cameraFollow.enabled = false;
		}
		Player.GetInstance().enabled = false;
		yield return new WaitForSeconds(1f);
		blinkManager.FadeToBlink(base.gameObject, () =>
		{
			endSceneTimeline.SetActive(value: true);
		}, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		UnityEngine.Object.Destroy(Player.GetInstance());
	}

	private void SetNurseAoneExpression(string expressionType)
	{
		GameObject gameObject = GameObject.Find("nurseAone");
		if (gameObject == null)
		{
			Debug.LogError("Не найден объект 'nurseAone'");
			return;
		}
		_nurseAnimator = gameObject.GetComponent<Animator>();
		if (_nurseAnimator == null)
		{
			Debug.LogError("Не найден аниматор 'nurseAone'");
			return;
		}
		string text = "Aone" + expressionType;
		if (!string.IsNullOrEmpty(_lastAnimationName))
		{
			_lastNormalizedTime = _nurseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		_nurseAnimator.Play(text, 0, _lastNormalizedTime);
		_lastAnimationName = text;
	}

	private void NormalEndAoneSmile()
	{
		SetNurseAoneExpression("Smile");
	}

	private void NormalEndAoneAngry()
	{
		SetNurseAoneExpression("Angry");
	}

	public static void SetDialogueForObject(string objectName, string dialogueName)
	{
		GameObject gameObject = GameObject.Find(objectName);
		if (gameObject == null)
		{
			Debug.LogError("Object: " + objectName + " Not Found!");
			return;
		}
		DialogueTrigger componentInChildren = gameObject.GetComponentInChildren<DialogueTrigger>();
		if (componentInChildren == null)
		{
			Debug.LogError($"Not Found Dialogue Trigger For Object: {componentInChildren}");
		}
		else
		{
			componentInChildren.DialogueName = dialogueName;
		}
	}

	public void StartNight(int nightIndex, string nightName)
	{
		Player player = Player.GetInstance();
		player.IsPaused = false;
		player.ChangeState(new PlayerIdleState());
		player.enabled = false;
		StartCoroutine(FadeSequence(nightName));
	}

	private IEnumerator FadeSequence(string sceneName)
	{
		IsNightLoading = true;
		StartCoroutine(FadeOutAmbienceSounds(0.5f));
		yield return FadeCanvasGroup(blackScreen, 0f, 1f, 2f);
		yield return PlaySoundsWithDelay();
		SoundManager.PlaySound(SoundType.ClockStop);
		yield return FadeCanvasGroup(textImage, 0f, 1f, 2f);
		yield return new WaitForSeconds(1f);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		asyncLoad.allowSceneActivation = false;
		yield return FadeCanvasGroupSmooth(textImage, 1f, 0f, 2f);
		IsNightLoading = false;
		asyncLoad.allowSceneActivation = true;
	}

	private IEnumerator FadeOutAmbienceSounds(float duration)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Ambience");
		List<AudioSource> sources = new List<AudioSource>();
		GameObject[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			AudioSource component = array2[i].GetComponent<AudioSource>();
			if (component != null && component.volume > 0f)
			{
				sources.Add(component);
			}
		}
		float time = 0f;
		Dictionary<AudioSource, float> originalVolumes = sources.ToDictionary((AudioSource s) => s, (AudioSource s) => s.volume);
		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			foreach (AudioSource item in sources)
			{
				if (item != null)
				{
					item.volume = Mathf.Lerp(originalVolumes[item], 0f, t);
				}
			}
			yield return null;
		}
		foreach (AudioSource item2 in sources)
		{
			if (item2 != null)
			{
				item2.volume = 0f;
			}
		}
	}

	private IEnumerator PlaySoundsWithDelay()
	{
		SoundManager.PlaySound(SoundType.Clock1);
		yield return new WaitForSeconds(1f);
		SoundManager.PlaySound(SoundType.Clock2);
		yield return new WaitForSeconds(1f);
		SoundManager.PlaySound(SoundType.Clock3);
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
	}

	private IEnumerator FadeCanvasGroupSmooth(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			float t = elapsedTime / duration;
			canvasGroup.alpha = Mathf.SmoothStep(startAlpha, endAlpha, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
	}

	private IEnumerator LoadSceneAsync(string sceneName)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
		asyncLoad.allowSceneActivation = false;
		while (asyncLoad.progress < 0.9f)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		asyncLoad.allowSceneActivation = true;
	}

	private void EndSceneFirstSwitch()
	{
		SetDialogueForObject("SignalReceiver", "Night/endSceneSecondLine");
	}

	private void EndSceneSecondSwitch()
	{
		SetDialogueForObject("SignalReceiver", "Night/endSceneThirdLine");
	}

	private void EndSceneThirdSwitch()
	{
		if (!GlobalSaveSystem.AreAllKarmasTrue())
		{
			SetDialogueForObject("SignalReceiver", "Night/endSceneFirstLineNormalEnd");
		}
		else
		{
			SetDialogueForObject("SignalReceiver", "Night/endSceneFirstLineTrueEnd");
		}
	}

	private void EndSceneTrueEndSwitch()
	{
		SetDialogueForObject("SignalReceiver", "Night/endSceneSecondLineTrueEnd");
	}

	private void StartNormalEndScene()
	{
		blinkManager.FadeToBlink(base.gameObject, () =>
		{
			SceneManager.LoadScene("NormalEndScene");
		}, allowVerticalMovement: true, 0f, 0f, base.gameObject);
	}

	private void TrueEndFirstSwitch()
	{
		SwitchToTrueEnd(2);
	}

	private void TrueEndSecondSwitch()
	{
		SwitchToTrueEnd(3);
	}

	private void TrueEndThirdSwitch()
	{
		SwitchToTrueEnd(4);
	}

	private void TrueEndFourthSwitch()
	{
		SwitchToTrueEnd(5);
	}

	public void SwitchToTrueEnd(int endIndex)
	{
		StartCoroutine(SwitchTrueEndWithFade($"Sprites/The_end_Succubus_{endIndex}", endIndex == 5));
	}

	private IEnumerator SwitchTrueEndWithFade(string spritePath, bool playSound)
	{
		GameObject gameObject = GameObject.Find("EndFader");
		if (gameObject == null)
		{
			Debug.LogError("Объект EndFader не найден.");
			yield break;
		}
		CanvasGroup faderGroup = gameObject.GetComponent<CanvasGroup>();
		if (faderGroup == null)
		{
			Debug.LogError("На объекте EndFader отсутствует CanvasGroup.");
			yield break;
		}
		GameObject dialoguePanel = GameObject.Find("DialoguePanel");
		if (dialoguePanel != null)
		{
			dialoguePanel.SetActive(value: false);
		}
		Time.timeScale = 0f;
		float fadeTime = 0.5f;
		float timer = 0f;
		while (timer < fadeTime)
		{
			timer += Time.unscaledDeltaTime;
			faderGroup.alpha = Mathf.Clamp01(timer / fadeTime);
			yield return null;
		}
		faderGroup.alpha = 1f;
		Sprite sprite = Resources.Load<Sprite>(spritePath);
		GameObject gameObject2 = GameObject.Find("trueEndBG");
		if (gameObject2 != null && sprite != null)
		{
			Image component = gameObject2.GetComponent<Image>();
			if (component != null)
			{
				component.sprite = sprite;
			}
			yield return null;
		}
		timer = 0f;
		while (timer < fadeTime)
		{
			timer += Time.unscaledDeltaTime;
			faderGroup.alpha = 1f - Mathf.Clamp01(timer / fadeTime);
			yield return null;
		}
		faderGroup.alpha = 0f;
		Time.timeScale = 1f;
		if (dialoguePanel != null && !playSound)
		{
			dialoguePanel.SetActive(value: true);
		}
		yield return new WaitForSeconds(0.5f);
		if (playSound && blinkManager != null)
		{
			blinkManager.BlinkWithCustomSpeed(0.5f, base.gameObject, () =>
			{
				SceneManager.LoadScene("MainMenu");
			}, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		}
		yield return new WaitForSeconds(0.5f);
		if (playSound)
		{
			SoundManager.PlaySound(SoundType.SuccubusSnap, null, 0.15f);
		}
	}

	private void ReturnToMainMenu()
	{
	}

	private void StartFirstNight()
	{
		StartNight(0, "FirstNight");
	}

	private void StartSecondNight()
	{
		StartNight(1, "SecondNight");
	}

	private void StartThirdNight()
	{
		StartNight(2, "ThirdNight");
	}

	private void StartFourthNight()
	{
		StartNight(3, "FourthNight");
	}

	private void StartFifthNight()
	{
		StartNight(4, "FifthNight");
	}

	public void StartNightmare()
	{
		StartFifthNight();
	}

	private void LoadEnemySettings(EnemyType[] enemyTypes)
	{
		NightManager.enemiesInNightProp.Clear();
		foreach (EnemyType enemyType in enemyTypes)
		{
			BaseEnemySettings baseEnemySettings = LoadEnemySettingsFromResources(enemyType);
			if (baseEnemySettings != null)
			{
				NightManager.enemiesInNightProp.Add(baseEnemySettings);
			}
			else
			{
				Debug.LogWarning($"Не удалось загрузить настройки для врага: {enemyType}");
			}
		}
	}

	private BaseEnemySettings LoadEnemySettingsFromResources(EnemyType type)
	{
		string text = "";
		text = ((type != EnemyType.Nintendo) ? $"enemySettings/{type}Settings" : $"enemySettings/{type}EntitySettings");
		return Resources.Load<BaseEnemySettings>(text);
	}
}
