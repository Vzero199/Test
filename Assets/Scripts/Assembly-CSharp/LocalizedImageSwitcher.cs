using UnityEngine;
using UnityEngine.UI;

public class LocalizedImageSwitcher : MonoBehaviour
{
	[Header("Спрайты по языкам")]
	public Sprite spriteRU;

	public Sprite spriteEN;

	public Sprite spriteCN;

	public Sprite spriteJP;

	private Image image;

	private void Awake()
	{
		image = GetComponent<Image>();
		if (image == null)
		{
			Debug.LogError("[LocalizedImageSwitcher] Отсутствует компонент Image на объекте " + base.gameObject.name);
		}
		else
		{
			SwitchSpriteByLanguage();
		}
	}

	public void SwitchSpriteByLanguage()
	{
		switch (GameSettings.CurrentLanguage)
		{
		case "RU":
			image.sprite = spriteRU;
			break;
		case "EN":
			image.sprite = spriteEN;
			break;
		case "CN":
			image.sprite = spriteCN;
			break;
		case "JP":
			image.sprite = spriteJP;
			break;
		default:
			Debug.LogWarning("[LocalizedImageSwitcher] Неизвестный язык: " + GameSettings.CurrentLanguage);
			break;
		}
	}
}
