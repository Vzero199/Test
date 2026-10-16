using System;
using System.Collections;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Playables;

public class TVEnemy : ObjectEnemy
{
	[Header("Настройки TVEnemy")]
	private BoxCollider boxCollider;

	private Animator anim;

	public Animator newAnimator;

	public RuntimeAnimatorController newAnimatorController;

	[SerializeField]
	private GameObject spawnPrefab;

	private GameObject hairObject;

	private Animator hairAnimator;

	private SpriteRenderer hairRenderer;

	private Transform blockTransform;

	private PlayableDirector playableDirector;

	private static TVEnemy instance;

	private GameObject triggeredObject;

	private string triggeredRoom;

	public bool isHScene;

	private static bool isTrapped;

	private Dictionary<AudioSource, Coroutine> activeFadeCoroutines = new Dictionary<AudioSource, Coroutine>();

	private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

	private TimelineManager timelineManager;

	private GameObject[] ambienceObjects;

	private string currentHairAnim = "Empty";

	private AudioSource triggerAudio;

	public GameObject SpawnPrefab
	{
		set
		{
			spawnPrefab = value;
		}
	}

	public static bool IsTrapped => isTrapped;

	public static TVEnemy GetInstance()
	{
		if (instance == null)
		{
			instance = UnityEngine.Object.FindObjectOfType<TVEnemy>();
			if (instance == null)
			{
				Debug.LogError("TVEnemy instance not found in the scene.");
			}
		}
		return instance;
	}

	protected override void Start()
	{
		hairObject = UnityEngine.Object.FindObjectOfType<TVHairs>(includeInactive: true).gameObject;
		if (hairObject == null)
		{
			Debug.LogError("hairObject in Null");
			return;
		}
		hairAnimator = hairObject.GetComponent<Animator>();
		if (hairAnimator == null)
		{
			Debug.LogError("hairAnimator in Null");
			return;
		}
		hairRenderer = hairObject.GetComponent<SpriteRenderer>();
		if (hairRenderer == null)
		{
			Debug.LogError("hairRenderer in Null");
			return;
		}
		base.Start();
		ambienceObjects = GameObject.FindGameObjectsWithTag("Ambience");
		currentHairAnim = "Empty";
		hairAnimator.Play(currentHairAnim);
		hairRenderer.enabled = false;
		isTrapped = false;
		instance = UnityEngine.Object.FindObjectOfType<TVEnemy>();
		base.gameObject.layer = LayerMask.NameToLayer("FlashLightReact");
		timelineManager = TimelineManager.GetInstance();
		playableDirector = TimelineManager.GetInstance().Director;
		InitializeProp();
		StoreOriginalVolumes();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (currentHairAnim == "Empty" || currentHairAnim == "HairCatch" || currentHairAnim == "HairAppear")
		{
			return;
		}
		Player player = Player.GetInstance();
		if (player != null)
		{
			if (player.EndurancePercent > 0.5f)
			{
				PlayAnimation("HairWiggle1");
			}
			else
			{
				PlayAnimation("HairWiggle2");
			}
		}
	}

	public override void PlayAnimation(string animName)
	{
		if (!(currentHairAnim == animName))
		{
			currentHairAnim = animName;
			hairAnimator.Play(currentHairAnim);
			hairAnimator.Update(0f);
		}
	}

	private void StoreOriginalVolumes()
	{
		originalVolumes.Clear();
		GameObject[] array = ambienceObjects;
		for (int i = 0; i < array.Length; i++)
		{
			AudioSource component = array[i].GetComponent<AudioSource>();
			if (component != null && !originalVolumes.ContainsKey(component))
			{
				originalVolumes.Add(component, component.volume);
			}
		}
	}

	private void RestoreOriginalVolumes()
	{
		foreach (KeyValuePair<AudioSource, float> originalVolume in originalVolumes)
		{
			if (originalVolume.Key != null)
			{
				StopFadeCoroutine(originalVolume.Key);
				originalVolume.Key.volume = originalVolume.Value;
			}
		}
	}

	private void StopFadeCoroutine(AudioSource source)
	{
		if (activeFadeCoroutines.TryGetValue(source, out var value))
		{
			if (value != null)
			{
				StopCoroutine(value);
			}
			activeFadeCoroutines.Remove(source);
		}
	}

	private void AmbienceToVolume(float targetVolume)
	{
		GameObject[] array = ambienceObjects;
		for (int i = 0; i < array.Length; i++)
		{
			AudioSource component = array[i].GetComponent<AudioSource>();
			if (component != null)
			{
				FadeToVolume(component, targetVolume);
			}
		}
	}

	private void FadeToVolume(AudioSource source, float targetVolume)
	{
		StopFadeCoroutine(source);
		Coroutine value = StartCoroutine(FadeCoroutine(source, targetVolume));
		activeFadeCoroutines[source] = value;
	}

