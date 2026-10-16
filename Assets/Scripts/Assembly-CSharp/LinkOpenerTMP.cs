using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkOpenerTMP : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private TextMeshProUGUI textMeshPro;

	private void Awake()
	{
		textMeshPro = GetComponent<TextMeshProUGUI>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, eventData.enterEventCamera);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = textMeshPro.textInfo.linkInfo[num];
			Application.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}
}
