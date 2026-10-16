using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;

public class MebeebEnemy : ObjectEnemy
{
	private GameObject spritePrefab;

	public Vector3 spawnOffset = new Vector3(120f, -162f, 0f);

	private GameObject secondDoor;

	private bool isCaptured;

	private bool bearIsHappy;

	private static Dictionary<string, List<GameObject>> usedItemsByDoorPair = new Dictionary<string, List<GameObject>>();

	private string doorPairKey;

	private GameObject bearObject;

	private GameObject[] availableItems;

	[SerializeField]
	private GameObject[] bearPool;

	private List<GameObject> bearObjects = new List<GameObject>();

	public int ExcludeBearIndex { get; set; } = -1;

	public MebibPlush Plush { get; private set; }

	public bool BearIsHappy
	{
		get
		{
			return bearIsHappy;
		}
		set
		{
			bearIsHappy = value;
		}
	}

	public GameObject[] BearPool
	{
		set
		{
			bearPool = value;
		}
	}

	public bool IsCaptured => isCaptured;

	protected override void Start()
	{
		doorPairKey = GetDoorPairKey(base.gameObject.name);
		SpawnSpriteObject();
		currentRoom = base.gameObject.GetComponent<DoorTransit>().GotoObject.GetComponent<DoorTransit>().DoorRoom.name;
	}

	private string GetDoorPairKey(string doorName)
	{
		if (doorName.EndsWith("1") || doorName.EndsWith("2"))
		{
			return doorName.Remove(doorName.Length - 1);
		}
		return doorName;
	}

	private GameObject GetAvailableItem(GameObject[] itemPool)
	{
		if (itemPool == null || itemPool.Length == 0)
		{
			return null;
		}
		if (!usedItemsByDoorPair.ContainsKey(doorPairKey))
		{
			usedItemsByDoorPair[doorPairKey] = new List<GameObject>();
		}
		List<GameObject> list = usedItemsByDoorPair[doorPairKey];
		List<GameObject> list2 = new List<GameObject>();
		foreach (GameObject item in itemPool)
		{
			if (!list.Contains(item))
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0)
		{
			list.Clear();
			list2 = new List<GameObject>(itemPool);
		}
		if (list2.Count > 0)
		{
			int index = Random.Range(0, list2.Count);
			GameObject gameObject = list2[index];
			list.Add(gameObject);
			return gameObject;
		}
		return null;
	}

	public void CatchTrigger()
	{
		if (!bearIsHappy)
		{
			Debug.Log("Has Triggered");
			if (!isCaptured)
			{
				HandleCapture();
			}
		}
	}

	private void HandleCapture()
	{
		Player instance = Player.GetInstance();
		if (!GlobalSaveSystem.GetProgress("HintNDoll") && !GlobalSaveSystem.GetProgress("HintNDollReady"))
		{
			GlobalSaveSystem.SetProgress("HintNDollReady", value: true);
		}
		instance.ChangeState(new PlayerSexModeState());
		instance.IsPaused = true;
		instance.transform.rotation = Quaternion.identity;
		Vector3 position = instance.LastOpenDoor.GotoObject.transform.position + new Vector3(398f, 20f, 0f);
		instance.transform.position = position;
		SpriteRenderer component = instance.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.sortingLayerName = "Foreground";
			component.sortingOrder = 185;
		}
		SoundManager.PlaySound(SoundType.DollCatch);
		isCaptured = true;
		NightManager.GetInstance().HandleCatchMode(this);
	}

	protected override void Update()
	{
		base.Update();
		if (isCaptured && !(Player.GetInstance().CurrentState is PlayerSexModeState))
		{
			MebeebGone();
		}
	}

	public void MebeebGone()
	{
		if (bearObject != null)
		{
			Object.Destroy(bearObject.GetComponent<MebibPlush>());
			Object.Destroy(bearObject.GetComponent<Animator>());
			bearObject.gameObject.AddComponent<FadeAway>().StartFading(1f);
		}
		bearObjects.Clear();
		if (usedItemsByDoorPair.ContainsKey(doorPairKey))
		{
			usedItemsByDoorPair.Remove(doorPairKey);
		}
		string text = base.gameObject.name;
		GameObject gameObject = GameObject.Find(text.EndsWith("1") ? (text.Remove(text.Length - 1) + "2") : (text.Remove(text.Length - 1) + "1"));
		if (gameObject != null)
		{
			MebeebEnemy component = gameObject.GetComponent<MebeebEnemy>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
		if (this != null)
		{
			Object.Destroy(this);
		}
	}

	protected override void FixedUpdate()
	{
	}

	protected override void OnTriggerEnter(Collider collision)
	{
	}

	protected override void OnTriggerStay(Collider collision)
	{
	}

	private void SpawnSpriteObject()
	{
		if (bearPool == null || bearPool.Length < 2)
		{
			Debug.LogWarning("Bear pool is empty or does not have enough elements.");
			return;
		}
		int num;
		do
		{
			num = Random.Range(0, bearPool.Length);
		}
		while (num == ExcludeBearIndex);
		spritePrefab = bearPool[num];
		Vector3 position = base.transform.position + spawnOffset;
		bearObject = Object.Instantiate(spritePrefab, position, Quaternion.identity);
		bearObject.transform.SetParent(base.gameObject.transform);
		bearObject.GetComponent<SpriteRenderer>().sortingLayerName = "Entity";
		bearObjects.Add(bearObject);
		Plush = bearObject.GetComponent<MebibPlush>();
		if (Plush == null)
		{
			Debug.LogWarning("MebibPlush component not found on spawned object!");
		}
		else
		{
			GameObject[] runtimeItemPool = Plush.GetRuntimeItemPool();
			if (runtimeItemPool != null && runtimeItemPool.Length != 0)
			{
				GameObject availableItem = GetAvailableItem(runtimeItemPool);
				if (availableItem != null)
				{
					Plush.WantedItem = availableItem;
				}
			}
		}
		AddComponentsTobearObject(bearObject);
		string text = base.gameObject.name;
		if (text.EndsWith("1"))
		{
			CreateMebeebInSecondDoor(text.Remove(text.Length - 1) + "2", num);
		}
		else if (text.EndsWith("2"))
		{
			CreateMebeebInSecondDoor(text.Remove(text.Length - 1) + "1", num);
		}
	}

	private void CreateMebeebInSecondDoor(string secondDoorName, int excludeIndex)
	{
		GameObject gameObject = GameObject.Find(secondDoorName);
		if (gameObject != null)
		{
			if (gameObject.GetComponent<MebeebEnemy>() == null)
			{
				MebeebEnemy mebeebEnemy = gameObject.AddComponent<MebeebEnemy>();
				mebeebEnemy.bearPool = bearPool;
				mebeebEnemy.ExcludeBearIndex = excludeIndex;
			}
		}
		else
		{
			Debug.LogWarning("Second door not found: " + secondDoorName);
		}
	}

	private void AddComponentsTobearObject(GameObject obj)
	{
		if (obj != null)
		{
			Collider collider = obj.GetComponent<Collider>();
			if (collider == null)
			{
				collider = obj.AddComponent<BoxCollider>();
			}
			collider.isTrigger = true;
		}
		else
		{
			Debug.LogWarning("Spawned object is null.");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MebeebGone();
	}
}
