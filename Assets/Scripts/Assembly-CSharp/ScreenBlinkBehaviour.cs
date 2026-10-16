using UnityEngine;

public class ScreenBlinkBehaviour : StateMachineBehaviour
{
	private int loopCount;

	public int maxLoops = 3;

	private bool blinkTriggered;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		loopCount = 0;
		blinkTriggered = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float num = stateInfo.normalizedTime - (float)loopCount;
		if (stateInfo.normalizedTime >= (float)(loopCount + 1))
		{
			loopCount++;
		}
		if (loopCount == maxLoops - 1 && num >= 0.2f && !blinkTriggered)
		{
			BlinkManager.GetInstance().BlinkWithCustomSpeed(2f, BlinkManager.GetInstance().gameObject, null, allowVerticalMovement: true, 0f, 0f, BlinkManager.GetInstance().gameObject);
			blinkTriggered = true;
		}
		if (loopCount >= maxLoops)
		{
			animator.SetTrigger("NextAnimation");
		}
	}
}
