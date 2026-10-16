using System;
using System.Collections;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;

public class DoorTransit : InteractionObject
{
	private GameObject lockDoorUI;

	private const string PLAYER_STEP_SOURCE_TAG = "PlayerStepUniqueSource";

	[SerializeField]
	private GameObject gotoObject;

	[SerializeField]
	private GameObject doorRoom;

	private Dictionary<Entities, float> lastPlayTime = new Dictionary<Entities, float>();

	private Dictionary<Entities, float> cooldown = new Dictionary<Entities, float>();

	private HashSet<Entities> currentNearbyEntitiesSet = new HashSet<Entities>();

	private static readonly Dictionary<Entities, DoorTransit> NearestDoorCache = new Dictionary<Entities, DoorTransit>();

	private static readonly List<DoorTransit> AllDoors = new List<DoorTransit>();

	[SerializeField]
	private float soundVolume = 0.2f;

	private bool IsRecentlyOpened;

	public override string KeyItemName
	{
		get
		{
			return keyItemName;
		}
		set
		{
			keyItemName = value;
			UpdateLockUIVisibility();
			Player.GetInstance()?.HandlePlayerInteractUI();
			Player.GetInstance()?.ManualTriggerCheck();
		}
	}

	public GameObject GotoObject => gotoObject;

	public GameObject DoorRoom => doorRoom;

	private void UpdateLockUIVisibility()
	{
		bool active = keyItemName == "subSisKey" || keyItemName == "close";
		lockDoorUI.SetActive(active);
	}

	private void Awake()
	{
		lockDoorUI = base.transform.Find("LockDoorUI").gameObject;
		if (doorRoom == null)
		{
			doorRoom = GetRoomContainingObject(base.gameObject);
		}
		UpdateLockUIVisibility();
		AllDoors.Add(this);
	}

	private void OnDestroy()
	{
		AllDoors.Remove(this);
	}

	private void OnEnable()
	{
		SoundLoopBus.OnLoopSoundPlay = (Action<Entities>)Delegate.Combine(SoundLoopBus.OnLoopSoundPlay, new Action<Entities>(HandleLoopSound));
	}

	private void OnDisable()
	{
		SoundLoopBus.OnLoopSoundPlay = (Action<Entities>)Delegate.Remove(SoundLoopBus.OnLoopSoundPlay, new Action<Entities>(HandleLoopSound));
	}

	private void HandleLoopSound(Entities entity)
	{
		if (entity == null || gotoObject == null || base.gameObject.name.Contains("From", StringComparison.OrdinalIgnoreCase) || !IsEntityInsideSoundZone(entity))
		{
			return;
		}
		if (!NearestDoorCache.TryGetValue(entity, out var value) || value == null || value != this)
		{
			DoorTransit doorTransit = FindNearestDoor(entity);
			NearestDoorCache[entity] = doorTransit;
			if (doorTransit != this)
			{
				return;
			}
		}
		PlayEntitySound(entity);
	}

	private DoorTransit FindNearestDoor(Entities entity)
	{
		DoorTransit result = null;
		float num = float.MaxValue;
		foreach (DoorTransit allDoor in AllDoors)
		{
			if (!(allDoor == null) && !(allDoor.gotoObject == null) && allDoor.IsEntityInsideSoundZone(entity))
			{
				float sqrMagnitude = (entity.transform.position - allDoor.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = allDoor;
				}
			}
		}
		return result;
	}

