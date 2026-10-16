using UnityEngine;

public class ExtraTVSceneOnEnter : MonoBehaviour
{
	public bool isTriggered;

	private DialogueTrigger dialogTrig => GetComponent<DialogueTrigger>();

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && !isTriggered)
		{
			isTriggered = true;
			dialogTrig.StartDialogue();
			Player.GetInstance().IsMoving = false;
			Player.GetInstance().MoveDirection = Vector3.zero;
			Player.GetInstance().ChangeState(new PlayerIdleState());
		}
	}
}
