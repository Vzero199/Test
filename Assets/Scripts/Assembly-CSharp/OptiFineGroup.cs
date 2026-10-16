using System.Collections.Generic;
using UnityEngine;

public class OptiFineGroup : MonoBehaviour
{
	private Camera mainCamera;

	private float visibilityMultiplier;

	private float updateInterval;

	private float nextUpdateTime;

	private Transform cameraTransform;

	private List<Renderer> renderers = new List<Renderer>();

	public void Initialize(int groupIndex, int totalGroups, Camera camera, float multiplier, float interval)
	{
		mainCamera = camera;
		visibilityMultiplier = multiplier;
		updateInterval = interval;
		cameraTransform = camera.transform;
		nextUpdateTime = Time.time + interval / (float)totalGroups * (float)groupIndex;
	}

	private void Update()
	{
		if (Time.time >= nextUpdateTime)
		{
			UpdateRenderersVisibility();
			nextUpdateTime += updateInterval;
		}
	}

	private void UpdateRenderersVisibility()
	{
		if (cameraTransform == null || mainCamera == null)
		{
			return;
		}
		Vector2 vector = new Vector2(cameraTransform.position.x, cameraTransform.position.y);
		float num = mainCamera.orthographicSize * visibilityMultiplier;
		float num2 = num * num;
		foreach (Renderer renderer in renderers)
		{
			if (!(renderer == null))
			{
				Vector2 vector2 = new Vector2(renderer.transform.position.x, renderer.transform.position.y);
				bool flag = (vector - vector2).sqrMagnitude <= num2;
				renderer.enabled = flag;
			}
		}
	}

	public void ForceUpdateRenderersVisibility()
	{
		UpdateRenderersVisibility();
	}

	public void UpdateVisibilityMultiplier(float newMultiplier)
	{
		visibilityMultiplier = newMultiplier;
	}
}
