using System.Collections;
using UnityEngine;

public class StepAnimation : MonoBehaviour
{
	private Animator animator;

	private SpriteRenderer spriteRenderer;

	private Material fadeMaterial;

	private Coroutine fadeCoroutine;

	private bool isInMask;

	private void Start()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
		{
			fadeMaterial = new Material(Shader.Find("Sprites/Default"));
			spriteRenderer.material = fadeMaterial;
		}
		SetMaterialAlpha(0f);
	}

	public void SetStepAnimation(int stepNumber)
	{
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
		animator.Play("Step" + stepNumber);
		CreatePostStep(stepNumber);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Mask"))
		{
			SetInMask(inMask: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Mask"))
		{
			SetInMask(inMask: false);
		}
	}

	private void CreatePostStep(int stepNumber)
	{
		GameObject obj = Object.Instantiate(base.gameObject, base.transform.position, Quaternion.identity);
		obj.transform.SetParent(base.gameObject.transform);
		StepAnimation component = obj.GetComponent<StepAnimation>();
		if (component != null)
		{
			component.PlayPostStepAnimation(stepNumber);
		}
	}

	public void PlayPostStepAnimation(int stepNumber)
	{
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
		animator.Play("PostStep" + stepNumber);
	}

	public void SetDirection(float moveDirectionX)
	{
		if (moveDirectionX > 0f && base.transform.rotation.eulerAngles.y == 180f)
		{
			base.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x, 0f, base.transform.rotation.eulerAngles.z);
		}
		else if (moveDirectionX < 0f && base.transform.rotation.eulerAngles.y == 0f)
		{
			base.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x, 180f, base.transform.rotation.eulerAngles.z);
		}
	}

	public void SelfDestroy()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetInMask(bool inMask)
	{
		if (isInMask == inMask)
		{
			return;
		}
		isInMask = inMask;
		if (isInMask)
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			SetMaterialAlpha(1f);
		}
		else
		{
			if (fadeCoroutine != null)
			{
				StopCoroutine(fadeCoroutine);
			}
			fadeCoroutine = StartCoroutine(FadeOut(1f));
		}
	}

	private IEnumerator FadeOut(float duration)
	{
		float startAlpha = fadeMaterial.color.a;
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			float materialAlpha = Mathf.Lerp(startAlpha, 0f, time / duration);
			SetMaterialAlpha(materialAlpha);
			yield return null;
		}
		SetMaterialAlpha(0f);
	}

	private void SetMaterialAlpha(float alpha)
	{
		Color color = fadeMaterial.color;
		color.a = alpha;
		fadeMaterial.color = color;
	}
}
