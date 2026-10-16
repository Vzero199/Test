using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private bool lastInteractableState;

	private void Start()
	{
		lastInteractableState = button.interactable;
		UpdateButtonState();
	}

	private void Update()
	{
		UpdateButtonState();
	}

	private void UpdateButtonState()
	{
		if (button.interactable)
		{
			SetButtonNormal();
		}
		else
		{
			SetButtonDisabled();
		}
	}

	private void SetButtonNormal()
	{
		button.GetComponent<Animator>()?.SetTrigger("langTransp");
	}

	private void SetButtonDisabled()
	{
		button.GetComponent<Animator>()?.SetTrigger("langTransp");
	}
}
