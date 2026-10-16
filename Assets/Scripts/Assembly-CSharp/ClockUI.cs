using UnityEngine;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform minuteHand;

	[SerializeField]
	private RectTransform hourHand;

	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private Timer timer;

	private void Update()
	{
		if (!(timer == null))
		{
			float elapsedTime = timer.ElapsedTime;
			if (elapsedTime >= 360f)
			{
				progressImage.fillAmount = 1f;
				return;
			}
			float num = elapsedTime % 60f * 6f;
			minuteHand.localRotation = Quaternion.Euler(0f, 0f, 0f - num);
			float num2 = elapsedTime % 360f / 360f * 360f;
			hourHand.localRotation = Quaternion.Euler(0f, 0f, 0f - num2);
			progressImage.fillAmount = elapsedTime % 360f / 360f;
		}
	}
}
