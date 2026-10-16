using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class NightManager : MonoBehaviour
{
	[SerializeField]
	private Timer timer;

	[Range(10f, 60f)]
	[SerializeField]
	public float vanishTime = 30f;

	[SerializeField]
	[Range(0f, 60f)]
	public float respawnTime = 30f;

	[SerializeField]
	[Range(2f, 10f)]
	private float globalRespawnDelay = 5f;

	[SerializeField]
	[Range(20f, 60f)]
	private float nintendoRefreshTargetTime = 15f;

	[SerializeField]
	[Range(0f, 10f)]
	public int maxEnemies;

	[SerializeField]
	public bool useFixedMaxItems;

	[SerializeField]
	public int maxNormalItems = 20;

	[SerializeField]
	public int maxSpecialItems = 5;

	[SerializeField]
	public List<BaseEnemySettings> enemiesInNight = new List<BaseEnemySettings>();

	[SerializeField]
	private ItemSpawnSettings[] itemsInNight;

	[SerializeField]
	private GameObject shinePrefab;

	[SerializeField]
	private GameObject UICanvas;

	[SerializeField]
	private Light2D globalLight;

	[SerializeField]
	[Range(0f, 100f)]
	private float chanceToItemSpawn = 40f;

	[SerializeField]
	public GameObject[] normalSpawnPoints;

	[SerializeField]
	public GameObject[] specialSpawnPoints;

	[SerializeField]
	public GameObject[] nintendosDildosSP;

	private List<GameObject> spawnedShineItems = new List<GameObject>();

	public static ItemSpawnSettings[] ItemsInNightProp = null;

	private float nintendoTargetChancePlayer;

	private float nintendoTargetChanceToChasePlayer;

	private float nintendoTargetChanceLamp;

	private float hideTimer;

	[Range(0f, 10f)]
	public static int maxEnemiesProp = -1;

	[Range(0f, 50f)]
	public static int maxItemsProp = -1;

	[Range(0f, 100f)]
	public static float chanceToItemSpawnProp = -1f;

	public static List<BaseEnemySettings> enemiesInNightProp = new List<BaseEnemySettings>();

	private float nintendoRefreshTargetTimer;

	private bool isWW;

	private GameObject WWObject;

	private WhiteWomanEnemySettings WWSettings;

	private MebeebEnemySettings mebeebSettings;

	private Player player;

	private float currentHour;

	private float currentHalfHour;

	private GameObject[] enemies;

	private ShineItem[] shineItems;

	private BoxCollider[] spawnAreaColliders;

	private Dictionary<BaseEnemySettings, float> enemyPriorities = new Dictionary<BaseEnemySettings, float>();

	private Dictionary<GameObject, Coroutine> respawnCoroutines = new Dictionary<GameObject, Coroutine>();

	private Coroutine stopGameCoroutine;

	private Nintendo nintendo;

	private bool isNintendoRefreshTarget;

	private List<BaseEnemySettings> spawnPool = new List<BaseEnemySettings>();

	private static NightManager instance;

	private string resourcePath = "prefabs/NintendoDildo";

	private string resourcePathForIco = "prefabs/mapDildoIcon";

	private GameObject spawnedDildo;

	private GameObject spawnedDildoIco;

	private bool isDildoQuest;

	private Coroutine SpawnAfterDelayCoroutine;

	private Dictionary<GameObject, Coroutine> pendingClearEnemies = new Dictionary<GameObject, Coroutine>();

	private bool hasSpawnedSkipWatch;

	private bool hasPickedUpSkipWatch;

	private bool skipWatchProtected;

	public Coroutine StopGameCoroutine => stopGameCoroutine;

	public static NightManager Instance => instance;

	private void OnDestroy()
	{
		if (stopGameCoroutine != null)
		{
			StopCoroutine(stopGameCoroutine);
			stopGameCoroutine = null;
		}
		if (SpawnAfterDelayCoroutine != null)
		{
			StopCoroutine(SpawnAfterDelayCoroutine);
			SpawnAfterDelayCoroutine = null;
		}
		if (spawnedDildo != null)
		{
			UnityEngine.Object.Destroy(spawnedDildo);
		}
		if (spawnedDildoIco != null)
		{
			UnityEngine.Object.Destroy(spawnedDildoIco);
		}
		isDildoQuest = false;
		spawnedDildoIco = null;
		instance = null;
		if (shineItems != null)
		{
			ShineItem[] array = shineItems;
			foreach (ShineItem shineItem in array)
			{
				if (shineItem != null)
				{
					UnityEngine.Object.Destroy(shineItem.gameObject);
				}
			}
		}
		spawnedShineItems.Clear();
	}

	public void FadeWWObject(GameObject WW)
	{
		StartCoroutine(FadeAndDestroy(WW));
	}

	private IEnumerator FadeAndDestroy(GameObject target)
	{
		if (target == null)
		{
			yield break;
		}
		SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
		if (sr == null)
		{
			UnityEngine.Object.Destroy(target);
			yield break;
		}
		float duration = 1f;
		float elapsed = 0f;
		Color originalColor = sr.color;
		while (elapsed < duration)
		{
			if (target == null || sr == null)
			{
				yield break;
			}
			elapsed += Time.deltaTime;
			float a = Mathf.Lerp(1f, 0f, elapsed / duration);
			sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
			yield return null;
		}
		if (target != null)
		{
			UnityEngine.Object.Destroy(target);
		}
	}

	private void Awake()
	{
		stopGameCoroutine = null;
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static NightManager GetInstance()
	{
		return instance;
	}

	private void SetSettings()
	{
		if (ItemsInNightProp != null && ItemsInNightProp.Length != 0)
		{
			itemsInNight = ItemsInNightProp;
			if (chanceToItemSpawnProp > -1f)
			{
				chanceToItemSpawn = chanceToItemSpawnProp;
			}
			Difficulty difficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2);
			if (difficulty == Difficulty.Easy || difficulty == Difficulty.Practice)
			{
				chanceToItemSpawn = Mathf.Min(chanceToItemSpawn + 20f, 100f);
			}
		}
		if (enemiesInNightProp.Count != 0)
		{
			enemiesInNight = enemiesInNightProp.ToList();
			if (maxEnemiesProp > -1)
			{
				maxEnemies = maxEnemiesProp;
			}
		}
	}

	public Dictionary<GameObject, DoorTransit> ConvertToDoorTransits(Dictionary<GameObject, InteractionObject> originalDictCache)
	{
		return originalDictCache.Where((KeyValuePair<GameObject, InteractionObject> pair) => pair.Value is DoorTransit).ToDictionary((KeyValuePair<GameObject, InteractionObject> pair) => pair.Key, (KeyValuePair<GameObject, InteractionObject> pair) => (DoorTransit)pair.Value);
	}

	private void CalculateChaseChance()
	{
		string[] obj = new string[6] { "HintNHideReady", "HintNSupNurseKeyReady", "HintNPosterReady", "HintNBatsReady", "HintNDollReady", "HintNWhisperReady" };
		nintendoTargetChanceToChasePlayer = 0f;
		string[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			if (GlobalSaveSystem.GetProgress(array[i]))
			{
				nintendoTargetChanceToChasePlayer += 0.2f;
			}
		}
	}

	private void Start()
	{
		CalculateChaseChance();
		ResetSkipWatchSpawn();
		SetSettings();
		player = UnityEngine.Object.FindObjectOfType<Player>();
		if (player == null)
		{
			Debug.LogError("Объект игрока не найден");
		}
		enemies = new GameObject[maxEnemies];
		int num = (useFixedMaxItems ? maxNormalItems : normalSpawnPoints.Length);
		int num2 = (useFixedMaxItems ? maxSpecialItems : specialSpawnPoints.Length);
		shineItems = new ShineItem[num + num2];
		spawnAreaColliders = (from obj in GameObject.FindGameObjectsWithTag("Spawncoll")
			select obj.GetComponent<BoxCollider>() into collider
			where collider != null
			select collider).ToArray();
		StartCoroutine(CreateNintendoAfterDelay());
		bool flag = enemiesInNight.Any((BaseEnemySettings setting) => setting is WhiteWomanEnemySettings);
		if (flag)
		{
			WhiteWomanEnemySettings whiteWomanEnemySettings = enemiesInNight.FirstOrDefault((BaseEnemySettings setting) => setting is WhiteWomanEnemySettings) as WhiteWomanEnemySettings;
			enemiesInNight.Remove(whiteWomanEnemySettings);
			isWW = flag;
			WWSettings = whiteWomanEnemySettings;
		}
		else
		{
			Debug.Log("WW Not Set in NightManager");
		}
		string text = SceneManager.GetActiveScene().name;
		if (text == "FirstNight" || text == "FirstNIght")
		{
			SpawnShineItems();
			AddHour();
		}
		if (enemiesInNight.Any((BaseEnemySettings setting) => setting is MebeebEnemySettings))
		{
			mebeebSettings = enemiesInNight.FirstOrDefault((BaseEnemySettings setting) => setting is MebeebEnemySettings) as MebeebEnemySettings;
			enemiesInNight.Remove(mebeebSettings);
		}
		InitializePriorities();
		if (globalLight != null)
		{
			globalLight.intensity = 0f;
		}
	}

	private IEnumerator CreateNintendoAfterDelay()
	{
		yield return new WaitForSeconds(10f);
		NintendoEntitySettings nintendoEntitySettings = enemiesInNight.OfType<NintendoEntitySettings>().FirstOrDefault();
		if (nintendoEntitySettings != null)
		{
			enemiesInNight.Remove(nintendoEntitySettings);
			nintendo = CreateNintendoInstance(nintendoEntitySettings);
			if (nintendo != null)
			{
				nintendoRefreshTargetTimer = 1f;
				StartCoroutine(SetNintendoTargetAfterInit());
			}
			else
			{
				Debug.LogError("Nintendo Not Spawned!");
			}
		}
		else
		{
			Debug.LogWarning("Nintendo Not Set in NightManager");
		}
	}

	private IEnumerator SetNintendoTargetAfterInit()
	{
		yield return new WaitForSeconds(0.5f);
		SetNewTargetForNintendo();
	}

	private Nintendo CreateNintendoInstance(NintendoEntitySettings entitySettings)
	{
		Vector3 randomSpawnPositionWithColliderHeight = GetRandomSpawnPositionWithColliderHeight(entitySettings);
		Nintendo nintendo = UnityEngine.Object.FindObjectOfType<Nintendo>();
		if (nintendo != null)
		{
			Debug.LogWarning("Nintendo is already Exists!");
			return nintendo;
		}
		Nintendo obj = new GameObject("Nintendo").AddComponent<Nintendo>();
		obj.Settings = entitySettings;
		obj.CurrentMoveSpeed = entitySettings.currentMoveSpeed;
		obj.CurrentInteractionSpeed = entitySettings.currentInteractionSpeed;
		obj.InteractionCooldown = entitySettings.interactionCooldown;
		obj.ChaseDistance = entitySettings.chaseDistance;
		obj.transform.position = new Vector3(randomSpawnPositionWithColliderHeight.x, randomSpawnPositionWithColliderHeight.y + 50f, randomSpawnPositionWithColliderHeight.z);
		obj.InteractionBoostTime = entitySettings.interactionBoostTime;
		obj.SetAnimatorController(entitySettings.animatorController);
		obj.SetAnimator(entitySettings.animator);
		obj.Initialize();
		return obj;
	}

	public void EndTvScene()
	{
		if (EndEvent.isGameOver || Player.GetInstance().CurrentHeartNumber <= 1)
		{
			Player.GetInstance().CurrentHeartNumber--;
		}
		else if (TVEnemy.GetInstance() != null)
		{
			TVEnemy.GetInstance().EndScene();
		}
	}

	public void AddHour()
	{
		currentHour++;
	}

	private void Update()
	{
		if (EndEvent.isGameOver)
		{
			return;
		}
		if (spawnedDildo == null && isDildoQuest)
		{
			isDildoQuest = false;
			UnityEngine.Object.Destroy(spawnedDildoIco);
			SoundManager.PlaySound(SoundType.NinLaugh);
			spawnedDildoIco = null;
			nintendo.HideAway();
		}
		if (timer.ElapsedTime / 60f > currentHour && !Timer.IsTimeBoostActive && timer.ElapsedTime < Timer.MaxTime)
		{
			currentHour++;
			nintendoTargetChanceToChasePlayer += 0.13f;
			ResetInstances();
			CancelAllRespawns();
		}
		if (mebeebSettings != null && timer.ElapsedTime / 40f > currentHalfHour)
		{
			currentHalfHour++;
			SpawnMebeebEnemy(mebeebSettings);
		}
		if (nintendo != null && nintendo.isReachedMainGoal && !isNintendoRefreshTarget)
		{
			if (nintendo.MainGoal == null || (nintendo.MainGoal != null && (bool)nintendo.MainGoal.GetComponent<TabLight>()))
			{
				nintendoRefreshTargetTimer = 2f;
			}
			else
			{
				nintendoRefreshTargetTimer = nintendoRefreshTargetTime;
			}
			isNintendoRefreshTarget = true;
		}
		if (stopGameCoroutine != null)
		{
			return;
		}
		CheckPlayerState();
		HandleTimers();
		GameObject[] array = enemies;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				Enemy component = gameObject.GetComponent<Enemy>();
				if (!(component == null) && stopGameCoroutine == null)
				{
					HandleCatchMode(component);
				}
			}
		}
		if (nintendo != null && nintendo.CurrentState is NintendoHaveFunState && stopGameCoroutine == null)
		{
			if (stopGameCoroutine != null)
			{
				StopCoroutine(stopGameCoroutine);
			}
			if (SpawnAfterDelayCoroutine != null)
			{
				StopCoroutine(SpawnAfterDelayCoroutine);
				SpawnAfterDelayCoroutine = null;
			}
			stopGameCoroutine = StartCoroutine(StopGameTime(null));
		}
	}

	private void CheckPlayerState()
	{
		if (!(nintendo != null) || !(Player.GetInstance().CurrentState is PlayerHideState))
		{
			return;
		}
		hideTimer += Time.deltaTime;
		if (hideTimer >= 10f)
		{
			nintendoTargetChancePlayer += 0.13f;
			nintendoTargetChanceToChasePlayer += 0.13f;
			EnableNintendoForceRollTarget();
			if (nintendoTargetChanceToChasePlayer >= 0.5f)
			{
				EnableNintendoForceQuest();
			}
			hideTimer = 0f;
		}
	}

	private void EnableNintendoForceRollTarget()
	{
		if (!nintendo.TargetIsPlayer() && UnityEngine.Random.Range(0f, 1f) <= nintendoTargetChancePlayer)
		{
			if (spawnedDildo != null)
			{
				UnityEngine.Object.Destroy(spawnedDildo);
				UnityEngine.Object.Destroy(spawnedDildoIco);
				spawnedDildo = null;
				spawnedDildoIco = null;
				isDildoQuest = false;
			}
			nintendo.SetTargetRace(Player.GetInstance().HideCabin);
			nintendoRefreshTargetTimer = 0f;
			nintendoTargetChancePlayer = 0f;
			isNintendoRefreshTarget = true;
		}
	}

	private void EnableNintendoForceQuest()
	{
		if (!nintendo.TargetIsPlayer() && UnityEngine.Random.Range(0f, 1f) <= nintendoTargetChanceToChasePlayer)
		{
			nintendo.SetTargetRace(Player.GetInstance().gameObject);
			SpawnNintendoDildo();
			nintendoRefreshTargetTimer = 0f;
			nintendoTargetChanceToChasePlayer = 0f;
			isNintendoRefreshTarget = true;
		}
	}

	private void HandleTimers()
	{
		if (nintendoRefreshTargetTimer > 0f && nintendo.isReachedMainGoal)
		{
			nintendoRefreshTargetTimer -= Time.deltaTime;
			nintendoRefreshTargetTimer = Mathf.Max(nintendoRefreshTargetTimer, 0f);
			if (nintendoRefreshTargetTimer == 0f)
			{
				SetNewTargetForNintendo();
			}
		}
	}

	private void SpawnNintendoDildo()
	{
		Transform randomSpawnPoint = GetRandomSpawnPoint();
		if (randomSpawnPoint == null)
		{
			return;
		}
		GameObject gameObject = FindMapSegmentAtPosition(randomSpawnPoint.position);
		if (!(gameObject == null))
		{
			GameObject gameObject2 = LoadPrefab(resourcePath);
			if (!(gameObject2 == null))
			{
				spawnedDildo = UnityEngine.Object.Instantiate(gameObject2, randomSpawnPoint.position, Quaternion.identity);
				isDildoQuest = true;
				AttachToUI(gameObject.name);
			}
		}
	}

	private Transform GetRandomSpawnPoint()
	{
		if (nintendosDildosSP == null || nintendosDildosSP.Length == 0)
		{
			Debug.LogWarning("Точки спавна не назначены!");
			return null;
		}
		GameObject playerCurrentRoom = Player.GetInstance().CurrentRoomObject;
		Transform[] array = (from sp in nintendosDildosSP
			where sp != null
			where HospitalManager.GetInstance().GetRoomContainingObject(sp.gameObject) != playerCurrentRoom
			select sp.transform).ToArray();
		if (array.Length != 0)
		{
			return array[UnityEngine.Random.Range(0, array.Length)];
		}
		Debug.LogWarning("Не найдено точек спавна вне текущей комнаты игрока. Используется случайная точка.");
		return nintendosDildosSP[UnityEngine.Random.Range(0, nintendosDildosSP.Length)].transform;
	}

	private GameObject FindMapSegmentAtPosition(Vector3 position)
	{
		GameObject obj = GameObject.FindGameObjectsWithTag("MapSegment").FirstOrDefault((GameObject gameObject) => gameObject.GetComponent<BoxCollider>()?.bounds.Contains(position) ?? false);
		if (obj == null)
		{
			Debug.LogError("Не найден объект MapSegment в указанной позиции спавна!");
		}
		return obj;
	}

	private GameObject LoadPrefab(string path)
	{
		GameObject obj = Resources.Load<GameObject>(path);
		if (obj == null)
		{
			Debug.LogError("Не удалось загрузить объект по пути: Resources/" + path);
		}
		return obj;
	}

	private void AttachToUI(string segmentName)
	{
		GameObject gameObject = GameObject.Find("mapNight");
		if (gameObject == null)
		{
			Debug.LogError("Не найден UI объект mapNight!");
			return;
		}
		GameObject gameObject2 = LoadPrefab(resourcePathForIco);
		if (!(gameObject2 == null))
		{
			spawnedDildoIco = UnityEngine.Object.Instantiate(gameObject2);
			spawnedDildoIco.transform.SetParent(gameObject.transform, worldPositionStays: false);
			RectTransform component = spawnedDildoIco.GetComponent<RectTransform>();
			component.anchoredPosition = segmentName switch
			{
				"WCMale3F" => new Vector2(-170f, 243f), 
				"LockerRoom3F" => new Vector2(125f, 243f), 
				"UtilityRoom3F" => new Vector2(420f, 243f), 
				"WCMale2F" => new Vector2(-170f, 118f), 
				"UtilityRoom2F" => new Vector2(420f, 118f), 
				"WCMale1F" => new Vector2(-8f, -18f), 
				"LockerRoom1F" => new Vector2(237f, -18f), 
				"UtilityRoom1F" => new Vector2(334f, -18f), 
				_ => throw new ArgumentException("Имя сегмента не найдено в списке!"), 
			};
		}
	}

	private void SetNewTargetForNintendo()
	{
		if (!nintendo.TargetIsPlayer())
		{
			nintendo.isReachedMainGoal = false;
			Debug.Log("Set New Target For Nintendo");
			isNintendoRefreshTarget = false;
			if (UnityEngine.Random.Range(0f, 1f) <= nintendoTargetChanceToChasePlayer && !(Player.GetInstance().CurrentState is PlayerHideState) && stopGameCoroutine == null)
			{
				Debug.Log("Nintendo target is Player");
				nintendo.SetTargetRace(Player.GetInstance().gameObject);
				SpawnNintendoDildo();
				nintendoTargetChanceToChasePlayer = 0f;
			}
			else
			{
				UnityEngine.Random.Range(0f, 1f);
				Debug.Log("Nintendo target is Hide");
				nintendo.HideAway();
			}
		}
	}

	private void CalculateLampChance()
	{
		nintendoTargetChanceLamp = 0f;
		GameObject[] array = GameObject.FindGameObjectsWithTag("TabLight");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].activeSelf)
			{
				nintendoTargetChanceLamp += 0f;
			}
		}
	}

	private GameObject GetRandomTabLight()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("TabLight");
		if (array.Length == 0)
		{
			Debug.LogWarning("No TabLights objects with script found.");
			return null;
		}
		int num = UnityEngine.Random.Range(0, array.Length);
		return array[num].transform.parent.gameObject;
	}

	private IEnumerator FadeShineItems(float targetAlpha, float duration)
	{
		List<SpriteRenderer> shineRenderers = new List<SpriteRenderer>();
		ShineItem[] array = shineItems;
		foreach (ShineItem shineItem in array)
		{
			if (shineItem != null && shineItem.gameObject != null)
			{
				SpriteRenderer component = shineItem.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					shineRenderers.Add(component);
				}
			}
		}
		if (shineRenderers.Count == 0)
		{
			yield break;
		}
		float elapsed = 0f;
		Color[] originalColors = shineRenderers.Select((SpriteRenderer sr) => sr.color).ToArray();
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			for (int num = 0; num < shineRenderers.Count; num++)
			{
				if (shineRenderers[num] != null)
				{
					Color color = originalColors[num];
					color.a = Mathf.Lerp(originalColors[num].a, targetAlpha, t);
					shineRenderers[num].color = color;
				}
			}
			yield return null;
		}
		for (int num2 = 0; num2 < shineRenderers.Count; num2++)
		{
			if (shineRenderers[num2] != null)
			{
				Color color2 = originalColors[num2];
				color2.a = targetAlpha;
				shineRenderers[num2].color = color2;
			}
		}
	}

	public void HandleCatchMode(Enemy enemyScript)
	{
		if (!(enemyScript == null) && !IsAnyOtherEnemyCatch(enemyScript) && (enemyScript.CurrentState is SexModeState || enemyScript is TVEnemy { isHScene: not false } || (enemyScript is MebeebEnemy && Player.GetInstance().CurrentState is PlayerSexModeState) || (enemyScript is WhiteWomanEnemy && WhiteWomanEnemy.IsCharmGame) || (enemyScript is SisSupEnemy && SisSupEnemy.IsTrapped)))
		{
			if (stopGameCoroutine != null)
			{
				StopCoroutine(stopGameCoroutine);
			}
			if (SpawnAfterDelayCoroutine != null)
			{
				StopCoroutine(SpawnAfterDelayCoroutine);
				SpawnAfterDelayCoroutine = null;
			}
			stopGameCoroutine = StartCoroutine(StopGameTime(enemyScript));
		}
	}

	public bool IsAnyOtherEnemyCatch(Enemy currentAspirant)
	{
		Enemy[] array = UnityEngine.Object.FindObjectsOfType<Enemy>();
		foreach (Enemy enemy in array)
		{
			if (!(enemy == null))
			{
				Enemy component = enemy.GetComponent<Enemy>();
				if (!(component == null) && !(component == currentAspirant) && !(component.GetType() == currentAspirant.GetType()) && (component.CurrentState is SexModeState || component is TVEnemy { isHScene: not false } || (component is MebeebEnemy && Player.GetInstance().CurrentState is PlayerSexModeState) || (component is WhiteWomanEnemy && WhiteWomanEnemy.IsCharmGame) || (component is SisSupEnemy && SisSupEnemy.IsTrapped)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator StopGameTime(Enemy luckyEnemy)
	{
		Player.GetInstance().BreakInteract();
		ShineItem[] allShineItems = UnityEngine.Object.FindObjectsOfType<ShineItem>();
		ShineItem[] array = allShineItems;
		foreach (ShineItem shineItem in array)
		{
			if (shineItem != null)
			{
				shineItem.StartUnshineEffect();
				shineItem.GetIOCollider().enabled = false;
			}
		}
		GameObject gameObject = GameObject.Find("BGM");
		AudioSource bgmSource = null;
		if (gameObject != null)
		{
			bgmSource = gameObject.GetComponent<AudioSource>();
			if (bgmSource != null)
			{
				bgmSource.mute = true;
			}
		}
		if (spawnedDildo != null)
		{
			UnityEngine.Object.Destroy(spawnedDildo);
			UnityEngine.Object.Destroy(spawnedDildoIco);
			spawnedDildo = null;
			spawnedDildoIco = null;
			isDildoQuest = false;
			if (luckyEnemy != null)
			{
				nintendo.HideAway();
			}
		}
		if (luckyEnemy != null)
		{
			Debug.Log(luckyEnemy.name + " is LuckyEnemy");
		}
		if (nintendo != null && !(nintendo.CurrentState is NintendoHaveFunState))
		{
			SpriteRenderer component = nintendo.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				Color color = component.color;
				color.a = 0f;
				component.color = color;
			}
			foreach (SpriteRenderer item in from r in nintendo.GetComponentsInChildren<SpriteRenderer>()
				where r.gameObject.name == "ShadowObject"
				select r)
			{
				Color color2 = item.color;
				color2.a = 0f;
				item.color = color2;
			}
		}
		if (!(luckyEnemy is SisSupEnemy))
		{
			UICanvas.SetActive(value: false);
		}
		Timer.IsEnabled = false;
		bool isLockUIBeenDisabled = false;
		if (Elevator.IsLockUIVisible())
		{
			Elevator.GetTriggeredInstance()?.UpdateLockUIVisibility(isLockUIBeenDisabled);
			isLockUIBeenDisabled = true;
		}
		if (luckyEnemy != null && !(luckyEnemy is MebeebEnemy))
		{
			ReducePriority(luckyEnemy.Settings);
		}
		GameObject[] array2 = enemies;
		foreach (GameObject gameObject2 in array2)
		{
			if (gameObject2 == null)
			{
				Debug.LogWarning("Enemy is Null");
			}
			else if (gameObject2 != null && gameObject2.GetComponent<Enemy>() == null)
			{
				Debug.LogWarning("Enemy Script Not Found");
			}
			else if (!(luckyEnemy != null) || !(gameObject2 == luckyEnemy.gameObject))
			{
				gameObject2.GetComponent<Enemy>().IsChaseMode = false;
			}
		}
		if (luckyEnemy != null)
		{
			ClearEnemies(luckyEnemy.gameObject);
		}
		else
		{
			FullClearEnemies();
		}
		bool isSpawn = false;
		try
		{
			if (luckyEnemy != null)
			{
				if (luckyEnemy is WhiteWomanEnemy ww)
				{
					while ((luckyEnemy != null && WhiteWomanEnemy.IsCharmGame) || ww == null)
					{
						Timer.IsEnabled = false;
						yield return null;
					}
				}
				else if (luckyEnemy is PosterEnemy && luckyEnemy.TargetChase != null)
				{
					while (luckyEnemy != null && luckyEnemy.TargetChase != null)
					{
						Timer.IsEnabled = false;
						yield return null;
					}
				}
				else if (luckyEnemy is SisSupEnemy && SisSupEnemy.IsTrapped)
				{
					while (luckyEnemy != null && SisSupEnemy.IsTrapped)
					{
						Timer.IsEnabled = false;
						yield return null;
					}
				}
				else
				{
					while (luckyEnemy != null && luckyEnemy.gameObject != null)
					{
						Timer.IsEnabled = false;
						yield return null;
					}
				}
			}
			else
			{
				while (nintendo.CurrentState is NintendoHaveFunState)
				{
					Timer.IsEnabled = false;
					yield return null;
				}
			}
			isSpawn = true;
		}
		finally
		{
			NightManager nightManager = this;
			nightManager.FullClearEnemies();
			Timer.IsEnabled = true;
			if (isLockUIBeenDisabled && Player.GetInstance().InteractionElevatorTimer > 0.5f)
			{
				Elevator.GetTriggeredInstance()?.UpdateLockUIVisibility(isLockUIBeenDisabled);
			}
			nightManager.UICanvas.SetActive(value: true);
			nightManager.stopGameCoroutine = null;
			if (nightManager.nintendo != null && !(nightManager.nintendo.CurrentState is NintendoHaveFunState))
			{
				SpriteRenderer component2 = nightManager.nintendo.GetComponent<SpriteRenderer>();
				if (component2 != null)
				{
					Color color3 = component2.color;
					color3.a = 1f;
					component2.color = color3;
				}
				foreach (SpriteRenderer item2 in from r in nightManager.nintendo.GetComponentsInChildren<SpriteRenderer>()
					where r.gameObject.name == "ShadowObject"
					select r)
				{
					Color shadowColor = item2.color;
					shadowColor.a = 1f;
					item2.color = shadowColor;
				}
			}
			ShineItem[] i = allShineItems;
			foreach (ShineItem component in i)
			{
				if (component != null)
				{
					component.StartShineEffect();
					component.GetIOCollider().enabled = true;
				}
			}
			Nintendo.ShowNintendo();
			if (bgmSource != null)
			{
				bgmSource.mute = false;
			}
			if (isSpawn)
			{
				nightManager.SpawnAfterDelayCoroutine = nightManager.StartCoroutine(nightManager.SpawnAfterDelay());
			}
		}
	}

	private IEnumerator SpawnAfterDelay()
	{
		try
		{
			yield return new WaitForSeconds(globalRespawnDelay);
		}
		finally
		{
			NightManager nightManager = this;
			if (!EndEvent.isGameOver && Player.GetInstance().CurrentHeartNumber > 0)
			{
				nightManager.SpawnEnemies();
			}
			nightManager.SpawnAfterDelayCoroutine = null;
		}
	}

	private void HandleMissingEnemyScript(GameObject enemy, string objectTag)
	{
		if (!respawnCoroutines.ContainsKey(enemy))
		{
			switch (objectTag)
			{
			default:
				_ = objectTag == "PosterGanger";
				break;
			case "TV":
				StartRespawnCoroutine(enemy, GetEnemySettingsFromPool<TVEnemySettings>());
				break;
			case "Poster":
				StartRespawnCoroutine(enemy, GetEnemySettingsFromPool<PosterEnemySettings>());
				break;
			case "PalateDoor":
				StartRespawnCoroutine(enemy, GetEnemySettingsFromPool<MebeebEnemySettings>());
				break;
			case "NurseCurtains":
				break;
			}
		}
	}

	private void StartRespawnCoroutine(GameObject enemy, BaseEnemySettings settings)
	{
		if (!(settings == null))
		{
			Coroutine value = StartCoroutine(RespawnEnemy(enemy, settings));
			respawnCoroutines[enemy] = value;
		}
	}

	private IEnumerator RespawnEnemy(GameObject enemy, BaseEnemySettings settings)
	{
		yield return new WaitForSeconds(respawnTime);
		if (timer.ElapsedTime / 60f > currentHour)
		{
			Debug.LogWarning("Респавн отменён из-за смены игрового часа.");
			yield break;
		}
		new Vector3(0f, 0f, 0f);
		if (!(settings is TVEnemySettings enemySettings))
		{
			if (!(settings is PosterEnemySettings enemySettings2))
			{
				if (!(settings is SisEnemySettings enemySettings3))
				{
					if (!(settings is WhisperEnemySettings enemySettings4))
					{
						if (!(settings is VampireEnemySettings enemySettings5))
						{
							if (!(settings is HoodieCatEnemySettings enemySettings6))
							{
								if (!(settings is NunEnemySettings enemySettings7))
								{
									if (settings is SisSupEnemySettings enemySettings8)
									{
										enemies[Array.IndexOf(enemies, enemy)] = SpawnSisSupEnemy(enemySettings8);
									}
									else
									{
										Debug.LogError("Тип настроек врага не распознан.");
									}
								}
								else
								{
									enemies[Array.IndexOf(enemies, enemy)] = SpawnNunEnemy(enemySettings7);
								}
							}
							else
							{
								Vector3 randomSpawnPositionWithColliderHeight = GetRandomSpawnPositionWithColliderHeight(enemySettings6);
								enemies[Array.IndexOf(enemies, enemy)] = SpawnHoodieCatEnemy(enemySettings6, randomSpawnPositionWithColliderHeight);
							}
						}
						else
						{
							Vector3 randomSpawnPositionWithColliderHeight = GetRandomSpawnPositionWithColliderHeight(enemySettings5);
							enemies[Array.IndexOf(enemies, enemy)] = SpawnVampireEnemy(enemySettings5, randomSpawnPositionWithColliderHeight);
						}
					}
					else
					{
						Vector3 randomSpawnPositionWithColliderHeight = GetRandomSpawnPositionWithColliderHeight(enemySettings4);
						enemies[Array.IndexOf(enemies, enemy)] = SpawnWhisperEnemy(enemySettings4, randomSpawnPositionWithColliderHeight);
					}
				}
				else
				{
					Vector3 randomSpawnPositionWithColliderHeight = GetRandomSpawnPositionWithColliderHeight(enemySettings3);
					enemies[Array.IndexOf(enemies, enemy)] = SpawnSisEnemy(enemySettings3, randomSpawnPositionWithColliderHeight);
				}
			}
			else
			{
				enemies[Array.IndexOf(enemies, enemy)] = SpawnPosterEnemy(enemySettings2);
			}
		}
		else
		{
			enemies[Array.IndexOf(enemies, enemy)] = SpawnTVEnemy(enemySettings);
		}
		if (respawnCoroutines.ContainsKey(enemy))
		{
			respawnCoroutines.Remove(enemy);
		}
	}

	private void CancelAllRespawns()
	{
		foreach (Coroutine value in respawnCoroutines.Values)
		{
			StopCoroutine(value);
		}
		respawnCoroutines.Clear();
	}

	private T GetEnemySettingsFromPool<T>() where T : BaseEnemySettings
	{
		foreach (BaseEnemySettings item in spawnPool)
		{
			if (item is T result)
			{
				return result;
			}
		}
		return null;
	}

	private void InitializePriorities()
	{
		foreach (BaseEnemySettings item in enemiesInNight)
		{
			enemyPriorities[item] = item.initialPriority;
		}
	}

	private void ResetInstances()
	{
		if (enemiesInNight.Count != 0)
		{
			ClearEnemies();
			SpawnEnemies();
		}
		SpawnWW();
		SpawnShineItems();
	}

	private void ClearEnemies(GameObject exceptionEnemy = null)
	{
		spawnPool = GetSpawnPool();
		GameObject[] array = enemies;
		foreach (GameObject gameObject in array)
		{
			if (gameObject == null || gameObject == exceptionEnemy || (exceptionEnemy != null && exceptionEnemy.GetComponent<SisSupEnemy>() != null && gameObject.GetComponent<NurseSpawner>() != null) || (exceptionEnemy != null && exceptionEnemy.GetComponent<PosterEnemy>() != null && gameObject.GetComponent<PosterEnemy>() != null))
			{
				continue;
			}
			BaseEnemySettings enemySettings = GetEnemySettings(gameObject);
			bool flag = spawnPool.Contains(enemySettings);
			Enemy component = gameObject.GetComponent<Enemy>();
			Debug.Log(gameObject.name);
			if (component != null)
			{
				Debug.Log(component.CanBeCleared());
			}
			if (component != null && !component.CanBeCleared() && exceptionEnemy == null)
			{
				if (!pendingClearEnemies.ContainsKey(gameObject))
				{
					Debug.Log($"Enemy {gameObject} go to Vanish");
					Coroutine value = StartCoroutine(WaitForEnemyClear(gameObject, flag));
					pendingClearEnemies[gameObject] = value;
				}
			}
			else if (flag && exceptionEnemy == null)
			{
				Debug.Log($"Enemy {gameObject} go to Respawn");
				RespawnEnemy(gameObject, enemySettings);
			}
			else
			{
				Debug.Log($"Enemy {gameObject} go to Destroy");
				StartCoroutine(DestroyEnemy(gameObject, vanishTime));
			}
		}
		enemies = new GameObject[maxEnemies];
	}

	public void OnForceRespawn(InputAction.CallbackContext context)
	{
	}

	private IEnumerator WaitForEnemyClear(GameObject enemy, bool isInNextPool)
	{
		if (enemy == null)
		{
			pendingClearEnemies.Remove(enemy);
			yield break;
		}
		Enemy enemyScript = enemy.GetComponent<Enemy>();
		if (enemyScript == null)
		{
			pendingClearEnemies.Remove(enemy);
			yield break;
		}
		while (enemy != null && enemyScript != null && ((!enemyScript.CanBeCleared() && stopGameCoroutine == null) || enemyScript.CurrentState is SexModeState || (enemyScript is PosterEnemy posterEnemy && posterEnemy.EnemyPhantom != null && !posterEnemy.CanBeCleared()) || (enemyScript is TVEnemy tVEnemy && !tVEnemy.CanBeCleared())))
		{
			if (stopGameCoroutine == null)
			{
				HandleCatchMode(enemyScript);
			}
			yield return null;
			if (enemy == null || enemyScript == null)
			{
				pendingClearEnemies.Remove(enemy);
				yield break;
			}
		}
		if (stopGameCoroutine == null)
		{
			HandleCatchMode(enemyScript);
		}
		if ((!((enemy != null) & isInNextPool) || stopGameCoroutine != null) && enemy != null)
		{
			StartCoroutine(DestroyEnemy(enemy, vanishTime));
		}
		pendingClearEnemies.Remove(enemy);
	}

	private void FullClearEnemies()
	{
		if (WWObject != null)
		{
			StartCoroutine(DestroyEnemy(WWObject, vanishTime));
		}
		GameObject[] array = enemies;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				ForceDestroyEnemy(gameObject);
			}
		}
		enemies = new GameObject[maxEnemies];
	}

	private BaseEnemySettings GetEnemySettings(GameObject enemy)
	{
		if (enemy.TryGetComponent<SisEnemy>(out var component))
		{
			return component.Settings;
		}
		if (enemy.TryGetComponent<HoodieCatEnemy>(out var component2))
		{
			return component2.Settings;
		}
		if (enemy.TryGetComponent<VampireEnemy>(out var component3))
		{
			return component3.Settings;
		}
		if (enemy.TryGetComponent<WhisperEnemy>(out var component4))
		{
			return component4.Settings;
		}
		if (enemy.TryGetComponent<TVEnemy>(out var component5))
		{
			return component5.Settings;
		}
		if (enemy.TryGetComponent<PosterEnemy>(out var _))
		{
			return GetEnemySettingsFromPool<PosterEnemySettings>();
		}
		if (enemy.TryGetComponent<WhiteWomanEnemy>(out var component7))
		{
			return component7.Settings;
		}
		if (enemy.TryGetComponent<SisSupEnemy>(out var component8))
		{
			return component8.Settings;
		}
		if (enemy.TryGetComponent<NurseSpawner>(out var _))
		{
			return GetEnemySettingsFromPool<SisSupEnemySettings>();
		}
		return null;
	}

	public void ForceRespawnEnemy(GameObject enemy)
	{
		if (enemy == null)
		{
			Debug.LogWarning("ForceRespawnEnemy получил null");
			return;
		}
		int num = Array.IndexOf(enemies, enemy);
		if (num == -1 && enemy != WWObject)
		{
			Debug.LogWarning("Враг не найден в массиве enemies");
			return;
		}
		BaseEnemySettings enemySettings = GetEnemySettings(enemy);
		if (enemySettings == null)
		{
			Debug.LogWarning("Не удалось получить настройки врага для респавна");
		}
		else
		{
			StartCoroutine(ForceRespawnCoroutine(enemy, enemySettings, num));
		}
	}

	private IEnumerator ForceRespawnCoroutine(GameObject enemy, BaseEnemySettings settings, int index)
	{
		yield return StartCoroutine(DestroyEnemy(enemy, vanishTime));
		GameObject wWObject;
		if (!(settings is TVEnemySettings enemySettings))
		{
			if (!(settings is PosterEnemySettings enemySettings2))
			{
				if (settings is WhiteWomanEnemySettings enemySettings3)
				{
					wWObject = SpawnWhiteWomanEnemy(enemySettings3);
					WWObject = wWObject;
					yield break;
				}
				if (!(settings is SisSupEnemySettings enemySettings4))
				{
					Debug.LogError("Неизвестный тип настроек для ForceRespawn");
					yield break;
				}
				wWObject = SpawnSisSupEnemy(enemySettings4);
			}
			else
			{
				wWObject = SpawnPosterEnemy(enemySettings2);
			}
		}
		else
		{
			wWObject = SpawnTVEnemy(enemySettings);
		}
		if (wWObject != null)
		{
			enemies[index] = wWObject;
		}
		else
		{
			Debug.LogError("Респавн вернул null");
		}
	}

	private void SpawnEnemies()
	{
		foreach (BaseEnemySettings item in spawnPool)
		{
			if (!(item == null) && (!(item is MebeebEnemySettings) || (timer.ElapsedTime != 0f && timer.ElapsedTime / 60f > currentHour)))
			{
				GameObject gameObject = CreateEnemyInstance(item);
				if (gameObject != null)
				{
					AddEnemyToScene(gameObject);
					ReducePriority(item);
				}
			}
		}
	}

	private void SpawnWW()
	{
		if (isWW && UnityEngine.Random.Range(0f, 1f) <= 0.8f)
		{
			if (WWObject != null)
			{
				UnityEngine.Object.Destroy(WWObject.GetComponent<WhiteWomanEnemy>());
				WWObject = null;
			}
			Debug.Log("WW spawned");
			WWObject = SpawnWhiteWomanEnemy(WWSettings);
		}
	}

	private List<BaseEnemySettings> GetSpawnPool()
	{
		if (enemiesInNight.Count == 0)
		{
			return new List<BaseEnemySettings>();
		}
		List<BaseEnemySettings> list = new List<BaseEnemySettings>();
		List<BaseEnemySettings> list2 = new List<BaseEnemySettings>();
		foreach (BaseEnemySettings key in enemyPriorities.Keys)
		{
			if (!(key is NintendoEntitySettings))
			{
				if (key is SisEnemySettings || key is HoodieCatEnemySettings || key is VampireEnemySettings || key is WhisperEnemySettings)
				{
					list.Add(key);
				}
				else
				{
					list2.Add(key);
				}
			}
		}
		List<BaseEnemySettings> list3 = new List<BaseEnemySettings>();
		if (list.Count > 0)
		{
			list3 = (from a in list
				orderby enemyPriorities[a] descending, Guid.NewGuid()
				select a).Take(2).ToList();
		}
		else
		{
			Debug.LogWarning("Атакующих врагов нет!");
		}
		List<BaseEnemySettings> collection = new List<BaseEnemySettings>();
		if (list2.Count > 0)
		{
			collection = (from s in list2
				orderby enemyPriorities[s] descending, Guid.NewGuid()
				select s).ToList();
			int count = Mathf.Max(0, maxEnemies - list3.Count);
			collection = collection.Take(count).ToList();
		}
		List<BaseEnemySettings> list4 = new List<BaseEnemySettings>();
		list4.AddRange(list3);
		list4.AddRange(collection);
		return (from e in list4
			orderby enemyPriorities[e] descending, Guid.NewGuid()
			select e).ToList();
	}

	private void AddEnemyToScene(GameObject enemy)
	{
		for (int i = 0; i < enemies.Length; i++)
		{
			if (enemies[i] == null)
			{
				enemies[i] = enemy;
				return;
			}
		}
		string text = string.Join(", ", enemies.Select((GameObject e) => (!(e != null)) ? "null" : e.name));
		Debug.Log("Failed Add Enemy To Array: " + enemy.name + ". Current enemies: " + text);
	}

	private void ReducePriority(BaseEnemySettings settings)
	{
		Debug.Log($"Enemy Priority Reduced For {settings}");
		if (settings == null)
		{
			Debug.LogError("Enemy Settings is Null");
		}
		else if (enemyPriorities.ContainsKey(settings))
		{
			if (settings is NunEnemySettings || settings is SisSupEnemySettings)
			{
				enemyPriorities[settings] -= 2f;
			}
			else
			{
				enemyPriorities[settings]--;
			}
			Debug.Log(enemyPriorities[settings]);
		}
	}

	private GameObject CreateEnemyInstance(BaseEnemySettings enemySettings)
	{
		GameObject gameObject = null;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		if (enemySettings is SisEnemySettings || enemySettings is VampireEnemySettings || enemySettings is HoodieCatEnemySettings || enemySettings is WhisperEnemySettings)
		{
			vector = GetRandomSpawnPositionWithColliderHeight(enemySettings);
		}
		Debug.Log("Enemy type: " + enemySettings.enemyType);
		switch (enemySettings.enemyType)
		{
		case EnemyType.SisEnemy:
			gameObject = SpawnSisEnemy(enemySettings as SisEnemySettings, vector);
			break;
		case EnemyType.WhisperEnemy:
			gameObject = SpawnWhisperEnemy(enemySettings as WhisperEnemySettings, vector);
			break;
		case EnemyType.VampireEnemy:
			gameObject = SpawnVampireEnemy(enemySettings as VampireEnemySettings, vector);
			break;
		case EnemyType.HoodieCatEnemy:
			gameObject = SpawnHoodieCatEnemy(enemySettings as HoodieCatEnemySettings, vector);
			break;
		case EnemyType.NunEnemy:
			gameObject = SpawnNunEnemy(enemySettings as NunEnemySettings);
			break;
		case EnemyType.SisSupEnemy:
			gameObject = SpawnSisSupEnemy(enemySettings as SisSupEnemySettings);
			break;
		case EnemyType.PosterEnemy:
			gameObject = SpawnPosterEnemy(enemySettings as PosterEnemySettings);
			break;
		case EnemyType.TVEnemy:
			gameObject = SpawnTVEnemy(enemySettings as TVEnemySettings);
			break;
		default:
			gameObject = CreateEnemy(enemySettings);
			gameObject.transform.position = vector;
			break;
		case EnemyType.MebeebEnemy:
			break;
		}
		return gameObject;
	}

	public GameObject[] GetActiveEnemies()
	{
		if (enemies == null)
		{
			Debug.LogWarning("Enemies array is null. Returning an empty array.");
			return new GameObject[0];
		}
		return enemies.Where((GameObject e) => e != null).ToArray();
	}

	public int GetVanishedEnemyCount()
	{
		int num = maxEnemies + respawnCoroutines.Count;
		int num2 = GetActiveEnemies().Length;
		return num - num2;
	}

	private GameObject SpawnNunEnemy(NunEnemySettings enemySettings)
	{
		NunEnemy nunEnemy = UnityEngine.Object.FindObjectOfType<NunEnemy>();
		if (nunEnemy != null)
		{
			return nunEnemy.gameObject;
		}
		List<BoxCollider> list = new List<BoxCollider>();
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		Difficulty difficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2);
		bool flag = difficulty == Difficulty.Hard;
		BoxCollider[] array = spawnAreaColliders;
		foreach (BoxCollider boxCollider in array)
		{
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(boxCollider.gameObject);
			if (roomContainingObject == null)
			{
				continue;
			}
			string text = NormalizeStageName(Player.GetInstance().CurrentStageName);
			string text2 = NormalizeStageName(HospitalManager.GetInstance().FindNearestRoomStartingWith(roomContainingObject, "Stage"));
			if (roomContainingObject.name.StartsWith("Stage") || roomContainingObject.name.StartsWith("Palate") || roomContainingObject.name.StartsWith("Bridge") || !(currentRoomObject.name != roomContainingObject.name) || (!flag && !(text == text2)))
			{
				continue;
			}
			DoorTransit doorTransit = null;
			BoxCollider component = roomContainingObject.GetComponent<BoxCollider>();
			DoorTransit[] array2 = UnityEngine.Object.FindObjectsOfType<DoorTransit>();
			foreach (DoorTransit doorTransit2 in array2)
			{
				BoxCollider component2 = doorTransit2.GetComponent<BoxCollider>();
				if (component2 != null && component.bounds.Intersects(component2.bounds))
				{
					doorTransit = doorTransit2;
					break;
				}
			}
			bool flag2 = false;
			if (doorTransit != null && doorTransit.GotoObject != null)
			{
				GameObject gotoObject = doorTransit.GotoObject;
				BoxCollider component3 = gotoObject.GetComponent<BoxCollider>();
				float num = gotoObject.transform.position.x;
				if (component3 != null)
				{
					num += component3.center.x * gotoObject.transform.lossyScale.x;
				}
				if (Mathf.Abs(Player.GetInstance().transform.position.x - num) > 1000f)
				{
					flag2 = true;
				}
			}
			bool flag3 = !currentRoomObject.name.StartsWith("Stage");
			if (flag2 | flag3)
			{
				list.Add(boxCollider);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("Не найдено подходящих зон для спавна");
			return null;
		}
		BoxCollider collider = list[UnityEngine.Random.Range(0, list.Count)];
		GameObject gameObject = new GameObject("NunEnemy");
		NunEnemy nunEnemy2 = gameObject.AddComponent<NunEnemy>();
		ApplySettings(gameObject, enemySettings);
		Vector3 randomPointInCollider = GetRandomPointInCollider(collider);
		gameObject.transform.position = randomPointInCollider;
		if (enemySettings.captureBar == null)
		{
			nunEnemy2.CaptureBar = FindInactiveObjects.FindObjectWithTagIncludingInactive("CaptureBar").GetComponent<ProgressBar>();
		}
		nunEnemy2.prefab = enemySettings.prefab;
		float num2 = enemySettings.captureTime;
		float num3 = enemySettings.initialSlowdownPercent;
		float num4 = enemySettings.additionalSlowdownPercent;
		switch (difficulty)
		{
		case Difficulty.Practice:
		case Difficulty.Easy:
			num2 += 10f;
			num3 *= 0f;
			num4 *= 0f;
			break;
		case Difficulty.Hard:
			num2 -= 10f;
			num3 *= 1.5f;
			num4 *= 1.5f;
			break;
		}
		nunEnemy2.CaptureTime = num2;
		nunEnemy2.InitialSlowdownPercent = num3;
		nunEnemy2.AdditionalSlowdownPercent = num4;
		nunEnemy2.ColliderSize = enemySettings.colliderSize;
		nunEnemy2.nunCatchAnimatorController = enemySettings.nunCatchAnimatorController;
		return gameObject;
	}

	public string NormalizeStageName(string stageName)
	{
		Match match = Regex.Match(stageName, "^Stage\\d+");
		if (!match.Success)
		{
			return stageName;
		}
		return match.Value;
	}

	private Vector3 GetRandomPointInCollider(BoxCollider collider)
	{
		Vector3 min = collider.bounds.min;
		Vector3 max = collider.bounds.max;
		return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
	}

	private GameObject SpawnSisSupEnemy(SisSupEnemySettings enemySettings)
	{
		SisSupEnemy sisSupEnemy = UnityEngine.Object.FindObjectOfType<SisSupEnemy>();
		if (sisSupEnemy != null)
		{
			return sisSupEnemy.gameObject;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("NurseCurtains");
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		string currentStageName = Player.GetInstance().CurrentStageName;
		_ = Player.GetInstance().transform.position;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(gameObject);
			if (roomContainingObject == null)
			{
				continue;
			}
			string stageName = HospitalManager.GetInstance().FindNearestRoomStartingWith(roomContainingObject, "Stage");
			if (!IsSpawnValidForSupports(roomContainingObject))
			{
				continue;
			}
			if (NormalizeStageName(stageName) == NormalizeStageName(currentStageName))
			{
				if (roomContainingObject.name != currentRoomObject.name)
				{
					list.Add(gameObject);
				}
			}
			else
			{
				list2.Add(gameObject);
			}
		}
		List<GameObject> list3 = ((list.Count > 0) ? list : list2);
		if (list3.Count == 0)
		{
			Debug.LogError("Не найдено подходящих зон для спавна");
			return null;
		}
		GameObject gameObject2 = list3[UnityEngine.Random.Range(0, list3.Count)];
		NurseSpawner nurseSpawner = gameObject2.AddComponent<NurseSpawner>();
		ApplySettings(gameObject2, enemySettings);
		nurseSpawner.newSprite = enemySettings.newSprite;
		nurseSpawner.bedNewSprite = enemySettings.bedNewSprite;
		nurseSpawner.prefabToSpawn = enemySettings.prefabToSpawn;
		nurseSpawner.Settings = enemySettings;
		if (enemySettings.hornyBar == null)
		{
			nurseSpawner.hornyBar = FindInactiveObjects.FindObjectWithTagIncludingInactive("HornyBar").GetComponent<ProgressBar>();
		}
		else
		{
			nurseSpawner.hornyBar = enemySettings.hornyBar.GetComponent<ProgressBar>();
		}
		nurseSpawner.additionalSpritePrefab = enemySettings.additionalSpritePrefab;
		nurseSpawner.spawnOffset = enemySettings.spawnOffset;
		nurseSpawner.Initialize();
		float num = enemySettings.hornyTime;
		switch ((Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2))
		{
		case Difficulty.Practice:
		case Difficulty.Easy:
			num += 15f;
			break;
		case Difficulty.Normal:
			num += 5f;
			break;
		}
		nurseSpawner.SetPrefabFields(enemySettings.currentMoveSpeed, enemySettings.currentInteractionSpeed, enemySettings.interactionCooldown, enemySettings.resistanceTime, enemySettings.viewDistance, enemySettings.shinePrefab, enemySettings.keyObjectPrefab, num);
		return gameObject2;
	}

	public void SpawnShineItems()
	{
		hasSpawnedSkipWatch = false;
		skipWatchProtected = false;
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		ShineItem[] array = shineItems;
		foreach (ShineItem shineItem in array)
		{
			if (shineItem == null)
			{
				continue;
			}
			Vector3 position = Player.GetInstance().transform.position;
			Vector3 position2 = shineItem.transform.position;
			float num = Mathf.Abs(position.x - position2.x);
			float num2 = Mathf.Abs(position.y - position2.y);
			if (num <= 200f && num2 <= 200f)
			{
				hashSet.Add(position2);
				if (shineItem.GetStoredPrefab() != null && (shineItem.GetStoredPrefab().name == "skipWatch" || shineItem.GetStoredPrefab().GetComponent<TimeBoostItem>() != null))
				{
					skipWatchProtected = true;
				}
			}
			else
			{
				UnityEngine.Object.Destroy(shineItem.gameObject);
			}
		}
		spawnedShineItems.RemoveAll((GameObject x) => x == null);
		int num3 = (useFixedMaxItems ? maxNormalItems : normalSpawnPoints.Length);
		int num4 = (useFixedMaxItems ? maxSpecialItems : specialSpawnPoints.Length);
		ShuffleArrayUnity(normalSpawnPoints);
		ShuffleArrayUnity(specialSpawnPoints);
		int count = Mathf.RoundToInt((float)num3 * chanceToItemSpawn / 100f);
		int count2 = Mathf.RoundToInt((float)num4 * chanceToItemSpawn / 100f);
		List<ItemSpawnSettings> items = itemsInNight.Where((ItemSpawnSettings itemSpawnSettings) => itemSpawnSettings.weight > 0f && itemSpawnSettings.spawnCategory == ItemSpawnSettings.SpawnCategory.Normal).ToList();
		List<ItemSpawnSettings> source = itemsInNight.Where((ItemSpawnSettings itemSpawnSettings) => itemSpawnSettings.weight > 0f && itemSpawnSettings.spawnCategory == ItemSpawnSettings.SpawnCategory.Special).ToList();
		source = source.Where((ItemSpawnSettings item) => !IsSkipWatch(item) || (!hasPickedUpSkipWatch && !hasSpawnedSkipWatch && !skipWatchProtected)).ToList();
		List<GameObject> list = BuildDeck(items, count);
		List<GameObject> list2 = BuildDeck(source, count2);
		int num5 = Mathf.Max(0, num3 - list.Count);
		int num6 = Mathf.Max(0, num4 - list2.Count);
		for (int num7 = 0; num7 < num5; num7++)
		{
			list.Add(null);
		}
		for (int num8 = 0; num8 < num6; num8++)
		{
			list2.Add(null);
		}
		list = list.OrderBy((GameObject x) => UnityEngine.Random.value).ToList();
		list2 = list2.OrderBy((GameObject x) => UnityEngine.Random.value).ToList();
		shineItems = new ShineItem[num3 + num4];
		int num9 = 0;
		for (int num10 = 0; num10 < normalSpawnPoints.Length && num10 < list.Count; num10++)
		{
			Vector3 position3 = normalSpawnPoints[num10].transform.position;
			if (!hashSet.Contains(position3))
			{
				SpawnShineAt(position3, list[num10], num9++);
			}
		}
		for (int num11 = 0; num11 < specialSpawnPoints.Length && num11 < list2.Count; num11++)
		{
			Vector3 position4 = specialSpawnPoints[num11].transform.position;
			if (hashSet.Contains(position4))
			{
				continue;
			}
			GameObject prefab = list2[num11];
			if (!IsSkipWatchPrefab(prefab) || !hasSpawnedSkipWatch)
			{
				SpawnShineAt(position4, prefab, num9++);
				if (IsSkipWatchPrefab(prefab))
				{
					hasSpawnedSkipWatch = true;
				}
			}
		}
	}

	private void ShuffleArrayUnity<T>(T[] array)
	{
		int num = array.Length;
		while (num > 1)
		{
			num--;
			int num2 = UnityEngine.Random.Range(0, num + 1);
			T val = array[num2];
			array[num2] = array[num];
			array[num] = val;
		}
	}

	private List<GameObject> BuildDeck(List<ItemSpawnSettings> items, int count)
	{
		List<GameObject> list = new List<GameObject>();
		float num = items.Sum((ItemSpawnSettings i) => i.weight);
		bool flag = false;
		foreach (ItemSpawnSettings item in items)
		{
			int num2 = Mathf.RoundToInt(item.weight / num * (float)count);
			if (IsSkipWatch(item))
			{
				if (!flag && num2 > 0)
				{
					list.Add(item.itemPrefab);
					flag = true;
				}
			}
			else
			{
				for (int num3 = 0; num3 < num2; num3++)
				{
					list.Add(item.itemPrefab);
				}
			}
		}
		while (list.Count < count && items.Count > 0)
		{
			list.Add(items[0].itemPrefab);
		}
		return list;
	}

	private bool IsSkipWatch(ItemSpawnSettings item)
	{
		if (item != null && item.itemPrefab != null)
		{
			if (!(item.itemPrefab.name == "skipWatch"))
			{
				return item.itemPrefab.GetComponent<TimeBoostItem>() != null;
			}
			return true;
		}
		return false;
	}

	private bool IsSkipWatchPrefab(GameObject prefab)
	{
		if (prefab != null)
		{
			if (!(prefab.name == "skipWatch"))
			{
				return prefab.GetComponent<TimeBoostItem>() != null;
			}
			return true;
		}
		return false;
	}

	private void SpawnShineAt(Vector3 pos, GameObject prefab, int index)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(shinePrefab, pos, Quaternion.identity);
		ShineItem component = gameObject.GetComponent<ShineItem>();
		if (prefab != null && component != null)
		{
			component.SetShineItem(prefab);
		}
		shineItems[index] = component;
		spawnedShineItems.Add(gameObject);
	}

	public void ResetSkipWatchSpawn()
	{
	}

	public void ToggleOfSkipWatchSpawn()
	{
	}

	private void NormalizeItemChances()
	{
		Difficulty difficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2);
		float num = 0f;
		ItemSpawnSettings[] array = itemsInNight;
		foreach (ItemSpawnSettings itemSpawnSettings in array)
		{
			float num2 = itemSpawnSettings.weight;
			if ((difficulty == Difficulty.Practice || difficulty == Difficulty.Easy) && itemSpawnSettings.itemPrefab != null && itemSpawnSettings.itemPrefab.name == "redPill")
			{
				num2 = Mathf.Min(itemSpawnSettings.weight + 20f, 100f);
			}
			num += num2;
		}
		if (num <= 0f)
		{
			Debug.LogWarning("Общий вес равен 0. Все вероятности установлены в 0.");
			array = itemsInNight;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].normalizedChance = 0f;
			}
			return;
		}
		array = itemsInNight;
		foreach (ItemSpawnSettings itemSpawnSettings2 in array)
		{
			float num3 = itemSpawnSettings2.weight;
			if ((difficulty == Difficulty.Practice || difficulty == Difficulty.Easy) && itemSpawnSettings2.itemPrefab != null && itemSpawnSettings2.itemPrefab.name == "redPill")
			{
				num3 = Mathf.Min(itemSpawnSettings2.weight + 20f, 100f);
			}
			itemSpawnSettings2.normalizedChance = num3 / num;
		}
	}

	private GameObject CreateEnemy(BaseEnemySettings settings, bool isTemporary = false)
	{
		GameObject gameObject = new GameObject(settings.enemyType.ToString());
		if (!isTemporary)
		{
			gameObject.AddComponent<Enemy>();
			ApplySettings(gameObject, settings);
		}
		return gameObject;
	}

	private GameObject SpawnSisEnemy(SisEnemySettings enemySettings, Vector3 spawnPosition)
	{
		SisEnemy sisEnemy = UnityEngine.Object.FindObjectOfType<SisEnemy>();
		if (sisEnemy != null)
		{
			return sisEnemy.gameObject;
		}
		GameObject gameObject = new GameObject("SisEnemy");
		SisEnemy sisEnemy2 = gameObject.AddComponent<SisEnemy>();
		ApplySettings(gameObject, enemySettings);
		gameObject.transform.position = spawnPosition;
		sisEnemy2.ColliderSize = enemySettings.colliderSize;
		sisEnemy2.IsAngry = enemySettings.isAngry;
		GameObject gameObject2 = Resources.Load<GameObject>("prefabs/Enemies/sislight");
		if (gameObject2 != null)
		{
			UnityEngine.Object.Instantiate(gameObject2, gameObject.transform).transform.localPosition = new Vector3(62.2f, 16.6f, 0f);
		}
		return gameObject;
	}

	private GameObject SpawnWhisperEnemy(WhisperEnemySettings enemySettings, Vector3 spawnPosition)
	{
		WhisperEnemy whisperEnemy = UnityEngine.Object.FindObjectOfType<WhisperEnemy>();
		if (whisperEnemy != null)
		{
			return whisperEnemy.gameObject;
		}
		GameObject gameObject = new GameObject("WhisperEnemy");
		WhisperEnemy whisperEnemy2 = gameObject.AddComponent<WhisperEnemy>();
		ApplySettings(gameObject, enemySettings);
		gameObject.transform.position = spawnPosition;
		whisperEnemy2.ColliderSize = enemySettings.colliderSize;
		whisperEnemy2.IsAngry = enemySettings.isAngry;
		return gameObject;
	}

	private GameObject SpawnVampireEnemy(VampireEnemySettings enemySettings, Vector3 spawnPosition)
	{
		VampireEnemy vampireEnemy = UnityEngine.Object.FindObjectOfType<VampireEnemy>();
		if (vampireEnemy != null)
		{
			return vampireEnemy.gameObject;
		}
		GameObject gameObject = new GameObject("VampireEnemy");
		VampireEnemy vampireEnemy2 = gameObject.AddComponent<VampireEnemy>();
		ApplySettings(gameObject, enemySettings);
		vampireEnemy2.ColliderSize = enemySettings.colliderSize;
		vampireEnemy2.BatsMoveSpeed = enemySettings.BatsMoveSpeed;
		gameObject.transform.position = spawnPosition;
		return gameObject;
	}

	private GameObject SpawnHoodieCatEnemy(HoodieCatEnemySettings enemySettings, Vector3 spawnPosition)
	{
		HoodieCatEnemy hoodieCatEnemy = UnityEngine.Object.FindObjectOfType<HoodieCatEnemy>();
		if (hoodieCatEnemy != null)
		{
			return hoodieCatEnemy.gameObject;
		}
		GameObject gameObject = new GameObject("HoodieCatEnemy");
		HoodieCatEnemy hoodieCatEnemy2 = gameObject.AddComponent<HoodieCatEnemy>();
		ApplySettings(gameObject, enemySettings);
		gameObject.transform.position = spawnPosition;
		hoodieCatEnemy2.StepPrefab = enemySettings.stepPrefab;
		hoodieCatEnemy2.ColliderSize = enemySettings.colliderSize;
		hoodieCatEnemy2.IsAngry = enemySettings.isAngry;
		return gameObject;
	}

	private GameObject SpawnMebeebEnemy(MebeebEnemySettings enemySettings)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("PalateDoor");
		List<GameObject> list = new List<GameObject>();
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			float num = Mathf.Abs(gameObject.transform.position.x - Player.GetInstance().transform.position.x);
			DoorTransit component = gameObject.GetComponent<DoorTransit>();
			string text = component.DoorRoom.name;
			string text2 = component.GotoObject.GetComponent<DoorTransit>().DoorRoom.name;
			if (currentRoomObject.name != text2 && (currentRoomObject.name != text || num > 900f) && IsSpawnValidForSupports(component.GotoObject) && gameObject.GetComponent<MebeebEnemy>() == null)
			{
				list.Add(gameObject);
			}
		}
		if (list.Count <= 1)
		{
			Debug.LogWarning("Не найдено подходящих зон для спавна или заняты все доступные палаты");
			return null;
		}
		GameObject obj = list[UnityEngine.Random.Range(0, list.Count)];
		MebeebEnemy mebeebEnemy = obj.AddComponent<MebeebEnemy>();
		ApplySettings(mebeebEnemy.gameObject, enemySettings);
		Difficulty difficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2);
		mebeebEnemy.BearPool = enemySettings.GetBearPoolForDifficulty(difficulty);
		mebeebEnemy.spawnOffset = enemySettings.spawnOffset;
		return obj;
	}

	private GameObject SpawnPosterEnemy(PosterEnemySettings enemySettings)
	{
		PosterEnemy[] array = UnityEngine.Object.FindObjectsOfType<PosterEnemy>();
		foreach (PosterEnemy posterEnemy in array)
		{
			if (posterEnemy.CompareTag("Poster"))
			{
				return posterEnemy.gameObject;
			}
		}
		GameObject[] array2 = GameObject.FindGameObjectsWithTag("Poster");
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		string currentStageName = Player.GetInstance().CurrentStageName;
		Vector3 position = Player.GetInstance().transform.position;
		Difficulty difficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2);
		GameObject[] array3 = array2;
		foreach (GameObject gameObject in array3)
		{
			if (difficulty == Difficulty.Practice || difficulty == Difficulty.Easy)
			{
				SpriteRenderer componentInChildren = gameObject.GetComponentInChildren<SpriteRenderer>();
				if (componentInChildren != null)
				{
					string text = ((componentInChildren.sprite != null) ? componentInChildren.sprite.name : string.Empty);
					if (text == "Picture1" || text == "Poster2")
					{
						continue;
					}
				}
			}
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(gameObject);
			if (roomContainingObject == null)
			{
				continue;
			}
			string stageName = HospitalManager.GetInstance().FindNearestRoomStartingWith(roomContainingObject, "Stage");
			float num = Mathf.Abs(gameObject.transform.position.x - position.x);
			if (NormalizeStageName(stageName) == NormalizeStageName(currentStageName))
			{
				if (roomContainingObject.name != currentRoomObject.name || num > 900f)
				{
					list.Add(gameObject);
				}
			}
			else
			{
				list2.Add(gameObject);
			}
		}
		List<GameObject> list3 = ((list.Count > 0) ? list : list2);
		if (list3.Count == 0)
		{
			Debug.LogError("Не найдено подходящих зон для спавна");
			return null;
		}
		GameObject obj = list3[UnityEngine.Random.Range(0, list3.Count)];
		PosterEnemy posterEnemy2 = obj.AddComponent<PosterEnemy>();
		ApplySettings(posterEnemy2.gameObject, enemySettings);
		posterEnemy2.SetDisappearanceDelayForSpriteParent(enemySettings.disappearanceDelay);
		posterEnemy2.RespawnTime = enemySettings.respawnTime;
		posterEnemy2.UseRoomBasedSpawn = enemySettings.useRoomBasedSpawn;
		posterEnemy2.shouldUseBaseUpdate = enemySettings.shouldUseBaseUpdate;
		posterEnemy2.SetSpeedForSpriteParent(enemySettings.currentMoveSpeed, enemySettings.currentInteractionSpeed);
		Debug.Log("Create Poster Instance");
		return obj;
	}

	private GameObject SpawnTVEnemy(TVEnemySettings enemySettings)
	{
		TVEnemy tVEnemy = UnityEngine.Object.FindObjectOfType<TVEnemy>();
		if (tVEnemy != null)
		{
			return tVEnemy.gameObject;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("TV");
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		string currentStageName = Player.GetInstance().CurrentStageName;
		Vector3 position = Player.GetInstance().transform.position;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(gameObject);
			if (roomContainingObject == null)
			{
				continue;
			}
			string stageName = HospitalManager.GetInstance().FindNearestRoomStartingWith(roomContainingObject, "Stage");
			float num = Mathf.Abs(gameObject.transform.position.x - position.x);
			if (NormalizeStageName(stageName) == NormalizeStageName(currentStageName))
			{
				if (roomContainingObject.name != currentRoomObject.name || num > 900f)
				{
					list.Add(gameObject);
				}
			}
			else
			{
				list2.Add(gameObject);
			}
		}
		List<GameObject> list3 = ((list.Count > 0) ? list : list2);
		if (list3.Count == 0)
		{
			Debug.LogError("Не найдено подходящих зон для спавна");
			return null;
		}
		GameObject gameObject2 = list3[UnityEngine.Random.Range(0, list3.Count)];
		TVEnemy tVEnemy2 = gameObject2.AddComponent<TVEnemy>();
		ApplySettings(gameObject2.gameObject, enemySettings);
		tVEnemy2.SpawnPrefab = enemySettings.spawnPrefab;
		tVEnemy2.newAnimatorController = enemySettings.newAnimatorController;
		return gameObject2;
	}

	private GameObject SpawnWhiteWomanEnemy(WhiteWomanEnemySettings enemySettings)
	{
		WhiteWomanEnemy whiteWomanEnemy = UnityEngine.Object.FindObjectOfType<WhiteWomanEnemy>();
		if (whiteWomanEnemy != null)
		{
			return whiteWomanEnemy.gameObject;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("Elevator");
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		string currentStageName = Player.GetInstance().CurrentStageName;
		Vector3 position = Player.GetInstance().transform.position;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(gameObject);
			if (roomContainingObject == null)
			{
				continue;
			}
			string stageName = HospitalManager.GetInstance().FindNearestRoomStartingWith(roomContainingObject, "Stage");
			float num = Mathf.Abs(gameObject.transform.position.x - position.x);
			if (!gameObject.name.StartsWith("Floor"))
			{
				continue;
			}
			if (NormalizeStageName(stageName) == NormalizeStageName(currentStageName))
			{
				if (roomContainingObject.name != currentRoomObject.name || num > 900f)
				{
					list.Add(gameObject);
				}
			}
			else
			{
				list2.Add(gameObject);
			}
		}
		List<GameObject> list3 = ((list.Count > 0) ? list : list2);
		if (list3.Count == 0)
		{
			Debug.LogWarning("Не найдено объектов с именем, начинающимся на 'Floor'.");
			return null;
		}
		GameObject gameObject2 = list3[UnityEngine.Random.Range(0, list3.Count)];
		WhiteWomanEnemy whiteWomanEnemy2 = gameObject2.AddComponent<WhiteWomanEnemy>();
		ApplySettings(gameObject2, enemySettings);
		whiteWomanEnemy2.CharmGameTimer = enemySettings.charmGameTimer;
		whiteWomanEnemy2.EnemyPrefabWithAnimator = enemySettings.enemyPrefabWithAnimator;
		if (enemySettings.largeBar == null)
		{
			Transform transform = FindInactiveObjects.FindObjectWithTagIncludingInactive("CharmGame").transform;
			whiteWomanEnemy2.charmGamePanel = transform.GetComponent<RectTransform>();
			whiteWomanEnemy2.largeBar = FindInactiveObjects.FindObjectWithTagIncludingInactive("MainBar").GetComponent<RectTransform>();
			whiteWomanEnemy2.smallBar = FindInactiveObjects.FindObjectWithTagIncludingInactive("SmallBar").GetComponent<RectTransform>();
			whiteWomanEnemy2.circle = FindInactiveObjects.FindObjectWithTagIncludingInactive("SlideBall").GetComponent<RectTransform>();
			whiteWomanEnemy2.penaltyBarProgress = FindInactiveObjects.FindObjectWithTagIncludingInactive("HornyBar").GetComponent<ProgressBar>();
		}
		return gameObject2;
	}

	private void ApplySettings(GameObject enemy, BaseEnemySettings settings)
	{
		Enemy component = enemy.GetComponent<Enemy>();
		if (component != null)
		{
			PlayerPrefs.GetInt("GameDifficulty", 2);
			float moveSpeed = settings.currentMoveSpeed;
			float interactionSpeed = settings.currentInteractionSpeed;
			float resistanceTime = settings.resistanceTime;
			float viewDistance = settings.viewDistance;
			float hearDistance = settings.hearDistance;
			float chaseTime = settings.chaseTime;
			ApplyDifficultyModifiers(ref moveSpeed, ref interactionSpeed, ref resistanceTime, ref viewDistance, ref chaseTime);
			component.Settings = settings;
			component.SetBaseSpeed(moveSpeed, interactionSpeed);
			component.InteractionCooldown = settings.interactionCooldown;
			component.resistanceTime = resistanceTime;
			component.ViewDistance = viewDistance;
			component.HearDistance = hearDistance;
			component.ChaseTime = chaseTime;
			component.HunterInstinctCooldown = settings.hunterInstinctCooldown;
			component.HunterInstinctTime = settings.hunterInstinctTime;
			component.SetAnimator(settings.animator);
			component.SetAnimatorController(settings.animatorController);
		}
	}

	private void ApplyDifficultyModifiers(ref float moveSpeed, ref float interactionSpeed, ref float resistanceTime, ref float viewDistance, ref float chaseTime)
	{
		switch ((Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2))
		{
		case Difficulty.Practice:
			moveSpeed *= 0.7f;
			interactionSpeed *= 0.7f;
			resistanceTime *= 0.5f;
			viewDistance *= 0.8f;
			chaseTime *= 0.7f;
			break;
		case Difficulty.Easy:
			moveSpeed *= 0.9f;
			interactionSpeed *= 0.9f;
			resistanceTime *= 0.7f;
			viewDistance *= 0.9f;
			chaseTime *= 0.85f;
			break;
		case Difficulty.Hard:
			moveSpeed *= 1.1f;
			interactionSpeed *= 1.1f;
			resistanceTime *= 1.2f;
			viewDistance *= 1.1f;
			chaseTime *= 1.2f;
			break;
		case Difficulty.Normal:
			break;
		}
	}

	private IEnumerator DestroyEnemy(GameObject enemyObject, float vanishDelay)
	{
		if (enemyObject == null)
		{
			yield break;
		}
		Debug.Log(enemyObject.name + " Try To Destroy");
		Enemy component = enemyObject.GetComponent<Enemy>();
		if (enemyObject.GetComponent<NurseSpawner>() != null)
		{
			Debug.Log(enemyObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(enemyObject.GetComponent<NurseSpawner>());
			enemyObject = null;
			yield break;
		}
		enemyObject = null;
		if (component == null || component is MebeebEnemy)
		{
			yield break;
		}
		if (component is PosterEnemy)
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			{
				foreach (GameObject item in FindAllObjectsWithTag.GetAllObjectsWithTag("Poster"))
				{
					UnityEngine.Object.Destroy(item.GetComponent<PosterEnemy>());
					UnityEngine.Object.Destroy(item.GetComponent<GangerSpawner>());
				}
				yield break;
			}
		}
		if (component is TVEnemy)
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			{
				foreach (GameObject item2 in FindAllObjectsWithTag.GetAllObjectsWithTag("TV"))
				{
					UnityEngine.Object.Destroy(item2.GetComponent<TVEnemy>());
				}
				yield break;
			}
		}
		if (component is WhiteWomanEnemy)
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			foreach (GameObject item3 in FindAllObjectsWithTag.GetAllObjectsWithTag("Elevator"))
			{
				UnityEngine.Object.Destroy(item3.GetComponent<WhiteWomanEnemy>());
			}
			WWObject = null;
		}
		else
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			component.gameObject.AddComponent<FadeAway>().StartFading(1f);
		}
	}

	private void ForceDestroyEnemy(GameObject enemyObject)
	{
		if (enemyObject == null)
		{
			return;
		}
		Enemy component = enemyObject.GetComponent<Enemy>();
		if (enemyObject.GetComponent<NurseSpawner>() != null)
		{
			Debug.Log(enemyObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(enemyObject.GetComponent<NurseSpawner>());
			enemyObject = null;
			return;
		}
		enemyObject = null;
		if (component == null)
		{
			return;
		}
		Debug.Log(component.name);
		if (component is MebeebEnemy)
		{
			return;
		}
		if (component is PosterEnemy)
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			{
				foreach (GameObject item in FindAllObjectsWithTag.GetAllObjectsWithTag("Poster"))
				{
					UnityEngine.Object.Destroy(item.GetComponent<PosterEnemy>());
					UnityEngine.Object.Destroy(item.GetComponent<GangerSpawner>());
				}
				return;
			}
		}
		if (component is TVEnemy)
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			{
				foreach (GameObject item2 in FindAllObjectsWithTag.GetAllObjectsWithTag("TV"))
				{
					UnityEngine.Object.Destroy(item2.GetComponent<TVEnemy>());
				}
				return;
			}
		}
		if (component is WhiteWomanEnemy)
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			foreach (GameObject item3 in FindAllObjectsWithTag.GetAllObjectsWithTag("Elevator"))
			{
				UnityEngine.Object.Destroy(item3.GetComponent<WhiteWomanEnemy>());
			}
			WWObject = null;
		}
		else
		{
			Debug.Log(component.gameObject.name + " Is Cleared");
			UnityEngine.Object.Destroy(component);
			component.gameObject.AddComponent<FadeAway>().StartFading(1f);
		}
	}

	private bool IsSpawnValidForAttackers(GameObject spawnPosition)
	{
		GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(spawnPosition);
		if (roomContainingObject == null)
		{
			return true;
		}
		GameObject[] array = enemies;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null && IsEnemyInRoom(gameObject, roomContainingObject) && (gameObject.GetComponent<SisSupEnemy>() != null || gameObject.GetComponent<MebeebEnemy>() != null || gameObject.GetComponent<NunEnemy>() != null))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsSpawnValidForSupports(GameObject spawnPosition)
	{
		GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(spawnPosition);
		if (roomContainingObject == null)
		{
			return true;
		}
		GameObject[] array = enemies;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null && gameObject.HasScript("MebeebEnemy") && IsEnemyInRoom(gameObject.GetComponent<DoorTransit>().GotoObject, roomContainingObject))
			{
				return false;
			}
			if (gameObject != null && IsEnemyInRoom(gameObject, roomContainingObject) && (gameObject.GetComponent<SisSupEnemy>() != null || gameObject.GetComponent<MebeebEnemy>() != null || gameObject.GetComponent<NunEnemy>() != null || gameObject.GetComponent<SisEnemy>() != null || gameObject.GetComponent<HoodieCatEnemy>() != null || gameObject.GetComponent<VampireEnemy>() != null || gameObject.GetComponent<WhisperEnemy>() != null || gameObject.GetComponent<NurseSpawner>() != null))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsEnemyInRoom(GameObject enemy, GameObject room)
	{
		if (enemy.name == "Nintendo")
		{
			return false;
		}
		if (HospitalManager.GetInstance().GetRoomContainingObject(enemy) == null)
		{
			Debug.LogError("can't Find Room Object For: " + enemy.name);
			return false;
		}
		if (room == null)
		{
			Debug.LogError("Room Wrong Object");
			return false;
		}
		return HospitalManager.GetInstance().GetRoomContainingObject(enemy).name == room.name;
	}

	private Vector3 GetRandomSpawnPosition()
	{
		if (spawnAreaColliders.Length == 0)
		{
			Debug.LogWarning("No spawn area colliders found.");
			return Vector3.zero;
		}
		GameObject currentRoomObject = Player.GetInstance().CurrentRoomObject;
		List<BoxCollider> list = new List<BoxCollider>();
		BoxCollider[] array = spawnAreaColliders;
		foreach (BoxCollider boxCollider in array)
		{
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(boxCollider.gameObject);
			if (roomContainingObject != null && roomContainingObject.name != currentRoomObject.name && IsSpawnValidForAttackers(roomContainingObject))
			{
				list.Add(boxCollider);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("Не найдено подходящих зон для спавна");
			return new Vector3(0f, 0f, 0f);
		}
		Bounds bounds = list[UnityEngine.Random.Range(0, list.Count)].bounds;
		return new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y), UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
	}

	private Vector3 GetRandomSpawnPositionWithColliderHeight(BaseEnemySettings enemySettings)
	{
		Vector3 randomSpawnPosition = GetRandomSpawnPosition();
		float num = 0f;
		_ = enemySettings.colliderSize;
		num = enemySettings.colliderSize.y;
		randomSpawnPosition.y += num / 2f;
		return randomSpawnPosition;
	}

	private bool IsInsideStageRoom(Vector3 position)
	{
		return true;
	}

	private bool IsInsideWardRoom(Vector3 position)
	{
		return true;
	}
}
