using SmallHedge.SoundManager;
using UnityEngine;

public class HideThing : InteractionObject
{
	[SerializeField]
	private GameObject hiddenObject;

	private SpriteRenderer objectRenderer;

	private Collider objectCollider;

	private static HideThing instanceWithPlayer;

	private Animator shakeAnimator;

	private BoxCollider boxCollider;

	public GameObject HiddenObject => hiddenObject;

	public override bool CheckConditions()
	{
		if (Player.GetInstance().CanInteract && base.CheckConditions())
		{
			return base.enabled;
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

	public static HideThing GetPlayerHideInstance()
	{
		return instanceWithPlayer;
	}

	protected override void Start()
	{
		base.Start();
		shakeAnimator = GetComponentInParent<Animator>();
		boxCollider = GetComponent<BoxCollider>();
	}

	public void ForceExit()
	{
		Debug.Log("Force Quit");
		if (hiddenObject == null)
		{
			return;
		}
		SpriteRenderer component = hiddenObject.GetComponent<SpriteRenderer>();
		hiddenObject = null;
		if (component != null)
		{
			SetSpriteAlpha(component, 1f);
		}
		if (hiddenObject != null)
		{
			if (hiddenObject.GetComponent<Entities>() is Player player)
			{
				player.HideCabin = null;
				player.ChangeState(new PlayerIdleState());
			}
			Transform transform = hiddenObject.transform.Find("ShadowObject");
			if (transform != null)
			{
				transform.gameObject.SetActive(value: true);
			}
		}
	}

	protected override void InteractComplete()
	{
		base.InteractComplete();
		Debug.Log(interactableObject.name + " interacted with " + base.gameObject.name);
		Entities component = interactableObject.GetComponent<Entities>();
		shakeAnimator.Play("Shake");
		if (hiddenObject != null && hiddenObject.GetComponent<Nintendo>() != null && component is Player)
		{
			hiddenObject.GetComponent<Nintendo>().WrongDoor();
			return;
		}
		if ((!(component is Player player) || !(player.CurrentState is PlayerHideState)) && (!(component is Nintendo nintendo) || !(nintendo.CurrentState is NintendoHideState)) && hiddenObject == null)
		{
			if (component is Player)
			{
				if (!GlobalSaveSystem.GetProgress("HintNHide"))
				{
					GlobalSaveSystem.SetProgress("HintNHide", value: true);
					GlobalSaveSystem.SetProgress("HintNHideReady", value: false);
				}
				instanceWithPlayer = this;
			}
			hiddenObject = interactableObject;
			objectRenderer = hiddenObject.GetComponent<SpriteRenderer>();
			objectCollider = hiddenObject.GetComponent<Collider>();
			_ = component.GetComponent<Nintendo>() != null;
			if (objectRenderer != null && objectCollider != null)
			{
				SetSpriteAlpha(objectRenderer, 0f);
				if (component is Player player2)
				{
					player2.HideCabin = base.gameObject;
				}
				if (base.transform.childCount > 0 || base.gameObject.name == "CurtainsHide")
				{
					SoundManager.PlaySoundAtPosition(SoundType.TechiesHide, base.transform.position);
				}
				else if (base.gameObject.name == "OpenBox")
				{
					SoundManager.PlaySoundAtPosition(SoundType.LockerHide, base.transform.position);
				}
				else
				{
					SoundManager.PlaySoundAtPosition(SoundType.CabinHide, base.transform.position);
				}
				component.EnterHideState();
				HideEntityShadow();
				component.transform.position = new Vector3(base.transform.parent.position.x, component.transform.position.y, component.transform.position.z);
			}
		}
		else if (((component is Player player3 && player3.CurrentState is PlayerHideState) || (component is Nintendo nintendo2 && nintendo2.CurrentState is NintendoHideState)) && hiddenObject != null)
		{
			_ = component is Nintendo;
			if (objectRenderer != null && objectCollider != null)
			{
				SetSpriteAlpha(objectRenderer, 1f);
				objectCollider.enabled = true;
				component.ExitHideState();
				ShowEntityShadow();
				if (component is Player player4)
				{
					player4.HideCabin = null;
				}
				if (base.transform.childCount > 0 || base.gameObject.name == "CurtainsHide")
				{
					SoundManager.PlaySoundAtPosition(SoundType.TechiesHide, base.transform.position);
				}
				else if (base.gameObject.name == "OpenBox")
				{
					SoundManager.PlaySoundAtPosition(SoundType.LockerHide, base.transform.position);
				}
				else
				{
					SoundManager.PlaySoundAtPosition(SoundType.CabinHide, base.transform.position);
				}
				objectRenderer = null;
				objectCollider = null;
				hiddenObject = null;
			}
		}
		InteractionManager.Instance.StartInteractionCooldown(component);
		currentRadius = 0f;
		isGrowing = false;
	}

	private void SetSpriteAlpha(SpriteRenderer spriteRenderer, float alpha)
	{
		if (!(spriteRenderer == null))
		{
			Color color = spriteRenderer.color;
			color.a = alpha;
			spriteRenderer.color = color;
		}
	}

	private void HideEntityShadow()
	{
		Transform transform = interactableObject.transform.Find("ShadowObject");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: false);
		}
	}

	private void ShowEntityShadow()
	{
		Transform transform = interactableObject.transform.Find("ShadowObject");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: true);
		}
	}
}
