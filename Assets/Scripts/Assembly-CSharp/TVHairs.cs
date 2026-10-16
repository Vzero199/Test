using System.Collections;
using UnityEngine;

public class TVHairs : MonoBehaviour
{
	private Coroutine currentCoroutine;

	public void DisableHairs()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
				currentCoroutine = null;
			}
			currentCoroutine = StartCoroutine(DisableCoroutine());
		}
	}

	public void EnableHairs()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (currentCoroutine != null)
			{
				StopCoroutine(currentCoroutine);
				currentCoroutine = null;
			}
			currentCoroutine = StartCoroutine(EnableCoroutine());
		}
	}

	private IEnumerator DisableCoroutine()
	{
		SpriteRenderer spriteRenderer = base.gameObject.GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
		{
			Color color = spriteRenderer.color;
			float duration = 1f;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				float a = Mathf.Lerp(1f, 0f, elapsed / duration);
				spriteRenderer.color = new Color(color.r, color.g, color.b, a);
				elapsed += Time.deltaTime;
				yield return null;
			}
			spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);
		}
		yield return new WaitForSeconds(0f);
		base.gameObject.transform.position = new Vector3(0f, -10000f, 0f);
		currentCoroutine = null;
	}

	private IEnumerator EnableCoroutine()
	{
		SpriteRenderer spriteRenderer = base.gameObject.GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
		{
			Color color = spriteRenderer.color;
			float duration = 1f;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				float a = Mathf.Lerp(0f, 1f, elapsed / duration);
				spriteRenderer.color = new Color(color.r, color.g, color.b, a);
				elapsed += Time.deltaTime;
				yield return null;
			}
			spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
		}
		yield return new WaitForSeconds(0f);
		currentCoroutine = null;
	}

	public void Catch()
	{
		BlinkManager.GetInstance().FadeToBlink(Player.GetInstance().gameObject, SetSexMode);
		InteractionManager.Instance.StartInteractionCooldown(Player.GetInstance());
		if (Player.GetInstance().InteractObject != null)
		{
			Player.GetInstance().InteractObject.StopInteract();
			Player.GetInstance().InteractObject = null;
		}
		Player.GetInstance().IsPaused = true;
	}

	public void SetSexMode()
	{
		Player.GetInstance().ChangeState(new PlayerIdleState());
		Player.GetInstance().IsPaused = true;
		Player.GetInstance().GetComponent<Animator>().Play("HairCatch");
	}
}
