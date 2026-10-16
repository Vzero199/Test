using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.UI;

public class PosterEnemy : ObjectEnemy
{
	[SerializeField]
	private float disappearanceDelay = 11f;

	[SerializeField]
	private float respawnTime = 15f;

	public bool UseRoomBasedSpawn;

	[SerializeField]
	protected RuntimeAnimatorController posterController;

	private bool isEventTriggered;

	private bool isPlayerInside;

	public bool shouldUseBaseUpdate;

	private GameObject spriteParent;

	private bool isDisappearanceTimerStarted;

	private bool isDisappearing;

	private PosterEnemy enemyPhantom;

	private Sprite defaultSprite;

	private Coroutine changeSpriteCoroutine;

	private Coroutine eventCoroutine;

	private Coroutine disappearanceCoroutine;

	private Sprite newSprite;

	private GameObject partyDoppController;

	private Animator partyAnimator;

	private bool wasSpecialEventTriggered;

	public Vector3 ColliderSize { get; set; }

	public Vector3 ColliderCenter { get; set; }

	public float DisappearanceDelay
	{
		set
		{
			disappearanceDelay = value;
		}
	}

	public float RespawnTime
	{
		set
		{
			respawnTime = value;
		}
	}

	public bool IsPlayerInside
	{
		set
		{
			isPlayerInside = value;
		}
	}

	public bool IsDisappearing
	{
		get
		{
			return isDisappearing;
		}
		set
		{
			isDisappearing = value;
		}
	}

	public PosterEnemy EnemyPhantom => enemyPhantom;

	protected override string BEpath => "prefabs/BreatheEffects/BEDopp";

	public bool GetEventTrigger()
	{
		return isEventTriggered;
	}

	public void SetEventTrigger(bool value)
	{
		isEventTriggered = value;
	}

