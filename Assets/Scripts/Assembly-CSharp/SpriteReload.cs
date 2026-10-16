using UnityEngine;

public class SpriteReload : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	private void Update()
	{
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		spriteRenderer.sprite = spriteRenderer.sprite;
	}
}
