using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HairAnimationPlayer : MonoBehaviour
{
	public HairAnimationAsset animationAsset;

	private SpriteRenderer spriteRenderer;

	private float timer;

	private Sprite tempSprite;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (animationAsset == null || animationAsset.frames.Count == 0)
		{
			return;
		}
		timer += Time.deltaTime;
		Texture2D frame = animationAsset.GetFrame(timer);
		if (!(frame == null))
		{
			if (tempSprite == null || tempSprite.texture != frame)
			{
				tempSprite = Sprite.Create(frame, new Rect(0f, 0f, frame.width, frame.height), new Vector2(0.5f, 0.5f));
			}
			spriteRenderer.sprite = tempSprite;
		}
	}
}
