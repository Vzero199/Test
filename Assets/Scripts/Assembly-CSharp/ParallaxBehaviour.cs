using System.Collections.Generic;
using UnityEngine;

public class ParallaxBehaviour : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float parallaxStrength = 0.3f;

	[SerializeField]
	private bool disableVerticalParallax = true;

	[SerializeField]
	private List<SpriteRenderer> maskSpriteRenderers;

	private Transform cameraTransform;

	private Vector3 cameraPreviousPosition;

	private Vector3 defaultZLockedPosition;

	private float maskMinX;

	private float maskMaxX;

	private bool hasValidMasks;

	public float ParallaxStrength
	{
		get
		{
			return parallaxStrength;
		}
		set
		{
			parallaxStrength = Mathf.Clamp01(value);
		}
	}

	public bool DisableVerticalParallax
	{
		get
		{
			return disableVerticalParallax;
		}
		set
		{
			disableVerticalParallax = value;
		}
	}

	private void Start()
	{
		cameraTransform = Camera.main.transform;
		cameraPreviousPosition = cameraTransform.position;
		defaultZLockedPosition = new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z);
		hasValidMasks = maskSpriteRenderers != null && maskSpriteRenderers.Count > 0;
		if (hasValidMasks)
		{
			CalculateMaskBounds();
		}
	}

	private void LateUpdate()
	{
		if (!cameraTransform)
		{
			return;
		}
		float x = cameraTransform.position.x;
		if (!hasValidMasks || (x >= maskMinX && x <= maskMaxX))
		{
			Vector3 vector = cameraTransform.position - cameraPreviousPosition;
			if (disableVerticalParallax)
			{
				vector.y = 0f;
			}
			Vector3 b = base.transform.position + vector * parallaxStrength;
			b.z = defaultZLockedPosition.z;
			base.transform.position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * 10f);
		}
		cameraPreviousPosition = cameraTransform.position;
	}

	private void CalculateMaskBounds()
	{
		maskMinX = maskSpriteRenderers[0].bounds.min.x;
		maskMaxX = maskSpriteRenderers[0].bounds.max.x;
		foreach (SpriteRenderer maskSpriteRenderer in maskSpriteRenderers)
		{
			Bounds bounds = maskSpriteRenderer.bounds;
			maskMinX = Mathf.Min(maskMinX, bounds.min.x);
			maskMaxX = Mathf.Max(maskMaxX, bounds.max.x);
		}
	}
}
