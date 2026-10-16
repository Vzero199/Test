using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HorrorLightFlicker : MonoBehaviour
{
	public Light2D[] lights;

	public float minFlickerInterval = 0.1f;

	public float maxFlickerInterval = 2f;

	private HorrorLightFlicker flickerScript;

	private void Start()
	{
		if (lights.Length != 0)
		{
			StartCoroutine(FlickerRoutine());
		}
		else
		{
			Debug.LogError("Не назначены источники света в " + base.gameObject.name);
		}
	}

	private IEnumerator FlickerRoutine()
	{
		while (true)
		{
			float flickerTime = Random.Range(minFlickerInterval, maxFlickerInterval);
			SetLightsState(state: false);
			yield return new WaitForSeconds(flickerTime);
			SetLightsState(state: true);
			yield return new WaitForSeconds(flickerTime);
		}
	}

	private void SetLightsState(bool state)
	{
		Light2D[] array = lights;
		foreach (Light2D light2D in array)
		{
			if (light2D != null)
			{
				light2D.enabled = state;
			}
		}
	}
}
