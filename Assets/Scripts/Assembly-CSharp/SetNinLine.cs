using UnityEngine;

public class SetNinLine : MonoBehaviour
{
	private void Start()
	{
		bool progress = GlobalSaveSystem.GetProgress("NintendoKarma");
		DialogueTrigger componentInChildren = GetComponentInChildren<DialogueTrigger>();
		if (progress)
		{
			componentInChildren.DialogueName = "Extra/DontTellSecretNintendo";
		}
		else
		{
			componentInChildren.DialogueName = "Extra/TellSecretNintendo1";
		}
	}
}
