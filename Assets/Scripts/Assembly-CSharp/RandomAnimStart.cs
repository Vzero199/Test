using System.Collections;
using UnityEngine;

public class RandomAnimStart : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(SetRandomAnimationTime());
	}

	private IEnumerator SetRandomAnimationTime()
	{
		yield return new WaitForEndOfFrame();
		Animator component = GetComponent<Animator>();
		if (!(component == null))
		{
			component.Play(component.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, Random.Range(0f, 1f));
		}
	}
}
