using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GangerSpawner : MonoBehaviour
{
	private Sprite defaultSprite;

	private SpriteRenderer spriteRenderer;

	private Vector3 colliderSize;

	private Vector3 colliderCenter;

	private GameObject currentPosterEnemy;

	private List<GameObject> existingPosterGangers;

	public RuntimeAnimatorController posterAnimatorController;

	private float respawnTime;

	private float disappearanceDelay;

	private GameObject currentRoom;

	public bool UseRoomBasedSpawn;

	public GameObject CurrentRoom
	{
		get
		{
			return currentRoom;
		}
		set
		{
			currentRoom = value;
		}
	}

	public Vector3 ColliderSize
	{
		get
		{
			return colliderSize;
		}
		set
		{
			colliderSize = value;
		}
	}

	public Vector3 ColliderCenter
	{
		get
		{
			return colliderCenter;
		}
		set
		{
			colliderCenter = value;
		}
	}

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

	public Sprite DefaultSprite
	{
		set
		{
			defaultSprite = value;
		}
	}

	private void Start()
	{
		BoxCollider boxCollider = base.gameObject.GetComponent<BoxCollider>();
		if (boxCollider == null)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		boxCollider.size = ColliderSize;
		boxCollider.center = ColliderCenter;
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			Debug.LogError("SpriteRenderer not found!");
		}
		else
		{
			StartCoroutine(ChangeSpriteAfterTime(respawnTime));
		}
	}

	private void Update()
	{
		Player instance = Player.GetInstance();
		if (instance == null)
		{
			Debug.LogWarning("Player instance not found.");
		}
		else
		{
			if (!(GameObject.Find(instance.GetCurrentRoom()) != currentRoom))
			{
				return;
			}
			string text = instance.GetCurrentRoom();
			currentRoom = GameObject.Find(text);
			if (currentRoom == null)
			{
				Debug.LogWarning("Room not found.");
				return;
			}
			Collider component = currentRoom.GetComponent<Collider>();
			if (component == null)
			{
				Debug.LogWarning("Room collider not found.");
				return;
			}
			Vector3 center = component.bounds.center;
			Vector3 extents = component.bounds.extents;
			Collider[] array = Physics.OverlapBox(center, extents, Quaternion.identity);
			List<GameObject> list = new List<GameObject>();
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (collider.CompareTag("PosterGanger"))
				{
					list.Add(collider.gameObject);
				}
			}
			if (list.Count > 0)
			{
				existingPosterGangers = list;
			}
		}
	}

	private IEnumerator ChangeSpriteAfterTime(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = defaultSprite;
		}
		if (UseRoomBasedSpawn)
		{
			AddPosterEnemyToRoomWithPosterGanger();
		}
		else
		{
			AddPosterEnemyToRandomPosterGanger();
		}
		Object.Destroy(this);
	}

	private void AddPosterEnemyToRoomWithPosterGanger()
	{
		if (existingPosterGangers.Count == 0)
		{
			Debug.LogWarning("No objects found with tag 'PosterGanger' in the current room.");
			if (currentPosterEnemy != null)
			{
				Debug.Log("No new poster found, keeping enemy at current location.");
			}
			else
			{
				Debug.LogWarning("No poster enemy currently exists, spawning a new one in the initial position.");
			}
			return;
		}
		GameObject gameObject = existingPosterGangers[Random.Range(0, existingPosterGangers.Count - 1)];
		if (gameObject.GetComponent<PosterEnemy>() == null)
		{
			PosterEnemy posterEnemy = gameObject.AddComponent<PosterEnemy>();
			posterEnemy.AnimatorController = posterAnimatorController;
			posterEnemy.UseRoomBasedSpawn = UseRoomBasedSpawn;
			posterEnemy.RespawnTime = respawnTime;
			posterEnemy.DisappearanceDelay = disappearanceDelay;
			posterEnemy.ColliderSize = colliderSize;
			posterEnemy.ColliderCenter = colliderCenter;
			currentPosterEnemy = gameObject;
		}
		else
		{
			Debug.Log("PosterEnemy script is already attached to the selected object.");
		}
	}

	private void AddPosterEnemyToRandomPosterGanger()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("PosterGanger");
		if (array.Length == 0)
		{
			Debug.LogWarning("No objects found with tag 'PosterGanger'.");
			return;
		}
		GameObject gameObject = array[Random.Range(0, array.Length)];
		if (gameObject.GetComponent<PosterEnemy>() == null)
		{
			PosterEnemy posterEnemy = gameObject.AddComponent<PosterEnemy>();
			posterEnemy.AnimatorController = posterAnimatorController;
			posterEnemy.RespawnTime = respawnTime;
			posterEnemy.UseRoomBasedSpawn = UseRoomBasedSpawn;
			posterEnemy.DisappearanceDelay = disappearanceDelay;
			posterEnemy.ColliderSize = colliderSize;
			posterEnemy.ColliderCenter = colliderCenter;
		}
		else
		{
			Debug.Log("PosterEnemy script is already attached to the selected object.");
		}
	}
}
