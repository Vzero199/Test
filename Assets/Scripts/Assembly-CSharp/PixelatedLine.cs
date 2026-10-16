using System.Collections.Generic;
using UnityEngine;

public class PixelatedLine : MonoBehaviour
{
	public LineRenderer lineRenderer;

	public float pixelSize = 1f;

	private void Update()
	{
		Vector3[] array = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(array);
		Vector3[] array2 = GeneratePixelatedLine(array, pixelSize);
		lineRenderer.positionCount = array2.Length;
		lineRenderer.SetPositions(array2);
	}

	private Vector3[] GeneratePixelatedLine(Vector3[] originalPositions, float pixelSize)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < originalPositions.Length - 1; i++)
		{
			Vector3 a = originalPositions[i];
			Vector3 b = originalPositions[i + 1];
			int num = Mathf.CeilToInt(Vector3.Distance(a, b) / pixelSize);
			for (int j = 0; j <= num; j++)
			{
				Vector3 item = Vector3.Lerp(a, b, (float)j / (float)num);
				list.Add(item);
			}
		}
		return list.ToArray();
	}
}
