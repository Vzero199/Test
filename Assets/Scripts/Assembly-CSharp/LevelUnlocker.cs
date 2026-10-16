using UnityEngine;

public class LevelUnlocker : MonoBehaviour
{
	[SerializeField]
	private string progressKey;

	public void UnlockLevel()
	{
		if (!string.IsNullOrEmpty(progressKey))
		{
			GlobalSaveSystem.SetProgress(progressKey, value: true);
		}
	}
}
