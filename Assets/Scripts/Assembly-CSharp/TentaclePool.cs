using System.Collections.Generic;
using UnityEngine;

public class TentaclePool : MonoBehaviour
{
	[Header("Настройки пула")]
	[SerializeField]
	private GameObject tentaclePrefab;

	[SerializeField]
	private int initialPoolSize = 20;

	private Queue<GameObject> pool = new Queue<GameObject>();

	private void Awake()
	{
		for (int i = 0; i < initialPoolSize; i++)
		{
			GameObject gameObject = Object.Instantiate(tentaclePrefab);
			gameObject.SetActive(value: false);
			pool.Enqueue(gameObject);
		}
	}

	public GameObject GetTentacle()
	{
		if (pool.Count > 0)
		{
			GameObject obj = pool.Dequeue();
			obj.SetActive(value: true);
			return obj;
		}
		return Object.Instantiate(tentaclePrefab);
	}

	public void ReturnTentacle(GameObject tentacle)
	{
		tentacle.SetActive(value: false);
		pool.Enqueue(tentacle);
	}
}
