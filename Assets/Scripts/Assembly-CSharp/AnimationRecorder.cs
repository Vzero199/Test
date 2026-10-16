using UnityEngine;

public class AnimationRecorder : MonoBehaviour
{
	public Transform target;

	private AnimationClip animationClip;

	private float recordingTime = 5f;

	private float timer;

	private AnimationCurve curveX;

	private AnimationCurve curveY;

	private AnimationCurve curveZ;

	private void Start()
	{
		animationClip = new AnimationClip
		{
			frameRate = 60f
		};
		curveX = new AnimationCurve();
		curveY = new AnimationCurve();
		curveZ = new AnimationCurve();
	}

	private void Update()
	{
		if (timer <= recordingTime)
		{
			Vector3 localPosition = target.localPosition;
			curveX.AddKey(timer, localPosition.x);
			curveY.AddKey(timer, localPosition.y);
			curveZ.AddKey(timer, localPosition.z);
			timer += Time.deltaTime;
		}
		else
		{
			SaveAnimation();
			base.enabled = false;
		}
	}

	private void SaveAnimation()
	{
		animationClip.SetCurve("", typeof(Transform), "localPosition.x", curveX);
		animationClip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
		animationClip.SetCurve("", typeof(Transform), "localPosition.z", curveZ);
	}
}
