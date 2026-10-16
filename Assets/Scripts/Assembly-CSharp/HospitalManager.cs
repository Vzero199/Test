using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HospitalManager : MonoBehaviour
{
	public class RoomGraph
	{
		private Dictionary<GameObject, List<GameObject>> connections = new Dictionary<GameObject, List<GameObject>>();

		public IEnumerable<GameObject> Rooms => connections.Keys;

		public void AddConnection(GameObject room, GameObject door)
		{
			if (!connections.ContainsKey(room))
			{
				connections[room] = new List<GameObject>();
			}
			connections[room].Add(door);
		}

		public List<GameObject> GetDoors(GameObject room)
		{
			if (!connections.ContainsKey(room))
			{
				return new List<GameObject>();
			}
			return connections[room];
		}
	}

	public static RoomGraph roomGraph = new RoomGraph();

	private static HospitalManager instance;

	private Dictionary<GameObject, GameObject> objectToRoomCache;

	private static Dictionary<Collider, GameObject> roomDictionary = new Dictionary<Collider, GameObject>();

	private Dictionary<GameObject, DoorTransit> cachedDoorTransits = new Dictionary<GameObject, DoorTransit>();

	private Dictionary<GameObject, InteractionObject> interactionObjectCache = new Dictionary<GameObject, InteractionObject>();

	private void Start()
	{
		InitializeRoomGraph();
		instance = this;
	}

	public static HospitalManager GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<HospitalManager>();
		}
		return instance;
	}

	private void Awake()
	{
		InitializeSceneCache();
	}

	public void InitializeSceneCache()
	{
		FillInteractionObjectCache();
		InitializeDoorCache();
		InitializeRooms();
		InitializeRoomCache();
	}

	public DoorTransit GetDoorTransit(GameObject obj)
	{
		return GetInteractionObjectCache(obj) as DoorTransit;
	}

	public InteractionObject GetInteractionObjectCache(GameObject obj)
	{
		if (!interactionObjectCache.ContainsKey(obj))
		{
			interactionObjectCache[obj] = obj.GetComponent<InteractionObject>();
		}
		return interactionObjectCache[obj];
	}

	public void InitializeDoorCache()
	{
		cachedDoorTransits = new Dictionary<GameObject, DoorTransit>();
		foreach (KeyValuePair<GameObject, InteractionObject> item in interactionObjectCache)
		{
			if (item.Value is DoorTransit value)
			{
				cachedDoorTransits.Add(item.Key, value);
			}
		}
	}

	private void FillInteractionObjectCache()
	{
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.TryGetComponent<InteractionObject>(out var component))
			{
				interactionObjectCache[gameObject] = component;
			}
		}
	}

	private void InitializeRooms()
	{
		roomDictionary.Clear();
		GameObject[] array = GameObject.FindGameObjectsWithTag("RoomNamer");
		foreach (GameObject gameObject in array)
		{
			Collider component = gameObject.GetComponent<Collider>();
			if (component != null)
			{
				roomDictionary[component] = gameObject;
			}
		}
	}

	public void InvalidateRoomCache()
	{
		objectToRoomCache.Clear();
	}

	private void OnDisable()
	{
		objectToRoomCache.Clear();
	}

	public int CountDoorTransitsInCurrentRoom(GameObject currentRoomObject)
	{
		int num = 0;
		foreach (KeyValuePair<GameObject, DoorTransit> cachedDoorTransit in cachedDoorTransits)
		{
			if (currentRoomObject == GetInstance().GetRoomContainingObject(cachedDoorTransit.Key))
			{
				num++;
			}
		}
		return num;
	}

	public GameObject GetRoomContainingObject(GameObject obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (objectToRoomCache.TryGetValue(obj, out var cachedRoom))
		{
			if (cachedRoom == null || !roomDictionary.Any((KeyValuePair<Collider, GameObject> pair) => pair.Value == cachedRoom))
			{
				objectToRoomCache.Remove(obj);
				return null;
			}
			KeyValuePair<Collider, GameObject> keyValuePair = roomDictionary.FirstOrDefault((KeyValuePair<Collider, GameObject> pair) => pair.Value == cachedRoom);
			if (keyValuePair.Key != null && keyValuePair.Key.bounds.Contains(obj.transform.position))
			{
				return cachedRoom;
			}
			objectToRoomCache.Remove(obj);
		}
		foreach (KeyValuePair<Collider, GameObject> item in roomDictionary)
		{
			if (!(item.Key == null) && !(item.Value == null) && item.Key.bounds.Contains(obj.transform.position))
			{
				objectToRoomCache[obj] = item.Value;
				return item.Value;
			}
		}
		return null;
	}

	public void InitializeRoomCache()
	{
		objectToRoomCache = new Dictionary<GameObject, GameObject>();
		foreach (KeyValuePair<GameObject, DoorTransit> cachedDoorTransit in cachedDoorTransits)
		{
			GameObject key = cachedDoorTransit.Key;
			foreach (KeyValuePair<Collider, GameObject> item in roomDictionary)
			{
				if (item.Key.bounds.Contains(key.transform.position))
				{
					objectToRoomCache[key] = item.Value;
					break;
				}
			}
		}
	}

	private void InitializeRoomGraph()
	{
		roomGraph = new RoomGraph();
		DoorTransit[] array = Object.FindObjectsOfType<DoorTransit>();
		foreach (DoorTransit doorTransit in array)
		{
			GameObject doorRoom = doorTransit.DoorRoom;
			if (doorRoom != null)
			{
				roomGraph.AddConnection(doorRoom, doorTransit.gameObject);
			}
		}
	}

	public string FindNearestRoomStartingWith(GameObject startRoom, string prefix)
	{
		Queue<GameObject> queue = new Queue<GameObject>();
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		queue.Enqueue(startRoom);
		hashSet.Add(startRoom);
		while (queue.Count > 0)
		{
			GameObject gameObject = queue.Dequeue();
			if (gameObject.name.StartsWith(prefix))
			{
				return gameObject.name;
			}
			foreach (GameObject door in roomGraph.GetDoors(gameObject))
			{
				GameObject gameObject2 = door.GetComponent<DoorTransit>()?.GotoObject.GetComponent<DoorTransit>()?.DoorRoom;
				if (gameObject2 != null && !hashSet.Contains(gameObject2))
				{
					queue.Enqueue(gameObject2);
					hashSet.Add(gameObject2);
				}
			}
		}
		return null;
	}

	public Stack<GameObject> FindShortestPath(GameObject startRoom, GameObject endRoom, GameObject objectInEndRoom)
	{
		Queue<GameObject> queue = new Queue<GameObject>();
		Dictionary<GameObject, GameObject> dictionary = new Dictionary<GameObject, GameObject>();
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		Vector3 targetPos = objectInEndRoom.transform.position;
		foreach (GameObject item in from door in roomGraph.GetDoors(startRoom)
			orderby Mathf.Abs(door.transform.position.x - targetPos.x)
			select door)
		{
			queue.Enqueue(item);
			dictionary[item] = null;
		}
		hashSet.Add(startRoom);
		while (queue.Count > 0)
		{
			GameObject gameObject = queue.Dequeue();
			DoorTransit component = gameObject.GetComponent<DoorTransit>();
			if (component == null)
			{
				continue;
			}
			GameObject gameObject2 = component.GotoObject?.GetComponent<DoorTransit>()?.DoorRoom;
			if (gameObject2 == null)
			{
				continue;
			}
			if (gameObject2 == endRoom)
			{
				return ConstructDoorPath(dictionary, gameObject);
			}
			if (hashSet.Contains(gameObject2))
			{
				continue;
			}
			hashSet.Add(gameObject2);
			foreach (GameObject item2 in from door in roomGraph.GetDoors(gameObject2)
				orderby Mathf.Abs(door.transform.position.x - targetPos.x)
				select door)
			{
				if (!dictionary.ContainsKey(item2))
				{
					queue.Enqueue(item2);
					dictionary[item2] = gameObject;
				}
			}
		}
		return new Stack<GameObject>();
	}

	private Stack<GameObject> ConstructDoorPath(Dictionary<GameObject, GameObject> cameFrom, GameObject endDoor)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject gameObject = endDoor;
		while (gameObject != null)
		{
			list.Add(gameObject);
			gameObject = (cameFrom.ContainsKey(gameObject) ? cameFrom[gameObject] : null);
		}
		Stack<GameObject> stack = new Stack<GameObject>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			stack.Push(list[num]);
		}
		return stack;
	}

	public void DebugRoomGraph()
	{
		foreach (GameObject room in roomGraph.Rooms)
		{
			Debug.Log("Room: " + room.name);
			foreach (GameObject door in roomGraph.GetDoors(room))
			{
				Debug.Log("  Door: " + door.name);
			}
		}
	}
}
