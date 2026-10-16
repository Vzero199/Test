using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
	public Slider slider;

	public TextMeshProUGUI sliderValueText;

	public TMP_InputField inputField;

	private void Start()
	{
		UpdateSliderValueText(slider.value);
		slider.onValueChanged.AddListener(UpdateSliderValueText);
		inputField.onEndEdit.AddListener(OnInputFieldValueChanged);
		slider.onValueChanged.Invoke(slider.value);
	}

	private void UpdateSliderValueText(float value)
	{
		if (value % 1f == 0f)
		{
			sliderValueText.text = ((int)value).ToString();
			inputField.text = ((int)value).ToString();
		}
		else
		{
			sliderValueText.text = value.ToString("0.0");
			inputField.text = value.ToString("0.0");
		}
	}

	private void OnInputFieldValueChanged(string inputValue)
	{
		if (float.TryParse(inputValue, out var result))
		{
			if (result >= slider.minValue && result <= slider.maxValue)
			{
				slider.value = result;
			}
			else
			{
				Debug.LogWarning("Значение вне диапазона слайдера!");
			}
		}
		else
		{
			Debug.LogWarning("Некорректный ввод!");
		}
	}

	private void OnDestroy()
	{
		slider.onValueChanged.RemoveListener(UpdateSliderValueText);
		inputField.onEndEdit.RemoveListener(OnInputFieldValueChanged);
	}
}
