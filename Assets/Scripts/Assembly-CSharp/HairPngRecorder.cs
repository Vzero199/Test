using System.IO;
using UnityEngine;

public static class HairPngRecorder
{
	public static void SaveCameraViewToPNG(Camera cam, RenderTexture rt, string path)
	{
		if (cam == null || rt == null)
		{
			Debug.LogError("Camera or RenderTexture missing");
			return;
		}
		cam.Render();
		RenderTexture.active = rt;
		Texture2D texture2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(path, bytes);
		Debug.Log("PNG saved: " + path);
		Object.DestroyImmediate(texture2D);
	}
}
