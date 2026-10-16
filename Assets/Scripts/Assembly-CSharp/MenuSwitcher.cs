using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuSwitcher : MonoBehaviour
{
	public GameObject[] menuPages;

	public float transitionDuration = 0.3f;

	public float inputProtectionTime = 1f;

	private int currentIndex;

	private bool isTransitioning;

	private float protectTimer;

	private void OnEnable()
	{
		isTransitioning = false;
		protectTimer = inputProtectionTime;
	}

	private void Start()
	{
		for (int i = 0; i < menuPages.Length; i++)
		{
			GameObject gameObject = menuPages[i];
			CanvasGroup obj = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
			bool flag = i == currentIndex;
			gameObject.SetActive(flag);
			obj.alpha = (flag ? 1f : 0f);
			obj.interactable = flag;
			obj.blocksRaycasts = flag;
		}
	}

	private void Update()
	{
		if (protectTimer > 0f)
		{
			protectTimer -= Time.unscaledDeltaTime;
		}
	}

	public void OnNavigate(InputAction.CallbackContext context)
	{
		if (context.performed && !isTransitioning && !(protectTimer > 0f) && base.gameObject.activeInHierarchy)
		{
			float x = context.ReadValue<Vector2>().x;
			if (x > 0f)
			{
				int toIndex = (currentIndex + 1) % menuPages.Length;
				StartCoroutine(SwitchPage(currentIndex, toIndex));
				currentIndex = toIndex;
			}
			else if (x < 0f)
			{
				int toIndex2 = (currentIndex - 1 + menuPages.Length) % menuPages.Length;
				StartCoroutine(SwitchPage(currentIndex, toIndex2));
				currentIndex = toIndex2;
			}
		}
	}

	private IEnumerator SwitchPage(int fromIndex, int toIndex)
	{
		isTransitioning = true;
		GameObject fromPage = menuPages[fromIndex];
		GameObject gameObject = menuPages[toIndex];
		CanvasGroup fromGroup = fromPage.GetComponent<CanvasGroup>();
		CanvasGroup toGroup = gameObject.GetComponent<CanvasGroup>();
		gameObject.SetActive(value: true);
		toGroup.alpha = 0f;
		toGroup.interactable = false;
		toGroup.blocksRaycasts = false;
		float timer = 0f;
		while (timer < transitionDuration)
		{
			float num = timer / transitionDuration;
			fromGroup.alpha = 1f - num;
			toGroup.alpha = num;
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		fromGroup.alpha = 0f;
		fromGroup.interactable = false;
		fromGroup.blocksRaycasts = false;
		fromPage.SetActive(value: false);
		toGroup.alpha = 1f;
		toGroup.interactable = true;
		toGroup.blocksRaycasts = true;
		isTransitioning = false;
	}
}
