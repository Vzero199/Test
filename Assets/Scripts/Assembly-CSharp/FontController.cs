using TMPro;
using UnityEngine;

public class FontController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text mainText;

	[SerializeField]
	private TMP_Text choice1Text;

	[SerializeField]
	private TMP_Text choice2Text;

	[SerializeField]
	private TMP_Text choice3Text;

	[SerializeField]
	private TMP_FontAsset spookyFont;

	[SerializeField]
	private TMP_FontAsset animeFont;

	[SerializeField]
	private TMP_FontAsset mangoFont;

	public void SetFontElevator()
	{
		mainText.font = spookyFont;
		choice1Text.font = spookyFont;
		choice2Text.font = spookyFont;
		choice3Text.font = spookyFont;
	}

	public void SetFontSimple()
	{
		mainText.font = animeFont;
		choice1Text.font = mangoFont;
		choice2Text.font = mangoFont;
		choice3Text.font = mangoFont;
	}
}
