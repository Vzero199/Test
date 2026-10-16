using UnityEngine;

public class HourlyMover : MonoBehaviour
{
	private Timer timer;

	private float lastHour;

	private bool flip;

	private void Start()
	{
		timer = Object.FindObjectOfType<Timer>();
		if (timer == null)
		{
			Debug.LogError("Timer not found in the scene!");
			base.enabled = false;
		}
		else
		{
			lastHour = Mathf.Floor(timer.ElapsedTime / 60f);
		}
	}

	private void Update()
	{
		float num = Mathf.Floor(timer.ElapsedTime / 60f);
		if (num > lastHour)
		{
			lastHour = num;
			MoveObject();
		}
	}

	private void MoveObject()
	{
		float x = (flip ? 1f : (-1f));
		base.transform.position += new Vector3(x, 0f, 0f);
	}
}
