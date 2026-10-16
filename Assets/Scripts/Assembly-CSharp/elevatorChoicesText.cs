using UnityEngine;

public class elevatorChoicesText : MonoBehaviour
{
	private Elevator elevator;

	private void Start()
	{
		elevator = GetComponent<Elevator>();
		HandleDialogue();
	}

	private void HandleDialogue()
	{
		string currentLanguage = GameSettings.CurrentLanguage;
		TextAsset textAsset = Resources.Load<TextAsset>("Dialogue/" + currentLanguage + "/elevator_choices");
		if (textAsset == null)
		{
			Debug.LogError("Dialogue file not found in path: Dialogue/" + currentLanguage + "/ :" + base.gameObject.name);
		}
		else
		{
			elevator.inkJSON = textAsset;
		}
	}
}
