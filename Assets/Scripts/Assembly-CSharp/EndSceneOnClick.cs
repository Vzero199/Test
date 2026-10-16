using UnityEngine;
using UnityEngine.InputSystem;

public class EndSceneOnClick : MonoBehaviour
{
	public void OnLeftClick(InputAction.CallbackContext context)
	{
		if (context.performed && base.gameObject.activeSelf)
		{
			GallerySceneViewerFoSuccGO.instance.ExitSexMode();
		}
	}
}
