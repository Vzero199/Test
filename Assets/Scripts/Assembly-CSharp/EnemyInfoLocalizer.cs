using SmallHedge.SoundManager;
using TMPro;
using UnityEngine;

public class EnemyInfoLocalizer : MonoBehaviour
{
	[Header("UI Элементы")]
	[SerializeField]
	private TMP_Text enemyNameText;

	[SerializeField]
	private TMP_Text hintText;

	[Header("Имя врага (по языкам)")]
	[TextArea]
	public string enemyNameRU;

	[TextArea]
	public string enemyNameEN;

	[TextArea]
	public string enemyNameCN;

	[TextArea]
	public string enemyNameJP;

	[Header("Подсказка (по языкам)")]
	[TextArea]
	public string hintRU;

	[TextArea]
	public string hintEN;

	[TextArea]
	public string hintCN;

	[TextArea]
	public string hintJP;

	public void ApplyLocalization()
	{
		SoundManager.PlaySound(SoundType.UIClick);
		string currentLanguage = GameSettings.CurrentLanguage;
		switch (currentLanguage)
		{
		case "RU":
			enemyNameText.text = enemyNameRU.Replace("\\n", "\n");
			hintText.text = hintRU.Replace("\\n", "\n");
			break;
		case "EN":
			enemyNameText.text = enemyNameEN.Replace("\\n", "\n");
			hintText.text = hintEN.Replace("\\n", "\n");
			break;
		case "CN":
			enemyNameText.text = enemyNameCN.Replace("\\n", "\n");
			hintText.text = hintCN.Replace("\\n", "\n");
			break;
		case "JP":
			enemyNameText.text = enemyNameJP.Replace("\\n", "\n");
			hintText.text = hintJP.Replace("\\n", "\n");
			break;
		default:
			Debug.LogWarning("[EnemyInfoLocalizer] Неизвестный язык: " + currentLanguage);
			break;
		}
	}
}
