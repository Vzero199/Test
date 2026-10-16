using UnityEngine;

public class DestroyOnStopgame : MonoBehaviour
{
	private void Update()
	{
		if (NightManager.GetInstance().StopGameCoroutine != null)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
