using System.Collections;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.UI;

public class SisSupEnemy : Enemy
{
	[SerializeField]
	private ProgressBar hornyBar;

	[SerializeField]
	private GameObject shinePrefab;

	[SerializeField]
	private GameObject keyObjectPrefab;

	[SerializeField]
	private float hornyTime = 20f;

	private float hornyChangeSpeed;

	private float hornyPercent;

	private DoorTransit foundDoor;

	private GameObject roomInstance;

	private GameObject shineParent;

	private DoorTransit[] doorsInRoom;

	private static bool isTrapped;

	public static bool isCaptured;

	private bool keyPlaced;

	private GameObject cameraPoint;

	private bool trapIsOver;

	private bool isTrapTriggering;

	private HideThing hiddenThing;

	public ProgressBar HornyBar
	{
		set
		{
			hornyBar = value;
		}
	}

	public GameObject ShinePrefab
	{
		set
		{
			shinePrefab = value;
		}
	}

	public GameObject KeyObjectPrefab
	{
		set
		{
			keyObjectPrefab = value;
		}
	}

	public float HornyTime
	{
		set
		{
			hornyTime = value;
		}
	}

	public static bool IsTrapped => isTrapped;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public void Nurse2BJSound()
	{
		SoundManager.PlaySound(SoundType.Nurse2BJ);
	}

	public void Nurse2DoggySound()
	{
		SoundManager.PlaySound(SoundType.Hloop1n2);
	}

	public void Nurse2MissionerSound()
	{
		SoundManager.PlaySound(SoundType.Hloop3);
	}

	public void Nurse2ClimaxSound()
	{
		SoundManager.PlaySound(SoundType.HClimax);
	}

	public void MattressPressSound()
	{
		SoundManager.PlaySound(SoundType.MattressPress);
	}

