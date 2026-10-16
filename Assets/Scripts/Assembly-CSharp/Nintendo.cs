using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Nintendo : Entities
{
	public BaseEnemySettings Settings;

	private float chaseDirection;

	[SerializeField]
	private float chaseDistance = 150f;

	public GameObject targetChase;

	private GameObject mainGoal;

	private INintendoState currentState;

	private float showRoflCooldown = 40f;

	[SerializeField]
	private float showRoflTimer;

	private float chaseDistanceDefault;

	protected int loopCount;

	private bool isPreparedForRofl;

	public bool isReachedMainGoal;

	private bool flipDirectionIsRight;

	private bool guardPositionIsTaken;

	private float defaultInteractionSpeed;

	public float InteractionBoostTime = 2f;

	private float InteractionBoostTimer;

	public float roflTime = 3f;

	private float roflTimer;

	private Coroutine guardCoroutine;

	private Coroutine roflEventCoroutine;

	public string mainGoalRoomName;

	private bool eventTriggered;

	private static Nintendo instance;

	public string currentRoomForObjective;

	public static string elevatorName;

	private Stack<GameObject> chaseStack = new Stack<GameObject>();

	private GameObject wantedObject;

	private static Material spriteDefaultMat;

	private Dictionary<string, string> hintSpriteMap = new Dictionary<string, string>
	{
		{ "HintNHideReady", "HintAfterLoseHP" },
		{ "HintNSupNurseKeyReady", "HintAfterLoseMed2" },
		{ "HintNPosterReady", "HintAfterLosePoster" },
		{ "HintNBatsReady", "HintAfterLoseVampire" },
		{ "HintNDollReady", "HintAfterLoseMebeb" },
		{ "HintNWhisperReady", "HintAfterLoseWhisper" }
	};

	protected string currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";

	protected int counterForWierdWord;

	private static Coroutine fadeCoroutine;

	public float ChaseDirection
	{
		set
		{
			chaseDirection = value;
		}
	}

	public float ChaseDistance
	{
		get
		{
			return chaseDistance;
		}
		set
		{
			chaseDistance = value;
		}
	}

	public GameObject MainGoal => mainGoal;

	public INintendoState CurrentState => currentState;

	public float ShowRoflCooldown => showRoflCooldown;

	public float ShowRoflTimer
	{
		get
		{
			return showRoflTimer;
		}
		set
		{
			showRoflTimer = value;
		}
	}

	public HideThing HideCabin { get; set; }

	public int LoopCount
	{
		get
		{
			return loopCount;
		}
		set
		{
			loopCount = value;
		}
	}

	public bool IsPreparedForRofl
	{
		get
		{
			return isPreparedForRofl;
		}
		set
		{
			isPreparedForRofl = value;
		}
	}

	public Coroutine GuardCoroutine
	{
		get
		{
			return guardCoroutine;
		}
		set
		{
			guardCoroutine = value;
		}
	}

	public Coroutine RoflEventCoroutine
	{
		get
		{
			return roflEventCoroutine;
		}
		set
		{
			roflEventCoroutine = value;
		}
	}

	public bool EventTriggered => eventTriggered;

	public static Nintendo Instance => instance;

	protected virtual string WWpath => currentWWpath;

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

	public static Nintendo GetInstance()
	{
		return instance;
	}

	protected override void Start()
	{
		counterForWierdWord = Random.Range(1, 4);
		roflTimer = roflTime;
		base.Start();
		fadeCoroutine = null;
		chaseStack = new Stack<GameObject>();
		AlignToRoomLine();
		ChangeState(new NintendoIdleState());
		chaseDistanceDefault = chaseDistance;
		spriteRenderer.sortingOrder = 60;
	}

	private void CreateShadowObject()
	{
		GameObject obj = new GameObject("ShadowObject");
		obj.AddComponent<SpriteRenderer>().sortingOrder = 1;
		obj.transform.SetParent(base.transform);
		Animator component = GetComponent<Animator>();
		component.Rebind();
		component.Update(0f);
	}

	private static Material GetSpriteDefaultMat()
	{
		if (spriteDefaultMat == null)
		{
			Shader shader = Shader.Find("Sprites/Default");
			if (shader == null)
			{
				Debug.LogError("Не найден шейдер 'Sprites/Default'");
				return null;
			}
			spriteDefaultMat = new Material(shader)
			{
				name = "Runtime_SpritesDefault"
			};
		}
		return spriteDefaultMat;
	}

	private void CreateWantedObject()
	{
		wantedObject = new GameObject("WantedObject");
		SpriteRenderer spriteRenderer = wantedObject.AddComponent<SpriteRenderer>();
		Sprite sprite = Resources.Load<Sprite>("Sprites/VibratorCloud");
		if (sprite == null)
		{
			Debug.LogError("Sprite 'Sprites/VibratorCloud' not found in Resources.");
			return;
		}
		spriteRenderer.sprite = sprite;
		spriteRenderer.sortingLayerName = "Foreground";
		spriteRenderer.sortingOrder = 80;
		spriteRenderer.sharedMaterial = GetSpriteDefaultMat();
		wantedObject.transform.SetParent(base.transform);
		wantedObject.transform.localPosition = new Vector3(-20f, 150f, 0f);
		wantedObject.SetActive(value: false);
	}

	public void CreateAudioSource()
	{
		string text = "masturbation-med2";
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
		if (sFXGroup != null)
		{
			audioSource.outputAudioMixerGroup = sFXGroup;
		}
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/" + text);
		if (audioClip == null)
		{
			Debug.LogWarning("[Audio] Клип '" + text + "' не найден в Resources/Audio!");
			return;
		}
		audioSource.clip = audioClip;
		audioSource.volume = 0.4f;
		audioSource.spatialBlend = 1f;
		audioSource.loop = true;
		AudioMixer audioMixer = SoundManager.GetInstance().GetAudioMixer();
		if (audioMixer != null)
		{
			AudioMixerGroup[] array = audioMixer.FindMatchingGroups("SFX");
			if (array.Length != 0)
			{
				audioSource.outputAudioMixerGroup = array[0];
			}
			else
			{
				Debug.LogWarning("[Audio] Группа 'SFX' не найдена в миксере!");
			}
		}
		else
		{
			Debug.LogWarning("[Audio] AudioMixer не найден!");
		}
		audioSource.dopplerLevel = 0f;
		audioSource.minDistance = 100f;
		audioSource.maxDistance = 400f;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.Play();
	}

	public void DestroyAudioSource()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (component != null)
		{
			component.Stop();
			Object.Destroy(component);
		}
		else
		{
			Debug.LogWarning("[Audio] AudioSource не найден на объекте!");
		}
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

	public void Initialize()
	{
		Collider component = GetComponent<Collider>();
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			spriteRenderer = base.gameObject.AddComponent<SpriteRenderer>();
		}
		spriteRenderer.sortingLayerName = "Entity";
		BoxCollider boxCollider;
		if (component == null)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		else
		{
			boxCollider = component as BoxCollider;
			if (boxCollider == null)
			{
				Object.Destroy(component);
				boxCollider = base.gameObject.AddComponent<BoxCollider>();
			}
		}
		if (boxCollider != null)
		{
			boxCollider.isTrigger = true;
			boxCollider.size = new Vector3(90f, 180f, 200f);
		}
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			animator = base.gameObject.AddComponent<Animator>();
			animator.runtimeAnimatorController = animatorController;
		}
		CreateShadowObject();
		CreateWantedObject();
	}

	public void DestroyInstances()
	{
		if (base.MaskPrefabInstance != null)
		{
			Object.Destroy(base.MaskPrefabInstance);
		}
		if (base.LightPrefabInstance != null)
		{
			Object.Destroy(base.LightPrefabInstance);
		}
	}

	public void SetTargetRace(GameObject objectivePoint)
	{
		if (this == null || base.gameObject == null || !this)
		{
			return;
		}
		chaseStack.Clear();
		if (targetChase != null && CurrentState is NintendoHideState && (bool)targetChase.GetComponent<HideThing>())
		{
			Debug.Log(base.gameObject.name + " Exit From: " + targetChase.name);
			targetChase.GetComponent<HideThing>().ForceExit();
			ChangeState(new NintendoChaseState());
		}
		else if (objectivePoint != null && mainGoal != null && objectivePoint != mainGoal)
		{
			ChangeState(new NintendoChaseState());
		}
		mainGoal = objectivePoint;
		Debug.Log("MainGoal for: " + base.gameObject.name + " Is: " + mainGoal.name);
		chaseStack.Push(mainGoal);
		if (mainGoal == null)
		{
			SetChaseTarget(null);
			return;
		}
		GameObject gameObject = HospitalManager.GetInstance().GetRoomContainingObject(mainGoal);
		if (gameObject == null)
		{
			StaticObjectRoom component = mainGoal.GetComponent<StaticObjectRoom>();
			if (component != null)
			{
				gameObject = component.roomName;
			}
		}
		if (gameObject != null && base.CurrentRoomObject != null)
		{
			Debug.Log("Current Room: " + base.CurrentRoomObject.name + ", Target Room: " + gameObject.name);
		}
		mainGoalRoomName = gameObject.name;
		if (base.CurrentRoomObject.name == gameObject.name)
		{
			SetChaseTarget(chaseStack.Pop());
			return;
		}
		Stack<GameObject> stack = HospitalManager.GetInstance().FindShortestPath(base.CurrentRoomObject, gameObject, base.gameObject);
		if (stack.Count > 0)
		{
			while (stack.Count > 0)
			{
				chaseStack.Push(stack.Pop());
			}
			CheckStack();
			GameObject gameObject2 = chaseStack.Pop();
			base.InteractObject = HospitalManager.GetInstance().GetInteractionObjectCache(gameObject2);
			SetChaseTarget(gameObject2);
		}
		else
		{
			Debug.LogWarning("No path found to the target room.");
		}
	}

	public bool TargetIsPlayer()
	{
		return ObjectIsPlayer(targetChase);
	}

	public bool ObjectIsPlayer(GameObject value)
	{
		if (value != null)
		{
			return value == Player.GetInstance().gameObject;
		}
		return false;
	}

	public bool PlayerIsFucked()
	{
		if (Player.GetInstance().gameObject.activeSelf)
		{
			return Player.GetInstance().IsCaught;
		}
		return true;
	}

	public bool CheckPlayerAttention()
	{
		Player player = Player.GetInstance();
		float num = player.transform.position.x - base.transform.position.x;
		bool num2 = num > 0f && num < 150f && player.FaceRight;
		bool flag = num < 0f && num > -150f && !player.FaceRight;
		if (num2 | flag)
		{
			return !player.IsMoving;
		}
		return false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		fadeCoroutine = null;
		instance = null;
	}

	private void CheckStack()
	{
		GameObject[] array = chaseStack.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Debug.Log(array[i].name);
		}
	}

	private void Update()
	{
		if (currentState != null)
		{
			currentState.UpdateState(this);
		}
		CheckReachedMainTarget();
	}

	public void CheckReachedMainTarget()
	{
		if (mainGoal != null && !isReachedMainGoal && !(CurrentState is NintendoShowRoflState) && Mathf.Abs(mainGoal.transform.position.x - base.transform.position.x) <= chaseDistance && mainGoalRoomName == currentRoom && !(Player.GetInstance().CurrentState is PlayerHideState))
		{
			if (TargetIsPlayer())
			{
				ChangeState(new NintendoWithPlayerIdleState());
			}
			Debug.Log("Main Target is Reached");
			isReachedMainGoal = true;
		}
	}

	public void ShowWantedItem()
	{
		if (wantedObject != null)
		{
			wantedObject.SetActive(value: true);
		}
	}

	public void HideWantedItem()
	{
		if (wantedObject != null)
		{
			wantedObject.SetActive(value: false);
		}
	}

	public void EndSexMode()
	{
		if (Player.GetInstance().CurrentHeartNumber > 1)
		{
			blinkManager.FadeToBlink(Player.GetInstance().gameObject, ExitSexMode);
		}
		else
		{
			ExitSexMode();
		}
	}

	public void CheckForHint()
	{
		string pendingHint = GetPendingHint();
		if (!string.IsNullOrEmpty(pendingHint))
		{
			ShowHint(pendingHint);
		}
	}

	private string GetPendingHint()
	{
		foreach (KeyValuePair<string, string> item in hintSpriteMap)
		{
			string key = item.Key.Replace("Ready", "");
			if (GlobalSaveSystem.GetProgress(item.Key) && !GlobalSaveSystem.GetProgress(key))
			{
				return item.Key;
			}
		}
		return null;
	}

	private void ShowHint(string readyKey)
	{
		GameObject gameObject = GameObject.FindWithTag("NintendoHint");
		if (gameObject == null)
		{
			Debug.LogError("NintendoHint object with tag not found.");
			return;
		}
		Time.timeScale = 0f;
		if (!hintSpriteMap.TryGetValue(readyKey, out var value))
		{
			Debug.LogError("No sprite mapping found for " + readyKey);
			return;
		}
		string currentLanguage = GameSettings.CurrentLanguage;
		string text = currentLanguage + "_" + value;
		string text2 = "Sprites/NinHints/" + currentLanguage + "/" + text;
		Sprite sprite = Resources.Load<Sprite>(text2);
		if (sprite == null)
		{
			Debug.LogError("Sprite not found at path " + text2);
			return;
		}
		Transform transform = gameObject.transform.Find("HintImage");
		if (transform == null)
		{
			Debug.LogError("HintImage child not found inside NintendoHint.");
			return;
		}
		Image component = transform.GetComponent<Image>();
		if (component != null)
		{
			component.sprite = sprite;
		}
		CanvasGroup component2 = gameObject.GetComponent<CanvasGroup>();
		if (component2 != null)
		{
			gameObject.SetActive(value: true);
			StartCoroutine(FadeInHint(component2));
		}
		HintUIController component3 = gameObject.GetComponent<HintUIController>();
		if (component3 != null)
		{
			component3.SetHintShown(value: true);
		}
		else
		{
			Debug.LogError("hintUIController not Set!");
		}
		SoundManager.PlaySound(SoundType.NinHint);
		GlobalSaveSystem.SetProgress(readyKey.Replace("Ready", ""), value: true);
		GlobalSaveSystem.SetProgress(readyKey, value: false);
	}

	private IEnumerator FadeInHint(CanvasGroup canvasGroup)
	{
		float duration = 0.5f;
		float time = 0f;
		canvasGroup.alpha = 0f;
		while (time < duration)
		{
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
			time += Time.unscaledDeltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	public void UltraHunt()
	{
		Enemy[] array = Object.FindObjectsOfType<Enemy>();
		foreach (Enemy enemy in array)
		{
			if (enemy is SisEnemy || enemy is HoodieCatEnemy || enemy is VampireEnemy)
			{
				Debug.Log("enemy " + enemy.name + " is Chase From Nintendo");
				enemy.HunterInstinctTimer = Random.Range(enemy.HunterInstinctTime.x * 2f, enemy.HunterInstinctTime.y * 2f);
				enemy.ChaseTimer = enemy.ChaseTime * 3f;
				enemy.ChangeState(new ChaseState());
			}
		}
	}

	public void InterraptRoflEvent()
	{
		if (roflEventCoroutine != null)
		{
			StopCoroutine(roflEventCoroutine);
			roflEventCoroutine = null;
		}
	}

	public void StartLiftAnim()
	{
		StartCoroutine(LiftAnim());
	}

	private IEnumerator LiftAnim()
	{
		eventTriggered = true;
		yield return new WaitForSeconds(0.5f);
		ChangeState(new NintendoWithPlayerIdleState());
		InitializeCurrentRoom();
		AlignToRoomLine();
		Elevator.ClearTrigger();
		yield return null;
		eventTriggered = false;
	}

	public void RoflRunAwayBoost()
	{
		ChangeState(new NintendoChaseState());
		isPreparedForRofl = false;
		AddSpeedModifier("RunAwayBoost", 1.5f);
	}

	public void HandleTimers()
	{
		if (InteractionBoostTimer > 0f)
		{
			InteractionBoostTimer -= Time.deltaTime;
			InteractionBoostTimer = Mathf.Max(InteractionBoostTimer, 0f);
			if (InteractionBoostTimer == 0f)
			{
				RemoveSpeedModifier("RunAwayBoost");
			}
		}
		if (roflTimer > 0f)
		{
			roflTimer -= Time.deltaTime;
			roflTimer = Mathf.Max(roflTimer, 0f);
		}
		if (showRoflTimer > 0f)
		{
			showRoflTimer -= Time.deltaTime;
			showRoflTimer = Mathf.Max(showRoflTimer, 0f);
		}
	}

	public void HandleInteraction()
	{
		HideThing hideThing = null;
		if (interactObject != null)
		{
			hideThing = interactObject.GetComponent<HideThing>();
		}
		if (hideThing != null && hideThing.HiddenObject != null && hideThing.HiddenObject.GetComponent<Player>() != null)
		{
			Debug.Log("Nintendo Go Away Event");
			guardCoroutine = StartCoroutine(GetAwayEvent(0.5f));
			ChangeState(new NintendoFalseGuardState());
		}
		else if (base.CanInteract)
		{
			interactObject.Interact(base.gameObject);
		}
	}

	public void SetChaseTarget(GameObject newTarget)
	{
		targetChase = newTarget;
		HandleChaseDistance();
	}

	private void HandleChaseDistance()
	{
		if (targetChase.GetComponent<Player>() != null)
		{
			chaseDistance = chaseDistanceDefault;
		}
		else
		{
			chaseDistance = 0f;
		}
	}

	public void WrongDoor()
	{
		BlinkManager.GetInstance().BlinkWithCustomSpeed(2f, base.gameObject, StartSex, allowVerticalMovement: false, 0f, 0f, targetChase);
	}

	public void WrongDoorReverse()
	{
		WrongDoor();
	}

	private void StartSex()
	{
		ChangeState(new NintendoHaveFunState());
	}

	protected override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);
		if (currentState != null)
		{
			currentState.EnterTriggerState(this, other);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (currentState != null)
		{
			currentState.StayTriggerState(this, other);
		}
	}

	public void CheckTriggerForBetrayl(GameObject gameObject)
	{
		Enemy component = gameObject.GetComponent<Enemy>();
		if (component != null && CurrentState is NintendoFalseGuardState && guardPositionIsTaken)
		{
			StartCoroutine(roflGuardMoment(component));
		}
	}

	public void CheckTriggerForInteract(GameObject gameObject)
	{
		if (gameObject == targetChase && HospitalManager.GetInstance().GetInteractionObjectCache(targetChase) != null)
		{
			Debug.Log($"Trigger Object is: {gameObject}");
			interactObject = HospitalManager.GetInstance().GetInteractionObjectCache(targetChase);
			ChangeState(new NintendoInteractState());
			chaseDirection = 0f;
		}
	}

	public void CheckStayTriggerForInteract(GameObject gameObject)
	{
		if (interactObject == null && gameObject == targetChase && HospitalManager.GetInstance().GetInteractionObjectCache(targetChase) != null)
		{
			Debug.Log($"Trigger Object is: {gameObject}");
			interactObject = HospitalManager.GetInstance().GetInteractionObjectCache(targetChase);
			ChangeState(new NintendoInteractState());
			chaseDirection = 0f;
		}
	}

	private IEnumerator roflGuardMoment(Enemy enemyScript)
	{
		animator.Play("RoflGuardMoment");
		enemyScript.ChangeState(new DetectionState());
		while (!IsAnimationFinished("RoflGuardMomentLaugh"))
		{
			yield return null;
		}
		if (guardCoroutine != null)
		{
			StopCoroutine(guardCoroutine);
			guardCoroutine = null;
		}
		RoflRunAwayBoost();
		HideAway();
	}

	public void HideAway()
	{
		guardCoroutine = null;
		HideThing[] array = Object.FindObjectsOfType<HideThing>();
		if (array.Length != 0)
		{
			array = array.OrderBy((HideThing _) => Random.value).ToArray();
			HideThing hideThing = null;
			HideThing[] array2 = array;
			foreach (HideThing hideThing2 in array2)
			{
				GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(hideThing2.gameObject);
				if (roomContainingObject != null && roomContainingObject.name != currentRoom)
				{
					hideThing = hideThing2;
					Debug.Log("Nintendo hide in: " + hideThing.name);
					break;
				}
			}
			if (hideThing != null)
			{
				SetTargetRace(hideThing.gameObject);
				guardPositionIsTaken = false;
			}
			else
			{
				Debug.LogWarning("No suitable object found in other rooms.");
			}
		}
		else
		{
			Debug.LogWarning("No objects of type HideThing found!");
		}
	}

	private bool IsAnimationFinished(string name, float procent = 1f)
	{
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName(name))
		{
			if (currentAnimatorStateInfo.normalizedTime >= procent)
			{
				return !animator.IsInTransition(0);
			}
			return false;
		}
		return false;
	}

	protected override void OnTriggerExit(Collider other)
	{
		if (!eventTriggered)
		{
			base.OnTriggerExit(other);
			if (currentState != null)
			{
				currentState.ExitTriggerState(this, other);
			}
		}
	}

	public bool PlayerIsObjective()
	{
		return mainGoal == Player.GetInstance().gameObject;
	}

	private GameObject FindDoorToStage(GameObject target)
	{
		GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(target);
		if (roomContainingObject == null)
		{
			return null;
		}
		Vector3 vector = CalculateRoomBounds(roomContainingObject);
		Collider[] array = Physics.OverlapBox(CalculateRoomCenter(roomContainingObject), vector / 2f);
		List<GameObject> list = new List<GameObject>();
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			DoorTransit component = array2[i].GetComponent<DoorTransit>();
			if (component != null)
			{
				list.Add(component.gameObject);
			}
		}
		if (list.Count != 0)
		{
			float num = float.MaxValue;
			GameObject gameObject = null;
			foreach (GameObject item in list)
			{
				float num2 = Vector3.Distance(base.transform.position, item.transform.position);
				if (num2 < num)
				{
					num = num2;
					gameObject = item;
				}
			}
			if (gameObject != null)
			{
				return gameObject;
			}
			Debug.LogError("No valid door found.");
			return null;
		}
		Debug.LogError("ДВЕРЬ НЕ НАЙДЕНА");
		return null;
	}

	public override void InitializeCurrentRoom()
	{
		base.InitializeCurrentRoom();
		if (mainGoal != null && !(currentState is NintendoHideState))
		{
			SetTargetRace(mainGoal);
		}
	}

	protected override void HandleCurrentRoom(Collider other)
	{
		base.HandleCurrentRoom(other);
		if (mainGoal != null && !(currentState is NintendoHideState))
		{
			SetTargetRace(mainGoal);
		}
	}

	public void Chase()
	{
		if (!(targetChase == null))
		{
			Player player = ((mainGoal == Player.GetInstance().gameObject) ? Player.GetInstance() : null);
			if (player != null && mainGoalRoomName != player.CurrentRoom)
			{
				SetTargetRace(mainGoal);
			}
			Vector3 position = targetChase.transform.position;
			Vector3 position2 = base.transform.position;
			float num = Mathf.Abs(position.x - position2.x);
			if (TargetIsPlayer() && num < chaseDistance * 2f)
			{
				CurrentMoveSpeed = player.BaseMoveSpeed;
			}
			else
			{
				CurrentMoveSpeed = base.BaseMoveSpeed;
			}
			if (num < chaseDistance || (SisSupEnemy.IsTrapped && chaseStack.Count <= 2))
			{
				chaseDirection = 0f;
			}
			else
			{
				chaseDirection = ((num >= StageManager.chunkWidth * 6f && currentRoom == "Stage3") ? (0f - Mathf.Sign(position.x - position2.x)) : Mathf.Sign(position.x - position2.x));
			}
		}
	}

	public void ChangeState(INintendoState newState)
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

	public void StartRoflEvent()
	{
		roflEventCoroutine = StartCoroutine(RoflEvent(0.5f));
	}

	public void StartFalseGuardEvent()
	{
		guardCoroutine = StartCoroutine(FalseGuardEvent(0.5f));
	}

	private IEnumerator GetAwayEvent(float delay)
	{
		float falseGuardPosition = GetFalseGuardPosition();
		while (!guardPositionIsTaken)
		{
			MoveToGuardPosition(falseGuardPosition);
			yield return null;
		}
		base.FaceRight = !base.FaceRight;
		chaseDirection = 0f;
		animator.Play("GetAwayMoment");
		GameObject original = Resources.Load<GameObject>("prefabs/UI/agreesiveUI");
		GameObject agreesiveEmoji = Object.Instantiate(original, base.transform.position + new Vector3(30f, 166f, 0f), Quaternion.identity);
		agreesiveEmoji.transform.SetParent(base.transform);
		agreesiveEmoji.transform.localPosition = new Vector3(30f, 166f, 0f);
		guardPositionIsTaken = false;
		while (Player.GetInstance().CurrentState is PlayerHideState && !IsAnimationFinished("GetAwayMoment"))
		{
			chaseDirection = 0f;
			yield return null;
		}
		Object.Destroy(agreesiveEmoji);
		while (!guardPositionIsTaken)
		{
			MoveToGuardPosition(targetChase.transform.position.x);
			yield return null;
		}
		if (Player.GetInstance().CurrentState is PlayerHideState)
		{
			WrongDoorReverse();
			yield break;
		}
		guardCoroutine = null;
		ChangeState(new NintendoInteractState());
	}

	public override void EnterHideState()
	{
		ChangeState(new NintendoHideState());
	}

	public override void ExitHideState()
	{
		ChangeState(new NintendoChaseState());
	}

	private IEnumerator FalseGuardEvent(float delay)
	{
		float falseGuardPosition = GetFalseGuardPosition();
		while (!guardPositionIsTaken)
		{
			MoveToGuardPosition(falseGuardPosition);
			yield return null;
		}
		UltraHunt();
		float timer = 0f;
		while (Player.GetInstance().CurrentState is PlayerHideState || CurrentState is NintendoHaveFunState)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			timer += Time.deltaTime;
			if (timer >= 15f && !currentAnimatorStateInfo.IsName("RoflGuardMoment"))
			{
				UltraHunt();
				timer = 0f;
			}
			yield return null;
		}
		if (Player.GetInstance().gameObject.activeSelf)
		{
			guardPositionIsTaken = false;
			ChangeState(new NintendoChaseState());
		}
		else
		{
			HideAway();
		}
		guardCoroutine = null;
	}

	private float GetFalseGuardPosition()
	{
		if (targetChase == null)
		{
			Debug.LogError("targetChase in NULL");
			return 0f;
		}
		HospitalManager hospitalManager = HospitalManager.GetInstance();
		GameObject roomContainingObject = hospitalManager.GetRoomContainingObject(base.gameObject);
		if (hospitalManager.CountDoorTransitsInCurrentRoom(roomContainingObject) >= 2)
		{
			return targetChase.transform.position.x - 100f;
		}
		float num = FindDoorToStage(base.gameObject).transform.position.x - base.transform.position.x;
		flipDirectionIsRight = num > 0f;
		if (!flipDirectionIsRight)
		{
			return targetChase.transform.position.x - 100f;
		}
		return targetChase.transform.position.x + 100f;
	}

	private void MoveToGuardPosition(float falseGuardPosition)
	{
		if (targetChase == null)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (Mathf.Abs(falseGuardPosition - position.x) <= 20f)
		{
			base.FaceRight = flipDirectionIsRight;
			animator.Play("Idle");
			chaseDirection = 0f;
			guardPositionIsTaken = true;
		}
		else
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
			{
				animator.Play("Run");
			}
			chaseDirection = Mathf.Sign(falseGuardPosition - position.x);
		}
	}

	private IEnumerator RoflEvent(float delay)
	{
		showRoflTimer = showRoflCooldown;
		float distance = Player.GetInstance().transform.position.x - base.transform.position.x;
		if (CheckPlayerAttention())
		{
			animator.Play("Idle");
			chaseDirection = 0f;
			base.FaceRight = Player.GetInstance().FaceRight;
			yield return new WaitForSeconds(delay * 2f);
			if ((distance < 50f && !Player.GetInstance().FaceRight) || (distance > 50f && Player.GetInstance().FaceRight))
			{
				base.FaceRight = Player.GetInstance().FaceRight;
				yield return new WaitForSeconds(delay * 2f);
				if ((distance < 50f && !Player.GetInstance().FaceRight) || (distance > 50f && Player.GetInstance().FaceRight))
				{
					isPreparedForRofl = true;
					animator.Play("ShowRofl");
					SoundManager.PlaySoundAtObject(SoundType.NinCloth, base.gameObject);
					yield return new WaitForSeconds(delay * 8f);
					RoflRunAwayBoost();
				}
				else
				{
					yield return new WaitForSeconds(delay * 2f);
					if (!TargetIsPlayer())
					{
						ChangeState(new NintendoChaseState());
					}
				}
			}
			else
			{
				yield return new WaitForSeconds(delay * 2f);
				if (!TargetIsPlayer())
				{
					ChangeState(new NintendoChaseState());
				}
			}
		}
		else
		{
			ChangeState(new NintendoChaseState());
		}
	}

	public void AnimationLoopFinished()
	{
		loopCount++;
		animator.SetInteger("HloopCount", loopCount);
	}

	public virtual void ExitSexMode()
	{
		EndEvent.GetInstance().UnblockSkip();
		GetAnimator().SetInteger("HloopCount", 0);
		LoopCount = 0;
		Player.GetInstance().CurrentHeartNumber--;
		if (Player.GetInstance().CurrentHeartNumber <= 0)
		{
			GetAnimator().Play("FunMode");
		}
		else
		{
			HideAway();
		}
	}

	private void ChangePosition()
	{
		targetChase = null;
		ChangeState(new NintendoIdleState());
	}

	protected override float GetDirection()
	{
		return chaseDirection;
	}

	public void EnableFastWWEffect()
	{
		currentWWpath = "prefabs/WierdWordEffects/WWFuckFast";
	}

	public void ResetWWEffect()
	{
		currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";
	}

	private void CreateEffect(string path, Vector3? position = null)
	{
		GameObject gameObject = Resources.Load<GameObject>(path);
		if (gameObject != null)
		{
			Object.Instantiate(gameObject, position ?? base.transform.position, Quaternion.identity, base.transform).name = gameObject.name;
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
				Object.Destroy(item.gameObject);
				return;
			}
		}
		Debug.LogWarning("Child with name '" + childName + "' not found.");
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
				Vector3 vector = Random.insideUnitSphere.normalized * 50f;
				vector.y = 0f;
				Vector3 position = base.transform.position + vector;
				Object.Instantiate(gameObject, position, Quaternion.identity, base.transform);
				counterForWierdWord = Random.Range(1, 4);
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

	public static void HideNintendo()
	{
		if (instance == null)
		{
			Debug.LogWarning("HideNintendo: instance is null");
			return;
		}
		if (fadeCoroutine != null)
		{
			Debug.Log("HideNintendo: stopping existing fade coroutine");
			instance.StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = instance.StartCoroutine(FadeOutNintendo());
		Debug.Log("HideNintendo: started FadeOutNintendo coroutine");
	}

	public static void ShowNintendo()
	{
		if (instance == null)
		{
			Debug.LogWarning("ShowNintendo: instance is null");
			return;
		}
		if (fadeCoroutine != null)
		{
			Debug.Log("ShowNintendo: stopping existing fade coroutine");
			instance.StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = instance.StartCoroutine(FadeInNintendo());
		Debug.Log("ShowNintendo: started FadeInNintendo coroutine");
	}

	private static IEnumerator FadeOutNintendo()
	{
		Debug.Log("FadeOutNintendo: started");
		float duration = 1f;
		float elapsedTime = 0f;
		SpriteRenderer mainSr = instance.GetComponent<SpriteRenderer>();
		Transform child = ((instance.transform.childCount > 0) ? instance.transform.GetChild(0) : null);
		SpriteRenderer childSr = child?.GetComponent<SpriteRenderer>();
		Transform entityShadow = instance.transform.Find("ShadowObject");
		SpriteRenderer shadowSr = entityShadow?.GetComponent<SpriteRenderer>();
		Debug.Log($"FadeOutNintendo: mainSr = {mainSr != null}, child = {child != null}, childSr = {childSr != null}, entityShadow = {entityShadow != null}, shadowSr = {shadowSr != null}");
		if (mainSr == null)
		{
			Debug.LogWarning("FadeOutNintendo: mainSr is null, fading may not work");
		}
		float startMainAlpha = ((mainSr != null) ? 1f : 0f);
		float num = ((childSr != null) ? 1f : 0f);
		float num2 = ((shadowSr != null) ? 0.35f : 0f);
		Debug.Log($"FadeOutNintendo: start alphas - main:{startMainAlpha}, child:{num}, shadow:{num2}");
		Animator component = instance.GetComponent<Animator>();
		if (component != null)
		{
			PlaySoundLoopStereo behaviour = component.GetBehaviour<PlaySoundLoopStereo>();
			if (behaviour != null)
			{
				behaviour.isActive = false;
				Debug.Log("FadeOutNintendo: disabled sound behaviour");
			}
		}
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float a = Mathf.Lerp(startMainAlpha, 0f, elapsedTime / duration);
			if (mainSr != null)
			{
				mainSr.color = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, a);
			}
			if (childSr != null)
			{
				childSr.color = new Color(childSr.color.r, childSr.color.g, childSr.color.b, a);
			}
			if (shadowSr != null)
			{
				shadowSr.color = new Color(shadowSr.color.r, shadowSr.color.g, shadowSr.color.b, a);
			}
			yield return null;
		}
		if (mainSr != null)
		{
			mainSr.color = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, 0f);
		}
		if (childSr != null)
		{
			childSr.color = new Color(childSr.color.r, childSr.color.g, childSr.color.b, 0f);
		}
		if (shadowSr != null)
		{
			shadowSr.color = new Color(shadowSr.color.r, shadowSr.color.g, shadowSr.color.b, 0f);
		}
		if (child != null)
		{
			child.gameObject.SetActive(value: false);
			Debug.Log("FadeOutNintendo: disabled child GameObject");
		}
		if (entityShadow != null)
		{
			entityShadow.gameObject.SetActive(value: false);
			Debug.Log("FadeOutNintendo: disabled shadow GameObject");
		}
		fadeCoroutine = null;
		Debug.Log("FadeOutNintendo: completed");
	}

	private static IEnumerator FadeInNintendo()
	{
		Debug.Log("FadeInNintendo: started");
		float duration = 1f;
		float elapsedTime = 0f;
		SpriteRenderer mainSr = instance.GetComponent<SpriteRenderer>();
		Transform transform = ((instance.transform.childCount > 0) ? instance.transform.GetChild(0) : null);
		SpriteRenderer childSr = transform?.GetComponent<SpriteRenderer>();
		Transform transform2 = instance.transform.Find("ShadowObject");
		SpriteRenderer shadowSr = transform2?.GetComponent<SpriteRenderer>();
		Debug.Log($"FadeInNintendo: mainSr = {mainSr != null}, child = {transform != null}, childSr = {childSr != null}, entityShadow = {transform2 != null}, shadowSr = {shadowSr != null}");
		if (mainSr == null)
		{
			Debug.LogWarning("FadeInNintendo: mainSr is null, cannot fade in");
			yield break;
		}
		float startAlpha = 0f;
		float targetAlpha = 1f;
		Debug.Log($"FadeInNintendo: startAlpha = {startAlpha}, targetAlpha = {targetAlpha}");
		if (Mathf.Approximately(startAlpha, targetAlpha))
		{
			Debug.Log("FadeInNintendo: already fully visible, skipping alpha animation");
			Animator component = instance.GetComponent<Animator>();
			if (component != null)
			{
				PlaySoundLoopStereo behaviour = component.GetBehaviour<PlaySoundLoopStereo>();
				if (behaviour != null)
				{
					behaviour.isActive = true;
					Debug.Log("FadeInNintendo: enabled sound behaviour (skip)");
				}
			}
			fadeCoroutine = null;
			yield break;
		}
		if (transform != null)
		{
			if (!transform.gameObject.activeSelf)
			{
				transform.gameObject.SetActive(value: true);
				Debug.Log("FadeInNintendo: activated child GameObject");
			}
			if (childSr != null)
			{
				childSr.color = new Color(childSr.color.r, childSr.color.g, childSr.color.b, startAlpha);
			}
		}
		if (transform2 != null)
		{
			if (!transform2.gameObject.activeSelf)
			{
				transform2.gameObject.SetActive(value: true);
				Debug.Log("FadeInNintendo: activated shadow GameObject");
			}
			if (shadowSr != null)
			{
				shadowSr.color = new Color(shadowSr.color.r, shadowSr.color.g, shadowSr.color.b, startAlpha);
			}
		}
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration;
			float a = Mathf.Lerp(startAlpha, targetAlpha, t);
			mainSr.color = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, a);
			if (childSr != null)
			{
				childSr.color = new Color(childSr.color.r, childSr.color.g, childSr.color.b, a);
			}
			if (shadowSr != null)
			{
				shadowSr.color = new Color(shadowSr.color.r, shadowSr.color.g, shadowSr.color.b, a);
			}
			yield return null;
		}
		mainSr.color = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, targetAlpha);
		if (childSr != null)
		{
			childSr.color = new Color(childSr.color.r, childSr.color.g, childSr.color.b, targetAlpha);
		}
		if (shadowSr != null)
		{
			shadowSr.color = new Color(shadowSr.color.r, shadowSr.color.g, shadowSr.color.b, targetAlpha);
		}
		Animator component2 = instance.GetComponent<Animator>();
		if (component2 != null)
		{
			PlaySoundLoopStereo behaviour2 = component2.GetBehaviour<PlaySoundLoopStereo>();
			if (behaviour2 != null)
			{
				behaviour2.isActive = true;
				Debug.Log("FadeInNintendo: enabled sound behaviour");
			}
		}
		fadeCoroutine = null;
		Debug.Log("FadeInNintendo: completed");
	}

	public override AudioClip GetMovementSound()
	{
		if (CurrentState is NintendoHideState || CurrentState is NintendoIdleState || CurrentState is NintendoInteractState)
		{
			return null;
		}
		string[] array = new string[4] { "NinFootSteps 1", "NinFootSteps 2", "NinFootSteps 3", "NinFootSteps 4" };
		int num = Random.Range(0, array.Length);
		string text = array[num];
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/" + text);
		if (audioClip == null)
		{
			Debug.LogWarning("[Audio] Клип '" + text + "' не найден в Resources/Audio!");
			return null;
		}
		return audioClip;
	}
}
