using System.Collections;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WhiteWomanEnemy : ObjectEnemy
{
	[Header("Game Elements")]
	public RectTransform largeBar;

	public RectTransform charmGamePanel;

	public RectTransform smallBar;

	public RectTransform circle;

	public ProgressBar penaltyBarProgress;

	private AudioSource audioSource;

	[Header("Enemy Prefab with Animator")]
	[SerializeField]
	private GameObject enemyPrefabWithAnimator;

	private Animator enemyAnimator;

	private GameObject longLightForm;

	private GameObject longLightUp;

	private GameObject elevatorLightForm;

	private GameObject WW;

	public static bool isLoseEvent;

	private Coroutine smallBarMovementCoroutine;

	[SerializeField]
	private float charmGameTimer = 15f;

	private float penaltySpeed = 0.15f;

	private float rewardSpeed = 0.06f;

	private string elevatorName;

	private GameObject UIParty;

	private bool isCircleInSmallBar;

	public static bool eventTriggered;

	public static bool isForceOpenDoor;

	private static bool enterPressed;

	private static bool charmGameRunning;

	private Canvas gameCanvas;

	public GameObject EnemyPrefabWithAnimator
	{
		set
		{
			enemyPrefabWithAnimator = value;
		}
	}

	public float CharmGameTimer
	{
		set
		{
			charmGameTimer = value;
		}
	}

	public string ElevatorName
	{
		get
		{
			return elevatorName;
		}
		set
		{
			elevatorName = value;
		}
	}

	public static bool IsCharmGame => charmGameRunning;

	protected override string BEpath => "prefabs/BreatheEffects/BEWhite";

	protected override void OnTriggerEnter(Collider collision)
	{
	}

	public void Hloop1Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Hloop1n2, base.gameObject);
	}

	public void Hloop2Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Hloop1n2, base.gameObject);
	}

	public void Hloop3Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Hloop3, base.gameObject);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.HClimax, base.gameObject);
	}

	public static void OnEnter(InputAction.CallbackContext context)
	{
		if (!context.action.triggered)
		{
			enterPressed = false;
		}
		else
		{
			enterPressed = context.action.triggered;
		}
	}

	protected override void Start()
	{
		base.Start();
		charmGameRunning = false;
		isForceOpenDoor = false;
		eventTriggered = false;
		smallBarMovementCoroutine = null;
		GetComponentInParent<Elevator>().IsInElevatorChoice = false;
		UIParty = GameObject.Find("UI party");
		longLightForm = base.transform.Find("LongLightForm")?.gameObject;
		longLightUp = base.transform.Find("LongLightUp")?.gameObject;
		elevatorLightForm = base.transform.Find("ElevatorLightForm")?.gameObject;
		WW = base.transform.Find("WW")?.gameObject;
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null && enemyPrefabWithAnimator == null)
		{
			Color color = component.color;
			color.a = 0f;
			component.color = color;
			StartCoroutine(FadeIn(component, 1f));
		}
		if (enemyPrefabWithAnimator != null)
		{
			InitializeAnimator();
		}
	}

	private IEnumerator FadeIn(SpriteRenderer renderer, float duration)
	{
		float elapsedTime = 0f;
		Color color = renderer.color;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration);
			renderer.color = color;
			yield return null;
		}
		color.a = 1f;
		renderer.color = color;
	}

	private void OnEnable()
	{
		Player.GetInstance().OnStageChanged += HandleStageChange;
	}

	private void OnDisable()
	{
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().OnStageChanged -= HandleStageChange;
		}
	}

	private void HandleStageChange(string newStage)
	{
		if (NightManager.GetInstance().NormalizeStageName(newStage) != NightManager.GetInstance().NormalizeStageName(CurrentStageName) && NightManager.GetInstance().StopGameCoroutine == null && !EndEvent.isGameOver && CanBeCleared())
		{
			Debug.Log(base.name + " переспавнен из-за смены stage");
			NightManager.GetInstance().ForceRespawnEnemy(base.gameObject);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (enemyPrefabWithAnimator != null && !charmGameRunning && GetComponentInParent<Elevator>().IsInElevatorChoice && !eventTriggered)
		{
			charmGameRunning = true;
			StartCoroutine(CharmGame());
		}
	}

	private IEnumerator CharmGame()
	{
		isForceOpenDoor = true;
		NightManager.GetInstance().HandleCatchMode(this);
		yield return null;
		audioSource = SoundManager.PlayLoopingSound(SoundType.ElevatorMove);
		Transform transform = base.transform.Find("WW");
		Vector3 initialWwPosition = transform.localPosition;
		List<Transform> wwChildren = new List<Transform>();
		foreach (Transform item in transform)
		{
			wwChildren.Add(item);
			item.SetParent(transform.parent, worldPositionStays: true);
		}
		BlinkManager.GetInstance().BlinkWithCustomSpeed(2f, Player.GetInstance().gameObject, null, allowVerticalMovement: true, 0f, 0f, base.gameObject.transform.parent.gameObject);
		yield return new WaitForSeconds(0.5f);
		Nintendo.HideNintendo();
		CameraFollow.GetInstance().SetFollowTarget(base.gameObject.transform.Find("cameraTarget"));
		Player.GetInstance().gameObject.SetActive(value: false);
		Object.FindObjectOfType<Nintendo>();
		enemyAnimator.Play("IdleGame");
		ToggleRenderers(state: false);
		yield return new WaitForSeconds(4f);
		StartCharmGame();
		float elapsedTime = 0f;
		int lastProgressSegment = 0;
		while (elapsedTime < charmGameTimer)
		{
			UpdateCirclePosition();
			CheckCircleInSmallBar();
			if (isCircleInSmallBar)
			{
				DecreasePenaltyBar();
			}
			else
			{
				IncreasePenaltyBar();
			}
			int num = Mathf.FloorToInt(penaltyBarProgress.GetProgress() * 10f);
			if (num != lastProgressSegment)
			{
				UpdatePositionByProgress(num, initialWwPosition);
				lastProgressSegment = num;
			}
			if (penaltyBarProgress.GetProgress() >= 1f)
			{
				BlinkManager.GetInstance().BlinkWithCustomSpeed(2f, Player.GetInstance().gameObject, null, allowVerticalMovement: true, 0f, 0f, base.gameObject.transform.parent.gameObject);
				penaltyBarProgress.gameObject.SetActive(value: false);
				charmGamePanel.gameObject.SetActive(value: false);
				yield return new WaitForSeconds(0.5f);
				StartCoroutine(EndCharmGame(initialWwPosition, wwChildren));
				yield break;
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		StartCoroutine(EndCharmGame(initialWwPosition, wwChildren));
	}

	private void UpdatePositionByProgress(int progressSegment, Vector3 initialWwPosition)
	{
		float num = (float)progressSegment * 0.5f;
		if (progressSegment < 0)
		{
			progressSegment = 0;
		}
		base.transform.Find("WW").localPosition = initialWwPosition + Vector3.right * num;
	}

	private void StartCharmGame()
	{
		if (penaltyBarProgress == null)
		{
			Debug.Log("penaltyBarProgress is null");
		}
		penaltyBarProgress.gameObject.SetActive(value: true);
		penaltyBarProgress.ShowBar();
		penaltyBarProgress.UpdateProgress(0f);
		charmGamePanel.gameObject.SetActive(value: true);
		gameCanvas = Object.FindObjectOfType<Canvas>();
		smallBar.anchoredPosition = new Vector2(smallBar.anchoredPosition.x, largeBar.anchoredPosition.y - largeBar.sizeDelta.y / 2f + smallBar.sizeDelta.y / 2f);
		circle.anchoredPosition = new Vector2(smallBar.anchoredPosition.x, largeBar.anchoredPosition.y - largeBar.sizeDelta.y / 2f + circle.sizeDelta.y / 2f);
		smallBarMovementCoroutine = StartCoroutine(MoveSmallBarRandomly());
	}

	private void InitializeAnimator()
	{
		if (enemyPrefabWithAnimator != null && enemyAnimator == null)
		{
			GameObject gameObject = Object.Instantiate(enemyPrefabWithAnimator, base.transform.position, Quaternion.identity);
			gameObject.name = "WW";
			enemyAnimator = gameObject.GetComponent<Animator>();
			gameObject.transform.SetParent(base.gameObject.transform);
			gameObject.transform.localPosition = new Vector3(-81f, 81f, 0f);
			if (enemyAnimator == null)
			{
				Debug.LogError("Animator не найден на префабе врага!");
			}
			currentRoom = HospitalManager.GetInstance().GetRoomContainingObject(base.gameObject).name;
		}
		else
		{
			Debug.LogError("Префаб врага не назначен в инспекторе или аниматор уже инициализирован.");
		}
	}

	private Image FindFilledImageInChildren(Image parent)
	{
		Image[] componentsInChildren = parent.GetComponentsInChildren<Image>();
		foreach (Image image in componentsInChildren)
		{
			if (image.type == Image.Type.Filled)
			{
				return image;
			}
		}
		return null;
	}

	private IEnumerator EndCharmGame(Vector3 initialWwPosition, List<Transform> wwChildren)
	{
		if (eventTriggered)
		{
			yield break;
		}
		eventTriggered = true;
		if (smallBarMovementCoroutine != null)
		{
			StopCoroutine(smallBarMovementCoroutine);
			smallBarMovementCoroutine = null;
		}
		Transform transform = base.transform.Find("WW");
		if (transform != null)
		{
			transform.localPosition = initialWwPosition;
			foreach (Transform wwChild in wwChildren)
			{
				wwChild.SetParent(transform, worldPositionStays: true);
			}
		}
		if (penaltyBarProgress.GetProgress() >= 1f)
		{
			LoseEvent();
		}
		else
		{
			VictoryEvent();
		}
	}

	private void UpdateCirclePosition()
	{
		if (Player.GetInstance().IsActionButton)
		{
			circle.anchoredPosition += Vector2.up * Time.deltaTime * 300f;
		}
		else
		{
			circle.anchoredPosition += Vector2.down * Time.deltaTime * 300f;
		}
		float min = largeBar.anchoredPosition.y - largeBar.sizeDelta.y / 2f + circle.sizeDelta.y / 2f;
		float max = largeBar.anchoredPosition.y + largeBar.sizeDelta.y / 2f - circle.sizeDelta.y / 2f;
		circle.anchoredPosition = new Vector2(circle.anchoredPosition.x, Mathf.Clamp(circle.anchoredPosition.y, min, max));
	}

	private void CheckCircleInSmallBar()
	{
		float num = circle.anchoredPosition.y - circle.sizeDelta.y / 2f;
		float num2 = circle.anchoredPosition.y + circle.sizeDelta.y / 2f;
		float num3 = smallBar.anchoredPosition.y - smallBar.sizeDelta.y / 2f;
		float num4 = smallBar.anchoredPosition.y + smallBar.sizeDelta.y / 2f;
		isCircleInSmallBar = num >= num3 && num2 <= num4;
	}

	private void IncreasePenaltyBar()
	{
		if (penaltyBarProgress != null)
		{
			float progress = penaltyBarProgress.GetProgress();
			float penaltySpeedModifier = GetPenaltySpeedModifier(increase: true);
			float barValue = progress + penaltySpeed * penaltySpeedModifier * Time.deltaTime;
			penaltyBarProgress.UpdateProgress(barValue);
		}
	}

	private void DecreasePenaltyBar()
	{
		if (penaltyBarProgress != null)
		{
			float progress = penaltyBarProgress.GetProgress();
			float penaltySpeedModifier = GetPenaltySpeedModifier(increase: false);
			float num = progress - rewardSpeed * penaltySpeedModifier * Time.deltaTime;
			if (num < 0f)
			{
				num = 0f;
			}
			penaltyBarProgress.UpdateProgress(num);
		}
	}

	private float GetPenaltySpeedModifier(bool increase)
	{
		switch ((Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2))
		{
		case Difficulty.Practice:
		case Difficulty.Easy:
			if (!increase)
			{
				return 1.1f;
			}
			return 0.9f;
		case Difficulty.Normal:
			return 1f;
		case Difficulty.Hard:
			if (!increase)
			{
				return 0.9f;
			}
			return 1.1f;
		default:
			return 1f;
		}
	}

	private void LoseEvent()
	{
		if (enemyAnimator != null)
		{
			enemyAnimator.Play("SexMode");
			isLoseEvent = true;
		}
		SoundManager.GetInstance().StartCoroutine(SoundManager.FadeOutSound(audioSource, 0.1f, 0f));
		base.transform.Find("WW").localPosition = new Vector3(36f, 88f, 0f);
		Player.GetInstance().IsPaused = false;
		charmGamePanel.gameObject.SetActive(value: false);
		penaltyBarProgress.gameObject.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		smallBarMovementCoroutine = null;
		SoundManager.StopLoopingSound(audioSource);
		isForceOpenDoor = false;
		eventTriggered = false;
		isLoseEvent = false;
		charmGameRunning = false;
		Debug.Log(base.gameObject.name);
		WW = base.transform.Find("WW")?.gameObject;
		Debug.Log(WW != null);
		if (base.gameObject.name != "WW" && WW != null)
		{
			Debug.Log("WW Is Clear");
			WhiteWomanEnemy component = WW.GetComponent<WhiteWomanEnemy>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			if (NightManager.GetInstance() != null)
			{
				NightManager.GetInstance().FadeWWObject(WW);
			}
		}
	}

	public override void EndSexMode()
	{
		ExitSexMode();
	}

	protected override void ExitSexMode()
	{
		Nintendo.ShowNintendo();
		EnableSkipButton();
		if (enemyAnimator != null)
		{
			enemyAnimator.SetInteger("HloopCount", 0);
		}
		loopCount = 0;
		Player.GetInstance().CurrentHeartNumber--;
		if (Player.GetInstance().CurrentHeartNumber > 0)
		{
			Nintendo.ShowNintendo();
			isLoseEvent = false;
			Player.GetInstance().ElevatorInstance.GoToFloor(elevatorName, RestoreInstancesAfterBlink);
			Player.GetInstance().GetAnimator().enabled = true;
			Player.GetInstance().GetAnimator().Play("Idle_Night");
			Object.Destroy(base.transform.parent.GetComponent<WhiteWomanEnemy>(), 1f);
			Object.Destroy(base.gameObject, 1f);
		}
		else if (enemyAnimator != null)
		{
			enemyAnimator.Play("Fuck2");
		}
	}

	private void VictoryEvent()
	{
		charmGameRunning = false;
		charmGamePanel.gameObject.SetActive(value: false);
		WhiteWomanEnemy component = base.transform.Find("WW").gameObject.GetComponent<WhiteWomanEnemy>();
		Player.GetInstance().ElevatorInstance.GoToFloor(component.ElevatorName, RestoreInstancesAfterBlink);
		penaltyBarProgress.gameObject.SetActive(value: false);
		isLoseEvent = false;
		Object.Destroy(component.gameObject, 1f);
		Object.Destroy(this, 1f);
	}

	public void RestoreInstancesAfterBlink()
	{
		charmGameRunning = false;
		Player.GetInstance().gameObject.SetActive(value: true);
		Player.GetInstance().IsPaused = false;
		Player.GetInstance().GetComponent<Entities>().AlignToRoomLine();
		if (CameraFollow.GetInstance().FollowTarget != Player.GetInstance().transform)
		{
			CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
		}
		ToggleRenderers(state: true);
	}

	private IEnumerator MoveSmallBarRandomly()
	{
		_ = smallBar.anchoredPosition;
		while (true)
		{
			float num = Random.Range((0f - (largeBar.sizeDelta.y - smallBar.sizeDelta.y)) / 2f, (largeBar.sizeDelta.y - smallBar.sizeDelta.y) / 2f);
			Vector2 targetPosition = new Vector2(smallBar.anchoredPosition.x, largeBar.anchoredPosition.y + num);
			float elapsedTime = 0f;
			Vector2 startPosition = smallBar.anchoredPosition;
			while (elapsedTime < 0.5f)
			{
				smallBar.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / 0.5f);
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			smallBar.anchoredPosition = targetPosition;
		}
	}

	public void LetsShakeElevator()
	{
		base.transform.parent.parent.GetComponent<ObjectShake>().ShakeBaby();
	}

	public override bool CanBeCleared()
	{
		if (base.CanBeCleared())
		{
			return !charmGameRunning;
		}
		return false;
	}
}
