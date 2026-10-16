using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueTrigger : MonoBehaviour
{
	[Header("Visual Cue (optional)")]
	[SerializeField]
	private GameObject visualCue;

	[Header("Emote Animator")]
	[SerializeField]
	private Animator emoteAnimator;

	[Header("Ink JSON")]
	[SerializeField]
	private TextAsset inkJSON;

	[SerializeField]
	private string dialogueName;

	[Header("Playable Director")]
	[SerializeField]
	private PlayableDirector playableDirector;

	private bool playerInRange;

	public string DialogueName
	{
		get
		{
			return dialogueName;
		}
		set
		{
			dialogueName = value;
			HandleDialogue();
		}
	}

	private void Awake()
	{
		playerInRange = false;
		if (visualCue != null)
		{
			visualCue.SetActive(value: false);
		}
		HandleDialogue();
	}

	private void HandleDialogue()
	{
		if (!string.IsNullOrEmpty(dialogueName))
		{
			string currentLanguage = GameSettings.CurrentLanguage;
			TextAsset textAsset = Resources.Load<TextAsset>("Dialogue/" + currentLanguage + "/" + dialogueName);
			if (textAsset == null)
			{
				Debug.LogError("Dialogue file " + dialogueName + " not found in path: Dialogue/" + currentLanguage + "/" + dialogueName + " For:" + base.gameObject.name);
			}
			else
			{
				SetDialogue(textAsset);
			}
		}
	}

	public void SetDialogue(TextAsset value)
	{
		inkJSON = value;
	}

	public void StartDialogue()
	{
		if (DialogueManager.GetInstance().dialogueIsPlaying)
		{
			StartCoroutine(ExitAndStartDialogue());
		}
		else
		{
			StartDialogueInternal();
		}
	}

	private void StartDialogueInternal()
	{
		if (playableDirector != null && playableDirector.playableGraph.IsValid())
		{
			playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0.0);
		}
		DialogueManager.GetInstance().EnterDialogueMode(inkJSON, emoteAnimator);
		DialogueManager.GetInstance().PlayableDirector = playableDirector;
	}

	private IEnumerator ExitAndStartDialogue()
	{
		DialogueManager instance = DialogueManager.GetInstance();
		yield return instance.ExitDialogueMode();
		StartDialogueInternal();
	}

	private void Update()
	{
		Object.FindObjectOfType<Player>();
		if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
		{
			if (visualCue != null)
			{
				visualCue.SetActive(value: true);
			}
		}
		else if (visualCue != null)
		{
			visualCue.SetActive(value: false);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.tag == "Player")
		{
			playerInRange = true;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider.gameObject.tag == "Player")
		{
			playerInRange = false;
		}
	}
}
