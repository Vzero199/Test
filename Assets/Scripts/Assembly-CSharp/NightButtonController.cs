using UnityEngine;
using UnityEngine.UI;

public class NightButtonController : MonoBehaviour
{
	[SerializeField]
	private Button targetButton;

	private void Start()
	{
		if (targetButton == null)
		{
			Debug.LogError("Ошибка: кнопка не назначена в инспекторе!");
			return;
		}
		bool progress = GlobalSaveSystem.GetProgress("FifthNightIsComplete");
		targetButton.interactable = progress;
	}

	public void OnEnable()
	{
		bool progress = GlobalSaveSystem.GetProgress("FifthNightIsComplete");
		targetButton.interactable = progress;
	}
}
