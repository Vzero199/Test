using UnityEngine;

public class BatteryManager : MonoBehaviour
{
	public static BatteryManager Instance;

	private static Player player;

	public float accumulatedDrainTime;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Start()
	{
		player = Player.GetInstance();
	}

	private void Update()
	{
		if (!(player == null) && player.CurrentBatteryLevel > 0 && player.IsBatteryDraining)
		{
			accumulatedDrainTime += Time.deltaTime;
			while (accumulatedDrainTime >= player.BatteryDrainInterval)
			{
				DrainBattery();
				accumulatedDrainTime -= player.BatteryDrainInterval;
			}
		}
	}

	public void StartDrainBattery()
	{
		if (!(player == null))
		{
			player.IsBatteryDraining = true;
		}
	}

	public void InterruptBatteryDrain()
	{
		if (!(player == null))
		{
			player.IsBatteryDraining = false;
		}
	}

	private void DrainBattery()
	{
		if (player.CurrentBatteryLevel > 0)
		{
			player.CurrentBatteryLevel--;
			player.UpdateBatteryUI();
			if (player.CurrentBatteryLevel <= 0)
			{
				player.FlashlightOff();
				player.IsBatteryDraining = false;
				Debug.Log("Flashlight turned off due to empty battery.");
			}
		}
	}
}
