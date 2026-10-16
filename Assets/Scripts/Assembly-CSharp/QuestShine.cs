using System.Linq;
using System.Reflection;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Playables;

public class QuestShine : InteractionObject
{
	[SerializeField]
	private string questName;

	private TimelineManager timelineManager => TimelineManager.GetInstance();

	private BlinkManager blinkManager => BlinkManager.GetInstance();

	public string QuestName
	{
		get
		{
			return questName;
		}
		set
		{
			questName = value;
		}
	}

	public override bool CheckConditions()
	{
		if (Player.GetInstance().CanInteract)
		{
			return base.CheckConditions();
		}
		return false;
	}

	protected override bool CheckInteractButton()
	{
		if (player.IsActionButton)
		{
			return CheckConditions();
		}
		return false;
	}

	protected override void InteractComplete()
	{
		base.InteractComplete();
		MethodInfo method = GetType().GetMethod(questName, BindingFlags.Instance | BindingFlags.NonPublic);
		if (method != null)
		{
			method.Invoke(this, null);
		}
		else
		{
			GameObject gameObject = Resources.Load<GameObject>("Prefabs/UI_Nothing");
			if (gameObject == null && base.transform.childCount == 0)
			{
				Debug.LogError("UI_Nothing prefab not found!");
			}
			else if (base.transform.childCount == 0)
			{
				GameObject gameObject2 = GameObject.Find("WorldSpaceCanvas");
				if (gameObject2 == null)
				{
					Debug.LogError("WorldSpaceCanvas not found in scene!");
				}
				else
				{
					Transform parent = gameObject2.transform;
					GameObject currentHighlightedButton = Player.GetInstance().GetCurrentHighlightedButton();
					if (currentHighlightedButton != null)
					{
						GameObject obj = Object.Instantiate(gameObject, parent);
						obj.transform.SetAsFirstSibling();
						obj.transform.position = currentHighlightedButton.transform.position + new Vector3(11f, -3.5f, 0f);
						obj.transform.rotation = currentHighlightedButton.transform.rotation;
						obj.transform.localScale = currentHighlightedButton.transform.lossyScale;
						Player.GetInstance().DestroyButton(currentHighlightedButton);
						Debug.Log("Spawned UI_Nothing under WorldSpaceCanvas at button position.");
					}
					else
					{
						Debug.LogWarning("No highlighted button found for fallback UI.");
					}
				}
			}
		}
		base.gameObject.SetActive(value: false);
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().RemoveFromPlayerList(this);
		}
	}

	private void FindStuffQuestProgress()
	{
		SetDialogueToNPC("1Day/completeMainQuest", "nurseSakimu", "mainTalk");
		ClearSiblingsShines("findStuffQuestShine");
		SoundManager.PlaySound(SoundType.TakeItem);
		string[] array = new string[2] { "questShine2", "questShine3" };
		GameObject[] array2 = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = GameObject.Find(array[i]);
			if (array2[i] == null)
			{
				Debug.LogWarning("Объект с именем " + array[i] + " не найден на сцене.");
			}
		}
		GameObject[] array3 = array2;
		foreach (GameObject gameObject in array3)
		{
			if (!(gameObject == null))
			{
				gameObject.SetActive(value: false);
			}
		}
		QuestManager.GetInstance().FindAndToggleQuestTask("questTask1", toggle: false);
		QuestManager.GetInstance().FindAndToggleQuestTask("questTask2", toggle: true);
	}

	private void SecondDayPussyTimeline()
	{
		QuestManager.SetDialogueForObject("SignalReceiver", "2Day/startSideQuest2IMeanNicePussy");
		blinkManager.FadeToBlink(blinkManager.gameObject, () =>
		{
			LoadAndPlayTimeline("SecondDayPussyFeedTask");
		}, allowVerticalMovement: true, 0f, 0f, blinkManager.gameObject);
	}

	private void FourthDayBloodStepTimeline()
	{
		SetDialogueToNPC("4Day/completeSideQuest1BloodStep", "VampQuest", "sideTalk");
		QuestManager.GetInstance()?.RunMethod("FourthDayBloodStepProgress");
		LoadAndPlayTimeline("FourthDayBloodStepTask");
	}

	private void FifthDayStartNight()
	{
		QuestManager.GetInstance()?.RunMethod("StartFifthNight");
	}

	private void SecondDayLostGirlShow()
	{
		blinkManager.FadeToBlink(base.gameObject, () =>
		{
			QuestManager.GetInstance()?.RunMethod("SecondDayLostGirlShow");
			StartDialogue("2Day/completeSideQuest1LostGirl");
		}, allowVerticalMovement: true, 0f, 0f, base.gameObject);
	}

	public void StartDialogue(string dialogueName)
	{
		TextAsset inkJSON = HandleDialogue(dialogueName);
		if (!DialogueManager.GetInstance().dialogueIsPlaying)
		{
			DialogueManager.GetInstance().EnterDialogueMode(inkJSON, null);
			DialogueManager.GetInstance().PlayableDirector = null;
		}
	}

	private TextAsset HandleDialogue(string dialogueName)
	{
		if (string.IsNullOrEmpty(dialogueName))
		{
			return null;
		}
		string currentLanguage = GameSettings.CurrentLanguage;
		TextAsset textAsset = Resources.Load<TextAsset>("Dialogue/" + currentLanguage + "/" + dialogueName);
		if (textAsset == null)
		{
			Debug.LogError("Dialogue file " + dialogueName + " not found in path: Dialogue/" + currentLanguage + "/" + dialogueName + " For:" + base.gameObject.name);
			return null;
		}
		return textAsset;
	}

	private void ThirdDayMainQuest()
	{
		blinkManager.FadeToBlink(blinkManager.gameObject, () =>
		{
			LoadAndPlayTimeline("ThirdDayMainQuest");
		}, allowVerticalMovement: true, 0f, 0f, blinkManager.gameObject);
		SoundManager.PlaySound(SoundType.DoorOpen);
		GameObject gameObject = GameObject.Find("StageDoor303-1");
		if (gameObject != null)
		{
			DoorTransit component = gameObject.GetComponent<DoorTransit>();
			if (component != null)
			{
				component.KeyItemName = "";
			}
		}
		QuestManager.GetInstance().FindAndToggleQuestTask("questTask6", toggle: false);
		QuestManager.GetInstance().FindAndToggleQuestTask("questTask7", toggle: true);
	}

	private void FifthNightMainTimelineWithTrueEnd()
	{
		QuestManager.GetInstance()?.RunMethod("FifthNightMainTimelineWithTrueEnd");
	}

	private void LoadAndPlayTimeline(string timelineName)
	{
		string text = "Timelines/" + timelineName;
		PlayableAsset playableAsset = Resources.Load<PlayableAsset>(text);
		if (playableAsset != null && timelineManager != null && timelineManager.Director != null)
		{
			timelineManager.Director.playableAsset = playableAsset;
			timelineManager.Director.Play();
		}
		else
		{
			Debug.LogError("Не удалось загрузить Timeline: " + text);
		}
	}

	public static void SetDialogueToNPC(string dialogueName, string npcName, string dialogueType)
	{
		GameObject gameObject = GameObject.Find(npcName);
		if (gameObject == null)
		{
			Debug.LogError("Failed to Find " + npcName);
			return;
		}
		DialogueTrigger componentInChildren = gameObject.GetComponentInChildren<DialogueTrigger>();
		if (componentInChildren != null)
		{
			componentInChildren.DialogueName = dialogueName;
		}
		else
		{
			Debug.LogError("DialogueTrigger not found on " + npcName + ".");
		}
		InteractionObject componentInChildren2 = gameObject.GetComponentInChildren<NPCDialogueTrigger>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.KeyItemName = dialogueType;
		}
		else
		{
			Debug.LogError("NPCDialogueTrigger not found on " + npcName + ".");
		}
	}

	private void ClearSiblingsShines(string siblingsName)
	{
		QuestShine[] array = (from obj in Object.FindObjectsOfType<QuestShine>()
			where obj.name == siblingsName
			select obj).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			Object.Destroy(array[num].gameObject);
		}
	}
}
