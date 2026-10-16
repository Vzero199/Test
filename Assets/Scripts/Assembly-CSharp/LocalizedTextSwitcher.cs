using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTextSwitcher : MonoBehaviour
{
	[Header("Тексты по языкам")]
	[TextArea]
	public string textRU;

	[TextArea]
	public string textEN;

	[TextArea]
	public string textCN;

	[TextArea]
	public string textJP;

	private TextMeshProUGUI tmpText;

	private RectTransform imageRect;

	[Header("Настройки смещения Image")]
	public float offsetX = -10f;

	private FitTextWidth fitWidth;

	[Header("Настройки высоты для QuestTask")]
	[Tooltip("Базовая высота для одной строки")]
	public float questTaskBaseHeight = 75f;

	[Tooltip("Высота, добавляемая за каждую дополнительную строку")]
	public float questTaskLineHeight = 75f;

	private void Awake()
	{
		tmpText = GetComponent<TextMeshProUGUI>();
		fitWidth = GetComponent<FitTextWidth>();
		imageRect = base.transform.Find("Mark")?.GetComponent<RectTransform>();
		if (tmpText == null)
		{
			Debug.LogError("[LocalizedTextSwitcher] Отсутствует компонент TextMeshProUGUI на объекте " + base.gameObject.name);
		}
	}

	private void Start()
	{
		if (imageRect != null)
		{
			StartCoroutine(DelayedPositioning());
		}
	}

	private IEnumerator DelayedPositioning()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		PositionImageLeftOfFirstCharacter();
	}

	private void OnEnable()
	{
		GameSettings.OnLanguageChanged += OnLanguageChanged;
		UpdateText();
	}

	private void OnDisable()
	{
		GameSettings.OnLanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		if (!(tmpText == null))
		{
			switch (GameSettings.CurrentLanguage)
			{
			case "RU":
				tmpText.text = textRU.Replace("\\n", "\n");
				break;
			case "EN":
				tmpText.text = textEN.Replace("\\n", "\n");
				break;
			case "CN":
				tmpText.text = textCN.Replace("\\n", "\n");
				break;
			case "JP":
				tmpText.text = textJP.Replace("\\n", "\n");
				break;
			default:
				Debug.LogWarning("[LocalizedTextSwitcher] Неизвестный язык: " + GameSettings.CurrentLanguage);
				break;
			}
			if (fitWidth != null)
			{
				fitWidth.UpdateWidth();
			}
			UpdateHeightForQuestTask();
			if (imageRect != null)
			{
				StartCoroutine(PositionAfterTextUpdate());
			}
		}
	}

	private IEnumerator PositionAfterTextUpdate()
	{
		Canvas.ForceUpdateCanvases();
		tmpText.ForceMeshUpdate();
		yield return new WaitForEndOfFrame();
		PositionImageLeftOfFirstCharacter();
	}

	public void PositionImageLeftOfFirstCharacter()
	{
		if (imageRect == null || tmpText.textInfo.characterCount == 0)
		{
			if (imageRect != null)
			{
				StartCoroutine(DelayedPositioning());
			}
			return;
		}
		tmpText.ForceMeshUpdate();
		TMP_CharacterInfo tMP_CharacterInfo = tmpText.textInfo.characterInfo[0];
		Vector3 position = tmpText.transform.TransformPoint(tMP_CharacterInfo.bottomLeft);
		Vector3 vector = base.transform.InverseTransformPoint(position);
		vector.x += offsetX;
		float y = (tMP_CharacterInfo.topLeft.y + tMP_CharacterInfo.bottomLeft.y) / 2f;
		vector.y = y;
		imageRect.localPosition = new Vector3(vector.x, vector.y, imageRect.localPosition.z);
	}

	private void UpdateHeightForQuestTask()
	{
		if (base.gameObject.name.Contains("questTask") && !(tmpText == null))
		{
			tmpText.ForceMeshUpdate();
			int num = tmpText.textInfo.lineCount;
			if (num == 0)
			{
				num = 1;
			}
			float size = questTaskBaseHeight + (float)(num - 1) * questTaskLineHeight;
			RectTransform component = GetComponent<RectTransform>();
			if (component != null)
			{
				component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			}
			else
			{
				Debug.LogWarning("[QuestTask] На объекте " + base.gameObject.name + " нет RectTransform!");
			}
		}
	}
}
