using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using SmallHedge.SoundManager;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class DialogueManager : MonoBehaviour
{
	[Header("Params")]
	[SerializeField]
	private float typingSpeed = 0.04f;

	[Header("Load Globals JSON")]
	[SerializeField]
	private TextAsset loadGlobalsJSON;

	[Header("Dialogue UI")]
	[SerializeField]
	private GameObject dialoguePanel;

	[SerializeField]
	private GameObject continueIcon;

	[SerializeField]
	private TextMeshProUGUI dialogueText;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private Animator portraitAnimator;

	private Animator layoutAnimator;

	[Header("Choices UI")]
	[SerializeField]
	private GameObject[] choices;

	private TextMeshProUGUI[] choicesText;

	[Header("Audio")]
	[SerializeField]
	private DialogueAudioInfoSO defaultAudioInfo;

	[SerializeField]
	private DialogueAudioInfoSO[] audioInfos;

	[SerializeField]
	private bool makePredictable;

	private DialogueAudioInfoSO currentAudioInfo;

	private Dictionary<string, DialogueAudioInfoSO> audioInfoDictionary;

	private AudioSource audioSource;

	private bool isExitingDialogue;

	private PlayableDirector playableDirector;

	private Story currentStory;

	private bool canContinueToNextLine;

	private Coroutine displayLineCoroutine;

	private static DialogueManager instance;

	private const string SPEAKER_TAG = "speaker";

	private const string PORTRAIT_TAG = "portrait";

	private const string LAYOUT_TAG = "layout";

	private const string AUDIO_TAG = "audio";

	private DialogueVariables dialogueVariables;

	private InkExternalFunctions inkExternalFunctions;

	private CanvasGroup dialogueCanvasGroup;

	public PlayableDirector PlayableDirector
	{
		get
		{
			return playableDirector;
		}
		set
		{
			playableDirector = value;
		}
	}

	public bool dialogueIsPlaying { get; private set; }

	public void SetTypingSpeed(float value)
	{
		typingSpeed = value;
	}

	private void Awake()
	{
		if (instance != null)
		{
			Debug.LogWarning("Found more than one Dialogue Manager in the scene");
		}
		instance = this;
		dialogueVariables = new DialogueVariables(loadGlobalsJSON);
		inkExternalFunctions = new InkExternalFunctions();
		audioSource = base.gameObject.AddComponent<AudioSource>();
		currentAudioInfo = defaultAudioInfo;
		AudioMixerGroup uIGroup = SoundManager.GetUIGroup();
		if (uIGroup != null)
		{
			audioSource.outputAudioMixerGroup = uIGroup;
		}
		dialogueCanvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
		if (dialogueCanvasGroup == null)
		{
			dialogueCanvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
		}
		dialogueCanvasGroup.alpha = 0f;
	}

	public static DialogueManager GetInstance()
	{
		return instance;
	}

	private void Start()
	{
		dialogueIsPlaying = false;
		dialoguePanel.SetActive(value: false);
		layoutAnimator = dialoguePanel.GetComponent<Animator>();
		choicesText = new TextMeshProUGUI[choices.Length];
		for (int i = 0; i < choices.Length; i++)
		{
			choicesText[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();
		}
		InitializeAudioInfoDictionary();
	}

	private void InitializeAudioInfoDictionary()
	{
		audioInfoDictionary = new Dictionary<string, DialogueAudioInfoSO> { { defaultAudioInfo.id, defaultAudioInfo } };
		DialogueAudioInfoSO[] array = audioInfos;
		foreach (DialogueAudioInfoSO dialogueAudioInfoSO in array)
		{
			audioInfoDictionary.Add(dialogueAudioInfoSO.id, dialogueAudioInfoSO);
		}
	}

	private void SetCurrentAudioInfo(string id)
	{
		DialogueAudioInfoSO value = null;
		audioInfoDictionary.TryGetValue(id, out value);
		if (value != null)
		{
			currentAudioInfo = value;
		}
		else
		{
			Debug.LogWarning("Failed to find audio info for id: " + id);
		}
	}

	private void Update()
	{
		if (!dialogueIsPlaying || isExitingDialogue)
		{
			return;
		}
		if (canContinueToNextLine && currentStory.currentChoices.Count == 0 && Player.GetInstance() != null && Player.GetInstance().IsActionButton)
		{
			Player.GetInstance().IsActionButton = false;
			ContinueStory();
		}
		else if (!canContinueToNextLine && Player.GetInstance() != null && Player.GetInstance().IsActionButton)
		{
			Player.GetInstance().IsActionButton = false;
			if (displayLineCoroutine != null)
			{
				StopCoroutine(displayLineCoroutine);
				DisplayChoices();
				dialogueText.maxVisibleCharacters = dialogueText.text.Length;
				continueIcon.SetActive(value: true);
				canContinueToNextLine = true;
			}
		}
	}

	public void OnAction(InputAction.CallbackContext context)
	{
		if (context.performed && !isExitingDialogue)
		{
			if (canContinueToNextLine && currentStory.currentChoices.Count == 0)
			{
				ContinueStory();
			}
			else if (!canContinueToNextLine && displayLineCoroutine != null)
			{
				StopCoroutine(displayLineCoroutine);
				DisplayChoices();
				dialogueText.maxVisibleCharacters = dialogueText.text.Length;
				continueIcon.SetActive(value: true);
				canContinueToNextLine = true;
			}
		}
	}

	public void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator)
	{
		currentStory = new Story(inkJSON.text);
		dialogueIsPlaying = true;
		if (Player.GetInstance() != null)
		{
			Player.GetInstance()?.HandlePlayerInteractUI();
			Player.GetInstance()?.DestroyCurrentArrow();
		}
		dialoguePanel.SetActive(value: true);
		if (playableDirector != null && playableDirector.playableGraph.IsValid())
		{
			playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0.0);
		}
		dialogueVariables.StartListening(currentStory);
		inkExternalFunctions.Bind(currentStory, emoteAnimator);
		displayNameText.text = "???";
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().IsPaused = true;
			Player.GetInstance().IsActionButton = false;
		}
		StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, 0.3f));
		ContinueStory();
	}

	public void StopDialogueMode()
	{
		if (dialogueIsPlaying)
		{
			StartCoroutine(ExitDialogueMode());
		}
	}

	public IEnumerator ExitDialogueMode()
	{
		if (!isExitingDialogue)
		{
			isExitingDialogue = true;
			if (playableDirector != null && playableDirector.playableGraph.IsValid())
			{
				playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1.0);
				playableDirector = null;
			}
			else if (Player.GetInstance() != null)
			{
				Player.GetInstance().IsPaused = false;
			}
			yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, 0.3f));
			dialoguePanel.SetActive(value: false);
			dialogueVariables.StopListening(currentStory);
			inkExternalFunctions.Unbind(currentStory);
			dialogueIsPlaying = false;
			if (Player.GetInstance() != null)
			{
				Player.GetInstance()?.HandlePlayerInteractUI();
				Player.GetInstance()?.ManualTriggerCheck();
			}
			dialogueText.text = "";
			SetCurrentAudioInfo(defaultAudioInfo.id);
			isExitingDialogue = false;
		}
	}

	private void ContinueStory()
	{
		while (currentStory.canContinue)
		{
			string text = currentStory.Continue();
			if (!string.IsNullOrWhiteSpace(text))
			{
				HandleTags(currentStory.currentTags);
				if (displayLineCoroutine != null)
				{
					StopCoroutine(displayLineCoroutine);
				}
				displayLineCoroutine = StartCoroutine(DisplayLine(text));
				return;
			}
		}
		StartCoroutine(ExitDialogueMode());
	}

	private IEnumerator DisplayLine(string line)
	{
		dialogueText.text = line;
		dialogueText.maxVisibleCharacters = 0;
		continueIcon.SetActive(value: false);
		HideChoices();
		canContinueToNextLine = false;
		bool isAddingRichTextTag = false;
		char[] array = line.ToCharArray();
		foreach (char letter in array)
		{
			if (Player.GetInstance() != null && Player.GetInstance().IsActionButton)
			{
				dialogueText.maxVisibleCharacters = line.Length;
				continueIcon.SetActive(value: true);
				DisplayChoices();
				canContinueToNextLine = true;
				yield break;
			}
			if ((letter == '<') | isAddingRichTextTag)
			{
				isAddingRichTextTag = true;
				if (letter == '>')
				{
					isAddingRichTextTag = false;
				}
				continue;
			}
			PlayDialogueSound(dialogueText.maxVisibleCharacters, dialogueText.text[dialogueText.maxVisibleCharacters]);
			dialogueText.maxVisibleCharacters++;
			yield return new WaitForSeconds(typingSpeed);
			if (letter == ',' || letter == '.' || letter == '!' || letter == '?')
			{
				yield return new WaitForSeconds(typingSpeed * 5f);
			}
		}
		continueIcon.SetActive(value: true);
		DisplayChoices();
		canContinueToNextLine = true;
	}

	private void PlayDialogueSound(int currentDisplayedCharacterCount, char currentCharacter)
	{
		if (typingSpeed <= 0f)
		{
			return;
		}
		AudioClip[] dialogueTypingSoundClips = currentAudioInfo.dialogueTypingSoundClips;
		int frequencyLevel = currentAudioInfo.frequencyLevel;
		float minPitch = currentAudioInfo.minPitch;
		float maxPitch = currentAudioInfo.maxPitch;
		bool stopAudioSource = currentAudioInfo.stopAudioSource;
		if (currentDisplayedCharacterCount % frequencyLevel != 0)
		{
			return;
		}
		if (stopAudioSource)
		{
			audioSource.Stop();
		}
		AudioClip audioClip = null;
		if (makePredictable)
		{
			int hashCode = currentCharacter.GetHashCode();
			int num = Mathf.Abs(hashCode) % dialogueTypingSoundClips.Length;
			audioClip = dialogueTypingSoundClips[num];
			int num2 = (int)(minPitch * 100f);
			int num3 = (int)(maxPitch * 100f) - num2;
			if (num3 != 0)
			{
				float pitch = (float)(hashCode % num3 + num2) / 100f;
				audioSource.pitch = pitch;
			}
			else
			{
				audioSource.pitch = minPitch;
			}
		}
		else
		{
			int num4 = Random.Range(0, dialogueTypingSoundClips.Length);
			audioClip = dialogueTypingSoundClips[num4];
			float pitch2 = Random.Range(minPitch, maxPitch);
			audioSource.pitch = pitch2;
		}
		audioSource.PlayOneShot(audioClip);
	}

	private void HandleTags(List<string> currentTags)
	{
		foreach (string currentTag in currentTags)
		{
			string[] array = currentTag.Split(':');
			if (array.Length != 2)
			{
				Debug.LogError("Tag could not be appropriately parsed: " + currentTag);
			}
			string text = array[0].Trim();
			string text2 = array[1].Trim();
			switch (text)
			{
			case "speaker":
				displayNameText.text = text2;
				break;
			case "portrait":
				portraitAnimator.Play(text2);
				break;
			case "layout":
				layoutAnimator.Play(text2);
				break;
			case "audio":
				SetCurrentAudioInfo(text2);
				break;
			default:
				Debug.LogWarning("Tag came in but is not currently being handled: " + currentTag);
				break;
			}
		}
	}

	private void HideChoices()
	{
		GameObject[] array = choices;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	private void DisplayChoices()
	{
		List<Choice> currentChoices = currentStory.currentChoices;
		if (currentChoices.Count > choices.Length)
		{
			Debug.LogError("More choices were given than the UI can support. Number of choices given: " + currentChoices.Count);
		}
		int num = 0;
		foreach (Choice item in currentChoices)
		{
			choices[num].SetActive(value: true);
			choicesText[num].text = item.text;
			num++;
		}
		for (int i = num; i < choices.Length; i++)
		{
			choices[i].SetActive(value: false);
		}
		StartCoroutine(SelectFirstChoice());
	}

	private IEnumerator SelectFirstChoice()
	{
		EventSystem.current.SetSelectedGameObject(null);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForEndOfFrame();
		EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
	}

	public void MakeChoice(int choiceIndex)
	{
		if (!isExitingDialogue && canContinueToNextLine)
		{
			currentStory.ChooseChoiceIndex(choiceIndex);
			if (Player.GetInstance() != null && Player.GetInstance().IsActionButton)
			{
				Player.GetInstance().IsActionButton = false;
				ContinueStory();
			}
		}
	}

	public Ink.Runtime.Object GetVariableState(string variableName)
	{
		Ink.Runtime.Object value = null;
		dialogueVariables.variables.TryGetValue(variableName, out value);
		if (value == null)
		{
			Debug.LogWarning("Ink Variable was found to be null: " + variableName);
		}
		return value;
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
	{
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = endAlpha;
	}
}
