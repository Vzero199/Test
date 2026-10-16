using UnityEngine;

public class DisableSkipButton : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EndEvent instance = EndEvent.GetInstance();
		if (instance != null && !EndEvent.isGameOver)
		{
			instance.BlockSkip();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EndEvent instance = EndEvent.GetInstance();
		if (instance != null)
		{
			instance.UnblockSkip();
		}
	}
}
