using UnityEngine;

public class EnableSkipButton : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: true);
	}
}