	private bool IsEntityInsideSoundZone(Entities entity)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = entity.transform.position - position;
		if (Mathf.Abs(vector.x) <= 300f && Mathf.Abs(vector.y) <= 200f)
		{
			return Mathf.Abs(vector.z) <= 500f;
		}
		return false;
	}

	private void PlayEntitySound(Entities entity)
	{
		AudioClip movementSound = entity.GetMovementSound();
		if (movementSound == null || gotoObject == null || (cooldown.TryGetValue(entity, out var value) && Time.time - value < 0.5f))
		{
			return;
		}
		cooldown[entity] = Time.time;
		GameObject gameObject = new GameObject("DoorTransientSound");
		if (entity is Player)
		{
			gameObject.tag = "PlayerStepUniqueSource";
		}
		if (IsDistanceOnlyDoor())
		{
			PlayDistanceOnlySound(entity, movementSound);
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		gameObject.transform.parent = gotoObject.transform;
		float num = entity.transform.position.x - base.transform.position.x;
		Vector3 position = gotoObject.transform.position;
		gameObject.transform.position = new Vector3(position.x + num, position.y, position.z);
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
		if (sFXGroup != null)
		{
			audioSource.outputAudioMixerGroup = sFXGroup;
		}
		ConfigureAudioSource(audioSource, entity);
		audioSource.clip = movementSound;
		float num2 = Mathf.Abs(entity.transform.position.x - base.transform.position.x);
		float num3 = 300f;
		float num4 = Mathf.Clamp01(num2 / num3);
		float num5 = 1f - num4;
		float volume = soundVolume * 0.3f * num5;
		audioSource.volume = volume;
		audioSource.Play();
		StartCoroutine(DestroyAfterPlay(audioSource));
	}

	private void PlayDistanceOnlySound(Entities entity, AudioClip clip)
	{
		GameObject obj = new GameObject("DoorDistanceSound");
		obj.transform.position = gotoObject.transform.position;
		AudioSource audioSource = obj.AddComponent<AudioSource>();
		AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
		if (sFXGroup != null)
		{
			audioSource.outputAudioMixerGroup = sFXGroup;
		}
		ConfigureAudioSource(audioSource, entity);
		audioSource.clip = clip;
		float num = Vector3.Distance(entity.transform.position, base.transform.position);
		float num2 = 700f;
		float num3 = Mathf.Clamp01(num / num2);
		float num4 = 1f - num3;
		audioSource.volume = soundVolume * 0.5f * num4;
		audioSource.Play();
		StartCoroutine(DestroyAfterPlay(audioSource));
	}

	private IEnumerator DestroyAfterPlay(AudioSource source)
	{
		if (!(source == null))
		{
			while (source != null && source.isPlaying)
			{
				yield return null;
			}
			if (source != null)
			{
				UnityEngine.Object.Destroy(source.gameObject);
			}
		}
	}

	private bool ShouldPlaySoundForEntity(Entities entity)
	{
		if (entity.GetMovementSound() == null)
		{
			return false;
		}
		if (entity is Enemy enemy)
		{
			if (enemy.CurrentState is IdleState || enemy.CurrentState is InteractState)
			{
				return false;
			}
			return true;
		}
		if (entity is Nintendo nintendo)
		{
			if (nintendo.CurrentState is NintendoIdleState)
			{
				return false;
			}
			Animator component = nintendo.GetComponent<Animator>();
			if (component != null)
			{
				return component.GetCurrentAnimatorStateInfo(0).IsName("Run");
			}
		}
		else if (entity is Player player)
		{
			return player.CurrentState is PlayerRunState;
		}
		return true;
	}

	private void ConfigureAudioSource(AudioSource source, Entities entity)
	{
		source.spatialBlend = 1f;
		source.loop = false;
		source.dopplerLevel = 0f;
		source.minDistance = 10f;
		source.maxDistance = 400f;
		source.rolloffMode = AudioRolloffMode.Linear;
		source.playOnAwake = false;
	}

	private GameObject GetRoomContainingObject(GameObject obj)
	{
		Vector3 vector = CalculateRoomBounds(obj);
		Collider[] array = Physics.OverlapBox(obj.transform.position, vector / 2f);
		foreach (Collider collider in array)
		{
			if (collider.CompareTag("RoomNamer"))
			{
				return collider.gameObject;
			}
		}
		return null;
	}

	private Vector3 CalculateRoomBounds(GameObject roomObject)
	{
		BoxCollider component = roomObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			return component.size;
		}
		Renderer component2 = roomObject.GetComponent<Renderer>();
		if (component2 != null)
		{
			return component2.bounds.size;
		}
		return new Vector3(2f, 2f, 1000f);
	}

	public override bool CheckConditions()
	{
		if (Player.GetInstance().CanInteract && base.CheckConditions())
		{
			return !IsRecentlyOpened;
		}
		return false;
	}

	protected override bool CheckInteractButton()
	{
		if (Player.GetInstance().IsActionButton)
		{
			return CheckConditions();
		}
		return false;
	}

	private void PlayDoorSound()
	{
		if (!EndEvent.GetInstance().IsAnyScene() && !PauseMenu.IsPaused && !WhiteWomanEnemy.IsCharmGame && !SisSupEnemy.IsTrapped)
		{
			if (base.gameObject.name.IndexOf("palate", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("stage", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				SoundManager.PlaySoundAtObject(SoundType.DoorOpen, base.gameObject);
				SoundManager.PlaySoundAtObject(SoundType.DoorOpen, gotoObject);
			}
			if (base.gameObject.name.IndexOf("From", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("WC", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				SoundManager.PlaySoundAtObject(SoundType.WalkStairs, base.gameObject);
				SoundManager.PlaySoundAtObject(SoundType.WalkStairs, gotoObject);
			}
			if (base.gameObject.name.IndexOf("Utility", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("Staff", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("Bridge", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("Chief", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("Changing", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				SoundManager.PlaySoundAtObject(SoundType.UtilityOpenDoor, base.gameObject);
				SoundManager.PlaySoundAtObject(SoundType.UtilityOpenDoor, gotoObject);
			}
		}
	}

	private bool IsDistanceOnlyDoor()
	{
		string text = base.gameObject.name;
		if (text.IndexOf("Bridge", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("Utility", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("Staff", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("Chief", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("From", StringComparison.OrdinalIgnoreCase) < 0)
		{
			return text.IndexOf("WC", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return true;
	}

	protected override void InteractComplete()
	{
		PlayDoorSound();
		base.InteractComplete();
		BlinkManager blinkManager = UnityEngine.Object.FindObjectOfType<BlinkManager>();
		if (interactableObject.GetComponent<Enemy>() != null)
		{
			InteractionManager.Instance.StartInteractionCooldown(interactableObject.GetComponent<Enemy>());
		}
		if (interactableObject.GetComponent<Nintendo>() != null)
		{
			InteractionManager.Instance.StartInteractionCooldown(interactableObject.GetComponent<Nintendo>());
		}
		if (blinkManager != null)
		{
			if (interactableObject.GetComponent<Player>() != null)
			{
				blinkManager.FadeToBlink(interactableObject, CheckForCatch, allowVerticalMovement: true, 0f, 0f, gotoObject);
			}
			else
			{
				blinkManager.BlinkInstant(interactableObject, null, allowVerticalMovement: true, 0f, 0f, gotoObject);
			}
		}
		else
		{
			Debug.LogError("BlinkManager not found in the scene.");
		}
	}

	private void CheckForCatch()
	{
		if (!interactableObject.CompareTag("Player"))
		{
			return;
		}
		Player component = interactableObject.GetComponent<Player>();
		if (component != null)
		{
			InteractionManager.Instance.StartInteractionCooldown(component);
			component.LastOpenDoor = this;
			MebeebEnemy component2 = GetComponent<MebeebEnemy>();
			if (component2 != null)
			{
				component2.CatchTrigger();
			}
		}
		gotoObject.GetComponent<DoorTransit>().BoxColliderDisable();
	}

	public void BoxColliderDisable()
	{
		StartCoroutine(DisableColliderTemporary());
	}

	private IEnumerator DisableColliderTemporary()
	{
		BoxCollider boxCollider1 = base.gameObject.GetComponent<BoxCollider>();
		yield return new WaitForSeconds(0.5f);
		IsRecentlyOpened = true;
		yield return new WaitForSeconds(1f);
		IsRecentlyOpened = false;
		boxCollider1.enabled = false;
		boxCollider1.enabled = true;
	}
}
