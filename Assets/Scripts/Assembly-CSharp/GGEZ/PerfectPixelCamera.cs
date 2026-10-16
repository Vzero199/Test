using UnityEngine;

namespace GGEZ
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[HelpURL("http://ggez.org/posts/perfect-pixel-camera/")]
	[DisallowMultipleComponent]
	[AddComponentMenu("GGEZ/Camera/Perfect Pixel Camera")]
	public class PerfectPixelCamera : MonoBehaviour
	{
		[Tooltip("The number of texture pixels that fit in 1.0 world units. Common values are 8, 16, 32 and 64. If you're making a tile-based game, this is your tile size.")]
		[Range(1f, 64f)]
		public int TexturePixelsPerWorldUnit = 16;

		private Camera cameraComponent;

		private const float halfPixelOffsetIfNeededForD3D = 0f;

		public float SnapSizeWorldUnits { get; private set; }

		private void OnEnable()
		{
			cameraComponent = (Camera)GetComponent(typeof(Camera));
			LateUpdate();
		}

		private void OnDisable()
		{
			if (!(cameraComponent == null))
			{
				cameraComponent.ResetProjectionMatrix();
				cameraComponent = null;
			}
		}

		private void LateUpdate()
		{
			Camera camera = cameraComponent;
			camera.transparencySortMode = TransparencySortMode.Orthographic;
			camera.orthographic = true;
			camera.transform.rotation = Quaternion.identity;
			camera.orthographicSize = Mathf.Max(camera.orthographicSize, 1E-05f);
			Rect pixelRect = camera.pixelRect;
			float num = TexturePixelsPerWorldUnit;
			float num2 = Mathf.Max(1f, Mathf.Ceil(1f * pixelRect.height / (camera.orthographicSize * 2f * num)));
			float num3 = 1f * pixelRect.width / (num2 * 2f * num);
			float num4 = 1f * pixelRect.height / (num2 * 2f * num);
			float num5 = 1f / (num2 * num);
			float num6 = 0f * num5;
			float num7 = num6 - Mathf.Repeat(num5 + Mathf.Repeat(camera.transform.position.x, num5), num5);
			float num8 = num6 - Mathf.Repeat(num5 + Mathf.Repeat(camera.transform.position.y, num5), num5);
			SnapSizeWorldUnits = num5;
			camera.projectionMatrix = Matrix4x4.Ortho(0f - num3 + num7, num3 + num7, 0f - num4 + num8, num4 + num8, camera.nearClipPlane, camera.farClipPlane);
		}
	}
}
