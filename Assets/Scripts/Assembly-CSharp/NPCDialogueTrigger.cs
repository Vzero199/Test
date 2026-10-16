using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NPCDialogueTrigger : InteractionObject
{
	public DialogueTrigger dialogueTrigger;

	private GameObject indicator;

	public Canvas worldSpaceCanvas;

	public Sprite mainTalkIcon;

	public Sprite sideTalkIcon;

	public Sprite extraTalkIcon;

	public Sprite repeatTalkIcon;

	public Sprite sleepTalkIcon;

	private static bool sceneIsChanging;

	public override string KeyItemName
	{
		get
		{
			return keyItemName;
		}
		set
		{
			keyItemName = value;
			UpdateIndicator();
			Player.GetInstance()?.HandlePlayerInteractUI();
			Player.GetInstance()?.ManualTriggerCheck();
		}
	}

	protected override void Start()
	{
		base.Start();
		if (worldSpaceCanvas == null)
		{
			GameObject gameObject = GameObject.Find("WorldSpaceCanvas");
			if (gameObject != null)
			{
				worldSpaceCanvas = gameObject.GetComponent<Canvas>();
			}
		}
		UpdateIndicator();
	}

	protected override void Update()
	{
		base.Update();
		HandleStartTimelinePlaying();
		if (indicator != null)
		{
			indicator.transform.position = base.transform.position + Vector3.up * 100f;
		}
		UpdateIndicator();
	}

	private void HandleStartTimelinePlaying()
	{
	}

	public override bool CheckConditions()
	{
		if (string.IsNullOrEmpty(KeyItemName) || IsTimelinePlaying() || DialogueManager.GetInstance().dialogueIsPlaying)
		{
			return false;
		}
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
		if (dialogueTrigger == null)
		{
			Debug.LogError("Dialogue Trigger Not Set!");
		}
		else
		{
			dialogueTrigger.StartDialogue();
		}
	}

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		if (!sceneIsChanging)
		{
			KeyItemName = "";
		}
	}

	private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
	{
		sceneIsChanging = true;
	}

	private void UpdateIndicator()
	{
		if (indicator != null)
		{
			Object.Destroy(indicator);
			indicator = null;
		}
		if (worldSpaceCanvas == null || string.IsNullOrEmpty(KeyItemName) || IsTimelinePlaying() || DialogueManager.GetInstance().dialogueIsPlaying)
		{
			return;
		}
		Sprite sprite = null;
		switch (KeyItemName)
		{
		case "mainTalk":
			sprite = mainTalkIcon;
			break;
		case "sideTalk":
			sprite = sideTalkIcon;
			break;
		case "extraTalk":
			sprite = extraTalkIcon;
			break;
		case "repeatTalk":
			sprite = repeatTalkIcon;
			break;
		case "sleepTalk":
			sprite = sleepTalkIcon;
			break;
		case "examine":
			sprite = null;
			break;
		}
		if (sprite != null)
		{
			indicator = new GameObject("DialogueIndicator");
			indicator.transform.SetParent(worldSpaceCanvas.transform, worldPositionStays: false);
			indicator.transform.SetSiblingIndex(0);
			float num = 1.5f;
			float num2 = 0f;
			BoxCollider component = GetComponent<BoxCollider>();
			if (component != null)
			{
				num = component.bounds.extents.y + component.center.y + 0.2f;
				num2 = component.bounds.center.x - base.transform.position.x;
			}
			indicator.transform.position = new Vector3(base.transform.position.x + num2, base.transform.position.y + num, base.transform.position.z);
			Image image = indicator.AddComponent<Image>();
			image.sprite = sprite;
			image.rectTransform.sizeDelta = new Vector2(32f, 32f);
			image.preserveAspect = true;
		}
	}
}
