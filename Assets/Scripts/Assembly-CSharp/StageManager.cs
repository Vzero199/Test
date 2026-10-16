using System.Collections;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
	private enum ChunkDirection
	{
		ToLeft = -1,
		ToRight = 1
	}

	public const int ChunkCount = 12;

	private Transform[] chunks;

	public static float chunkWidth = 640f;

	private BoxCollider boxCollider;

	private bool isUpdateActive;

	private static StageManager instance;

	private Vector3 initialColliderCenter;

	public static StageManager GetInstance()
	{
		return instance;
	}

	private void Start()
	{
		instance = this;
		InitializeChunks();
		boxCollider = GetComponent<BoxCollider>();
		initialColliderCenter = boxCollider.center;
		StartCoroutine(ActivateUpdateAfterDelay(5f));
	}

	private IEnumerator ActivateUpdateAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		isUpdateActive = true;
	}

	private void Update()
	{
		if (isUpdateActive && !(Player.GetInstance() == null))
		{
			UpdateColliderPosition();
			CheckStagePosition();
		}
	}

	private void UpdateColliderPosition()
	{
		Vector3 position = CameraFollow.GetInstance().transform.position;
		position.y = boxCollider.center.y;
		position.z = boxCollider.center.z;
		float num = 600f;
		Vector3 vector = position - boxCollider.center;
		float num2 = Mathf.Min(vector.magnitude, num * Time.deltaTime);
		Vector3 center = boxCollider.center + vector.normalized * num2;
		boxCollider.center = center;
		Transform transform = base.transform.Find("HoldTheLine");
		if (transform != null)
		{
			transform.position = new Vector3(center.x, transform.position.y, transform.position.z);
		}
	}

	public void CheckStagePosition()
	{
		float num = boxCollider.bounds.min.x + 760f;
		float num2 = boxCollider.bounds.max.x - 760f;
		Transform[] array = chunks;
		foreach (Transform transform in array)
		{
			if (transform.position.x + chunkWidth / 2f < num)
			{
				MoveChunk(transform, ChunkDirection.ToRight);
			}
			else if (transform.position.x - chunkWidth / 2f > num2)
			{
				MoveChunk(transform, ChunkDirection.ToLeft);
			}
		}
	}

	private void MoveChunk(Transform chunk, ChunkDirection direction)
	{
		float num = chunkWidth * 12f * (float)direction;
		chunk.position += Vector3.right * num;
		chunks = chunks.OrderBy((Transform obj) => obj.position.x).ToArray();
	}

	private void InitializeChunks()
	{
		chunks = (from Transform child in base.transform
			where child.name != "HoldTheLine"
			select child).ToArray();
		Transform[] array = chunks;
		foreach (Transform transform in array)
		{
			BoxCollider boxCollider = transform.gameObject.GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				boxCollider = transform.gameObject.AddComponent<BoxCollider>();
			}
			boxCollider.size = new Vector3(chunkWidth, 550f, 50f);
			boxCollider.center = new Vector3(chunkWidth / 2f, -135f, 0f);
		}
	}

	public void CenterColliderOnPlayer(float playerX)
	{
		Vector3 center = boxCollider.center;
		center.x = playerX;
		boxCollider.center = center;
	}

	public void ResetColliderPosition()
	{
		boxCollider.center = initialColliderCenter;
	}
}
