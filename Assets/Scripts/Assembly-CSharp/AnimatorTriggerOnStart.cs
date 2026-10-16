using UnityEngine;

public class AnimatorTriggerOnStart : MonoBehaviour
{
	public Animator animator;

	public string triggerName;

	public bool hasTriggered;

	private void Start()
	{
		if (!GameSaveManager.IsLoading && animator != null && !string.IsNullOrEmpty(triggerName) && !hasTriggered)
		{
			animator.SetTrigger(triggerName);
			hasTriggered = true;
		}
	}
}
