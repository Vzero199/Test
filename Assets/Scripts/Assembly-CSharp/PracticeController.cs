using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class PracticeController : MonoBehaviour
{
	[SerializeField]
	private Player player;

	private void Awake()
	{
		if (PlayerPrefs.GetInt("GameDifficulty", 2) != 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void OnRefresh(InputAction.CallbackContext context)
	{
		if (context.performed && base.gameObject.activeInHierarchy)
		{
			SoundManager.PlaySound(SoundType.RedPills);
			player.enduranceSM.RestoreFull();
			player.RestoreBattery();
			player.CurrentHeartNumber++;
			player.CurrentHeartNumber++;
			player.CurrentHeartNumber++;
		}
	}
}
