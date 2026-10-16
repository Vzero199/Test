using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
	public TextMeshProUGUI fpsText;

	private float deltaTime;

	private float updateInterval = 0.25f;

	private float timeSinceLastUpdate;

	private void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
		timeSinceLastUpdate += Time.unscaledDeltaTime;
		if (timeSinceLastUpdate >= updateInterval)
		{
			float num = 1f / deltaTime;
			fpsText.text = $"{num:0.} FPS";
			timeSinceLastUpdate = 0f;
		}
	}
}
