using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;

public class VampireEnemy : Enemy
{
	private Enemy instance;

	private const float _metamorphosisMoveSpeed = 20f;

	[SerializeField]
	private Vector3 colliderSize;

	public float BatsMoveSpeed { get; set; }

	public Vector3 ColliderSize
	{
		set
		{
			colliderSize = value;
		}
	}

	protected override string WWpath => "prefabs/WierdWordEffects/WWFuckSlurp";

	protected override void Start()
	{
		base.Start();
		spriteRenderer.sortingOrder = 57;
		EnsureComponents();
		Initialize();
		CreateAudioSource();
	}

	protected override void Update()
	{
		if (!(animator == null) && !(base.CurrentState is SexModeState) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Disappearing"))
		{
			HandleTransformationLogic();
			SyncSpeedWithForm();
			base.Update();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (source != null)
		{
			PauseMenu.UnregisterPausableAudio(source);
		}
	}

	private bool IsInBatsForm()
	{
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (!currentAnimatorStateInfo.IsName("BatsChase") && !currentAnimatorStateInfo.IsName("Idle") && !currentAnimatorStateInfo.IsName("Metamorphosis") && !currentAnimatorStateInfo.IsName("Disappearing"))
		{
			return currentAnimatorStateInfo.IsName("ReverseMetamorphosis");
		}
		return true;
	}

	private bool IsTransforming()
	{
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (!currentAnimatorStateInfo.IsName("Metamorphosis"))
		{
			return currentAnimatorStateInfo.IsName("ReverseMetamorphosis");
		}
		return true;
	}

	private void HandleTransformationLogic()
	{
		if (IsTransforming())
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.normalizedTime >= 1f)
			{
				if (currentAnimatorStateInfo.IsName("Metamorphosis"))
				{
					animator.Play("BatsChase");
				}
				else if (currentAnimatorStateInfo.IsName("ReverseMetamorphosis"))
				{
					animator.Play("VampChase");
				}
			}
			return;
		}
		float num = Mathf.Abs(base.transform.position.x - Player.GetInstance().transform.position.x);
		bool flag = base.CurrentRoomObject == Player.GetInstance().CurrentRoomObject;
		if (IsInBatsForm())
		{
			if (((num <= 100f) & flag) && base.ViewDistance != 0f)
			{
				animator.Play("ReverseMetamorphosis");
				CurrentMoveSpeed = 20f;
			}
		}
		else if (num >= 400f || !flag)
		{
			animator.Play("Metamorphosis");
			CurrentMoveSpeed = 20f;
		}
	}

	private void SyncSpeedWithForm()
	{
		if (IsTransforming())
		{
			CurrentMoveSpeed = 20f;
			return;
		}
		float num = (IsInBatsForm() ? BatsMoveSpeed : base.BaseMoveSpeed);
		CurrentMoveSpeed = num;
		HandleBatsAudio(IsInBatsForm());
	}

	public void VampireBatsSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.VampireBats, base.gameObject);
	}

	public void VampireTransformationSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.VampireTransformation, base.gameObject);
	}

	public void Hloop1Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.PaizuriLoop1, base.gameObject);
	}

	public void Hloop2Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.PaizuriLoop2, base.gameObject);
	}

	public void Hloop3Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.PaizuriLoop3, base.gameObject);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.HClimax, base.gameObject);
	}

	private void CreateAudioSource()
	{
		source = base.gameObject.AddComponent<AudioSource>();
		source.gameObject.tag = "enemyNoise";
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/bats");
		if (audioClip == null)
		{
			Debug.LogWarning("[Audio] bats clip not found.");
			return;
		}
		source.clip = audioClip;
		source.loop = true;
		source.volume = 0.2f;
		source.spatialBlend = 1f;
		source.dopplerLevel = 0f;
		source.minDistance = 100f;
		source.maxDistance = 400f;
		source.rolloffMode = AudioRolloffMode.Linear;
		AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
		if (sFXGroup != null)
		{
			source.outputAudioMixerGroup = sFXGroup;
		}
		source.Play();
		PauseMenu.RegisterPausableAudio(source);
	}

	private void HandleBatsAudio(bool isBats)
	{
		if (!(source == null))
		{
			if (isBats && !source.isPlaying)
			{
				source.Play();
			}
			else if (!isBats && source.isPlaying)
			{
				source.Stop();
			}
		}
	}

	public override void WhileOnLight()
	{
		if (IsInBatsForm() && !IsTransforming())
		{
			FlashlightSword flashlightSword = Object.FindObjectOfType<FlashlightSword>();
			if (!(flashlightSword == null) && flashlightSword.EnemyResistanceTimers.ContainsKey(instance))
			{
				float num = flashlightSword.EnemyResistanceTimers[instance] / resistanceTime;
				CurrentMoveSpeed = base.BaseMoveSpeed * num;
			}
		}
	}

	public override void EscapeFromLight()
	{
		SyncSpeedWithForm();
	}

	public override bool IsLightSwordWeakness()
	{
		return IsInBatsForm();
	}

	public override void LightReact()
	{
		if (IsInBatsForm())
		{
			animator.Play("Disappearing");
		}
	}

	public override void ReactOnLightTrap()
	{
		if (IsInBatsForm())
		{
			animator.Play("Disappearing");
			CurrentMoveSpeed = 0f;
		}
		else
		{
			ChangeState(new StunState());
		}
	}

	protected override void HandleCatch()
	{
		if (!IsInBatsForm() && !isCatch && !(base.CurrentState is SexModeState) && !(BlinkManager.GetInstance() == null) && !BlinkManager.GetInstance().IsFadingIn && !animator.GetCurrentAnimatorStateInfo(0).IsName("Disappearing"))
		{
			isCatch = true;
			if (DialogueManager.GetInstance().dialogueIsPlaying)
			{
				StartCoroutine(DialogueManager.GetInstance().ExitDialogueMode());
			}
			StartCoroutine(CatchReset());
			InteractionManager.Instance.StartInteractionCooldown(Player.GetInstance());
			blinkManager.FadeToBlink(Player.GetInstance().gameObject, SetSexMode, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		}
	}

	protected override void Initialize()
	{
		if (base.EntityCollider != null)
		{
			base.EntityCollider.isTrigger = true;
			base.EntityCollider.size = colliderSize;
		}
		instance = this;
		base.Initialize();
		SyncSpeedWithForm();
	}

	private void EnsureComponents()
	{
		if (animator == null)
		{
			animator = base.gameObject.AddComponent<Animator>();
			animator.runtimeAnimatorController = animatorController;
		}
		if (base.EntityCollider == null)
		{
			base.EntityCollider = base.gameObject.AddComponent<BoxCollider>();
			base.EntityCollider.isTrigger = true;
		}
		if (rb == null)
		{
			rb = base.gameObject.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.sortingLayerName = "Entity";
		}
	}

	public void DissapearEnd()
	{
		Object.Destroy(base.gameObject);
	}
}
