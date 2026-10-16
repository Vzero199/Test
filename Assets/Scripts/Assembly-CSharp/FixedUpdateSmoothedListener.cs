using UnityEngine;

public class FixedUpdateSmoothedListener : MonoBehaviour
{
	public float smoothTime = 0.02f;

	private Vector3 targetPosition;

	private Vector3 velocity;

	private Transform audioListenerTransform;

	private void Awake()
	{
		AudioListener component = GetComponent<AudioListener>();
		if (component == null)
		{
			Camera main = Camera.main;
			if (main != null)
			{
				component = main.GetComponent<AudioListener>();
			}
		}
		if (component != null)
		{
			audioListenerTransform = component.transform;
			return;
		}
		Debug.LogError("FixedUpdateSmoothedListener: AudioListener not found! Sound smoothing will be disabled.");
		base.enabled = false;
	}

	private void Update()
	{
		targetPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		if (!(audioListenerTransform == null))
		{
			Vector3 position = Vector3.SmoothDamp(audioListenerTransform.position, targetPosition, ref velocity, smoothTime);
			audioListenerTransform.position = position;
		}
	}
}
