using System.Collections.Generic;
using UnityEngine;

public class ProgressKeyDisabler : MonoBehaviour
{
	[SerializeField]
	private string progressKey;

	private void Start()
	{
		if (new Dictionary<string, string>
		{
			{ "FirstNight", "FirstNightReached" },
			{ "SecondNight", "SecondNightReached" },
			{ "ThirdNight", "ThirdNightReached" },
			{ "FourthNight", "FourthNightReached" },
			{ "FifthNight", "FifthNightReached" },
			{ "TrueEnd", "TrueEndIsReceive" }
		}.TryGetValue(progressKey, out var value))
		{
			if (!GlobalSaveSystem.GetProgress(value))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			Debug.LogError("[ProgressKeyDisabler] Неверный ключ прогресса: " + progressKey);
			base.gameObject.SetActive(value: false);
		}
	}
}
