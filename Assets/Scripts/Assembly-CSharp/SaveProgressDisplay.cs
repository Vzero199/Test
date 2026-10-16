using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveProgressDisplay : MonoBehaviour
{
	[SerializeField]
	private List<string> saveKeys;

	[SerializeField]
	private TextMeshProUGUI progressText;

	private void Start()
	{
		UpdateProgressText();
	}

	private void UpdateProgressText()
	{
		if (progressText == null || saveKeys == null || saveKeys.Count == 0)
		{
			Debug.LogWarning("SaveProgressDisplay: Некорректные настройки! Проверь, что добавлены ключи и привязан TMP.");
			return;
		}
		int num = 0;
		foreach (string saveKey in saveKeys)
		{
			if (GlobalSaveSystem.GetProgress(saveKey))
			{
				num++;
			}
		}
		progressText.text = $"{num}/{saveKeys.Count}";
	}
}
