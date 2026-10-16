using UnityEngine;

public class ParentAnimator : MonoBehaviour
{
	public Animator childAnimator;

	public void PlayChildAnimation(string animationName)
	{
		if (childAnimator != null && !childAnimator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
		{
			childAnimator.Play(animationName);
		}
	}

	public void SetChildAnimationSpeed(float speed)
	{
		if (childAnimator != null)
		{
			childAnimator.speed = speed;
		}
	}
}
