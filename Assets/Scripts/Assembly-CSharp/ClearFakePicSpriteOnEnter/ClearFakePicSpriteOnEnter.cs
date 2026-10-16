using UnityEngine;

namespace ClearFakePicSpriteOnEnter
{
	public class ClearFakePicSpriteOnEnter : StateMachineBehaviour
	{
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			Transform transform = animator.transform.Find("FakePictureObject");
			if (transform != null)
			{
				transform.GetComponent<SpriteRenderer>().sprite = null;
			}
			Transform transform2 = animator.transform.Find("SecondFakePictureObject");
			if (transform2 != null)
			{
				Animator component = transform2.GetComponent<Animator>();
				if (component != null)
				{
					component.Play("Empty");
				}
			}
		}
	}
}
