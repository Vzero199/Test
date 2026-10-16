using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PPCHrum : MonoBehaviour
{
	[SerializeField]
	private int targetWidth = 640;

	[SerializeField]
	private int targetHeight = 360;

	[SerializeField]
	private float pixelsPerUnit = 100f;

	private Camera cam;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		AdjustCamera();
	}

	private void AdjustCamera()
	{
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}
		float num = (float)targetWidth / (float)targetHeight;
		float num2 = (float)Screen.width / (float)Screen.height / num;
		if (num2 < 1f)
		{
			Rect rect = cam.rect;
			rect.width = 1f;
			rect.height = num2;
			rect.x = 0f;
			rect.y = (1f - num2) / 2f;
			cam.rect = rect;
		}
		else
		{
			float num3 = 1f / num2;
			Rect rect2 = cam.rect;
			rect2.width = num3;
			rect2.height = 1f;
			rect2.x = (1f - num3) / 2f;
			rect2.y = 0f;
			cam.rect = rect2;
		}
		cam.orthographicSize = (float)targetHeight / (2f * pixelsPerUnit);
	}

	private void OnValidate()
	{
		AdjustCamera();
	}
}
