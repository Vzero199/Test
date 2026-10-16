using UnityEngine;

public class EmptySprite : MonoBehaviour
{
	[SerializeField]
	private Sprite emptySprite;

	public Sprite Sprite
	{
		get
		{
			return emptySprite;
		}
		set
		{
			emptySprite = value;
		}
	}
}
