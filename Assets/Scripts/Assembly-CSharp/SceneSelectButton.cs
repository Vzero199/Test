using UnityEngine;
using UnityEngine.UI;

public class SceneSelectButton : MonoBehaviour
{
	[SerializeField]
	private string progressKey;

	private Button button;

	private void Start()
	{
		button = GetComponent<Button>();
		if (!string.IsNullOrEmpty(progressKey) && !GlobalSaveSystem.GetProgress(progressKey))
		{
			button.interactable = false;
		}
	}
}
