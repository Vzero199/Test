using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderPercentageDisplay : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI percentageText;

	private void Start()
	{
		if (slider != null)
		{
			slider.onValueChanged.AddListener(UpdatePercentage);
			UpdatePercentage(slider.value);
		}
	}

	private void UpdatePercentage(float value)
	{
		if (percentageText != null && slider != null)
		{
			float num = (value - slider.minValue) / (slider.maxValue - slider.minValue);
			percentageText.text = Mathf.RoundToInt(num * 100f) + "%";
		}
	}

	private void OnDestroy()
	{
		if (slider != null)
		{
			slider.onValueChanged.RemoveListener(UpdatePercentage);
		}
	}
}
