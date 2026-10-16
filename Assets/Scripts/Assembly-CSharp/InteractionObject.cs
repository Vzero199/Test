using UnityEngine;
using UnityEngine.Playables;

public abstract class InteractionObject : MonoBehaviour
{
	[SerializeField]
	protected Sprite buttonIcon;

	[SerializeField]
	private float maxRadius = 1f;

	[SerializeField]
	private Sprite setButtonIcon;

	[SerializeField]
	private PlayableDirector timeline;

	protected Player player;

	protected GameObject interactableObject;

	protected BoxCollider IOCollider;

	protected bool isGrowing;

	protected float currentRadius;

	private float timer;

	[SerializeField]
	protected string keyItemName = "";

	private bool interactionComplete;

	public float MaxRadius => maxRadius;

	public float CurrentRadius => currentRadius;

	public virtual string KeyItemName
	{
		get
		{
			return keyItemName;
		}
		set
		{
			keyItemName = value;
			Player.GetInstance()?.HandlePlayerInteractUI();
			Player.GetInstance()?.ManualTriggerCheck();
		}
	}

	public bool InteractionComplete
	{
		get
		{
			return interactionComplete;
		}
		protected set
		{
			interactionComplete = value;
		}
	}

	public Sprite ButtonIcon
	{
		get
		{
			return buttonIcon;
		}
		set
		{
			buttonIcon = value;
		}
	}

	public BoxCollider GetIOCollider()
	{
		return IOCollider;
	}

	protected virtual void Start()
	{
		IOCollider = GetComponent<BoxCollider>();
		player = Player.GetInstance();
		if (timeline == null)
		{
			timeline = Object.FindObjectOfType<PlayableDirector>();
		}
	}

	protected bool IsTimelinePlaying()
	{
		if (timeline != null)
		{
			return timeline.state == PlayState.Playing;
		}
		return false;
	}

	protected virtual void Update()
	{
		HandleCircleGrowth();
	}

	private void HandleCircleGrowth()
	{
		if (Player.GetInstance() == null)
		{
			return;
		}
		Item currentSelectedItem = Player.GetInstance().CurrentSelectedItem;
		if (isGrowing && Player.GetInstance().gameObject.activeSelf && Player.GetInstance().InteractObject != null && Player.GetInstance().InteractObject.IOCollider.enabled)
		{
			if (string.IsNullOrEmpty(keyItemName) || (currentSelectedItem != null && keyItemName == currentSelectedItem.GetComponentInChildren<Item>().ItemName) || this is NPCDialogueTrigger || this is GallerySceneViewer)
			{
				GrowCircle();
			}
		}
		else
		{
			ShrinkCircle();
		}
	}

	private void GrowCircle()
	{
		float currentInteractionSpeed = player.CurrentInteractionSpeed;
		if (currentRadius < maxRadius)
		{
			currentRadius += currentInteractionSpeed * Time.deltaTime;
			currentRadius = Mathf.Min(currentRadius, maxRadius);
		}
		if (currentRadius >= maxRadius)
		{
			currentRadius = maxRadius;
			isGrowing = false;
			currentRadius = 0f;
			InteractComplete();
		}
	}

	private void ShrinkCircle()
	{
		float currentInteractionSpeed = player.CurrentInteractionSpeed;
		if (currentRadius > 0f)
		{
			currentRadius -= currentInteractionSpeed * Time.deltaTime;
			currentRadius = Mathf.Max(currentRadius, 0f);
		}
	}

	public virtual bool CheckConditions()
	{
		Item currentSelectedItem = Player.GetInstance().CurrentSelectedItem;
		if ((!IsTimelinePlaying() && string.IsNullOrEmpty(keyItemName)) || (currentSelectedItem != null && keyItemName == currentSelectedItem.GetComponentInChildren<Item>().ItemName) || this is NPCDialogueTrigger || this is GallerySceneViewer)
		{
			if (Player.GetInstance().CurrentState is PlayerHideState)
			{
				return this is HideThing;
			}
			return true;
		}
		return false;
	}

	protected abstract bool CheckInteractButton();

	public bool IsInteract()
	{
		return CheckInteractButton();
	}

	public void Interact(GameObject objectInDoor)
	{
		InteractionComplete = false;
		if (objectInDoor.GetComponent<Player>() != null && !CheckInteractButton())
		{
			StopInteract();
			return;
		}
		interactableObject = objectInDoor;
		if (objectInDoor.CompareTag("Player"))
		{
			if (!isGrowing)
			{
				isGrowing = true;
			}
		}
		else
		{
			InteractDelay();
		}
	}

	public void StopInteract()
	{
		isGrowing = false;
	}

	private void InteractDelay()
	{
		timer += Time.deltaTime;
		if (timer > 1f)
		{
			InteractComplete();
			timer = 0f;
		}
	}

	private void OnDisable()
	{
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().RemoveFromPlayerList(this);
		}
	}

	private void OnDestroy()
	{
		if (Player.GetInstance() != null)
		{
			Player.GetInstance().RemoveFromPlayerList(this);
		}
	}

	protected virtual void InteractComplete()
	{
		if (interactableObject != null && interactableObject.GetComponent<Player>() != null)
		{
			Player.GetInstance().IsActionButton = false;
			Player.GetInstance().SelectedObjectIndex = 0;
			Player.GetInstance().ManualTriggerCheck();
			Player.GetInstance().HandlePlayerInteractUI();
			Player.GetInstance().UpdateSelectedObjectUI();
		}
		if (interactableObject != null && this is DoorTransit doorTransit)
		{
			interactableObject.GetComponent<Entities>().CurrentRoom = HospitalManager.GetInstance().GetDoorTransit(doorTransit.GotoObject).DoorRoom.name;
		}
	}
}
