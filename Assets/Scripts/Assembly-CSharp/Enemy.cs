using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Entities
{
	public float resistanceTime = 2f;

	public BaseEnemySettings Settings;

	private bool isChaseMode;

	protected Transform player;

	[SerializeField]
	private float viewDistance = 200f;

	[SerializeField]
	private float hearDistance = 200f;

	protected AudioSource source;

	private IEnemyState currentState;

	public const float TimeToLostAnim = 2.5f;

	[SerializeField]
	private float chaseTimer;

	private float chaseTime = 20f;

	private float direction;

	private float previousDirection;

	private int consecutiveDirectionCount;

	[Header("Roam Direction Change Chance")]
	[SerializeField]
	private float directionChangeProbability = 0.2f;

	private float chaseDistanceDefault;

	private GameObject mainGoal;

	protected float chaseDirection;

	[SerializeField]
	private float hunterInstinctCooldown = 30f;

	[SerializeField]
	private float hunterInstinctCooldownTimer;

	[SerializeField]
	private Vector2 hunterInstinctTime = new Vector2(10f, 15f);

	[SerializeField]
	private float hunterInstinctTimer;

	private float transitionTimer;

	private float searchingTimer;

	private float changeDirInChaseTimer;

	private bool itKnow;

	private string mainGoalRoomName;

	[SerializeField]
	private bool isAngry;

	protected bool isCatch;

	private bool isIdle;

	private float idleTime = 3f;

	private float idleTimer;

	private DoorTransit doorToTrigger;

	protected int loopCount;

	[SerializeField]
	protected GameObject targetChase;

	private Stack<GameObject> chaseStack = new Stack<GameObject>();

	protected string currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";

	protected string previousWWpath;

	protected int counterForBreathe;

	protected int counterForWierdWord;

	public float ViewDistance
	{
		get
		{
			return viewDistance;
		}
		set
		{
			viewDistance = value;
		}
	}

	public float HearDistance
	{
		get
		{
			return hearDistance;
		}
		set
		{
			hearDistance = value;
		}
	}

	public float TimerToLostAnim { get; set; }

	public bool IsLostBoy { get; set; }

	public float ChaseTimer
	{
		get
		{
			return chaseTimer;
		}
		set
		{
			chaseTimer = value;
		}
	}

	public float ChaseTime
	{
		get
		{
			return chaseTime;
		}
		set
		{
			chaseTime = value;
		}
	}

	public IEnemyState CurrentState
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

	public GameObject MainGoal => mainGoal;

	public float ChaseDirection
	{
		set
		{
			chaseDirection = value;
		}
	}

	public float HunterInstinctCooldown
	{
		get
		{
			return hunterInstinctCooldown;
		}
		set
		{
			hunterInstinctCooldown = value;
		}
	}

	public float HunterInstinctCooldownTimer
	{
		get
		{
			return hunterInstinctCooldownTimer;
		}
		set
		{
			hunterInstinctCooldownTimer = value;
		}
	}

	public Vector2 HunterInstinctTime
	{
		get
		{
			return hunterInstinctTime;
		}
		set
		{
			hunterInstinctTime = value;
		}
	}

	public float HunterInstinctTimer
	{
		get
		{
			return hunterInstinctTimer;
		}
		set
		{
			hunterInstinctTimer = value;
		}
	}

	public string MainGoalRoomName => mainGoalRoomName;

	public bool IsAngry
	{
		get
		{
			return isAngry;
		}
		set
		{
			isAngry = value;
		}
	}

	public bool ItKnow
	{
		get
		{
			return itKnow;
		}
		set
		{
			itKnow = value;
		}
	}

	public bool IsCatch => isCatch;

	public float IdleTime => idleTime;

	public float IdleTimer
	{
		get
		{
			return idleTimer;
		}
		set
		{
			idleTimer = value;
		}
	}

	public bool IsIdle
	{
		get
		{
			return isIdle;
		}
		set
		{
			isIdle = value;
		}
	}

	public GameObject TargetChase
	{
		get
		{
			return targetChase;
		}
		set
		{
			SetTargetRace(value);
		}
	}

	public Stack<GameObject> ChaseStack
	{
		get
		{
			return chaseStack;
		}
		set
		{
			chaseStack = value;
			Debug.Log("rewrite Stack");
		}
	}

	public bool IsChaseMode
	{
		get
		{
			return isChaseMode;
		}
		set
		{
			isChaseMode = value;
		}
	}

	protected virtual string BEpath => "";

	protected virtual string WWpath => currentWWpath;

	public void UpdateLostTimer()
	{
		TimerToLostAnim = Mathf.Max(TimerToLostAnim - Time.deltaTime, 0f);
	}

	public void HunterInstinctTimerUpdate()
	{
		if (ChaseTimer <= 0f)
		{
			HunterInstinctTimer -= Time.deltaTime;
		}
	}

	public void HunterInstinctCooldownTimerUpdate()
	{
		hunterInstinctCooldownTimer -= Time.deltaTime;
	}

	public void ChangeState(IEnemyState newState)
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

	public void SetTargetRace(GameObject objectivePoint)
	{
		chaseStack.Clear();
		mainGoal = objectivePoint;
		chaseStack.Push(mainGoal);
		if (mainGoal == null)
		{
			SetChaseTarget(null);
			return;
		}
		GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(mainGoal);
		Debug.Log("Current Room: " + base.CurrentRoomObject.name + ", Target Room: " + roomContainingObject.name + " For Object Name " + base.gameObject.name);
		if (base.CurrentRoomObject.name == roomContainingObject.name)
		{
			SetChaseTarget(chaseStack.Pop());
			mainGoalRoomName = roomContainingObject.name;
			return;
		}
		mainGoalRoomName = roomContainingObject.name;
		Stack<GameObject> stack = HospitalManager.GetInstance().FindShortestPath(base.CurrentRoomObject, roomContainingObject, Player.GetInstance().gameObject);
		if (stack.Count > 0)
		{
			while (stack.Count > 0)
			{
				chaseStack.Push(stack.Pop());
			}
			CheckStack();
			GameObject gameObject = chaseStack.Pop();
			base.InteractObject = HospitalManager.GetInstance().GetInteractionObjectCache(gameObject);
			SetChaseTarget(gameObject);
		}
		else
		{
			Debug.LogWarning("No path found to the target room.");
		}
	}

	private void CheckStack()
	{
		GameObject[] array = chaseStack.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Debug.Log(array[i].name);
		}
	}

	public void SetChaseTarget(GameObject newTarget)
	{
		targetChase = newTarget;
	}

	public virtual void PlayAnimation(string stateAnim)
	{
		animator.Play(stateAnim);
	}

	protected override void Start()
	{
		base.Start();
		ChangeState(new IdleState());
		Initialize();
		ManualTriggerCheck();
	}

	protected virtual void Initialize()
	{
		counterForWierdWord = Random.Range(1, 4);
		base.gameObject.layer = LayerMask.NameToLayer("FlashLightReact");
		InitializeCurrentRoom();
		player = Player.GetInstance().transform;
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
		base.OnTriggerEnter(other);
		if (!(CurrentState is SexModeState) && !(CurrentState is StunState) && currentRoom != null)
		{
			if (other.gameObject == Player.GetInstance().gameObject && targetChase == other.gameObject)
			{
				HandleCatch();
			}
			else if (!(CurrentState is InteractState) && base.CanInteract && ((other.gameObject == targetChase && !targetChase.HasScript("Player")) || (CurrentState is RoamState && HospitalManager.GetInstance().GetDoorTransit(other.gameObject) != null && (RandomChance(0.3f) || HospitalManager.GetInstance().CountDoorTransitsInCurrentRoom(base.CurrentRoomObject) <= 2) && TimerToLostAnim <= 0f)))
			{
				base.InteractObject = HospitalManager.GetInstance().GetInteractionObjectCache(other.gameObject);
				ChangeState(new InteractState());
			}
		}
	}

	protected virtual void OnTriggerStay(Collider other)
	{
		HandleTrigger(other);
	}

	private void HandleTrigger(Collider other)
	{
		if (!(CurrentState is SexModeState) && !(CurrentState is StunState) && currentRoom != null)
		{
			if (other.gameObject == Player.GetInstance().gameObject && targetChase == other.gameObject)
			{
				HandleCatch();
			}
			else if (!(CurrentState is InteractState) && base.CanInteract && other.gameObject == targetChase && !targetChase.HasScript("Player"))
			{
				base.InteractObject = HospitalManager.GetInstance().GetInteractionObjectCache(other.gameObject);
				ChangeState(new InteractState());
			}
		}
	}

	private bool RandomChance(float value)
	{
		return value >= Random.Range(0f, 1f);
	}

	public void EnableFastWWEffect()
	{
		ChangeWWPath("prefabs/WierdWordEffects/WWFuckFast");
	}

	public void EnableSlurpWWEffect()
	{
		ChangeWWPath("prefabs/WierdWordEffects/WWFuckSlurp");
	}

	public void ResetWWEffect()
	{
		ChangeWWPath("prefabs/WierdWordEffects/WWFuckStandart");
	}

	private void ChangeWWPath(string newPath)
	{
		previousWWpath = currentWWpath;
		currentWWpath = newPath;
	}

	public void EnableRandomWWEffect()
	{
		previousWWpath = currentWWpath;
		if (currentWWpath == "prefabs/WierdWordEffects/WWFuckSlurp")
		{
			currentWWpath = previousWWpath;
		}
		else
		{
			currentWWpath = ((Random.value < 0.5f) ? "prefabs/WierdWordEffects/WWFuckSlurp" : ((currentWWpath == "prefabs/WierdWordEffects/WWFuckStandart") ? "prefabs/WierdWordEffects/WWFuckFast" : "prefabs/WierdWordEffects/WWFuckStandart"));
		}
	}

	public virtual void CreateBreatheEffect()
	{
		if (!string.IsNullOrEmpty(BEpath))
		{
			if (counterForBreathe <= 0)
			{
				CreateEffect(BEpath);
				counterForBreathe = Random.Range(1, 4);
			}
			else
			{
				counterForBreathe--;
			}
		}
	}

	public virtual void CreateWierdWordEffect()
	{
		if (string.IsNullOrEmpty(WWpath))
		{
			return;
		}
		if (counterForWierdWord <= 0)
		{
			Vector3 vector = Random.insideUnitSphere.normalized * 50f;
			Vector3 value = base.transform.position + vector;
			if (this is NunEnemy)
			{
				value.y += 50f;
			}
			CreateEffect(WWpath, value);
			counterForWierdWord = Random.Range(1, 4);
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
			GameObject obj = Object.Instantiate(gameObject, position ?? base.transform.position, Quaternion.identity, base.transform);
			obj.transform.localRotation = Quaternion.identity;
			obj.name = gameObject.name;
		}
		else
		{
			Debug.LogError("Не удалось загрузить эффект по пути: " + path);
		}
	}

	public void CreateCommonEffect(string effectName)
	{
		CreateCommonEffect(effectName, null);
	}

	public void CreateCommonEffect(string effectName, Vector3? position = null)
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
			CreateEffect(path, (base.transform.position + position) ?? base.transform.position);
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

	protected virtual void HandleCatch()
	{
		if (!isCatch && !(CurrentState is SexModeState) && Player.GetInstance().CanInteract && BlinkManager.GetInstance() != null && !BlinkManager.GetInstance().IsFadingIn)
		{
			isCatch = true;
			Player.GetInstance().BreakInteract();
			if (DialogueManager.GetInstance().dialogueIsPlaying)
			{
				StartCoroutine(DialogueManager.GetInstance().ExitDialogueMode());
			}
			StartCoroutine(CatchReset());
			InteractionManager.Instance.StartInteractionCooldown(Player.GetInstance());
			blinkManager.FadeToBlink(Player.GetInstance().gameObject, SetSexMode, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		}
	}

	protected IEnumerator CatchReset()
	{
		yield return new WaitForSeconds(2f);
		isCatch = false;
	}

	protected virtual void SetSexMode()
	{
		if (!(this == null) && !(base.gameObject == null))
		{
			CameraFollow.GetInstance().SetFollowTarget(base.transform);
			ChangeState(new SexModeState());
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		base.OnTriggerExit(other);
		if (!(CurrentState is SexModeState) && other.gameObject == targetChase && HospitalManager.GetInstance().GetInteractionObjectCache(targetChase) != null && CurrentState is InteractState)
		{
			InitializeCurrentRoom();
			if (HospitalManager.GetInstance().GetDoorTransit(targetChase) != null && chaseStack.Count != 0)
			{
				SetTargetRace(mainGoal);
			}
			interactObject.StopInteract();
			interactObject = null;
			ChangeState(new IdleState());
		}
	}

	public void CheckPlayerRoom()
	{
		if (!PlayerInCurrentRoom())
		{
			ChangeState(new ShortChaseState());
		}
	}

	public bool PlayerInCurrentRoom()
	{
		return Player.GetInstance().CurrentRoomObject == base.CurrentRoomObject;
	}

	public bool PlayerInCurrentStage()
	{
		return CurrentStageName == Player.GetInstance().CurrentStageName;
	}

	public override void EnterHideState()
	{
	}

	public override void ExitHideState()
	{
	}

	public bool PlayerInViewField()
	{
		if (Player.GetInstance().CurrentState is PlayerHideState || Player.GetInstance().CurrentRoomObject != base.CurrentRoomObject)
		{
			return false;
		}
		float x = Player.GetInstance().transform.position.x;
		float x2 = base.transform.position.x;
		if (Mathf.Abs(x - x2) > ViewDistance)
		{
			return false;
		}
		if (!base.FaceRight)
		{
			return x < x2;
		}
		return x > x2;
	}

	public bool IsPlayerStepInsideHearZone()
	{
		bool flag = false;
		bool flag2 = false;
		if (Player.GetInstance().CurrentState is PlayerRunState)
		{
			flag = CheckPosition(Player.GetInstance().transform.position);
		}
		GameObject gameObject = GameObject.FindGameObjectWithTag("PlayerStepUniqueSource");
		if (gameObject != null)
		{
			flag2 = CheckPosition(gameObject.transform.position);
		}
		return flag | flag2;
		bool CheckPosition(Vector3 position)
		{
			Vector3 vector = position - base.transform.position;
			if (Mathf.Abs(vector.x) <= HearDistance && Mathf.Abs(vector.y) <= 400f)
			{
				return Mathf.Abs(vector.z) <= 1000f;
			}
			return false;
		}
	}

	public bool EnemyInViewField()
	{
		if (Player.GetInstance().CurrentState is PlayerHideState || Player.GetInstance().CurrentRoomObject != base.CurrentRoomObject)
		{
			return false;
		}
		float x = Player.GetInstance().transform.position.x;
		float x2 = base.transform.position.x;
		float num = Mathf.Abs(x - x2);
		float num2 = 4000f;
		if (num > num2)
		{
			return false;
		}
		if (!Player.GetInstance().FaceRight)
		{
			return x > x2;
		}
		return x < x2;
	}

	public virtual void EndSexMode()
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

	protected virtual void ExitSexMode()
	{
		EndEvent.GetInstance().UnblockSkip();
		EnableSkipButton();
		animator.SetInteger("HloopCount", 0);
		loopCount = 0;
		Player instance = Player.GetInstance();
		instance.CurrentHeartNumber--;
		if (this is VampireEnemy && !GlobalSaveSystem.GetProgress("HintNBats") && !GlobalSaveSystem.GetProgress("HintNBatsReady"))
		{
			GlobalSaveSystem.SetProgress("HintNBatsReady", value: true);
		}
		if (instance.CurrentHeartNumber > 0)
		{
			Nintendo.ShowNintendo();
			Object.Destroy(base.LightPrefabInstance);
			if (CameraFollow.GetInstance().FollowTarget != Player.GetInstance().transform)
			{
				CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
			}
			player.gameObject.SetActive(value: true);
			instance.AlignToRoomLine();
			instance.IsPaused = false;
			instance.ChangeState(new PlayerIdleState());
			instance.GetAnimator().enabled = true;
			instance.GetAnimator().Play("Idle_Night");
			instance.StartAfterFrameIdle();
			if (!(this is SisEnemy) && !(this is PosterEnemy) && !(this is SisSupEnemy))
			{
				ToggleRenderers(state: true);
			}
			Object.Destroy(base.gameObject);
		}
		else if (this is WhisperEnemy)
		{
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
			animator.Play("Hloop2");
		}
	}

	public void AnimationLoopFinished()
	{
		loopCount++;
		animator.SetInteger("HloopCount", loopCount);
	}

	protected override void FixedUpdate()
	{
		if (!(CurrentState is SexModeState) && !isIdle)
		{
			base.FixedUpdate();
		}
	}

	protected virtual void Update()
	{
		if (currentState != null)
		{
			currentState.UpdateState(this);
		}
	}

	private void HandleInteraction()
	{
		interactObject.Interact(base.gameObject);
	}

	private void UpdateTimers()
	{
		UpdateTimer(ref changeDirInChaseTimer);
		UpdateTimer(ref searchingTimer);
	}

	public void UpdateChaseTimer()
	{
		if (!PlayerInCurrentRoom())
		{
			UpdateTimer(ref chaseTimer);
		}
		else if (chaseTimer > 0f)
		{
			chaseTimer = chaseTime;
		}
	}

	private void UpdateTimer(ref float timer)
	{
		timer = Mathf.Max(timer - Time.deltaTime, 0f);
	}

	private void HandleChaseMode()
	{
		if (CurrentState is ChaseState && GetCurrentRoom() != Player.GetInstance().GetCurrentRoom() && doorToTrigger == null && !isAngry && mainGoalRoomName != HospitalManager.GetInstance().GetRoomContainingObject(mainGoal).name)
		{
			Chase();
		}
	}

	public void RandomRoam()
	{
		if (direction != previousDirection)
		{
			consecutiveDirectionCount = 0;
		}
		consecutiveDirectionCount++;
		if (consecutiveDirectionCount > 2 && Random.value < directionChangeProbability)
		{
			direction = 0f - direction;
		}
		else
		{
			do
			{
				direction = Random.Range(-1f, 1f);
			}
			while (Mathf.Abs(direction) < 0.1f);
		}
		direction = Mathf.Sign(direction);
		previousDirection = direction;
		roamTime = Random.Range(10f, 15f);
	}

	protected override float GetDirection()
	{
		IEnemyState enemyState = CurrentState;
		if (!(enemyState is RoamState))
		{
			if (!(enemyState is ChaseState))
			{
				if (!(enemyState is InteractState))
				{
					if (!(enemyState is IdleState))
					{
						if (enemyState is ShortChaseState)
						{
							return Chase();
						}
						return 0f;
					}
					return 0f;
				}
				return 0f;
			}
			return Chase();
		}
		return Roam();
	}

	private float Roam()
	{
		if (!isMoving)
		{
			RandomRoam();
		}
		return direction;
	}

	public virtual void LightReact()
	{
	}

	public virtual bool IsLightSwordWeakness()
	{
		return false;
	}

	public virtual void WhileOnLight()
	{
	}

	public virtual void EscapeFromLight()
	{
	}

	private float Chase()
	{
		if ((!mainGoal.HasScript("Player") && mainGoalRoomName != HospitalManager.GetInstance().GetRoomContainingObject(mainGoal).name) || targetChase == null)
		{
			SetTargetRace(mainGoal);
		}
		float num = Mathf.Abs(targetChase.transform.position.x - base.transform.position.x);
		chaseDirection = ((num >= StageManager.chunkWidth * 6f && currentRoom == "Stage3") ? (0f - Mathf.Sign(targetChase.transform.position.x - base.transform.position.x)) : Mathf.Sign(targetChase.transform.position.x - base.transform.position.x));
		return chaseDirection;
	}

	public void DestroyAudio()
	{
		if (source != null)
		{
			source.Stop();
			source.enabled = false;
		}
	}

	public virtual void ReactOnLightTrap()
	{
	}

	public void StartBlind()
	{
		StartCoroutine(TemporaryBlindness());
	}

	public virtual void CreateStunEffect()
	{
	}

	private IEnumerator TemporaryBlindness()
	{
		float originalDistance = ViewDistance;
		ViewDistance = 0f;
		try
		{
			yield return new WaitForSeconds(5f);
		}
		finally
		{
			ViewDistance = originalDistance;
		}
	}

	public virtual bool CanBeCleared()
	{
		if (!(CurrentState is SexModeState) && ChaseTimer <= 0f && !(CurrentState is SpecialEventState) && !(CurrentState is AlternateCatchState) && !(CurrentState is LostBoyState) && !(CurrentState is DetectionState))
		{
			return CurrentRoom != Player.GetInstance().CurrentRoom;
		}
		return false;
	}
}
