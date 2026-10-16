using System.Collections.Generic;
using UnityEngine;

public class DoorProgressChecker : MonoBehaviour
{
	[SerializeField]
	private string progressKey;

	private DoorTransit doorTransit;

	private void Start()
	{
		doorTransit = GetComponent<DoorTransit>();
		string value;
		if (doorTransit == null)
		{
			Debug.LogError("DoorProgressChecker на " + base.gameObject.name + " не нашел компонент DoorTransit!");
		}
		else if (new Dictionary<string, string>
		{
			{ "FirstNight", "FirstNightIsComplete" },
			{ "SecondNight", "SecondNightIsComplete" },
			{ "ThirdNight", "ThirdNightIsComplete" },
			{ "FourthNight", "FourthNightIsComplete" },
			{ "FifthNight", "FifthNightIsComplete" },
			{ "TrueEnd", "TrueEndIsReceive" }
		}.TryGetValue(progressKey, out value))
		{
			bool progress = GlobalSaveSystem.GetProgress(value);
			Debug.Log($"Key {progressKey} Is {progress}");
			doorTransit.KeyItemName = (progress ? "" : "close");
			if (!((progressKey == "TrueEnd") & progress))
			{
				return;
			}
			SpriteRenderer component = GetComponent<SpriteRenderer>();
			if (component != null)
			{
				Sprite sprite = Resources.Load<Sprite>("Sprites/SecretDoorOpen");
				if (sprite != null)
				{
					component.sprite = sprite;
				}
				else
				{
					Debug.LogError("Не удалось загрузить спрайт Sprites/SecretDoorOpen");
				}
			}
			else
			{
				Debug.LogError("SpriteRenderer не найден на объекте " + base.gameObject.name);
			}
		}
		else
		{
			Debug.LogError("Неверный ключ: " + progressKey);
		}
	}
}
