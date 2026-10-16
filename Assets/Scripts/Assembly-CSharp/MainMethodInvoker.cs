using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMethodInvoker : MonoBehaviour
{
	[SerializeField]
	private string methodName = "TargetMethod";

	[SerializeField]
	private GameObject targetObject;

	private static bool isMethodInvokedThisFrame;

	private static int lastFrameCount;

	private bool IsInteractableInHierarchy(GameObject obj)
	{
		Debug.Log("Начинаем проверку интерактивности для " + obj.name);
		CanvasGroup componentInParent = obj.GetComponentInParent<CanvasGroup>();
		if (componentInParent != null)
		{
			Debug.Log($"Нашли CanvasGroup на {componentInParent.gameObject.name}: interactable={componentInParent.interactable}, alpha={componentInParent.alpha}");
			if (!componentInParent.interactable || componentInParent.alpha == 0f)
			{
				Debug.Log("Объект " + obj.name + " или его родитель (" + componentInParent.gameObject.name + ") не интерактивен.");
				return false;
			}
		}
		Debug.Log("Объект интерактивен по всей иерархии.");
		return true;
	}

	public void InvokeMethod(InputAction.CallbackContext context)
	{
		if (!base.gameObject.activeInHierarchy || !context.performed)
		{
			return;
		}
		if (lastFrameCount != Time.frameCount)
		{
			isMethodInvokedThisFrame = false;
			lastFrameCount = Time.frameCount;
		}
		if (isMethodInvokedThisFrame)
		{
			return;
		}
		isMethodInvokedThisFrame = true;
		if (targetObject == null || !targetObject.activeInHierarchy)
		{
			Debug.LogWarning("Целевой объект не задан или выключен.");
			return;
		}
		if (!IsInteractableInHierarchy(base.gameObject))
		{
			Debug.LogWarning("Один из родительских объектов имеет CanvasGroup с interactable = false или alpha = 0.");
			return;
		}
		Selectable component = targetObject.GetComponent<Selectable>();
		if (component != null && !component.interactable)
		{
			Debug.LogWarning("Элемент UI не interactable.");
			return;
		}
		Component[] components = targetObject.GetComponents<MonoBehaviour>();
		Component[] array = components;
		MethodInfo methodInfo = null;
		Component component2 = null;
		components = array;
		foreach (Component component3 in components)
		{
			methodInfo = component3.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodInfo != null)
			{
				component2 = component3;
				break;
			}
		}
		if (methodInfo == null || component2 == null)
		{
			Debug.LogWarning("Метод " + methodName + " не найден ни в одном MonoBehaviour на объекте " + targetObject.name + ".");
		}
		else
		{
			methodInfo.Invoke(component2, null);
		}
	}
}
