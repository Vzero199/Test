using UnityEngine;

public class ExitGalleryLoop : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, animatorStateInfo, layerIndex);
		if (GallerySceneViewer.instance != null)
		{
			GallerySceneViewer.instance.CheckToExitState();
		}
	}
}
