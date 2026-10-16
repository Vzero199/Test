using UnityEngine;

public class ObjectShake : MonoBehaviour
{
	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void ShakeBaby()
	{
		animator.Play("ShakeBaby");
	}
}
