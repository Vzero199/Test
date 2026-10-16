using UnityEngine;

public class SignalDialogueTrigger : MonoBehaviour
{
	private static SignalDialogueTrigger instance;

	private static DialogueTrigger timelineDialogueTrigger;

	public static SignalDialogueTrigger GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<SignalDialogueTrigger>();
		}
		return instance;
	}

	private void Awake()
	{
		timelineDialogueTrigger = GetComponent<DialogueTrigger>();
	}

	public static void SetDialogueByName(string value)
	{
		if (!(timelineDialogueTrigger == null))
		{
			timelineDialogueTrigger.DialogueName = value;
		}
	}
}
