using System.Collections;
using UnityEngine;

public class StartLine : MonoBehaviour
{
	public float measureTime = 15f;

	private Vector2 startPosition;

	private float distanceTravelled;

	private bool isMeasuring;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !isMeasuring)
		{
			startPosition = other.transform.position;
			isMeasuring = true;
			distanceTravelled = 0f;
			StartCoroutine(MeasureDistance(other.transform));
		}
	}

	private IEnumerator MeasureDistance(Transform player)
	{
		float elapsedTime = 0f;
		while (elapsedTime < measureTime)
		{
			distanceTravelled = Vector2.Distance(startPosition, player.position);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		player.position = startPosition;
		Debug.Log($"Игрок прошел расстояние: {distanceTravelled} единиц.");
		isMeasuring = false;
	}
}