	private IEnumerator FadeCoroutine(AudioSource source, float targetVolume)
	{
		float startVolume = source.volume;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			float t2 = Mathf.SmoothStep(0f, 1f, t);
			float volume = Mathf.Lerp(startVolume, targetVolume, t2);
			source.volume = volume;
			yield return null;
		}
		source.volume = targetVolume;
		if (activeFadeCoroutines.ContainsKey(source))
		{
			activeFadeCoroutines.Remove(source);
		}
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
		StopAllFadeCoroutines();
	}

	private void StopAllFadeCoroutines()
	{
		foreach (Coroutine value in activeFadeCoroutines.Values)
		{
			if (value != null)
			{
				StopCoroutine(value);
			}
		}
		activeFadeCoroutines.Clear();
	}

	private void HandleStageChange(string newStage)
	{
		if (NightManager.GetInstance().NormalizeStageName(newStage) != NightManager.GetInstance().NormalizeStageName(CurrentStageName) && NightManager.GetInstance().StopGameCoroutine == null && !EndEvent.isGameOver && CanBeCleared())
		{
			Debug.Log(base.name + " переспавнен из-за смены stage");
			NightManager.GetInstance().ForceRespawnEnemy(base.gameObject);
		}
	}

	private void LoadAndPlayTimeline()
	{
		string text = "Timelines/TVScene";
		PlayableAsset playableAsset = Resources.Load<PlayableAsset>(text);
		if (playableAsset != null && timelineManager != null && timelineManager.Director != null)
		{
			timelineManager.Director.playableAsset = playableAsset;
			timelineManager.Director.Play();
			AmbienceToVolume(0f);
		}
		else
		{
			Debug.LogError("Не удалось загрузить Timeline: " + text);
		}
	}

	public void InitializeProp()
	{
		if (newAnimatorController != null)
		{
			newAnimator.runtimeAnimatorController = newAnimatorController;
		}
		boxCollider = GetComponent<BoxCollider>();
		if (boxCollider == null)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y - 100f, boxCollider.center.z);
		}
		anim = GetComponent<Animator>();
		if (anim == null)
		{
			Debug.LogWarning("Animator not set");
		}
		anim.Play("TV Hissing");
		blinkManager = BlinkManager.GetInstance();
	}

	private IEnumerator DrainPlayerEndurance(Player player)
	{
		try
		{
			EnduranceStateMachine endurance = player.enduranceSM;
			player.isTired = false;
			player.enduranceSM.rollbackTarget = player.EndurancePercent;
			endurance.AddModifier(EnduranceModifier.TVDrain);
			endurance.SetExternalDrain(value: true);
			float enduranceDrainRate = 0.1f;
			Difficulty difficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty", 2);
			if (difficulty == Difficulty.Easy || difficulty == Difficulty.Practice)
			{
				enduranceDrainRate *= 0.8f;
			}
			while (hairObject != null && isTrapped)
			{
				float num = Time.deltaTime * enduranceDrainRate;
				endurance.Endurance -= ((player.CurrentState is PlayerRunState) ? num : (num * 0.75f));
				if (endurance.Endurance <= 0f)
				{
					PlayAnimation("HairCatch");
					player.IsPaused = true;
					player.ChangeState(new PlayerIdleState());
					isHScene = true;
					if (triggerAudio != null)
					{
						triggerAudio.Stop();
					}
					if (Nintendo.GetInstance() != null)
					{
						Nintendo.HideNintendo();
					}
					if (Player.GetInstance() != null)
					{
						Player.GetInstance().FlashlightOff();
					}
					NightManager.GetInstance().HandleCatchMode(this);
					break;
				}
				yield return null;
			}
		}
		finally
		{
			EnduranceStateMachine enduranceSM = player.enduranceSM;
			enduranceSM.SetExternalDrain(value: false);
			enduranceSM.RemoveModifier(EnduranceModifier.TVDrain);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (isTrapped && triggeredRoom != Player.GetInstance().GetCurrentRoom())
		{
			DespawnTrap();
		}
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("PlayerCatch") && !Player.GetInstance().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0)
			.IsName("HairCatch") && playableDirector.state != PlayState.Playing)
		{
			Player.GetInstance().ChangeState(new PlayerIdleState());
			Player.GetInstance().GetComponent<Animator>().Play("HairCatch");
		}
		if (!Player.GetInstance().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0)
			.IsName("HairCatch") || !(Player.GetInstance().GetComponent<Animator>().GetCurrentAnimatorStateInfo(0)
			.normalizedTime >= 1f))
		{
			return;
		}
		if (timelineManager != null && timelineManager.Director != null)
		{
			if (timelineManager.Director.state != PlayState.Playing)
			{
				LoadAndPlayTimeline();
			}
		}
		else
		{
			DespawnTrap();
		}
	}

	private void DespawnTrap()
	{
		anim.Play("Default");
		UnityEngine.Object.Destroy(this);
		if (triggeredObject != null)
		{
			UnityEngine.Object.Destroy(triggeredObject);
		}
		boxCollider.enabled = true;
		TVHairs component = hairObject.GetComponent<TVHairs>();
		if (component != null)
		{
			component.DisableHairs();
		}
		isTrapped = false;
		Player.GetInstance().IsPaused = false;
		Player.GetInstance().enduranceSM.BlockRegen = false;
		RestoreOriginalVolumes();
	}

	public void EndScene()
	{
		ExitSexMode();
	}

	protected override void ExitSexMode()
	{
		EnableSkipButton();
		Player component = player.GetComponent<Player>();
		component.CurrentHeartNumber--;
		if (Player.GetInstance().CurrentHeartNumber > 0)
		{
			component.enduranceSM.ChangeState(new EnduranceNormalState());
			RestoreOriginalVolumes();
			Nintendo.ShowNintendo();
			player.gameObject.SetActive(value: true);
			anim.Play("Default");
			isTrapped = false;
			component.IsPaused = false;
			component.IsNightSprites = true;
			component.IsMoving = false;
			component.ChangeState(new PlayerIdleState());
			component.PlayAnimation("Idle_Night");
			component.MoveDirection = Vector3.zero;
			component.IsShift = false;
			hairAnimator.Play("Empty");
			boxCollider.enabled = true;
			UnityEngine.Object.Destroy(this);
			if (hairRenderer != null)
			{
				hairRenderer.enabled = false;
			}
			if (hairObject != null)
			{
				base.gameObject.SetActive(value: false);
				base.gameObject.transform.position = new Vector3(0f, -10000f, 0f);
			}
			UnityEngine.Object.Destroy(triggeredObject);
		}
		else
		{
			LoadAndPlayTimeline();
		}
	}

	protected override void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == Player.GetInstance().gameObject && !isTrapped)
		{
			StartCoroutine(TriggerEvent());
		}
	}

	private IEnumerator TriggerEvent()
	{
		isTrapped = true;
		triggerAudio = SoundManager.PlaySoundAtObject(SoundType.TVTrigger, base.gameObject);
		triggeredRoom = Player.GetInstance().GetCurrentRoom();
		hairObject.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 40f - 100f, 0f);
		hairRenderer.enabled = true;
		hairRenderer.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
		yield return new WaitForSeconds(1f);
		PlayAnimation("HairAppear");
		TVHairs component = hairObject.GetComponent<TVHairs>();
		if (component != null)
		{
			component.EnableHairs();
		}
		boxCollider.enabled = false;
		triggeredObject = new GameObject("TriggeredObject");
		triggeredObject.transform.position = new Vector3(base.transform.position.x + 10f, base.transform.position.y, base.transform.position.z);
		newAnimator = triggeredObject.AddComponent<Animator>();
		triggeredObject.AddComponent<SpriteRenderer>().sortingOrder = 56;
		newAnimator.runtimeAnimatorController = animatorController;
		Player player = Player.GetInstance();
		if (player != null)
		{
			Player.GetInstance().enduranceSM.BlockRegen = true;
			StartCoroutine(DrainPlayerEndurance(player));
		}
		hairObject.SetActive(value: true);
		triggeredObject.AddComponent<TriggeredObjectController>().Initialize(hairObject);
		newAnimator.Play("Trigger");
	}

	private void HandleCatch(Action catchAction)
	{
		if (!blinkManager.IsFadingIn && !blinkManager.IsFadingOut)
		{
			player.gameObject.SetActive(value: false);
			blinkManager.FadeToBlink(Player.GetInstance().gameObject);
		}
		else if (blinkManager.IsFadingOut)
		{
			catchAction();
		}
	}

	private void HairCatch()
	{
		ChangeState(new AlternateCatchState());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoteItem.CloseToTv = false;
		if (triggerAudio != null)
		{
			triggerAudio.Stop();
		}
		StopAllFadeCoroutines();
		if (Player.GetInstance() != null)
		{
			EnduranceStateMachine enduranceSM = Player.GetInstance().enduranceSM;
			enduranceSM.SetExternalDrain(value: false);
			if (isTrapped)
			{
				enduranceSM.RemoveModifier(EnduranceModifier.TVDrain);
				enduranceSM.BlockRegen = false;
				enduranceSM.ChangeState(new EnduranceNormalState());
			}
		}
		anim.Play("Default");
		isTrapped = false;
		if (triggeredObject != null)
		{
			UnityEngine.Object.Destroy(triggeredObject);
		}
		if (hairObject != null)
		{
			TVHairs component = hairObject.GetComponent<TVHairs>();
			if (component != null)
			{
				component.DisableHairs();
			}
		}
		boxCollider.enabled = true;
		RestoreOriginalVolumes();
	}

	public override bool CanBeCleared()
	{
		if (base.CanBeCleared())
		{
			return !isTrapped;
		}
		return false;
	}
}
