using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmallHedge.SoundManager;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Player : Entities
{
	public delegate void RoomChangedHandler(string newRoom);

	[SerializeField]
	private CameraFollow followingCamera;

	[SerializeField]
	private GameObject shinePrefab;

	[SerializeField]
	private GameObject flashLight;

	[SerializeField]
	private GameObject map;

	[SerializeField]
	private Light2D eyeLight;

	[SerializeField]
	[Range(1f, 3f)]
	private float moveSpeedBoost = 1.5f;

	[SerializeField]
	private float slowDownFactor = 0.5f;

	[SerializeField]
	private Image[] hearts;

	[SerializeField]
	private bool isNightSprites;

	[SerializeField]
	private ProgressBar enduranceBar;

	[SerializeField]
	private Image[] inventoryCells;

	[SerializeField]
	private Sprite[] defaultCellSprites;

	[SerializeField]
	private Sprite[] filledCellSprites;

	[Range(5f, 60f)]
	[SerializeField]
	private float elevatorCooldown = 15f;

	[Header("Flashlight Battery Settings")]
	[SerializeField]
	private Image[] batteryUI;

	[SerializeField]
	private int maxBatteryLevel = 4;

	[SerializeField]
	[Range(1f, 100f)]
	private float batteryDrainInterval = 15f;

	private int currentBatteryLevel;

	private bool isBatteryDraining;

	private Coroutine batteryDrainCoroutine;

	private float animationSpeed = 5f;

	private List<GameObject> animatingButtons = new List<GameObject>();

	private int currentAnimatingIndex;

	private const float _highLightIntensity = 0.32f;

	private const float _lowLightIntensity = 0.32f;

	[SerializeField]
	private GameObject IOArrowPrefab;

	private GameObject currentArrow;

	[Range(150f, 400f)]
	public static float moveSpeedProp = 150f;

	[Range(100f, 400f)]
	public static float moveSpeedBoostProp = 2f;

	[Range(0f, 5f)]
	public static float interactionSpeedProp = 2f;

	[Range(5f, 60f)]
	public static float elevatorCooldownProp = 20f;

	[Range(1f, 100f)]
	public static float batteryDrainIntervalProp = 15f;

	private List<InteractionObject> interactableObjects = new List<InteractionObject>();

	private int selectedObjectIndex;

	private List<GameObject> disappearingButtons = new List<GameObject>();

	private GameObject buttonsContainer;

	private List<GameObject> buttons = new List<GameObject>();

	public TextMeshProUGUI wallet;

	[SerializeField]
	private Sprite emptyCell;

	private bool isFlashLightOn;

	private static Player instance;

	private Elevator elevatorInstance;

	private DoorTransit lastOpenDoor;

	private Canvas canvas;

	public bool isTired;

	private bool isCaught;

	private bool isShift;

	private bool isActionButton;

	public EnduranceStateMachine enduranceSM;

	private bool isPaused;

	private bool isInteracting;

	private float endurancePercent = 1f;

	[SerializeField]
	private float staminaLowThreshold = 0.3f;

	[SerializeField]
	private float thresholdHysteresis = 0.02f;

	[SerializeField]
	private float staminaSoundCooldown = 10f;

	private const float CrawlSpeedThreshold = 0.65f;

	private float lastStaminaSoundTime = -999f;

	private bool soundArmed = true;

	private IPlayerState currentState;

	private int selectedCellIndex = 2;

	private int loopCount;

	private float defaultSpeed;

	private float holdTime = 0.2f;

	private float buttonPressStartTime;

	private Dictionary<GameObject, Vector2> targetPositions = new Dictionary<GameObject, Vector2>();

	private float interactionElevatorTimer;

	[Header("Player Health Settings")]
	[Range(0f, 5f)]
	public int maxHeartNumber = 3;

	private int currentHeartNumber;

	[Range(0f, 4f)]
	public int maxInventoryCells = 3;

	private const string CANVAS_NAME = "Canvas";

	private InputAction selectCellAction;

	private InputAction useItemAction;

	private Item currentSelectedItem;

	private Coroutine slowDownCoroutine;

	private Vector3 cachedMoveDirection = Vector3.zero;

	private bool waitingForUnfreeze;

	public bool cachedShift;

	private bool waitingForShiftUnfreeze;

	private bool wasFlashLightOn;

	private RectTransform canvasRectTransform;

	private RectTransform buttonsContainerRectTransform;

	private Canvas worldSpaceCanvas;

	private Dictionary<GameObject, Image> progressBarsCache = new Dictionary<GameObject, Image>();

	private bool unlockIsClose;

	private bool isCrawlAnimType;

	protected string currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";

	protected string previousWWpath = "prefabs/WierdWordEffects/WWFuckStandart";

	protected int counterForWierdWord;

	public float MoveSpeedBoost
	{
		get
		{
			return moveSpeedBoost;
		}
		set
		{
			moveSpeedBoost = value;
		}
	}

	public bool IsNightSprites
	{
		get
		{
			return isNightSprites;
		}
		set
		{
			isNightSprites = value;
		}
	}

	public override string CurrentRoom
	{
		get
		{
			return currentRoom;
		}
		set
		{
			if (currentRoom != value)
			{
				currentRoom = value;
				OnRoomChanged?.Invoke(currentRoom);
			}
		}
	}

	public ProgressBar EnduranceBar
	{
		get
		{
			return enduranceBar;
		}
		private set
		{
			enduranceBar = value;
		}
	}

	public float BatteryDrainInterval => batteryDrainInterval;

	public GameObject HideCabin { get; set; }

	public bool IsBatteryDraining
	{
		get
		{
			return isBatteryDraining;
		}
		set
		{
			isBatteryDraining = value;
		}
	}

	public Coroutine BatteryDrainCoroutine
	{
		set
		{
			batteryDrainCoroutine = value;
		}
	}

	public List<InteractionObject> InteractableObjects => interactableObjects;

	public int SelectedObjectIndex
	{
		set
		{
			selectedObjectIndex = value;
		}
	}

	public GameObject ButtonsContainer => buttonsContainer;

	public int CurrentBatteryLevel
	{
		get
		{
			return currentBatteryLevel;
		}
		set
		{
			currentBatteryLevel = value;
		}
	}

	public float ElevatorCooldown => elevatorCooldown;

	public Image[] InventoryCells => inventoryCells;

	public override float CurrentMoveSpeed
	{
		get
		{
			return currentMoveSpeed;
		}
		set
		{
			currentMoveSpeed = Mathf.Clamp(value, 0f, base.MoveSpeedCup);
		}
	}

	public override float CurrentInteractionSpeed
	{
		get
		{
			return currentInteractionSpeed;
		}
		set
		{
			currentInteractionSpeed = Mathf.Clamp(value, 0f, base.InteractionSpeedCup);
		}
	}

	public bool IsFlashLightOn => isFlashLightOn;

	public Elevator ElevatorInstance
	{
		get
		{
			return elevatorInstance;
		}
		set
		{
			elevatorInstance = value;
		}
	}

	public Animator Animator
	{
		get
		{
			return animator;
		}
		set
		{
			animator = value;
		}
	}

	public bool IsShift
	{
		get
		{
			return isShift;
		}
		set
		{
			isShift = value;
		}
	}

	public bool IsPaused
	{
		get
		{
			return isPaused;
		}
		set
		{
			isPaused = value;
		}
	}

	public float EndurancePercent
	{
		get
		{
			return endurancePercent;
		}
		set
		{
			if (!enduranceSM.IsConsumptionPaused || !(endurancePercent > value))
			{
				float num = Mathf.Clamp01(value);
				if (num >= staminaLowThreshold + thresholdHysteresis)
				{
					soundArmed = true;
				}
				bool num2 = endurancePercent > staminaLowThreshold && num <= staminaLowThreshold;
				endurancePercent = num;
				enduranceBar?.UpdateProgress(endurancePercent);
				float unscaledTime = Time.unscaledTime;
				if (num2 && soundArmed && unscaledTime - lastStaminaSoundTime >= staminaSoundCooldown)
				{
					SoundManager.PlaySound(SoundType.StaminaEnds);
					lastStaminaSoundTime = unscaledTime;
					soundArmed = false;
				}
			}
		}
	}

	public IPlayerState CurrentState
	{
		get
		{
			return currentState;
		}
		set
		{
			ChangeState(value);
		}
	}

	public int LoopCount
	{
		set
		{
			loopCount = value;
		}
	}

	public override string CurrentStageName
	{
		get
		{
			return currentStageName;
		}
		set
		{
			if (currentStageName == null)
			{
				currentStageName = value;
				return;
			}
			NightManager nightManager = NightManager.GetInstance();
			if (nightManager == null)
			{
				currentStageName = value;
				return;
			}
			string text = nightManager.NormalizeStageName(value);
			string text2 = nightManager.NormalizeStageName(currentStageName);
			if (text != text2)
			{
				currentStageName = value;
				OnStageChanged?.Invoke(currentStageName);
			}
			else
			{
				currentStageName = value;
			}
		}
	}

	public float InteractionElevatorTimer
	{
		get
		{
			return interactionElevatorTimer;
		}
		set
		{
			interactionElevatorTimer = value;
		}
	}

	public Item CurrentSelectedItem
	{
		get
		{
			UpdateInventoryCellSprites();
			return currentSelectedItem;
		}
		set
		{
			currentSelectedItem = value;
			UpdateInventoryCellSprites();
		}
	}

	public int CurrentHeartNumber
	{
		get
		{
			return currentHeartNumber;
		}
		set
		{
			if (value < currentHeartNumber && !GlobalSaveSystem.GetProgress("HintNHide") && !GlobalSaveSystem.GetProgress("HintNHideReady"))
			{
				GlobalSaveSystem.SetProgress("HintNHideReady", value: true);
			}
			currentHeartNumber = Mathf.Clamp(value, 0, maxHeartNumber);
			if (currentHeartNumber == 0)
			{
				Debug.Log("GameOver");
			}
			UpdateHeartsNumber();
		}
	}

	public DoorTransit LastOpenDoor
	{
		get
		{
			return lastOpenDoor;
		}
		set
		{
			lastOpenDoor = value;
		}
	}

	public bool IsCaught
	{
		get
		{
			return isCaught;
		}
		set
		{
			isCaught = value;
		}
	}

	public bool IsActionButton
	{
		get
		{
			return isActionButton;
		}
		set
		{
			isActionButton = value;
		}
	}

	protected virtual string BEpath => "";

	protected virtual string WWpath => currentWWpath;

	public static event RoomChangedHandler OnRoomChanged;

	public event Action<string> OnStageChanged;

	public virtual void EndSexMode()
	{
		if (CurrentHeartNumber > 1)
		{
			blinkManager.FadeToBlink(GetInstance().gameObject, ExitSexMode);
		}
		else
		{
			ExitSexMode();
		}
	}

	public override AudioClip GetMovementSound()
	{
		string[] array = new string[4] { "heroStep1", "heroStep2", "heroStep3", "heroStep4" };
		int num = UnityEngine.Random.Range(0, array.Length);
		string text = array[num];
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/" + text);
		if (audioClip == null)
		{
			Debug.LogWarning("[Audio] Клип '" + text + "' не найден в Resources/Audio!");
			return null;
		}
		return audioClip;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		instance = null;
	}

	public override void EnterHideState()
	{
		ChangeState(new PlayerHideState());
	}

	public override void ExitHideState()
	{
		ChangeState(new PlayerIdleState());
	}

	public void SetInteracting(bool value)
	{
		isInteracting = value;
	}

	public float GetCrawlSpeedThreshold()
	{
		return 0.65f;
	}

	public float GetDefaultSpeed()
	{
		return defaultSpeed;
	}

	public static Player GetInstance()
	{
		return instance;
	}

	public void SlowDown(float slowDownDuration = 0.05f)
	{
		if (slowDownCoroutine != null)
		{
			StopCoroutine(slowDownCoroutine);
		}
		slowDownCoroutine = StartCoroutine(SlowDownCoroutine(slowDownDuration));
	}

	private IEnumerator SlowDownCoroutine(float slowDownDuration)
	{
		float currentPlayerMoveSpeed = CurrentMoveSpeed;
		float targetSpeed = CurrentMoveSpeed * slowDownFactor;
		float elapsedTime = 0f;
		while (elapsedTime < slowDownDuration)
		{
			elapsedTime += Time.deltaTime;
			currentPlayerMoveSpeed = Mathf.Lerp(currentPlayerMoveSpeed, targetSpeed, elapsedTime / slowDownDuration);
			yield return null;
		}
		yield return new WaitForSeconds(slowDownDuration);
		elapsedTime = 0f;
		while (elapsedTime < slowDownDuration)
		{
			elapsedTime += Time.deltaTime;
			currentPlayerMoveSpeed = Mathf.Lerp(targetSpeed, currentPlayerMoveSpeed, elapsedTime / slowDownDuration);
			yield return null;
		}
		CurrentMoveSpeed = currentPlayerMoveSpeed;
		slowDownCoroutine = null;
	}

	public void DollCorrectSound()
	{
		SoundManager.PlaySound(SoundType.DollCorrect);
	}

	public void DollErrorSound()
	{
		SoundManager.PlaySound(SoundType.DollError);
	}

	public void Hloop1Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop1n2);
	}

	public void Hloop2Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop1n2);
	}

	public void Hloop3Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop3);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySound(SoundType.HClimax);
	}

	public void TVAirTakeSound()
	{
		SoundManager.PlaySound(SoundType.TVAirTake);
	}

	public void TVCatchSound()
	{
		SoundManager.PlaySound(SoundType.TVCatch);
	}

	public void OnHeroStepSound()
	{
		SoundManager.PlaySound(SoundType.HeroWalking);
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void OnAction(InputAction.CallbackContext context)
	{
		if (TimelineManager.GetInstance() != null && TimelineManager.GetInstance().IsTimelinePlaying() && DialogueManager.GetInstance() != null && !DialogueManager.GetInstance().dialogueIsPlaying)
		{
			isActionButton = false;
		}
		else if (PauseMenu.IsPaused || Timer.IsTransitioning || ((!base.gameObject.activeInHierarchy || Time.timeScale == 0f) && !WhiteWomanEnemy.IsCharmGame) || BlinkManager.GetInstance().IsFade())
		{
			isActionButton = false;
		}
		else if (isPaused)
		{
			if (context.performed)
			{
				isActionButton = true;
			}
			else if (context.canceled)
			{
				isActionButton = false;
			}
		}
		else if (!context.performed)
		{
			isActionButton = false;
		}
		else
		{
			isActionButton = context.performed;
		}
	}

	public void OnEscape(InputAction.CallbackContext context)
	{
	}

	public void OnCheat(InputAction.CallbackContext context)
	{
	}

	public void OnMap(InputAction.CallbackContext context)
	{
		if (isPaused || PauseMenu.IsPaused || !base.enabled || Timer.IsTransitioning || !map.activeInHierarchy || !base.gameObject.activeInHierarchy || Time.timeScale == 0f || BlinkManager.GetInstance().IsFade() || !context.performed)
		{
			return;
		}
		SoundManager.PlaySound(SoundType.Map);
		if (!(map == null))
		{
			CanvasGroup component = map.GetComponent<CanvasGroup>();
			if (!(component == null))
			{
				StopAllCoroutines();
				StartCoroutine(FadeCanvasGroup(component, (!(component.alpha > 0f)) ? 1 : 0, 0.2f));
			}
		}
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
	{
		float startAlpha = canvasGroup.alpha;
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
			yield return null;
		}
		canvasGroup.alpha = targetAlpha;
		canvasGroup.interactable = targetAlpha > 0f;
		canvasGroup.blocksRaycasts = targetAlpha > 0f;
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		if (ShouldIgnoreInputCompletely())
		{
			ResetMovement();
			return;
		}
		Vector2 vector = context.ReadValue<Vector2>();
		cachedMoveDirection = new Vector3(vector.x, 0f, vector.y);
		if (cachedMoveDirection.magnitude > 0.1f)
		{
			if (IsTemporarilyFrozen())
			{
				waitingForUnfreeze = true;
			}
			else
			{
				ApplyCachedMovement();
			}
		}
		else
		{
			ResetMovement();
		}
	}

	public void OnConsole(InputAction.CallbackContext context)
	{
	}

	public void OnSelectCell(InputAction.CallbackContext context)
	{
		if (PauseMenu.IsPaused || Timer.IsTransitioning || !base.gameObject.activeInHierarchy || Time.timeScale == 0f || BlinkManager.GetInstance().IsFade())
		{
			return;
		}
		if (context.performed)
		{
			string text = context.control.name.ToLower();
			int num = -1;
			switch (text)
			{
			case "1":
			case "numpad1":
				num = 1;
				break;
			case "2":
			case "numpad2":
				num = 2;
				break;
			case "3":
			case "numpad3":
				num = 3;
				break;
			}
			if (num > 0 && num <= maxInventoryCells)
			{
				SelectCell(maxInventoryCells - num);
			}
			Image image = inventoryCells[selectedCellIndex];
			if (image.GetComponentInChildren<ItemToUse>() != null)
			{
				currentSelectedItem = image.GetComponentInChildren<ItemToUse>();
			}
			else if (image.GetComponentInChildren<QuestItem>() != null)
			{
				currentSelectedItem = image.GetComponentInChildren<QuestItem>();
			}
			else
			{
				currentSelectedItem = null;
			}
		}
		ManualTriggerCheck();
		DisplayInteractionButtons();
		UpdateSelectedObjectUI();
	}

	public void SetPause(bool isPaused)
	{
		isActionButton = false;
		if (!isPaused)
		{
			if (CurrentState is PlayerHideState)
			{
				HideThing component = HideCabin.GetComponent<HideThing>();
				if (component != null)
				{
					component.ForceExit();
					ChangeState(new PlayerIdleState());
				}
			}
			if (!isMoving)
			{
				ChangeState(new PlayerIdleState());
			}
		}
		AlignToRoomLine();
		ManualTriggerCheck();
		DisplayInteractionButtons();
		UpdateSelectedObjectUI();
		this.isPaused = isPaused;
	}

	public void OnUseItem(InputAction.CallbackContext context)
	{
		if (isPaused || PauseMenu.IsPaused || Timer.IsTransitioning || !base.gameObject.activeInHierarchy || Time.timeScale == 0f || BlinkManager.GetInstance().IsFade() || !isNightSprites)
		{
			return;
		}
		if (context.performed && inventoryCells[selectedCellIndex].transform.childCount > 0)
		{
			if (currentSelectedItem == null)
			{
				TryToCheckCell();
			}
			if (currentSelectedItem.GetComponentInChildren<ItemToUse>() == null || !currentSelectedItem.GetComponentInChildren<ItemToUse>().CanBeUsed())
			{
				return;
			}
			currentSelectedItem.GetComponentInChildren<ItemToUse>().UseItem();
			UnityEngine.Object.Destroy(inventoryCells[selectedCellIndex].transform.GetChild(0).gameObject);
			UpdateInventoryCellSprites();
		}
		StartCoroutine(CheckInNextUpdate());
	}

	private void TryToCheckCell()
	{
		Image image = inventoryCells[selectedCellIndex];
		if (image.GetComponentInChildren<ItemToUse>() != null)
		{
			currentSelectedItem = image.GetComponentInChildren<ItemToUse>();
		}
		else if (image.GetComponentInChildren<QuestItem>() != null)
		{
			currentSelectedItem = image.GetComponentInChildren<QuestItem>();
		}
		else
		{
			currentSelectedItem = null;
		}
	}

	public IEnumerator CheckInNextUpdate()
	{
		yield return null;
		ManualTriggerCheck();
		HandlePlayerInteractUI();
	}

	public void OnShift(InputAction.CallbackContext context)
	{
		if (ShouldIgnoreInputCompletely())
		{
			ResetShift();
		}
		else if (isTired)
		{
			cachedShift = false;
			ApplyCachedShift();
		}
		else
		{
			if (NightManager.GetInstance() == null && !Timer.HasUpdatedBasementDoor)
			{
				return;
			}
			if (base.TotalSpeedModifier <= 0.65f)
			{
				cachedShift = false;
				ApplyCachedShift();
				return;
			}
			cachedShift = context.action.triggered;
			if (IsTemporarilyFrozen())
			{
				waitingForShiftUnfreeze = true;
			}
			else
			{
				ApplyCachedShift();
			}
		}
	}

	public void TurnFlashLight(InputAction.CallbackContext context)
	{
		if (isPaused || eyeLight == null || !isNightSprites || PauseMenu.IsPaused || Timer.IsTransitioning || !base.gameObject.activeInHierarchy || Time.timeScale == 0f || BlinkManager.GetInstance().IsFade())
		{
			return;
		}
		if (context.started && !(CurrentState is PlayerHideState) && !(CurrentState is PlayerInteractState))
		{
			buttonPressStartTime = Time.time;
			wasFlashLightOn = isFlashLightOn;
			if (!isFlashLightOn && currentBatteryLevel > 0 && base.TotalSpeedModifier > 0.65f)
			{
				isFlashLightOn = true;
				SoundManager.PlaySound(SoundType.FlashlightOn);
				flashLight.SetActive(isFlashLightOn);
				if (!flashLight.GetComponent<Light2D>().enabled)
				{
					flashLight.GetComponent<Light2D>().enabled = true;
				}
				eyeLight.intensity = 0.8f;
				if (!isBatteryDraining)
				{
					BatteryManager.Instance.StartDrainBattery();
				}
			}
		}
		else
		{
			if (!context.canceled || !(Time.time - buttonPressStartTime <= holdTime) || !wasFlashLightOn)
			{
				return;
			}
			isFlashLightOn = false;
			SoundManager.PlaySound(SoundType.FlashlightOn);
			flashLight.SetActive(isFlashLightOn);
			if (HospitalManager.GetInstance() == null)
			{
				Debug.LogError("HospitalManager не инициализирован!");
				return;
			}
			if (eyeLight == null)
			{
				Debug.LogError("eyeLight не назначен!");
				return;
			}
			GameObject[] array = GameObject.FindGameObjectsWithTag("TabLight");
			bool flag = false;
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject.activeSelf)
				{
					GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(gameObject);
					if (roomContainingObject != null && roomContainingObject.name == currentRoom)
					{
						flag = true;
						break;
					}
				}
			}
			eyeLight.intensity = (flag ? 0.32f : 0.32f);
			BatteryManager.Instance.InterruptBatteryDrain();
		}
	}

	private void OnEnable()
	{
		Animator component = GetComponent<Animator>();
		if (component != null)
		{
			component.enabled = true;
		}
		if (spriteRenderer != null)
		{
			Color color = spriteRenderer.color;
			color.a = 1f;
			spriteRenderer.color = color;
		}
		if (currentState != null)
		{
			ChangeState(new PlayerIdleState());
		}
		if (currentState is PlayerIdleState)
		{
			if (IsNightSprites)
			{
				PlayAnimation("Idle_Night");
			}
			else
			{
				PlayAnimation("Idle_Day");
			}
		}
		if (buttonsContainer != null)
		{
			buttonsContainer.SetActive(value: true);
		}
		Transform transform = base.transform.Find("ShadowObject");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: true);
		}
		if (isNightSprites)
		{
			if (enduranceSM != null && enduranceSM.currentState is EnduranceTiredState)
			{
				enduranceSM.SetTargetColor(Color.red);
			}
			else
			{
				isTired = false;
			}
		}
	}

	public void OnDisable()
	{
		if (currentState != null && currentState is PlayerHideState)
		{
			ChangeState(new PlayerIdleState());
		}
		BatteryDrainDisable();
		DestroyCurrentArrow();
		isBatteryDraining = false;
		FlashlightOff();
		interactableObjects.Clear();
		foreach (GameObject button in buttons)
		{
			UnityEngine.Object.Destroy(button.gameObject);
		}
		buttons.Clear();
		foreach (GameObject animatingButton in animatingButtons)
		{
			UnityEngine.Object.Destroy(animatingButton.gameObject);
		}
		animatingButtons.Clear();
		foreach (GameObject disappearingButton in disappearingButtons)
		{
			UnityEngine.Object.Destroy(disappearingButton.gameObject);
		}
		disappearingButtons.Clear();
		if (buttonsContainer != null)
		{
			buttonsContainer.SetActive(value: false);
		}
	}

	public void BatteryDrainDisable()
	{
		if (BatteryManager.Instance != null)
		{
			BatteryManager.Instance.InterruptBatteryDrain();
		}
	}

	public void FlashlightOff()
	{
		if (Timer.HasLoadedNextScene || !Application.isPlaying)
		{
			return;
		}
		isFlashLightOn = false;
		if (flashLight == null)
		{
			return;
		}
		if (flashLight.activeSelf)
		{
			SoundManager.PlaySound(SoundType.FlashlightOn);
		}
		flashLight.SetActive(value: false);
		if (HospitalManager.GetInstance() == null || eyeLight == null)
		{
			return;
		}
		bool flag = false;
		GameObject[] array = GameObject.FindGameObjectsWithTag("TabLight");
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				if (HospitalManager.GetInstance() == null)
				{
					return;
				}
				GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(gameObject);
				if (roomContainingObject != null && roomContainingObject.name == currentRoom)
				{
					flag = true;
					break;
				}
			}
		}
		eyeLight.intensity = (flag ? 0.32f : 0.32f);
	}

	public void UpdateBatteryUI()
	{
		for (int i = 0; i < batteryUI.Length; i++)
		{
			batteryUI[i].enabled = i < currentBatteryLevel;
		}
	}

	public void RestoreBattery()
	{
		currentBatteryLevel = maxBatteryLevel;
		if (BatteryManager.Instance != null)
		{
			BatteryManager.Instance.accumulatedDrainTime = 0f;
		}
		UpdateBatteryUI();
		Debug.Log("Battery fully restored.");
	}

	public void ChangeState(IPlayerState newState)
	{
		if (newState == null)
		{
			Debug.LogError("Attempted to change to a null state.");
			return;
		}
		if (currentState != null && newState.GetType() == currentState.GetType())
		{
			Debug.Log("newState: " + newState.GetType().Name + " is equal to currentState: " + currentState.GetType().Name);
			return;
		}
		currentState?.ExitState(this);
		Debug.Log("Change State From: " + (currentState?.GetType().Name ?? "null") + " To: " + newState.GetType().Name);
		currentState = newState;
		currentState.EnterState(this);
	}

	public void PlayAnimation(string animName)
	{
		animator.Play(animName);
		animator.Update(0f);
	}

	private void SetSettings()
	{
		currentMoveSpeed = moveSpeedProp;
		MoveSpeedBoost = moveSpeedBoostProp;
		currentInteractionSpeed = interactionSpeedProp;
		elevatorCooldown = elevatorCooldownProp;
		batteryDrainInterval = batteryDrainIntervalProp;
	}

	private void SetSpeedBoostCups(float moveSpeedBoost, float interactionSpeedBoost = 1f)
	{
		base.InteractionSpeedCup = base.BaseInteractionSpeed * interactionSpeedBoost;
		base.MoveSpeedCup = base.BaseMoveSpeed * MoveSpeedBoost;
		MoveSpeedBoost = moveSpeedBoost - 1f;
	}

	public void StartAfterFrameIdle()
	{
		StartCoroutine(AfterFrameIdle());
	}

	private IEnumerator AfterFrameIdle()
	{
		yield return null;
		if (!isMoving)
		{
			GetAnimator().Play("Idle_Night");
		}
	}

	protected override void Start()
	{
		enduranceSM = new EnduranceStateMachine(this);
		SetSettings();
		slowDownCoroutine = null;
		base.Start();
		if (wallet != null)
		{
			wallet.text = "0";
		}
		unlockIsClose = false;
		SetSpeedBoostCups(MoveSpeedBoost);
		animator = GetComponent<Animator>();
		ChangeState(new PlayerIdleState());
		if (NightManager.GetInstance() == null)
		{
			AddSpeedModifier("DaySpeedBoost", 0.4f);
		}
		if (animator == null)
		{
			Debug.LogError("animator Not Found");
		}
		followingCamera = UnityEngine.Object.FindObjectOfType<CameraFollow>();
		if (!(GameObject.Find("Canvas") != null))
		{
			return;
		}
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		if (canvas == null && NightManager.GetInstance() != null)
		{
			Debug.LogError("Canvas Not Found");
		}
		GameObject gameObject = GameObject.Find("WorldSpaceCanvas");
		if (gameObject != null)
		{
			worldSpaceCanvas = gameObject.GetComponent<Canvas>();
			if (worldSpaceCanvas.renderMode != RenderMode.WorldSpace)
			{
				Debug.LogError("Canvas должен быть в режиме World Space!");
				return;
			}
			buttonsContainer = new GameObject("InteractionButtonsContainer", typeof(RectTransform));
			buttonsContainer.transform.SetParent(worldSpaceCanvas.transform, worldPositionStays: false);
			buttonsContainerRectTransform = buttonsContainer.GetComponent<RectTransform>();
		}
		if (!(NightManager.GetInstance() == null))
		{
			currentHeartNumber = maxHeartNumber;
			UpdateHeartsNumber();
			UpdateInventory();
			inventoryCells[selectedCellIndex].color = Color.yellow;
			currentBatteryLevel = maxBatteryLevel;
			UpdateBatteryUI();
			enduranceBar.gameObject.SetActive(value: true);
			AlignToRoomLine();
			UpdateInventoryCellSprites();
		}
	}

	public void OnTab(InputAction.CallbackContext context)
	{
		if (context.performed && interactableObjects.Count != 0 && !PauseMenu.IsPaused && !Timer.IsTransitioning && base.gameObject.activeInHierarchy && Time.timeScale != 0f && !BlinkManager.GetInstance().IsFade())
		{
			InteractionObject obj = ((selectedObjectIndex >= 0 && selectedObjectIndex < interactableObjects.Count) ? interactableObjects[selectedObjectIndex] : null);
			_ = interactableObjects.Count;
			selectedObjectIndex = (selectedObjectIndex + 1) % interactableObjects.Count;
			obj?.StopInteract();
			UpdateSelectedObjectUI();
		}
	}

	public void ForceStopInteract()
	{
		foreach (InteractionObject interactableObject in interactableObjects)
		{
			interactableObject?.StopInteract();
		}
	}

	public void UpdateSelectedObjectUI()
	{
		_ = NightManager.GetInstance() != null;
		interactableObjects = (from obj in interactableObjects
			where obj.CheckConditions()
			orderby obj.CheckConditions() descending
			select obj).ToList();
		if (buttons.Count != interactableObjects.Count)
		{
			Debug.Log(buttons.Count);
			Debug.Log(interactableObjects.Count);
			Debug.LogError("Количество кнопок и объектов взаимодействия не совпадает!");
			{
				foreach (InteractionObject interactableObject in interactableObjects)
				{
					Debug.Log(interactableObject.name);
				}
				return;
			}
		}
		if (buttons.Count == 1)
		{
			selectedObjectIndex = 0;
		}
		DestroyCurrentArrow();
		for (int num = 0; num < buttons.Count; num++)
		{
			bool flag = num == selectedObjectIndex;
			UpdateButtonAppearance(buttons[num], interactableObjects[num], flag);
			bool? flag2 = BlinkManager.GetInstance()?.IsFade();
			if (flag && !DialogueManager.GetInstance().dialogueIsPlaying && !IsTimelinePlaying() && flag2 == false)
			{
				CreateArrowIndicator(interactableObjects[num]);
			}
		}
	}

	public GameObject GetCurrentHighlightedButton()
	{
		foreach (GameObject disappearingButton in disappearingButtons)
		{
			Image component = disappearingButton.GetComponent<Image>();
			if (component != null && component.color == Color.white)
			{
				return disappearingButton;
			}
		}
		foreach (GameObject button in buttons)
		{
			Image component2 = button.GetComponent<Image>();
			if (component2 != null && component2.color == Color.white)
			{
				return button;
			}
		}
		return null;
	}

	public void DestroyCurrentArrow()
	{
		if (currentArrow != null)
		{
			UnityEngine.Object.Destroy(currentArrow);
			currentArrow = null;
		}
	}

	private void UpdateButtonAppearance(GameObject button, InteractionObject interactionObject, bool isSelected)
	{
		Image component = button.GetComponent<Image>();
		component.sprite = interactionObject.ButtonIcon;
		component.color = (isSelected ? Color.white : new Color(1f, 1f, 1f, 0.3f));
	}

	private void CreateArrowIndicator(InteractionObject interactionObject)
	{
		Transform transform = interactionObject.transform.Find("arrowCenter");
		Vector3 vector;
		float objectHeight;
		if (transform != null)
		{
			vector = transform.position;
			objectHeight = 0f;
		}
		else
		{
			vector = GetObjectCenter(interactionObject, out objectHeight);
		}
		Vector3 position = vector + new Vector3(0f, objectHeight / 2f + 1.5f, 0f);
		Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);
		currentArrow = UnityEngine.Object.Instantiate(IOArrowPrefab, worldSpaceCanvas.transform);
		RectTransform component = currentArrow.GetComponent<RectTransform>();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(worldSpaceCanvas.transform as RectTransform, screenPoint, worldSpaceCanvas.worldCamera, out var localPoint);
		component.anchoredPosition = localPoint;
	}

	private Vector3 GetObjectCenter(InteractionObject interactionObject, out float objectHeight)
	{
		Collider component = interactionObject.GetComponent<Collider>();
		if (component != null)
		{
			objectHeight = component.bounds.size.y;
			return component.bounds.center;
		}
		Collider2D component2 = interactionObject.GetComponent<Collider2D>();
		if (component2 != null)
		{
			objectHeight = component2.bounds.size.y;
			return component2.bounds.center;
		}
		objectHeight = 1f;
		return interactionObject.transform.position;
	}

	public void HandlePlayerInteractUI()
	{
		DisplayInteractionButtons();
	}

	private void DisplayInteractionButtons()
	{
		_ = NightManager.GetInstance() != null;
		HashSet<InteractionObject> hashSet = new HashSet<InteractionObject>();
		foreach (InteractionObject interactableObject in interactableObjects)
		{
			if (interactableObject.CheckConditions())
			{
				hashSet.Add(interactableObject);
			}
		}
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		foreach (GameObject button in buttons)
		{
			InteractionObject interactionObject = interactableObjects.Find((InteractionObject o) => $"Button_{o.GetHashCode()}" == button.name);
			if (interactionObject == null || !hashSet.Contains(interactionObject))
			{
				disappearingButtons.Add(button);
				list2.Add(button);
			}
		}
		foreach (GameObject item in list2)
		{
			buttons.Remove(item);
		}
		foreach (InteractionObject obj in hashSet)
		{
			if (!buttons.Exists((GameObject b) => b.name == $"Button_{obj.GetHashCode()}"))
			{
				GameObject gameObject = new GameObject($"Button_{obj.GetHashCode()}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
				gameObject.transform.SetParent(buttonsContainer.transform, worldPositionStays: false);
				gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(46f, 45f);
				gameObject.GetComponent<Image>().sprite = obj.ButtonIcon;
				CanvasGroup component = gameObject.GetComponent<CanvasGroup>();
				component.alpha = 0f;
				component.interactable = true;
				component.blocksRaycasts = true;
				buttons.Add(gameObject);
				list.Add(gameObject);
			}
		}
		foreach (GameObject button2 in buttons)
		{
			CanvasGroup component2 = button2.GetComponent<CanvasGroup>();
			if (component2 != null && !list.Contains(button2))
			{
				component2.alpha = 1f;
				component2.interactable = true;
				component2.blocksRaycasts = true;
			}
		}
		if (list.Count > 0)
		{
			animatingButtons = list;
			currentAnimatingIndex = 0;
		}
		CenterButtonsInContainer();
		UpdateSelectedObjectUI();
	}

	private void UpdateHeartsNumber()
	{
		for (int i = 0; i < hearts.Length; i++)
		{
			hearts[i].enabled = i < maxHeartNumber;
			if (i < currentHeartNumber)
			{
				hearts[i].enabled = true;
			}
			else
			{
				hearts[i].enabled = false;
			}
		}
	}

	public void UpdateInventoryCellSprites()
	{
		for (int i = 0; i < inventoryCells.Length; i++)
		{
			if (!(inventoryCells[i] == null))
			{
				if (inventoryCells[i].transform.childCount > 0)
				{
					inventoryCells[i].sprite = filledCellSprites[i];
				}
				else
				{
					inventoryCells[i].sprite = defaultCellSprites[i];
				}
			}
		}
	}

	public void UpdateInventory()
	{
		for (int i = 0; i < inventoryCells.Length; i++)
		{
			inventoryCells[i].enabled = i < maxInventoryCells;
		}
	}

	public void ManualTriggerCheck()
	{
		if (!(base.EntityCollider == null) && base.gameObject.activeInHierarchy)
		{
			Collider[] array = Physics.OverlapBox(base.transform.position, base.EntityCollider.size / 2f, base.transform.rotation);
			Debug.Log($"Collider Count: {array.Length}");
			Collider[] array2 = array;
			foreach (Collider other in array2)
			{
				OnTriggerEnter(other);
			}
		}
	}

	protected override void OnTriggerEnter(Collider other)
	{
		StageManager component = other.GetComponent<StageManager>();
		if (component != null)
		{
			component.CenterColliderOnPlayer(base.transform.position.x);
		}
		if (other.gameObject.CompareTag("MainCamera"))
		{
			return;
		}
		if (other.gameObject.CompareTag("MapSegment"))
		{
			if (map != null)
			{
				map.GetComponent<UIMapMover>().MoveToSegment(other.gameObject.name);
			}
			return;
		}
		base.OnTriggerEnter(other);
		InteractionObject component2 = other.GetComponent<InteractionObject>();
		if (component2 != null && !interactableObjects.Contains(component2) && component2.CheckConditions())
		{
			interactableObjects.Add(component2);
			DisplayInteractionButtons();
			UpdateSelectedObjectUI();
		}
		CurrentState?.HandleTrigger(this, other);
	}

	public void RemoveFromPlayerList(InteractionObject interactionObject)
	{
		if (interactionObject != null && interactableObjects.Contains(interactionObject))
		{
			if (selectedObjectIndex < interactableObjects.Count && interactableObjects[selectedObjectIndex] == interactionObject)
			{
				interactionObject.StopInteract();
				isInteracting = false;
			}
			interactableObjects.Remove(interactionObject);
			DisplayInteractionButtons();
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		StageManager component = other.GetComponent<StageManager>();
		if (component != null)
		{
			component.ResetColliderPosition();
		}
		base.OnTriggerExit(other);
		if (other.gameObject.CompareTag("MainCamera"))
		{
			return;
		}
		InteractionObject interactionObject = other.GetComponent<InteractionObject>();
		if (interactionObject != null && interactableObjects.Contains(interactionObject))
		{
			interactableObjects.Remove(interactionObject);
			Debug.Log("Removing button for: " + interactionObject.name);
			GameObject gameObject = buttons.FirstOrDefault((GameObject b) => b.name == $"Button_{interactionObject.GetHashCode()}");
			if (gameObject != null)
			{
				animatingButtons.Remove(gameObject);
				disappearingButtons.Add(gameObject);
				buttons.Remove(gameObject);
				DisplayInteractionButtons();
				UpdateSelectedObjectUI();
			}
		}
		TabLight componentInParent = other.GetComponentInParent<TabLight>();
		Transform transform = other.transform.Find("GuardLight");
		if (interactObject != null && interactObject.GetComponent<TabLight>() != null && componentInParent != null && (currentSelectedItem == null || currentSelectedItem.GetComponentInChildren<QuestItem>().ItemName == interactObject.KeyItemName))
		{
			_ = componentInParent == interactObject.GetComponent<TabLight>();
		}
		else if (elevatorInstance != null && other.gameObject == elevatorInstance.gameObject)
		{
			if (DialogueManager.GetInstance().dialogueIsPlaying)
			{
				StartCoroutine(DialogueManager.GetInstance().ExitDialogueMode());
			}
			DialogueManager.GetInstance().SetTypingSpeed(0.04f);
			Nintendo nintendo = UnityEngine.Object.FindObjectOfType<Nintendo>();
			if (!(nintendo != null) || !(Vector3.Distance(nintendo.transform.position, base.transform.position) <= 900f) || !(nintendo.targetChase == elevatorInstance.gameObject))
			{
				elevatorInstance.Animator.Play("Closed");
			}
		}
		else if (interactObject != null && !(CurrentState is PlayerHideState) && transform == null && other.gameObject == interactObject.gameObject)
		{
			inTrigger = false;
		}
	}

	protected override void FixedUpdate()
	{
		if (!isPaused && !(CurrentState is PlayerInteractState) && !(CurrentState is PlayerHideState))
		{
			base.FixedUpdate();
			if (CurrentState is PlayerCaughtState)
			{
				moveDirection = Vector3.zero;
			}
		}
	}

	public void HandleCurrentIntensity()
	{
		if (isFlashLightOn)
		{
			return;
		}
		if (HospitalManager.GetInstance() == null)
		{
			Debug.LogError("HospitalManager не инициализирован!");
		}
		else
		{
			if (eyeLight == null)
			{
				return;
			}
			if (CurrentState is PlayerSexModeState)
			{
				eyeLight.intensity = 0.52f;
				eyeLight.color = new Color32(byte.MaxValue, 227, 246, byte.MaxValue);
				return;
			}
			bool flag = GameObject.FindGameObjectsWithTag("TabLight")?.Where((GameObject tabLight) => tabLight.activeSelf).Any((GameObject tabLight) =>
			{
				GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(tabLight);
				return roomContainingObject != null && roomContainingObject.name == currentRoom;
			}) ?? false;
			eyeLight.color = Color.white;
			eyeLight.intensity = (flag ? 0.32f : 0.32f);
		}
	}

	public void SetEyeLightIntensity(float intensity)
	{
		eyeLight.intensity = intensity;
	}

	public void DestroyButton(GameObject button)
	{
		if (buttons.Contains(button))
		{
			buttons.Remove(button);
		}
		if (animatingButtons.Contains(button))
		{
			animatingButtons.Remove(button);
		}
		if (disappearingButtons.Contains(button))
		{
			disappearingButtons.Remove(button);
		}
		UnityEngine.Object.Destroy(button);
	}

	private void Update()
	{
		enduranceSM.Update();
		if (waitingForShiftUnfreeze && !IsTemporarilyFrozen())
		{
			ApplyCachedShift();
			waitingForShiftUnfreeze = false;
		}
		if (waitingForUnfreeze && !IsTemporarilyFrozen())
		{
			ApplyCachedMovement();
			waitingForUnfreeze = false;
		}
		if (currentAnimatingIndex < animatingButtons.Count)
		{
			GameObject gameObject = animatingButtons[currentAnimatingIndex];
			if (gameObject != null)
			{
				CanvasGroup component = gameObject.GetComponent<CanvasGroup>();
				component.alpha = Mathf.MoveTowards(component.alpha, 1f, animationSpeed * Time.deltaTime);
				if (component.alpha >= 1f)
				{
					currentAnimatingIndex++;
				}
			}
		}
		for (int num = disappearingButtons.Count - 1; num >= 0; num--)
		{
			GameObject gameObject2 = disappearingButtons[num];
			CanvasGroup component2 = gameObject2.GetComponent<CanvasGroup>();
			component2.alpha = Mathf.MoveTowards(component2.alpha, 0f, animationSpeed * Time.deltaTime);
			if (component2.alpha <= 0f)
			{
				Debug.Log("Destroying button: " + gameObject2.name);
				disappearingButtons.RemoveAt(num);
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		foreach (GameObject button in buttons)
		{
			if (targetPositions.ContainsKey(button))
			{
				RectTransform component3 = button.GetComponent<RectTransform>();
				component3.anchoredPosition = Vector2.Lerp(component3.anchoredPosition, targetPositions[button], Time.deltaTime * animationSpeed);
			}
		}
		if (isMoving)
		{
			followingCamera.SmoothMove();
		}
		if (currentState != null)
		{
			currentState.UpdateState(this);
		}
		UpdateInteractionProgress();
	}

	private void LateUpdate()
	{
		UpdateButtonsWorldPosition();
	}

	private bool ShouldIgnoreInputCompletely()
	{
		if (!isPaused && !PauseMenu.IsPaused && !Timer.IsTransitioning)
		{
			return !base.gameObject.activeInHierarchy;
		}
		return true;
	}

	private bool IsTemporarilyFrozen()
	{
		if (Time.timeScale != 0f)
		{
			return BlinkManager.GetInstance().IsFade();
		}
		return true;
	}

	private void ApplyCachedMovement()
	{
		moveDirection = cachedMoveDirection;
	}

	private void ApplyCachedShift()
	{
		isShift = cachedShift;
	}

	private void ResetShift()
	{
		isShift = false;
		waitingForShiftUnfreeze = false;
	}

	private void ResetMovement()
	{
		moveDirection = Vector3.zero;
		waitingForUnfreeze = false;
	}

	private void UpdateInteractionProgress()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			GameObject gameObject = buttons[i];
			InteractionObject interactionObject = interactableObjects[i];
			Vector3 position = gameObject.transform.position;
			float y = Mathf.Round(position.y - 0.5f) + 0.5f;
			gameObject.transform.position = new Vector3(position.x, y, position.z);
			if (!progressBarsCache.TryGetValue(gameObject, out var value))
			{
				value = CreateProgressBar(gameObject);
				progressBarsCache[gameObject] = value;
			}
			float fillAmount = Mathf.Clamp01(interactionObject.CurrentRadius / interactionObject.MaxRadius);
			value.fillAmount = fillAmount;
		}
	}

	private Image CreateProgressBar(GameObject button)
	{
		GameObject obj = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
		obj.transform.SetParent(button.transform, worldPositionStays: false);
		RectTransform component = obj.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		Image component2 = obj.GetComponent<Image>();
		component2.sprite = Resources.Load<Sprite>("Sprites/fillButton");
		component2.color = new Color(1f, 1f, 1f, 0.7f);
		component2.type = Image.Type.Filled;
		component2.fillMethod = Image.FillMethod.Vertical;
		component2.fillOrigin = 0;
		component2.fillAmount = 0f;
		return component2;
	}

	private void UpdateButtonsWorldPosition()
	{
		if (buttonsContainerRectTransform != null)
		{
			Vector3 position = base.transform.position + new Vector3(0f, 120f, 0f);
			buttonsContainerRectTransform.position = position;
			buttonsContainerRectTransform.rotation = Quaternion.identity;
			CenterButtonsInContainer();
		}
	}

	private void CenterButtonsInContainer()
	{
		if (buttons.Count != 0 && !(buttonsContainerRectTransform == null))
		{
			_ = buttonsContainerRectTransform.rect.width;
			float num = 46f;
			float num2 = 0f;
			float num3 = 0f - ((float)buttons.Count * num + (float)(buttons.Count - 1) * num2) / 2f + num / 2f;
			for (int i = 0; i < buttons.Count; i++)
			{
				float x = num3 + (float)i * (num + num2);
				targetPositions[buttons[i]] = new Vector2(x, 0f);
			}
		}
	}

	private void SelectCell(int index)
	{
		DeselectAllCells();
		selectedCellIndex = index;
		inventoryCells[index].color = Color.yellow;
	}

	private void DeselectAllCells()
	{
		Image[] array = inventoryCells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = Color.white;
		}
	}

	public void HandleInteractions()
	{
		if (interactableObjects.Count > 0 && selectedObjectIndex < interactableObjects.Count)
		{
			InteractionObject interactionObject = interactableObjects[selectedObjectIndex];
			if (interactionObject.CheckConditions())
			{
				isInteracting = true;
				interactionObject.Interact(base.gameObject);
			}
		}
	}

	public bool PressInteractButton()
	{
		if (interactableObjects.Count > 0 && selectedObjectIndex < interactableObjects.Count)
		{
			InteractionObject selectedObject = interactableObjects[selectedObjectIndex];
			GameObject gameObject = buttons.FirstOrDefault((GameObject b) => b.name == $"Button_{selectedObject.GetHashCode()}");
			if (gameObject != null && disappearingButtons.Contains(gameObject))
			{
				return false;
			}
			if (selectedObject.CheckConditions())
			{
				return selectedObject.IsInteract();
			}
		}
		return false;
	}

	public void BreakInteract()
	{
		if (interactableObjects.Count > 0 && selectedObjectIndex < interactableObjects.Count)
		{
			InteractionObject interactionObject = interactableObjects[selectedObjectIndex];
			if (interactionObject.CheckConditions())
			{
				interactionObject.StopInteract();
			}
		}
		isInteracting = false;
	}

	public void RemoveInteractionObject(InteractionObject interactionObject)
	{
		if (interactableObjects.Contains(interactionObject))
		{
			interactableObjects.Remove(interactionObject);
			DisplayInteractionButtons();
		}
	}

	private bool IsValidKeyItem()
	{
		if (string.IsNullOrEmpty(interactObject.KeyItemName) || interactObject is NPCDialogueTrigger)
		{
			return true;
		}
		if (currentSelectedItem != null)
		{
			Item componentInChildren = currentSelectedItem.GetComponentInChildren<Item>();
			if (componentInChildren != null)
			{
				return interactObject.KeyItemName == componentInChildren.ItemName;
			}
			return false;
		}
		return false;
	}

	public void UpdateInteractionTimer()
	{
		if (interactionElevatorTimer > 0f)
		{
			interactionElevatorTimer -= Time.deltaTime;
			interactionElevatorTimer = Mathf.Max(interactionElevatorTimer, 0f);
			if (interactionElevatorTimer <= 0.5f && !unlockIsClose)
			{
				unlockIsClose = true;
				Elevator.GetTriggeredInstance()?.UpdateLockUIVisibility(isShow: false);
			}
			if (interactionElevatorTimer <= 0f && unlockIsClose)
			{
				unlockIsClose = false;
				StartCoroutine(CheckInNextUpdate());
			}
		}
	}

	protected override float GetDirection()
	{
		return moveDirection.x;
	}

	protected override void Move()
	{
		base.Move();
		if (base.TotalSpeedModifier <= 0.65f || isTired)
		{
			isShift = false;
		}
		HandleStaminaRate();
	}

	protected override void HandleAnimBySpeed()
	{
		if (!IsNightSprites)
		{
			return;
		}
		float effectiveSpeedModifier = GetEffectiveSpeedModifier();
		Debug.Log($"SpeedModidir is {effectiveSpeedModifier} ");
		bool flag = effectiveSpeedModifier <= 0.65f;
		Debug.Log($"shouldBeCrawling is {flag} ");
		Debug.Log($"isCrawlAnimType is {isCrawlAnimType} ");
		if (flag != isCrawlAnimType)
		{
			if (flag)
			{
				SwitchToCrawl();
			}
			else
			{
				SwitchFromCrawl();
			}
			isCrawlAnimType = flag;
		}
	}

	private float GetEffectiveSpeedModifier()
	{
		float num = base.TotalSpeedModifier;
		if (speedModifiers.ContainsKey("MoveSpeedBoost"))
		{
			num -= MoveSpeedBoost;
		}
		return num;
	}

	private void SwitchToCrawl()
	{
		FlashlightOff();
		if (currentState is PlayerRunState)
		{
			ChangeState(new PlayerWalkState());
		}
		else if (currentState is PlayerWalkState)
		{
			PlayAnimation("Hero_Crawl");
		}
		else if (currentState is PlayerIdleState || currentState is PlayerInteractState)
		{
			PlayAnimation("Hero_Crawl_Idle");
		}
	}

	private void SwitchFromCrawl()
	{
		if (currentState is PlayerWalkState)
		{
			PlayAnimation("Walk_Night");
		}
		else if (currentState is PlayerIdleState || currentState is PlayerInteractState)
		{
			PlayAnimation("Idle_Night");
		}
	}

	public void HandleStaminaRate()
	{
		if (!(enduranceBar == null) && !isTired && isMoving)
		{
			if (stageManager != null)
			{
				stageManager.CheckStagePosition();
			}
			HandlePlayerFatigue();
		}
	}

	private void HandlePlayerFatigue()
	{
		if (!isTired)
		{
			AdjustSpeedAccordingToStamina();
		}
		else
		{
			RecoverFromFatigue();
		}
	}

	private void AdjustSpeedAccordingToStamina()
	{
		if (isTired)
		{
			if (isShift)
			{
				isShift = false;
			}
			if (CurrentState is PlayerRunState)
			{
				ChangeState(new PlayerWalkState());
			}
		}
		else if (isShift)
		{
			if (Mathf.Approximately(EndurancePercent, 0f) && !isTired)
			{
				EnterFatigueState();
				return;
			}
			if (!(CurrentState is PlayerRunState) && !(CurrentState is PlayerHideState) && !(CurrentState is PlayerInteractState))
			{
				ChangeState(new PlayerRunState());
			}
			AddSpeedModifier("MoveSpeedBoost", MoveSpeedBoost);
			if (!enduranceSM.IsConsumptionPaused)
			{
				enduranceSM.ConsumeRunning();
			}
		}
		else
		{
			if (!(CurrentState is PlayerWalkState) && !(CurrentState is PlayerHideState) && !(CurrentState is PlayerInteractState))
			{
				ChangeState(new PlayerWalkState());
			}
			RemoveSpeedModifier("MoveSpeedBoost");
		}
	}

	private void EnterFatigueState()
	{
		SoundManager.PlaySound(SoundType.StaminaEnds);
	}

	private void RecoverFromFatigue()
	{
		if (!(CurrentState is PlayerWalkState) && !(CurrentState is PlayerHideState) && !(CurrentState is PlayerInteractState))
		{
			ChangeState(new PlayerWalkState());
		}
	}

	public void SetEnduranceBarColor(Color color)
	{
		if (enduranceSM == null || enduranceBar == null)
		{
			return;
		}
		Image[] componentsInChildren = enduranceBar.GetComponentsInChildren<Image>();
		foreach (Image image in componentsInChildren)
		{
			if (!(image.gameObject.name == "RollbackZone"))
			{
				image.color = color;
			}
		}
	}

	public void SetItemInSelectedCell(GameObject item)
	{
		int num = -1;
		for (int num2 = maxInventoryCells - 1; num2 >= 0; num2--)
		{
			if (inventoryCells[num2].transform.childCount == 0)
			{
				num = num2;
				break;
			}
		}
		int num3 = ((num != -1) ? num : selectedCellIndex);
		Transform transform = inventoryCells[num3].transform;
		if (transform.childCount > 0)
		{
			GameObject gameObject = transform.GetChild(0).gameObject;
			UnityEngine.Object.Instantiate(shinePrefab, base.transform.position - new Vector3(0f, 0.4f, 0f), Quaternion.identity).GetComponent<ShineItem>().SetShineItem(gameObject);
			UnityEngine.Object.Destroy(gameObject);
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(item, transform);
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one;
		RectTransform component = item.GetComponent<RectTransform>();
		RectTransform component2 = gameObject2.GetComponent<RectTransform>();
		if (component != null && component2 != null)
		{
			component2.anchorMin = component.anchorMin;
			component2.anchorMax = component.anchorMax;
			component2.pivot = component.pivot;
			component2.sizeDelta = component.sizeDelta;
			component2.anchoredPosition = component.anchoredPosition;
		}
		Image component3 = gameObject2.GetComponent<Image>();
		if (component3 != null)
		{
			component3.preserveAspect = true;
		}
		if (item.GetComponent<TimeBoostItem>() != null)
		{
			NightManager.GetInstance()?.ToggleOfSkipWatchSpawn();
		}
		if (num3 == selectedCellIndex)
		{
			ItemToUse component4 = gameObject2.GetComponent<ItemToUse>();
			QuestItem component5 = gameObject2.GetComponent<QuestItem>();
			currentSelectedItem = ((component4 != null) ? ((Item)component4) : ((Item)component5));
		}
		UpdateInventoryCellSprites();
	}

	public void ExitSexMode()
	{
		EndEvent.GetInstance().UnblockSkip();
		animator.SetInteger("HloopCount", 0);
		loopCount = 0;
		if (CurrentHeartNumber <= 1)
		{
			CurrentHeartNumber--;
			if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Second"))
			{
				animator.Play("SecondHloop2");
			}
			else
			{
				animator.Play("Hloop2");
			}
		}
		else
		{
			Nintendo.ShowNintendo();
			ChangeState(new PlayerIdleState());
		}
	}

	public void AnimationLoopFinished()
	{
		loopCount++;
		animator.SetInteger("HloopCount", loopCount);
	}

	public void EnableFastWWEffect()
	{
		previousWWpath = currentWWpath;
		currentWWpath = "prefabs/WierdWordEffects/WWFuckFast";
	}

	public void EnableSlurpWWEffect()
	{
		previousWWpath = currentWWpath;
		currentWWpath = "prefabs/WierdWordEffects/WWFuckSlurp";
	}

	public void ResetWWEffect()
	{
		previousWWpath = currentWWpath;
		currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";
	}

	public virtual void CreateWierdWordEffect()
	{
		if (string.IsNullOrEmpty(WWpath))
		{
			return;
		}
		if (counterForWierdWord <= 0)
		{
			GameObject gameObject = Resources.Load<GameObject>(WWpath);
			if (gameObject != null)
			{
				Vector3 vector = UnityEngine.Random.insideUnitSphere.normalized * 50f;
				vector.y = 0f;
				Vector3 position = base.transform.position + vector;
				UnityEngine.Object.Instantiate(gameObject, position, Quaternion.identity, base.transform);
				counterForWierdWord = UnityEngine.Random.Range(1, 4);
			}
			else
			{
				Debug.LogError("Не удалось загрузить странное слово по пути: " + WWpath);
			}
		}
		else
		{
			counterForWierdWord--;
		}
	}

	private void CreateEffect(string path, Vector3? position = null)
	{
		GameObject gameObject = Resources.Load<GameObject>(path);
		if (gameObject != null)
		{
			UnityEngine.Object.Instantiate(gameObject, position ?? base.transform.position, Quaternion.identity, base.transform).name = gameObject.name;
		}
		else
		{
			Debug.LogError("Не удалось загрузить эффект по пути: " + path);
		}
	}

	public void CreateCommonEffect(string effectName)
	{
		string path = "prefabs/CommonEffects/" + effectName;
		switch (effectName)
		{
		case "LostBoySignEffect":
			CreateEffect(path, base.transform.position + new Vector3(15f, 80f, 0f));
			break;
		case "DetectionTriggerEffect":
			CreateEffect(path, base.transform.position + new Vector3(-20f, 150f, 0f));
			break;
		case "LustEffect":
			CreateEffect(path, base.transform.position + new Vector3(60f, 100f, 0f));
			break;
		case "NunAirThingEffect":
			CreateEffect(path, base.transform.position + new Vector3(-25f, 20f, 0f));
			break;
		default:
			CreateEffect(path);
			break;
		}
	}

	public void RemoveChildByName(string childName)
	{
		foreach (Transform item in base.transform)
		{
			if (item.name == childName || item.name.StartsWith(childName))
			{
				UnityEngine.Object.Destroy(item.gameObject);
				return;
			}
		}
		Debug.LogWarning("Child with name '" + childName + "' not found.");
	}
}
