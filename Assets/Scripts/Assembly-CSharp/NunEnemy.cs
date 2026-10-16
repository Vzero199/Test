using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class NunEnemy : Enemy
{
	[SerializeField]
	private Vector3 colliderSize;

	[SerializeField]
	private ProgressBar captureBar;

	[SerializeField]
	private float captureTime = 15f;

	[Header("Player Slowdown Percentages")]
	[Range(0f, 0.5f)]
	[SerializeField]
	private float initialSlowdownPercent = 0.3f;

	[Range(0f, 0.5f)]
	[SerializeField]
	private float additionalSlowdownPercent = 0.3f;

	public RuntimeAnimatorController nunCatchAnimatorController;

	public GameObject prefab;

	private float captureChangeSpeed;

	private float capturePercent;

	private float originalPlayerSpeed;

	private float originalPlayerInteractionSpeed;

	private bool hasSlowedInitial;

	private bool hasSlowedAdditional;

	private bool isCaptured;

	private bool isMuffled;

	private bool isTriggered;

	private GameObject soundObject;

	private GameObject childCameraObject;

	private GameObject spawnedMapNunIco;

	private GameObject previousRoom;

	private AudioSource audioSource;

	private GameObject audioSourceObject;

	public Vector3 ColliderSize
	{
		set
		{
			colliderSize = value;
		}
	}

	public ProgressBar CaptureBar
	{
		set
		{
			captureBar = value;
		}
	}

	public float CaptureTime
	{
		set
		{
			captureTime = value;
		}
	}

	public float InitialSlowdownPercent
	{
		set
		{
			initialSlowdownPercent = value;
		}
	}

	public float AdditionalSlowdownPercent
	{
		set
		{
			additionalSlowdownPercent = value;
		}
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

	protected override void Start()
	{
		base.Start();
		SpawnMapNunIco();
		HospitalManager.GetInstance().GetRoomContainingObject(base.gameObject);
		CreateAudioSource();
		previousRoom = Player.GetInstance().CurrentRoomObject;
		StartCoroutine(CheckRoomAndAttachAudioSource());
		CrossItem.NunIsExists = true;
	}

	private void SpawnMapNunIco()
	{
		GameObject gameObject = GameObject.Find("mapNight");
		if (gameObject == null)
		{
			Debug.LogError("Не найден UI объект mapNight!");
			return;
		}
		GameObject gameObject2 = FindCurrentMapSegment();
		if (!(gameObject2 == null))
		{
			GameObject gameObject3 = LoadPrefab("prefabs/mapNunIcon");
			if (!(gameObject3 == null))
			{
				spawnedMapNunIco = Object.Instantiate(gameObject3, gameObject.transform);
				spawnedMapNunIco.GetComponent<RectTransform>().anchoredPosition = GetMapNunIcoPosition(gameObject2.name);
			}
		}
	}

	private GameObject FindCurrentMapSegment()
	{
		return GameObject.FindGameObjectsWithTag("MapSegment").FirstOrDefault((GameObject obj) => obj.GetComponent<BoxCollider>()?.bounds.Contains(base.transform.position) ?? false);
	}

	private Vector2 GetMapNunIcoPosition(string segmentName)
	{
		return segmentName switch
		{
			"WCMale3F" => new Vector2(-209f, 236f), 
			"LockerRoom3F" => new Vector2(48f, 236f), 
			"UtilityRoom3F" => new Vector2(352f, 236f), 
			"WCMale2F" => new Vector2(-209f, 107f), 
			"UtilityRoom2F" => new Vector2(352f, 107f), 
			"WCMale1F" => new Vector2(-42f, -24f), 
			"LockerRoom1F" => new Vector2(152f, -24f), 
			"UtilityRoom1F" => new Vector2(265f, -24f), 
			_ => new Vector2(0f, 0f), 
		};
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

	public void NunSingSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.NunSing, base.gameObject);
	}

	public void PortalSound()
	{
		SoundManager.PlaySound(SoundType.Portal);
	}

	public void TentacleCatchSound()
	{
		SoundManager.PlaySound(SoundType.TVCatch);
	}

	public void VampireTransformationSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.VampireTransformation, base.gameObject);
	}

	protected override void Initialize()
	{
		base.Initialize();
		CreateChildObjectForCamera();
		InitializeVariables();
		Player.GetInstance().CreateCommonEffect("NunAirThingEffect");
		ShowCaptureBar();
	}

	private void CreateAudioSource()
	{
		string text = "nun Whisper";
		audioSourceObject = new GameObject("NunEnemyAudioSource");
		audioSourceObject.gameObject.tag = "enemyNoise";
		audioSourceObject.transform.SetParent(base.transform);
		CheckAudioPlacement();
		audioSource = audioSourceObject.AddComponent<AudioSource>();
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
		audioSource.volume = 0.02f;
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
		audioSource.minDistance = 200f;
		audioSource.maxDistance = 40000f;
		audioSource.rolloffMode = AudioRolloffMode.Linear;
		audioSource.Play();
		PauseMenu.RegisterPausableAudio(audioSource);
	}

	private IEnumerator CheckRoomAndAttachAudioSource()
	{
		while (true)
		{
			if (Player.GetInstance().CurrentRoomObject != previousRoom)
			{
				CheckAudioPlacement();
			}
			yield return null;
		}
	}

	private void CheckAudioPlacement()
	{
		previousRoom = Player.GetInstance().CurrentRoomObject;
		Debug.Log($"Nun Room Is {base.CurrentRoomObject}");
		if (Player.GetInstance().CurrentRoomObject == base.CurrentRoomObject)
		{
			Debug.Log("1");
			audioSourceObject.transform.SetParent(base.transform);
			audioSourceObject.transform.localPosition = Vector3.zero;
			return;
		}
		Stack<GameObject> stack = HospitalManager.GetInstance().FindShortestPath(Player.GetInstance().CurrentRoomObject, base.CurrentRoomObject, base.gameObject);
		if (stack.Count > 0)
		{
			List<GameObject> list = new List<GameObject>(stack);
			list.Reverse();
			GameObject gameObject = list[0];
			if (gameObject != base.gameObject)
			{
				audioSourceObject.transform.SetParent(gameObject.transform);
				audioSourceObject.transform.localPosition = Vector3.zero;
			}
		}
	}

	private void CreateChildObjectForCamera()
	{
		childCameraObject = new GameObject("CameraPoint");
		childCameraObject.transform.SetParent(base.transform);
		childCameraObject.transform.localPosition = new Vector3(0f, 150f, 0f);
	}

	private void InitializeVariables()
	{
		blinkManager = BlinkManager.GetInstance();
		captureChangeSpeed = Time.deltaTime / captureTime;
		player = Player.GetInstance().transform;
		originalPlayerSpeed = Player.GetInstance().BaseMoveSpeed;
		originalPlayerInteractionSpeed = Player.GetInstance().BaseInteractionSpeed;
		captureBar?.gameObject.SetActive(value: false);
	}

	protected override void OnTriggerEnter(Collider other)
	{
		HandleCurrentRoom(other);
		if (!(base.CurrentState is SexModeState) && !isTriggered && other.gameObject.HasScript("Player"))
		{
			NunDisapper();
		}
	}

	public void NunDisapper()
	{
		if (!isMuffled)
		{
			StartCoroutine(AudioFade());
			isMuffled = true;
			captureBar.gameObject.SetActive(value: false);
			captureBar.UpdateProgress(0f);
			isCaptured = true;
			animator.Play("Disappear");
		}
	}

	private IEnumerator AudioFade()
	{
		AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();
		if (audioSource == null)
		{
			Debug.LogWarning("AudioFade: No AudioSource found on " + audioSourceObject.name);
			yield break;
		}
		float startVolume = audioSource.volume;
		float duration = 1f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
			yield return null;
		}
		audioSource.volume = 0f;
	}

	protected override void OnTriggerStay(Collider other)
	{
	}

	public void ShowCaptureBar()
	{
		if (captureBar != null)
		{
			StartCoroutine(ShowCaptureBarWithDelay(1f));
		}
		else
		{
			Debug.LogError("captureBar is not assigned in the inspector.");
		}
	}

	private void IncreaseCapture()
	{
		if (Player.GetInstance().gameObject.activeSelf)
		{
			capturePercent += Time.deltaTime / captureTime;
			capturePercent = Mathf.Min(capturePercent, 1f);
			captureBar.UpdateProgress(capturePercent);
			Player instance = Player.GetInstance();
			if (capturePercent <= 0.5f && !hasSlowedInitial)
			{
				instance.AddSpeedModifier("NunInitialSlowdown", 0f - initialSlowdownPercent);
				hasSlowedInitial = true;
			}
			else if (capturePercent > 0.5f && !hasSlowedAdditional)
			{
				instance.AddSpeedModifier("NunAdditionalSlowdown", 0f - additionalSlowdownPercent);
				hasSlowedAdditional = true;
			}
		}
	}

	private void ApplySpeedReduction(float slowdownPercent)
	{
		Player instance = Player.GetInstance();
		instance.CurrentMoveSpeed = originalPlayerSpeed * (1f - slowdownPercent);
		instance.CurrentInteractionSpeed = originalPlayerInteractionSpeed * (1f - slowdownPercent);
	}

	private IEnumerator ShowCaptureBarWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (!captureBar.gameObject.activeSelf)
		{
			captureBar.gameObject.SetActive(value: true);
			captureBar.UpdateProgress(0f);
			captureBar.ShowBar();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (spawnedMapNunIco != null)
		{
			Object.Destroy(spawnedMapNunIco);
		}
		CrossItem.NunIsExists = false;
		if (audioSourceObject != null)
		{
			PauseMenu.UnregisterPausableAudio(audioSource);
		}
		if (audioSourceObject != null)
		{
			Object.Destroy(audioSourceObject);
		}
		if (soundObject != null)
		{
			Object.Destroy(soundObject);
		}
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().RemoveChildByName("NunAirThingEffect");
		}
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().RemoveSpeedModifier("NunInitialSlowdown");
			Player.GetInstance().RemoveSpeedModifier("NunAdditionalSlowdown");
		}
		if (!(captureBar == null) && captureBar.gameObject.activeSelf)
		{
			captureBar.gameObject.SetActive(value: false);
			captureBar.UpdateProgress(0f);
		}
	}

	protected override float GetDirection()
	{
		return 0f;
	}

	protected override void Update()
	{
		HandleTrappedMode();
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Disappear") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
		{
			ExitSexMode();
			CrossItem.NunIsExists = false;
		}
	}

	private void HandleTrappedMode()
	{
		if (!isCaptured)
		{
			if (captureBar.GetComponentInChildren<Image>().color.a > 0.8f)
			{
				IncreaseCapture();
			}
			if (capturePercent >= 1f && !isTriggered)
			{
				DialogueManager.GetInstance().StopDialogueMode();
				Player.GetInstance().ForceStopInteract();
				blinkManager.FadeToBlink(Player.GetInstance().gameObject, AfterBlink, allowVerticalMovement: true, 50f, 0f, base.gameObject);
				isTriggered = true;
			}
		}
	}

	public void AfterBlink()
	{
		captureBar.gameObject.SetActive(value: false);
		captureBar.UpdateProgress(0f);
		isCaptured = true;
		Player.GetInstance().ForceStopInteract();
		ChangeState(new SexModeState());
		if (spawnedMapNunIco != null)
		{
			Object.Destroy(spawnedMapNunIco);
		}
		if (audioSourceObject != null)
		{
			Object.Destroy(audioSourceObject);
		}
		if (soundObject != null)
		{
			Object.Destroy(soundObject);
		}
		NightManager.GetInstance().HandleCatchMode(this);
		CameraFollow.GetInstance().SetFollowTarget(childCameraObject.transform);
	}

	public override void ForceBlink(float speed)
	{
		BlinkManager.GetInstance().BlinkWithCustomSpeed(speed, Player.GetInstance().gameObject, null, allowVerticalMovement: true, 50f);
	}

	protected override void ExitSexMode()
	{
		EnableSkipButton();
		if (CameraFollow.GetInstance().FollowTarget != Player.GetInstance().transform && Player.GetInstance().CurrentHeartNumber > 1)
		{
			CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
		}
		captureBar.gameObject.SetActive(value: false);
		captureBar.UpdateProgress(0f);
		Player.GetInstance().RemoveSpeedModifier("NunInitialSlowdown");
		Player.GetInstance().RemoveSpeedModifier("NunAdditionalSlowdown");
		if (isMuffled)
		{
			Nintendo.ShowNintendo();
			animator.SetInteger("HloopCount", 0);
			loopCount = 0;
			Object.Destroy(base.gameObject);
			player.gameObject.SetActive(value: true);
		}
		else
		{
			base.ExitSexMode();
		}
	}

	public override bool CanBeCleared()
	{
		return false;
	}
}
