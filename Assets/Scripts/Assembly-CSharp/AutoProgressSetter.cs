using UnityEngine;

public class AutoProgressSetter : MonoBehaviour
{
	[SerializeField]
	private string progressKey;

	private void Start()
	{
		if (!GameSaveManager.IsLoading)
		{
			if (!string.IsNullOrEmpty(progressKey))
			{
				GlobalSaveSystem.SetProgress(progressKey, value: true);
				Debug.Log("[AutoProgressSetter] Установлен ключ " + progressKey + " в true");
			}
			else
			{
				Debug.LogWarning("[AutoProgressSetter] Ключ не задан!");
			}
		}
	}
}
