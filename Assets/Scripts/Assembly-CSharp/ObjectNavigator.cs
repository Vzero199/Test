using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectNavigator : MonoBehaviour
{
	[SerializeField]
	private List<Transform> targets;

	private int currentIndex = -1;

	private float maxDistance = 500f;

	private float lastInputTime = float.NegativeInfinity;

	private readonly float inputCooldown = 0.5f;

	private readonly Dictionary<string, string> progressKeysMap = new Dictionary<string, string>
	{
		{ "FirstNight", "FirstNightIsComplete" },
		{ "SecondNight", "SecondNightIsComplete" },
		{ "ThirdNight", "ThirdNightIsComplete" },
		{ "FourthNight", "FourthNightIsComplete" },
		{ "FifthNight", "FifthNightIsComplete" },
		{ "TrueEnd", "TrueEndIsReceive" }
	};

	private bool IsGallerySceneActive()
	{
		if (!GallerySceneViewer.isGalleryScene && !GallerySceneViewerForSuccTE.isGalleryScene && !GallerySceneViewerFoSuccGO.isGalleryScene)
		{
			return GallerySceneViewerForTV.isGalleryScene;
		}
		return true;
	}

	public void OnNavigate(InputAction.CallbackContext context)
	{
		if (!PauseMenu.IsPaused && !Timer.IsTransitioning && Time.timeScale != 0f && !BlinkManager.GetInstance().IsFade() && !IsGallerySceneActive() && context.performed && !(Time.time - lastInputTime < inputCooldown))
		{
			lastInputTime = Time.time;
			string text = context.control.name.ToLower();
			int num = 0;
			switch (text)
			{
			default:
				return;
			case "1":
			case "numpad1":
				num = -1;
				break;
			case "3":
			case "numpad3":
				num = 1;
				break;
			}
			TryMove(num);
		}
	}

	private void TryMove(int direction)
	{
		int count = targets.Count;
		if (count == 0)
		{
			return;
		}
		if (currentIndex < 0 || currentIndex >= count)
		{
			float num = float.MaxValue;
			int num2 = -1;
			for (int i = 0; i < count; i++)
			{
				if (IsTargetAvailable(i))
				{
					float num3 = Mathf.Abs(base.transform.position.x - targets[i].position.x);
					if (!(num3 <= 200f) && num3 <= maxDistance && num3 < num)
					{
						num = num3;
						num2 = i;
					}
				}
			}
			currentIndex = ((num2 != -1) ? num2 : (-1));
			if (currentIndex == -1)
			{
				for (int j = 0; j < count; j++)
				{
					if (IsTargetAvailable(j) && Mathf.Abs(base.transform.position.x - targets[j].position.x) > 200f)
					{
						currentIndex = j;
						break;
					}
				}
			}
			if (currentIndex == -1)
			{
				Debug.Log("Нет подходящих объектов для перемещения.");
				return;
			}
		}
		int num4 = count;
		int num5 = currentIndex;
		while (num4-- > 0)
		{
			num5 = (num5 + direction + count) % count;
			if (IsTargetAvailable(num5) && Mathf.Abs(base.transform.position.x - targets[num5].position.x) > 200f)
			{
				currentIndex = num5;
				MoveToTarget(targets[currentIndex]);
				return;
			}
		}
		for (int k = 0; k < count; k++)
		{
			if (IsTargetAvailable(k) && Mathf.Abs(base.transform.position.x - targets[k].position.x) > 200f)
			{
				currentIndex = k;
				MoveToTarget(targets[k]);
				return;
			}
		}
		Debug.Log("Нет подходящих объектов для перемещения.");
	}

	private bool IsTargetAvailable(int index)
	{
		if (index < 0 || index >= targets.Count)
		{
			return false;
		}
		Transform transform = targets[index];
		TargetPoint component = transform.GetComponent<TargetPoint>();
		if (component == null)
		{
			Debug.LogError("Объект " + transform.name + " не имеет TargetPoint компонента.");
			return false;
		}
		string requiredKey = component.RequiredKey;
		if (!progressKeysMap.TryGetValue(requiredKey, out var value))
		{
			Debug.LogError("Неверный ключ: " + requiredKey + " на объекте " + transform.name);
			return false;
		}
		if (!GlobalSaveSystem.GetProgress(value))
		{
			return false;
		}
		return true;
	}

	private void MoveToTarget(Transform target)
	{
		Player.GetInstance().transform.position = target.position;
		Player.GetInstance().AlignToRoomLine();
		Debug.Log("Переместились к " + target.name);
	}
}