	public void Nurse2MasturbationSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Nurse2Masturbation, base.gameObject);
	}

	public override void EndSexMode()
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

	protected override void Initialize()
	{
		base.Initialize();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sortingOrder = 50;
		blinkManager = BlinkManager.GetInstance();
		cameraPoint = new GameObject("CameraPoint");
		cameraPoint.transform.position = base.transform.position + new Vector3(100f, -50f, 0f);
		if (blinkManager == null)
		{
			Debug.LogError("BlinkManager не найден!");
			return;
		}
		hornyChangeSpeed = ((hornyTime > 0f) ? (Time.deltaTime / hornyTime) : 0f);
		if (shinePrefab != null)
		{
			shineParent = new GameObject("ShineParent");
		}
		else
		{
			Debug.LogError("ShinePrefab не назначен в инспекторе!");
		}
		player = Player.GetInstance()?.transform;
		if (hornyBar != null)
		{
			hornyBar.gameObject.SetActive(value: false);
		}
		else
		{
			Debug.LogError("HornyBar не назначен в инспекторе!");
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
	}

	private void HandleStageChange(string newStage)
	{
		if (NightManager.GetInstance().NormalizeStageName(newStage) != NightManager.GetInstance().NormalizeStageName(CurrentStageName) && NightManager.GetInstance().StopGameCoroutine == null && !EndEvent.isGameOver && CanBeCleared())
		{
			Debug.Log(base.name + " переспавнен из-за смены stage");
			GameObject enemy = GetComponentInParent<NurseSpawner>().gameObject;
			NightManager.GetInstance().ForceRespawnEnemy(enemy);
		}
	}

	private DoorTransit FindDoorWithRoom(string currentRoom)
	{
		DoorTransit[] array = Object.FindObjectsOfType<DoorTransit>();
		foreach (DoorTransit doorTransit in array)
		{
			if (doorTransit.DoorRoom.name == currentRoom)
			{
				return doorTransit;
			}
		}
		return null;
	}

	protected override void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("RoomNamer"))
		{
			currentRoom = other.gameObject.name;
		}
	}

	public void ShowHornyBar()
	{
		if (hornyBar != null)
		{
			StartCoroutine(ShowHornyBarWithDelay(1f));
		}
		else
		{
			Debug.LogError("HornyBar is not assigned in the inspector.");
		}
	}

	private void IncreaseHorny()
	{
		hornyPercent += Time.deltaTime / hornyTime;
		hornyPercent = Mathf.Min(hornyPercent, 1f);
		hornyBar.UpdateProgress(hornyPercent);
	}

	private IEnumerator ShowHornyBarWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (!hornyBar.gameObject.activeSelf)
		{
			hornyBar.gameObject.SetActive(value: true);
			hornyBar.UpdateProgress(0f);
			hornyBar.ShowBar();
		}
	}

	protected override float GetDirection()
	{
		return 0f;
	}

	protected override void Update()
	{
		if (isTrapped)
		{
			HandleTrappedMode();
		}
		else
		{
			CheckIfTrapped();
		}
	}

	private void HandleTrappedMode()
	{
		if (isCaptured)
		{
			return;
		}
		if (hornyBar != null)
		{
			Image componentInChildren = hornyBar.GetComponentInChildren<Image>();
			if ((object)componentInChildren != null && componentInChildren.color.a > 0.8f)
			{
				IncreaseHorny();
			}
		}
		if (hornyPercent >= 1f && !trapIsOver)
		{
			GameObject[] movableObjects = new GameObject[2]
			{
				CameraFollow.GetInstance().gameObject,
				Player.GetInstance().gameObject
			};
			Player.GetInstance().ForceStopInteract();
			blinkManager.FadeToBlink(movableObjects, AfterBlink, allowVerticalMovement: false, 100f, 0f, base.gameObject);
			trapIsOver = true;
			ShineItem[] array = Object.FindObjectsOfType<ShineItem>();
			foreach (ShineItem shineItem in array)
			{
				if (shineItem != null)
				{
					shineItem.StartUnshineEffect();
				}
			}
		}
		Player instance = Player.GetInstance();
		if (isTrapped && currentRoom != instance.GetCurrentRoom())
		{
			isTrapped = false;
			DoorTransit doorTransit = Player.GetInstance().InteractObject as DoorTransit;
			if (doorTransit != null)
			{
				SoundManager.PlaySoundAtObject(SoundType.DoorOpen, doorTransit.gameObject);
				SoundManager.PlaySoundAtObject(SoundType.DoorOpen, doorTransit.GotoObject.gameObject);
			}
			StartCoroutine(EndSexModeWithDelay(0.5f));
		}
	}

	private IEnumerator EndSexModeWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		ExitSexMode();
	}

	public void AfterBlink()
	{
		hornyBar?.gameObject.SetActive(value: false);
		hornyBar?.UpdateProgress(0f);
		if (Player.GetInstance().CurrentState is PlayerHideState)
		{
			HideThing[] array = Object.FindObjectsOfType<HideThing>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ForceExit();
			}
		}
		Player.GetInstance().ForceStopInteract();
		player.gameObject.SetActive(value: false);
		ClearShineItems();
		isCaptured = true;
		CameraFollow.GetInstance().SetFollowTarget(cameraPoint.transform);
		ChangeState(new SexModeState());
		GameObject gameObject = GameObject.Find("HideUI");
		if (gameObject != null)
		{
			gameObject.SetActive(value: false);
		}
		else
		{
			Debug.LogWarning("Объект HideUI не найден!");
		}
	}

	private void CheckIfTrapped()
	{
		if (currentRoom == Player.GetInstance().GetCurrentRoom() && !isTrapTriggering)
		{
			StartCoroutine(HandleTrapWithDelay());
		}
	}

	private IEnumerator HandleTrapWithDelay()
	{
		isTrapTriggering = true;
		yield return new WaitForSeconds(1f);
		isTrapped = true;
		Debug.Log("SUP NURSE TRUE");
		NightManager.GetInstance().HandleCatchMode(this);
		ShowHornyBar();
		Nintendo.HideNintendo();
		GameObject[] array = GameObject.FindGameObjectsWithTag("RoomNamer");
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name == currentRoom)
			{
				roomInstance = gameObject;
				break;
			}
		}
		InitializeRoomState();
	}

	private void InitializeRoomState()
	{
		BoxCollider component = roomInstance.GetComponent<BoxCollider>();
		Collider[] array = Physics.OverlapBox(component.bounds.center, component.size / 2f);
		InitializeDoorsInRoom(array);
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			HideThing component2 = array2[i].GetComponent<HideThing>();
			if (component2 != null)
			{
				hiddenThing = component2;
				hiddenThing.enabled = false;
				break;
			}
		}
		List<Collider> furnitureColliders = GetFurnitureColliders(array);
		PlaceShineItems(furnitureColliders);
	}

	private void InitializeDoorsInRoom(Collider[] collidersInsideRoom)
	{
		List<DoorTransit> list = new List<DoorTransit>();
		for (int i = 0; i < collidersInsideRoom.Length; i++)
		{
			DoorTransit component = collidersInsideRoom[i].GetComponent<DoorTransit>();
			if (component != null)
			{
				list.Add(component);
				component.KeyItemName = "subSisKey";
			}
		}
		doorsInRoom = list.ToArray();
	}

	private List<Collider> GetFurnitureColliders(Collider[] collidersInsideRoom)
	{
		List<Collider> list = new List<Collider>();
		foreach (Collider collider in collidersInsideRoom)
		{
			if (collider.CompareTag("Furniture"))
			{
				list.Add(collider);
			}
		}
		return list;
	}

	private void PlaceShineItems(List<Collider> furnitureColliders)
	{
		int num = Random.Range(0, furnitureColliders.Count);
		for (int i = 0; i < furnitureColliders.Count; i++)
		{
			Collider collider = furnitureColliders[i];
			GameObject gameObject = Object.Instantiate(shinePrefab, collider.bounds.center, Quaternion.identity, shineParent.transform);
			if (i == num && !keyPlaced)
			{
				GameObject shineItem = Object.Instantiate(keyObjectPrefab, gameObject.transform.position, Quaternion.identity);
				gameObject.GetComponent<ShineItem>().SetShineItem(shineItem);
				keyPlaced = true;
			}
		}
	}

	private void SetDoorsTrigger()
	{
		if (doorsInRoom != null && doorsInRoom.Length != 0)
		{
			DoorTransit[] array = doorsInRoom;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].KeyItemName = null;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		trapIsOver = false;
		if (hiddenThing != null)
		{
			hiddenThing.enabled = true;
		}
		isCaptured = false;
		if (hornyBar != null)
		{
			hornyBar?.gameObject.SetActive(value: false);
			hornyBar?.UpdateProgress(0f);
		}
		ClearShineItems();
		isTrapped = false;
		SetDoorsTrigger();
		NurseSpawner componentInParent = GetComponentInParent<NurseSpawner>();
		if (componentInParent != null)
		{
			Object.Destroy(componentInParent);
		}
		else
		{
			Debug.Log("NurseSpawner не найден.");
		}
	}

	protected override void ExitSexMode()
	{
		EnableSkipButton();
		hornyBar?.gameObject.SetActive(value: false);
		hornyBar?.UpdateProgress(0f);
		ClearShineItems();
		SetDoorsTrigger();
		NurseSpawner componentInParent = GetComponentInParent<NurseSpawner>();
		if (componentInParent != null && Player.GetInstance().CurrentHeartNumber > 1)
		{
			Object.Destroy(componentInParent);
		}
		else
		{
			Debug.Log("NurseSpawner не найден.");
		}
		if (Player.GetInstance().CurrentHeartNumber > 1)
		{
			Nintendo.ShowNintendo();
			if (CameraFollow.GetInstance().FollowTarget.gameObject != Player.GetInstance().gameObject)
			{
				CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
			}
		}
		if (isCaptured)
		{
			if (!GlobalSaveSystem.GetProgress("HintNSupNurseKey") && !GlobalSaveSystem.GetProgress("HintNSupNurseKeyReady"))
			{
				GlobalSaveSystem.SetProgress("HintNSupNurseKeyReady", value: true);
			}
			Image[] inventoryCells = Player.GetInstance().InventoryCells;
			for (int i = 0; i < inventoryCells.Length; i++)
			{
				QuestItem componentInChildren = inventoryCells[i].GetComponentInChildren<QuestItem>();
				if (componentInChildren != null && componentInChildren.ItemName == "subSisKey")
				{
					Object.Destroy(componentInChildren.gameObject);
				}
			}
			_ = Player.GetInstance().CurrentHeartNumber;
			_ = 1;
			foundDoor = FindDoorWithRoom(currentRoom);
			if (foundDoor != null)
			{
				Player.GetInstance().transform.position = foundDoor.GotoObject.transform.position;
			}
			base.ExitSexMode();
		}
		else
		{
			if (!GlobalSaveSystem.GetProgress("HintNSupNurseKey"))
			{
				GlobalSaveSystem.SetProgress("HintNSupNurseKey", value: true);
				GlobalSaveSystem.SetProgress("HintNSupNurseKeyReady", value: false);
			}
			Image[] inventoryCells = Player.GetInstance().InventoryCells;
			for (int i = 0; i < inventoryCells.Length; i++)
			{
				QuestItem componentInChildren2 = inventoryCells[i].GetComponentInChildren<QuestItem>();
				if (componentInChildren2 != null && componentInChildren2.ItemName == "subSisKey")
				{
					Object.Destroy(componentInChildren2.gameObject);
				}
			}
			Object.Destroy(base.gameObject);
		}
		if (Player.GetInstance().CurrentHeartNumber >= 1)
		{
			isCaptured = false;
			isTrapped = false;
		}
	}

	public void ClearShineItems()
	{
		Object.Destroy(shineParent);
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
