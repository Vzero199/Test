using UnityEngine;

public class FollowUIElement : MonoBehaviour
{
	public RectTransform targetUI;

	public RectTransform particleUI;

	public Canvas canvas;

	private void LateUpdate()
	{
		Vector3 position = targetUI.position;
		particleUI.position = position;
	}
}