	public void DoppAppearSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.DoppAppear, base.gameObject);
	}

	public void DoppSnapSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.DoppSnap, base.gameObject);
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

	public void DoppStepSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.DoppStep, base.gameObject);
	}

	public void DoppPuffSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.DoppPoof, base.gameObject);
	}

	protected override void Start()
	{
		changeSpriteCoroutine = null;
		eventCoroutine = null;
		disappearanceCoroutine = null;
		base.gameObject.layer = LayerMask.NameToLayer("FlashLightReact");
		if (base.gameObject.name == "SpriteParent")
		{
			AnimatorInitialize();
			RigidBodyInitialize();
			BoxColliderInitialize();
			GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0f);
		}
		InitializeCurrentRoom();
		if (blinkManager == null)
		{
			blinkManager = BlinkManager.GetInstance();
		}
		if (animator == null)
		{
			animator = partyAnimator;
		}
		if (GetComponent<Animator>() == null)
		{
			base.gameObject.AddComponent<Animator>().runtimeAnimatorController = animatorController;
		}
		if (base.gameObject.name == "SpriteParent")
		{
			player = Player.GetInstance().transform;
			Debug.Log("Set Empty For" + base.gameObject.name);
			ChangeState(new EmptyState());
			base.SexStateLightPrefab = Resources.Load<GameObject>("prefabs/SexStateLight");
			base.SexStateMaskPrefab = Resources.Load<GameObject>("prefabs/SexStateMask");
			spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				spriteRenderer.sortingOrder = 58;
			}
			ManualTriggerCheck();
			return;
		}
		defaultSprite = GetComponent<PosterData>().PosterSprite;
		CreatepartyDoppController();
		CheckColliderSettings();
		InitializeEnemy();
		objectCollider = GetComponent<BoxCollider>();
		if (objectCollider != null)
		{
			ColliderSize = objectCollider.size;
			ColliderCenter = objectCollider.center;
		}
		else
		{
			Debug.LogError("BoxCollider not found!");
		}
		GameObject gameObject = partyDoppController.transform.Find("SpriteParent").gameObject;
		enemyPhantom = gameObject.GetComponent<PosterEnemy>();
		ReplaceSprite();
		base.Start();
	}

	private void OnEnable()
	{
		if (base.name != "SpriteParent")
		{
			Player.GetInstance().OnStageChanged += HandleStageChange;
		}
	}

	private void OnDisable()
	{
		if (Player.GetInstance() != null && base.name != "SpriteParent")
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

	public void SetSpeedForSpriteParent(float moveValue, float interationValue)
	{
		PosterEnemy componentInChildren = base.transform.GetChild(0).GetComponentInChildren<PosterEnemy>();
		componentInChildren.SetBaseSpeed(moveValue, interationValue);
		componentInChildren.CurrentMoveSpeed = moveValue;
		componentInChildren.CurrentInteractionSpeed = interationValue;
	}

	public void SetDisappearanceDelayForSpriteParent(float value)
	{
		PosterEnemy componentInChildren = base.transform.GetChild(0).GetComponentInChildren<PosterEnemy>();
		if (componentInChildren != null)
		{
			componentInChildren.DisappearanceDelay = value;
		}
	}

	private void CreatepartyDoppController()
	{
		partyDoppController = base.transform.GetChild(0).gameObject;
		partyAnimator = partyDoppController.GetComponent<Animator>();
	}

	protected override void FixedUpdate()
	{
		if (base.gameObject.name == "SpriteParent")
		{
			base.FixedUpdate();
		}
	}

	protected override void HandleCatch()
	{
		if (!(base.CurrentState is SexModeState) && !(base.CurrentState is IdleState) && Player.GetInstance().CanInteract && BlinkManager.GetInstance() != null && !BlinkManager.GetInstance().IsFadingIn)
		{
			Player.GetInstance().ChangeState(new PlayerIdleState());
			Player.GetInstance().IsPaused = true;
			if (DialogueManager.GetInstance().dialogueIsPlaying)
			{
				DialogueManager.GetInstance().StopDialogueMode();
			}
			Player.GetInstance().BreakInteract();
			ChangeState(new IdleState());
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("snap"))
			{
				DialogueManager.GetInstance().StopDialogueMode();
				animator.Play("snap");
			}
		}
	}

	private void CheckColliderSettings()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		if (component == null)
		{
			Debug.LogError("BoxCollider not found on PosterEnemy.");
			return;
		}
		if (component.center != new Vector3(0f, -120f, 0f))
		{
			Debug.LogWarning("BoxCollider center is not right. Setting center to (0,-120,0).");
			component.center = new Vector3(0f, -120f, 0f);
		}
		if (!component.isTrigger)
		{
			Debug.LogWarning("BoxCollider is not set as a trigger. Setting it to trigger.");
			component.isTrigger = true;
		}
	}

	public bool IsCurrentAnimationStartingWith(Animator animator, string prefix)
	{
		if (animator == null)
		{
			return false;
		}
		AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(0);
		if (currentAnimatorClipInfo.Length != 0)
		{
			return currentAnimatorClipInfo[0].clip.name.StartsWith(prefix);
		}
		return false;
	}

	public override bool IsLightSwordWeakness()
	{
		if (!isEventTriggered)
		{
			return base.gameObject.name != "SpriteParent";
		}
		return false;
	}

	public override void LightReact()
	{
		if (isEventTriggered || base.gameObject.name == "SpriteParent")
		{
			return;
		}
		if (spriteRenderer.sprite == null)
		{
			partyAnimator.Play("DisappearPoster4");
		}
		else
		{
			switch (spriteRenderer.sprite.name)
			{
			case "Picture1Replace":
				partyAnimator.Play("DisappearPoster1");
				break;
			case "Picture2Replace":
				partyAnimator.Play("DisappearPoster2");
				break;
			case "Picture3Replace":
				partyAnimator.Play("DisappearPoster3");
				break;
			}
		}
		StartCoroutine(DestroyAfterFrame());
		if (!GlobalSaveSystem.GetProgress("HintNPoster"))
		{
			GlobalSaveSystem.SetProgress("HintNPoster", value: true);
			GlobalSaveSystem.SetProgress("HintNPosterReady", value: false);
		}
		Debug.Log(" Set Disappear For " + base.gameObject.name);
	}

	private IEnumerator DestroyAfterFrame()
	{
		yield return null;
		Object.Destroy(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (wasSpecialEventTriggered && !IsCurrentAnimationStartingWith(partyAnimator, "DisappearPoster") && !IsCurrentAnimationStartingWith(animator, "DisappearPoster"))
		{
			PlayCorrespondingTakeBack();
		}
		OnDestroyReset();
	}

	public void OnDestroyReset()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (defaultSprite != null)
		{
			component.sprite = defaultSprite;
		}
		if (base.gameObject.name != "SpriteParent")
		{
			Debug.Log("Poster Destroy Reset");
			if (!IsCurrentAnimationStartingWith(partyAnimator, "DisappearPoster") && !IsCurrentAnimationStartingWith(animator, "DisappearPoster"))
			{
				Debug.Log("Poster Clear Sprite");
				partyDoppController.transform.Find("FakePictureObject").GetComponent<SpriteRenderer>().sprite = null;
				SetSecondFakePictureObjectAnim("Empty");
			}
			Object.Destroy(GetComponent<Animator>());
			Object.Destroy(this);
			PosterEnemy componentInChildren = base.transform.GetChild(0).GetComponentInChildren<PosterEnemy>();
			if (componentInChildren != null)
			{
				componentInChildren.ResetComponent();
			}
		}
	}

	public void ResetComponent()
	{
		if (animator != null)
		{
			animator.Play("Empty");
		}
		isEventTriggered = false;
		isPlayerInside = false;
		isDisappearanceTimerStarted = false;
		isDisappearing = false;
		disappearanceDelay = (Settings as PosterEnemySettings).disappearanceDelay;
		respawnTime = 15f;
		shouldUseBaseUpdate = false;
		Debug.Log("Set Empty For" + base.gameObject.name);
		ChangeState(new EmptyState());
		if (base.LightPrefabInstance != null)
		{
			Object.Destroy(base.LightPrefabInstance);
		}
		if (base.MaskPrefabInstance != null)
		{
			Object.Destroy(base.MaskPrefabInstance);
		}
		base.transform.localPosition = new Vector3(0f, -100f, 0f);
		if (objectCollider != null)
		{
			objectCollider.size = ColliderSize;
			objectCollider.center = ColliderCenter;
		}
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.sprite = defaultSprite;
		}
		if (changeSpriteCoroutine != null)
		{
			StopCoroutine(changeSpriteCoroutine);
			changeSpriteCoroutine = null;
		}
		if (eventCoroutine != null)
		{
			StopCoroutine(eventCoroutine);
			eventCoroutine = null;
		}
		if (disappearanceCoroutine != null)
		{
			StopCoroutine(disappearanceCoroutine);
			disappearanceCoroutine = null;
		}
		_ = partyAnimator != null;
		targetChase = null;
		Debug.Log(base.gameObject.name + " = PosterEnemy успешно сброшен в исходное состояние.");
	}

	protected override void Update()
	{
		if (isDisappearing || this == null || base.gameObject == null || base.gameObject.name != "SpriteParent")
		{
			return;
		}
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("snap") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f && !blinkManager.IsFadingIn && !blinkManager.IsFadingOut)
		{
			if (NightManager.GetInstance() != null && NightManager.GetInstance().IsAnyOtherEnemyCatch(this))
			{
				PosterEnemy componentInChildren = base.transform.GetChild(0).GetComponentInChildren<PosterEnemy>();
				if (componentInChildren != null)
				{
					componentInChildren.ResetComponent();
				}
			}
			else
			{
				blinkManager.BlinkWithCustomSpeed(3f, base.gameObject, SetSexMode, allowVerticalMovement: false);
			}
		}
		if (shouldUseBaseUpdate && !isDisappearanceTimerStarted)
		{
			disappearanceCoroutine = StartCoroutine(StartDisappearanceTimer(disappearanceDelay));
		}
		UpdateEnemy();
	}

	protected override void ExitSexMode()
	{
		EnableSkipButton();
		animator.SetInteger("HloopCount", 0);
		loopCount = 0;
		Player instance = Player.GetInstance();
		instance.CurrentHeartNumber--;
		if (!GlobalSaveSystem.GetProgress("HintNPoster") && !GlobalSaveSystem.GetProgress("HintNPosterReady"))
		{
			GlobalSaveSystem.SetProgress("HintNPosterReady", value: true);
		}
		if (Player.GetInstance().CurrentHeartNumber > 0)
		{
			Nintendo.ShowNintendo();
			player.gameObject.SetActive(value: true);
			CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
			Object.Destroy(base.LightPrefabInstance);
			Object.Destroy(base.MaskPrefabInstance);
			Object.Destroy(base.transform.parent.parent.GetComponent<PosterEnemy>());
			instance.AlignToRoomLine();
			instance.IsPaused = false;
			instance.ChangeState(new PlayerIdleState());
			instance.GetAnimator().enabled = true;
			instance.GetAnimator().Play("Idle_Night");
			base.transform.localPosition = new Vector3(0f, -100f, 0f);
			base.TargetChase = null;
			Debug.Log("Set Empty For" + base.gameObject.name);
			GetAnimator().Play("Empty");
			ResetComponent();
		}
		else
		{
			GetAnimator().Play("Hloop1");
		}
	}

	protected override void OnTriggerEnter(Collider collision)
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("TrapDisappearing"))
		{
			return;
		}
		if (base.gameObject.name == "SpriteParent")
		{
			if (base.IsChaseMode)
			{
				base.OnTriggerEnter(collision);
			}
		}
		else if (!isDisappearing && !TVEnemy.IsTrapped && collision.CompareTag("Player") && !isEventTriggered)
		{
			isPlayerInside = true;
			if (eventCoroutine != null)
			{
				StopCoroutine(eventCoroutine);
			}
			eventCoroutine = StartCoroutine(StartEventAfterDelay(0.5f));
		}
	}

	protected override void OnTriggerStay(Collider other)
	{
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("TrapDisappearing") && !isDisappearing)
		{
			base.OnTriggerStay(other);
		}
	}

	protected override void OnTriggerExit(Collider collision)
	{
		if (TVEnemy.IsTrapped)
		{
			return;
		}
		if (base.gameObject.name == "SpriteParent")
		{
			if (base.CurrentState is ChaseState)
			{
				base.OnTriggerExit(collision);
			}
		}
		else if (!isDisappearing && !(base.CurrentState is ChaseState) && collision.CompareTag("Player") && !isEventTriggered)
		{
			isPlayerInside = false;
			if (eventCoroutine != null)
			{
				StopCoroutine(eventCoroutine);
				eventCoroutine = null;
			}
			if (!isEventTriggered)
			{
				StartCoroutine(SpecialEventCoroutine());
			}
		}
	}

	private IEnumerator StartEventAfterDelay(float delay)
	{
		if (!isDisappearing)
		{
			yield return new WaitForSeconds(delay);
			if (isPlayerInside)
			{
				StartCoroutine(SpecialEventCoroutine());
			}
		}
	}

	private IEnumerator StealCoroutine()
	{
		Image[] inventoryCells = Player.GetInstance().InventoryCells;
		foreach (Image image in inventoryCells)
		{
			if (image.GetComponentInChildren<ItemToUse>() != null)
			{
				isEventTriggered = true;
				Debug.Log("stealed");
				animator.Play("Disappear");
				Object.Destroy(image.GetComponentInChildren<ItemToUse>().gameObject);
				yield break;
			}
		}
		StartCoroutine(SpecialEventCoroutine());
	}

	private IEnumerator SpecialEventCoroutine()
	{
		if (!isDisappearing)
		{
			partyAnimator.Play("SpecialEvent");
			isEventTriggered = true;
			wasSpecialEventTriggered = true;
			SetSecondFakePictureObjectAnim("handDisp");
			Player player = Player.GetInstance();
			yield return new WaitForSeconds(1f);
			player.IsPaused = false;
			player.ChangeState(new PlayerIdleState());
			yield return new WaitForSeconds(1f);
			if (animator != null)
			{
				animator.Play("Empty");
			}
			partyDoppController.transform.Find("FakePictureObject").gameObject.GetComponent<SpriteRenderer>().sprite = null;
			PlayCorrespondingTakeBack();
			enemyPhantom.IsChaseMode = true;
			enemyPhantom.ChangeState(new ChaseState());
			enemyPhantom.ChaseTimer = enemyPhantom.ChaseTime;
			enemyPhantom.TabLightTriggerCheck();
			enemyPhantom.shouldUseBaseUpdate = true;
		}
	}

	public void TabLightTriggerCheck()
	{
		if (base.EntityCollider == null || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(base.transform.position, base.EntityCollider.size / 2f, base.transform.rotation);
		Debug.Log($"Collider Count: {array.Length}");
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			TabLight component = array2[i].GetComponent<TabLight>();
			if (component != null)
			{
				component.OnTriggerTrap(base.EntityCollider);
			}
		}
	}

	private void PlayCorrespondingTakeBack()
	{
		if (spriteRenderer == null || partyAnimator == null)
		{
			return;
		}
		if (spriteRenderer.sprite == null)
		{
			partyAnimator.Play("TakeBack4");
			return;
		}
		switch (spriteRenderer.sprite.name)
		{
		case "Picture1Replace":
			partyAnimator.Play("TakeBack1");
			break;
		case "Picture2Replace":
			partyAnimator.Play("TakeBack2");
			break;
		case "Picture3Replace":
			partyAnimator.Play("TakeBack3");
			break;
		default:
			partyAnimator.Play("TakeBack4");
			break;
		}
	}

	private void HandlePlayerCaught(Player player)
	{
		player.ChangeState(new PlayerCaughtState());
		bool flag = player.transform.position.x > base.transform.position.x;
		player.transform.rotation = Quaternion.Euler(0f, flag ? 180f : 0f, 0f);
	}

	private void AdjustPlayerPositionIfNeeded(Player player)
	{
		BoxCollider component = player.GetComponent<BoxCollider>();
		BoxCollider component2 = GetComponent<BoxCollider>();
		if (component2.bounds.Intersects(component.bounds))
		{
			AdjustPlayerPosition(player, component2.bounds.size.x + 0.1f);
		}
	}

	private void AdjustPlayerPosition(Player player, float offset)
	{
		bool num = player.transform.position.x > base.transform.position.x;
		Vector3 position = player.transform.position;
		if (num)
		{
			position.x += offset;
		}
		else
		{
			position.x -= offset;
		}
		Rigidbody component = player.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.MovePosition(position);
			return;
		}
		Debug.LogWarning("Player Rigidbody not found.");
		player.transform.position = position;
	}

	private void InitializeEnemy()
	{
		Initialize();
		shouldUseBaseUpdate = true;
	}

	private void StartChasingPlayer(Player player)
	{
		ChangeState(new ChaseState());
	}

	private IEnumerator StartDisappearanceTimer(float delay)
	{
		if (!(base.CurrentState is ChaseState))
		{
			yield break;
		}
		isDisappearanceTimerStarted = true;
		yield return new WaitForSeconds(delay);
		if (!(base.CurrentState is SexModeState) && !animator.GetCurrentAnimatorStateInfo(0).IsName("snap"))
		{
			isDisappearing = true;
			base.IsChaseMode = false;
			ChangeState(new IdleState());
			animator.Play("Disappear");
			CurrentMoveSpeed = 0f;
			yield return new WaitForSeconds(0f);
			while (animator.GetCurrentAnimatorStateInfo(0).IsName("Disappear") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
			{
				yield return new WaitForSeconds(0f);
			}
			Debug.Log("Set Empty For" + base.gameObject.name);
			animator.Play("Empty");
			base.transform.localPosition = new Vector3(0f, -100f, 0f);
			Object.Destroy(base.transform.parent.parent.GetComponent<PosterEnemy>());
			isDisappearing = false;
			isDisappearanceTimerStarted = false;
		}
	}

	private void OnDisappearAnimationEnd()
	{
		GangerSpawner gangerSpawner = spriteParent.AddComponent<GangerSpawner>();
		gangerSpawner.ColliderSize = ColliderSize;
		gangerSpawner.ColliderCenter = ColliderCenter;
		gangerSpawner.RespawnTime = respawnTime;
		gangerSpawner.DefaultSprite = defaultSprite;
		gangerSpawner.UseRoomBasedSpawn = UseRoomBasedSpawn;
		gangerSpawner.DisappearanceDelay = disappearanceDelay;
		Object.Destroy(base.gameObject);
	}

	private void ReplaceSprite()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component == null)
		{
			Debug.LogError("SpriteRenderer not found on PosterEnemy.");
			return;
		}
		newSprite = null;
		Sprite sprite = null;
		string spriteName = ((component.sprite != null) ? component.sprite.name : null);
		(newSprite, sprite) = GetSpritesBasedOnCurrent(spriteName);
		if (newSprite != null)
		{
			component.sprite = newSprite;
		}
		if (sprite != null)
		{
			SetFakePictureObject(sprite, component.sortingOrder);
		}
	}

	private (Sprite, Sprite) GetSpritesBasedOnCurrent(string spriteName)
	{
		Sprite item = null;
		Sprite item2 = null;
		switch (spriteName)
		{
		case "Picture1":
			item = Resources.Load<Sprite>("Sprites/Dopp/Picture1Replace");
			item2 = Resources.Load<Sprite>("Sprites/Dopp/FakePicture1");
			animator.Play("Idle1");
			break;
		case "Picture2":
			item = Resources.Load<Sprite>("Sprites/Dopp/Picture2Replace");
			item2 = Resources.Load<Sprite>("Sprites/Dopp/FakePicture2");
			SetSecondFakePictureObjectAnim("handIdle");
			animator.Play("Idle2");
			break;
		case "Picture3":
			item = Resources.Load<Sprite>("Sprites/Dopp/Picture3Replace");
			item2 = Resources.Load<Sprite>("Sprites/Dopp/FakePicture3");
			animator.Play("Idle3");
			break;
		case "Poster2":
			item2 = Resources.Load<Sprite>("Sprites/Dopp/Poster_Dopp");
			animator.Play("Idle4");
			break;
		case "Picture1Replace":
			item = Resources.Load<Sprite>("Sprites/Dopp/Picture1Replace");
			item2 = Resources.Load<Sprite>("Sprites/Dopp/FakePicture1");
			animator.Play("Idle1");
			break;
		case "Picture2Replace":
			item = Resources.Load<Sprite>("Sprites/Dopp/Picture2Replace");
			item2 = Resources.Load<Sprite>("Sprites/Dopp/FakePicture2");
			SetSecondFakePictureObjectAnim("handIdle");
			animator.Play("Idle2");
			break;
		case "Picture3Replace":
			item = Resources.Load<Sprite>("Sprites/Dopp/Picture3Replace");
			item2 = Resources.Load<Sprite>("Sprites/Dopp/FakePicture3");
			animator.Play("Idle3");
			break;
		case null:
			item2 = Resources.Load<Sprite>("Sprites/Dopp/Poster_Dopp");
			animator.Play("Idle4");
			break;
		default:
			Debug.LogWarning("Sprite does not match any replaceable sprite. Sprite name is " + spriteName);
			break;
		}
		return (item, item2);
	}

	private void SetFakePictureObject(Sprite fakeSprite, int baseSortingOrder)
	{
		GameObject gameObject = partyDoppController.transform.Find("FakePictureObject").gameObject;
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		component.sprite = fakeSprite;
		SetMask(gameObject, fakeSprite);
		SetAdditionalSpriteObject(fakeSprite, gameObject, component.sortingOrder);
	}

	private void SetMask(GameObject parentObject, Sprite fakeSprite)
	{
		parentObject.transform.Find("FakePictureMask").gameObject.GetComponent<SpriteMask>().sprite = fakeSprite;
	}

	private void SetAdditionalSpriteObject(Sprite fakeSprite, GameObject parentObject, int fakePicSortingOrder)
	{
		parentObject.transform.Find("AdditionalSpriteObject").gameObject.GetComponent<SpriteRenderer>();
	}

	private void SetSecondFakePictureObjectAnim(string animName)
	{
		GameObject gameObject = partyDoppController.transform.Find("SecondFakePictureObject").gameObject;
		if (!(gameObject == null) && gameObject.activeSelf)
		{
			gameObject.GetComponent<Animator>().Play(animName);
		}
	}

	public override AudioClip GetMovementSound()
	{
		if (!IsDisappearing)
		{
			return null;
		}
		string[] array = new string[5] { "DoppFootSteps 1", "DoppFootSteps 2", "DoppFootSteps 3", "DoppFootSteps 4", "DoppFootSteps 5" };
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

	public override void ReactOnLightTrap()
	{
		if (base.IsChaseMode)
		{
			ChangeState(new EmptyState());
			animator.Play("TrapDisappearing");
		}
	}

	public void DestroyPoster()
	{
		Object.Destroy(base.transform.parent.parent.GetComponent<PosterEnemy>());
	}

	public override bool CanBeCleared()
	{
		if (enemyPhantom != null && !enemyPhantom.IsChaseMode && base.ChaseTimer <= 0f && CurrentRoom != Player.GetInstance().CurrentRoom && !(enemyPhantom.CurrentState is SexModeState) && !(enemyPhantom.CurrentState is IdleState) && !(enemyPhantom.CurrentState is ChaseState) && !(enemyPhantom.CurrentState is SpecialEventState) && !(enemyPhantom.CurrentState is AlternateCatchState) && !(enemyPhantom.CurrentState is LostBoyState) && !(enemyPhantom.CurrentState is DetectionState) && !enemyPhantom.IsDisappearing)
		{
			return !isEventTriggered;
		}
		return false;
	}
}
