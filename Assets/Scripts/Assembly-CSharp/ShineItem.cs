using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.UI;

public class ShineItem : InteractionObject
{
	[SerializeField]
	private GameObject itemInShine;

	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Animator not found on ShineItem!");
		}
	}

	public void StartShineEffect()
	{
		if (animator != null)
		{
			animator.Play("StartShineEffect");
		}
	}

	public GameObject GetStoredPrefab()
	{
		return itemInShine;
	}

	public void StartUnshineEffect()
	{
		if (animator != null)
		{
			animator.Play("StartUnshineEffect");
		}
	}

	public void SetShineItem(GameObject item)
	{
		itemInShine = new GameObject("item");
		Image image = itemInShine.AddComponent<Image>();
		Item component = item.GetComponent<Item>();
		QuestItem component2 = item.GetComponent<QuestItem>();
		if (component == null && component2 == null)
		{
			Debug.LogError("Item does not have a valid script!");
			Object.Destroy(itemInShine);
			return;
		}
		if (component != null)
		{
			(itemInShine.AddComponent(component.GetType()) as Item).ItemName = component.ItemName;
		}
		else if (component2 != null)
		{
			(itemInShine.AddComponent(component2.GetType()) as QuestItem).ItemName = component2.ItemName;
		}
		Image component3 = item.GetComponent<Image>();
		if (component3 != null)
		{
			image.sprite = component3.sprite;
			itemInShine.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
		}
		else
		{
			Debug.LogError("Item does not have an Image component!");
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
		Player instance = Player.GetInstance();
		InteractionManager.Instance.StartInteractionCooldown(instance);
		if (itemInShine == null)
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
					GameObject currentHighlightedButton = instance.GetCurrentHighlightedButton();
					if (currentHighlightedButton != null)
					{
						GameObject obj = Object.Instantiate(gameObject, parent);
						obj.transform.SetAsFirstSibling();
						obj.transform.position = currentHighlightedButton.transform.position + new Vector3(11f, -3.5f, 0f);
						obj.transform.rotation = currentHighlightedButton.transform.rotation;
						obj.transform.localScale = currentHighlightedButton.transform.lossyScale;
						instance.DestroyButton(currentHighlightedButton);
						Debug.Log("Spawned UI_Nothing under WorldSpaceCanvas at button position.");
					}
					else
					{
						Debug.LogWarning("No highlighted button found for fallback UI.");
					}
				}
			}
			instance.RemoveInteractionObject(this);
			Object.Destroy(base.gameObject);
			base.InteractComplete();
			return;
		}
		Item component = itemInShine.GetComponent<Item>();
		if (component is KeyItem && component.ItemName == "subSisKey")
		{
			SoundManager.PlaySound(SoundType.CoinFind);
		}
		if (component != null)
		{
			Debug.Log("Player Take " + component.ItemName + " From Shine");
			if (component.ItemName == "coin")
			{
				SoundManager.PlaySound(SoundType.CoinFind);
				int num = int.Parse(instance.wallet.text);
				instance.wallet.text = (num + 100).ToString();
				base.InteractComplete();
				instance.RemoveInteractionObject(this);
				Object.Destroy(base.gameObject);
				Object.Destroy(itemInShine);
				return;
			}
			if (component.ItemName == "cable" || component.ItemName == "Cable")
			{
				TryActivateCableHint();
			}
		}
		base.InteractComplete();
		if (itemInShine != null)
		{
			SoundManager.PlaySound(SoundType.TakeItem);
			instance.SetItemInSelectedCell(itemInShine);
		}
		instance.RemoveInteractionObject(this);
		Object.Destroy(base.gameObject);
		Object.Destroy(itemInShine);
	}

	private void TryActivateCableHint()
	{
		HintSystem hintSystem = Object.FindObjectOfType<HintSystem>();
		if (hintSystem != null)
		{
			hintSystem.ActivateHint("HintCable");
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(itemInShine);
	}
}
