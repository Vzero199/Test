using UnityEngine;
using UnityEngine.UI;

namespace GGEZ
{
	public class ZoomDemo : MonoBehaviour
	{
		public Camera MainCamera;

		public AnimationCurve TimeToOrthographicSize;

		public Text TitleText;

		public Text BodyText;

		public Text SwitchModesButtonText;

		private ZoomDemoMode mode = ZoomDemoMode.Marching;

		public void SwitchModes()
		{
			switch (mode)
			{
			case ZoomDemoMode.Shimmering:
				mode = ZoomDemoMode.Bubbling;
				SwitchModesButtonText.text = "Switch to Marching";
				break;
			case ZoomDemoMode.Bubbling:
				mode = ZoomDemoMode.Marching;
				SwitchModesButtonText.text = "Switch to Shimmering";
				break;
			case ZoomDemoMode.Marching:
				mode = ZoomDemoMode.Shimmering;
				SwitchModesButtonText.text = "Switch to Bubbling";
				break;
			}
			TitleText.text = mode.ToString();
		}

		private void Start()
		{
			SwitchModes();
		}

		private void Update()
		{
			PerfectPixelCamera perfectPixelCamera = MainCamera.GetComponent(typeof(PerfectPixelCamera)) as PerfectPixelCamera;
			bool flag = perfectPixelCamera != null && perfectPixelCamera.isActiveAndEnabled;
			float orthographicSize = MainCamera.orthographicSize;
			float num = orthographicSize;
			if (flag)
			{
				float num2 = Mathf.Max(1f, Mathf.Ceil(1f * MainCamera.pixelRect.height / (MainCamera.orthographicSize * 2f * (float)perfectPixelCamera.TexturePixelsPerWorldUnit)));
				num = 1f * MainCamera.pixelRect.height / (num2 * 2f * (float)perfectPixelCamera.TexturePixelsPerWorldUnit);
			}
			switch (mode)
			{
			case ZoomDemoMode.Shimmering:
				MainCamera.orthographicSize = TimeToOrthographicSize.Evaluate(Time.time);
				MainCamera.transform.position = Vector3.back * 10f;
				if (flag)
				{
					BodyText.text = "The Perfect Pixel Camera snaps the orthographicSize of the camera component behind the scenes to fix zooming issues (" + orthographicSize.ToString("0.0") + " snapped to " + num.ToString("0.0") + ")";
				}
				else
				{
					BodyText.text = "A miscalibrated camera shows a shimmer or wave pattern as it zooms. Add the PerfectPixelCamera component to the MainCamera object to check out the fix, or tap the button to see other errors.";
				}
				break;
			case ZoomDemoMode.Bubbling:
				MainCamera.transform.position = new Vector3(Mathf.Cos(Time.time * 0.5f) * 5f, Mathf.Sin(Time.time * 0.5f) * 5f, -10f);
				if (flag)
				{
					BodyText.text = "With the PerfectPixelCamera enabled, panning doesn't jiggle anymore!";
				}
				else
				{
					BodyText.text = "When panning, the image appears to jiggle and bubble. Add the PerfectPixelCamera component to the MainCamera object to check out the fix, or tap the button to see other errors.";
				}
				break;
			case ZoomDemoMode.Marching:
				MainCamera.orthographicSize = (float)MainCamera.pixelHeight / 32f * 0.5f;
				MainCamera.transform.position = new Vector3((Mathf.PingPong(Time.time * 0.1f, 2f) - 1f) * 0.1f, (Mathf.PingPong(Time.time * 0.1f, 2f) - 1f) * 3f, -10f);
				if (flag)
				{
					BodyText.text = "With the PerfectPixelCamera enabled, the camera's projection matrix is automatically offset by less than a pixel and marching is fixed.";
				}
				else
				{
					BodyText.text = "Even with orthographicSize set correctly to " + MainCamera.orthographicSize.ToString("0.0") + ", an alignment issue is easily visible at the border at max zoom. Use the scale slider to zoom in. Add the PerfectPixelCamera component to the MainCamera object to check out the fix, or tap the button to see other errors.";
				}
				break;
			}
		}
	}
}
