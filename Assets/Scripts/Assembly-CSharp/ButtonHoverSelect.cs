using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSelect : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}
}
