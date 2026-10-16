using UnityEngine;

[DisallowMultipleComponent]
public class PosterData : MonoBehaviour
{
	[SerializeField]
	private Sprite posterSprite;

	public Sprite PosterSprite => posterSprite;

	public void SetSprite(Sprite sprite)
	{
		posterSprite = sprite;
	}
}
