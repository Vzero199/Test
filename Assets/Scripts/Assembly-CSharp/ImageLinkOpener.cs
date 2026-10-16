using UnityEngine;
using UnityEngine.EventSystems;

public class ImageLinkOpener : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private string url = "https://example.com";

	public void OnPointerClick(PointerEventData eventData)
	{
		Application.OpenURL(url);
	}
}
