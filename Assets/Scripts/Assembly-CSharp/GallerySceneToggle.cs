using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GallerySceneToggle : MonoBehaviour
{
	public static bool LoopScene;

	[SerializeField]
	private Toggle loopToggle;

	private float lastInputTime;

	private float inputCooldown = 0.5f;

	private void Start()
	{
		if (loopToggle != null)
		{
			loopToggle.isOn = LoopScene;
		}
	}

	public void OnToggleLoopScene(InputAction.CallbackContext context)
	{
		if (PauseMenu.IsPaused || Timer.IsTransitioning || Time.timeScale == 0f || BlinkManager.GetInstance().IsFade() || !context.performed || Time.time - lastInputTime < inputCooldown)
		{
			return;
		}
		lastInputTime = Time.time;
		string text = context.control.name.ToLower();
		if (text == "2" || text == "numpad2")
		{
			LoopScene = !LoopScene;
			Debug.Log($"GallerySceneToggle.LoopScene switched to {LoopScene}");
			if (loopToggle != null)
			{
				loopToggle.isOn = LoopScene;
			}
		}
	}
}
