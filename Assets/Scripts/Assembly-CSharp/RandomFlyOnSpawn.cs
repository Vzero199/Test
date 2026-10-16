using UnityEngine;

public class RandomFlyOnSpawn : MonoBehaviour
{
	public float forceMin = 2f;

	public float forceMax = 5f;

	private Rigidbody2D rb;

	private Transform parentTransform;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		parentTransform = base.transform.parent;
	}

	private void Start()
	{
		ApplyRandomForce();
	}

	private void ApplyRandomForce()
	{
		if (rb != null && parentTransform != null)
		{
			Vector2 vector = (base.transform.position - parentTransform.position).normalized;
			float num = Random.Range(forceMin, forceMax);
			rb.AddForce(vector * num, ForceMode2D.Impulse);
		}
		else
		{
			Debug.LogWarning("Rigidbody2D не найден на объекте или родитель не установлен!");
		}
	}
}
