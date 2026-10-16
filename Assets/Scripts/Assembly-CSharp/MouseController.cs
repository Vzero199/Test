using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
	public static MouseController Instance;

	private StandaloneInputModule inputModule;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (Player.GetInstance() != null)
		{
			DisableMouse();
		}
		else
		{
			EnableMouse();
		}
	}

	public void DisableMouse()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		if (inputModule != null)
		{
			inputModule.inputOverride = new NullInputModule();
		}
	}

	public void EnableMouse()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		if (inputModule != null)
		{
			inputModule.inputOverride = null;
		}
	}
}
