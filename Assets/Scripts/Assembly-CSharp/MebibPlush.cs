using System.Collections;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MebibPlush : InteractionObject
{
	private GameObject wantedItem;

	private GameObject spawnedWantedItem;

	[SerializeField]
	private GameObject[] itemPool;

	[SerializeField]
	private GameObject fourthNightItem;

	[SerializeField]
	private GameObject fifthNightItem;

	[SerializeField]
	private Sprite[] sprites;

	private SpriteRenderer spriteRenderer;

	private Item currentItem;

	private bool happyBear;

	public GameObject WantedItem
	{
		get
		{
			return wantedItem;
		}
		set
		{
			wantedItem = value;
			keyItemName = wantedItem.name;
			CreateWantedItemObject();
		}
	}

	public GameObject[] ItemPool
	{
		get
		{
			return itemPool;
		}
		set
		{
			itemPool = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			Debug.LogError("SpriteRenderer not found!");
			return;
		}
		Collider component = GetComponent<Collider>();
		spriteRenderer.sortingLayerName = "Entity";
		BoxCollider boxCollider;
		if (component == null)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		else
		{
			boxCollider = component as BoxCollider;
			if (boxCollider == null)
			{
				Object.Destroy(component);
				boxCollider = base.gameObject.AddComponent<BoxCollider>();
			}
		}
		if (boxCollider != null)
		{
			boxCollider.isTrigger = true;
		}
	}

	public GameObject[] GetRuntimeItemPool()
	{
		List<GameObject> list = new List<GameObject>(itemPool);
		string text = SceneManager.GetActiveScene().name;
		if (text == "FourthNight")
		{
			if (fourthNightItem != null)
			{
				list.Add(fourthNightItem);
			}
		}
		else if (text == "FifthNight")
		{
			if (fourthNightItem != null)
			{
				list.Add(fourthNightItem);
			}
			if (fifthNightItem != null)
			{
				list.Add(fifthNightItem);
			}
		}
		return list.ToArray();
	}

	private GameObject[] GetExtendedItemPool()
	{
		List<GameObject> list = new List<GameObject>(itemPool);
		string text = SceneManager.GetActiveScene().name;
		if (text == "FourthNight")
		{
			if (fourthNightItem != null)
			{
				list.Add(fourthNightItem);
			}
		}
		else if (text == "FifthNight")
		{
			if (fourthNightItem != null)
			{
				list.Add(fourthNightItem);
			}
			if (fifthNightItem != null)
			{
				list.Add(fifthNightItem);
			}
		}
		return list.ToArray();
	}

	protected override void Update()
	{
		base.Update();
		UpdateSpriteBasedOnPlayerPosition();
		base.transform.localPosition = new Vector3(0f, base.transform.localPosition.y, base.transform.localPosition.z);
	}

	private void UpdateSpriteBasedOnPlayerPosition()
	{
		if (sprites == null || sprites.Length < 4)
		{
			Debug.LogWarning("Sprites array is not properly set up.");
			return;
		}
		Vector3 position = Player.GetInstance().transform.position;
		Vector3 position2 = base.transform.position;
		if (happyBear)
		{
			spriteRenderer.sprite = ((position.x > position2.x) ? sprites[3] : sprites[2]);
		}
		else
		{
			spriteRenderer.sprite = ((position.x > position2.x) ? sprites[0] : sprites[1]);
		}
	}

	private void CreateWantedItemObject()
	{
		if (wantedItem == null)
		{
			Debug.LogWarning("WantedItem is not assigned.");
			return;
		}
		Vector3 position = base.transform.position + Vector3.up * 50f;
		spawnedWantedItem = Object.Instantiate(wantedItem, position, Quaternion.identity);
		spawnedWantedItem.transform.SetParent(base.transform);
		SpriteRenderer component = spawnedWantedItem.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.sortingLayerName = "Entity";
		}
		CopyAnimator(GetComponent<Animator>(), spawnedWantedItem);
		CreateShadow(spawnedWantedItem, component);
	}

	private void CopyAnimator(Animator source, GameObject target)
	{
		if (!(source == null) && !(source.runtimeAnimatorController == null))
		{
			Animator animator = target.GetComponent<Animator>();
			if (animator == null)
			{
				animator = target.AddComponent<Animator>();
			}
			animator.runtimeAnimatorController = source.runtimeAnimatorController;
		}
	}

	private void CreateShadow(GameObject parent, SpriteRenderer parentRenderer)
	{
		GameObject obj = new GameObject("Shadow");
		obj.transform.SetParent(parent.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/blackCircleForBear");
		spriteRenderer.sortingLayerName = "Entity";
		spriteRenderer.sortingOrder = ((parentRenderer != null) ? (parentRenderer.sortingOrder - 1) : 0);
		Color color = spriteRenderer.color;
		color.a = 0f;
		spriteRenderer.color = color;
		StartCoroutine(FadeShadow(spriteRenderer, 0.5f, 0.5f));
	}

	private IEnumerator FadeShadow(SpriteRenderer renderer, float targetAlpha, float duration)
	{
		float time = 0f;
		Color c = renderer.color;
		while (time < duration)
		{
			time += Time.deltaTime;
			c.a = Mathf.Lerp(0f, targetAlpha, time / duration);
			renderer.color = c;
			yield return null;
		}
		c.a = targetAlpha;
		renderer.color = c;
	}

	public override bool CheckConditions()
	{
		if (Player.GetInstance().CanInteract && base.CheckConditions())
		{
			return !happyBear;
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

	private void OnDestroy()
	{
		if (spawnedWantedItem != null)
		{
			Object.Destroy(spawnedWantedItem.GetComponent<Animator>());
			spawnedWantedItem.gameObject.AddComponent<FadeAway>().StartFading(1f);
			Transform child = spawnedWantedItem.transform.GetChild(0);
			if (child != null)
			{
				Object.Destroy(child.GetComponent<Animator>());
				child.gameObject.AddComponent<FadeAway>().StartFading(1f);
			}
		}
	}

	protected override void InteractComplete()
	{
		happyBear = true;
		Object.Destroy(Player.GetInstance().CurrentSelectedItem.GetComponentInChildren<Item>().gameObject);
		Player.GetInstance().UpdateInventoryCellSprites();
		base.InteractComplete();
		InteractionManager.Instance.StartInteractionCooldown(Player.GetInstance());
		if (!GlobalSaveSystem.GetProgress("HintNDoll"))
		{
			GlobalSaveSystem.SetProgress("HintNDoll", value: true);
			GlobalSaveSystem.SetProgress("HintNDollReady", value: false);
		}
		SoundManager.PlaySound(SoundType.BearHappy);
		MebeebEnemy component = base.transform.parent.gameObject.GetComponent<MebeebEnemy>();
		component.BearIsHappy = true;
		if (spawnedWantedItem != null)
		{
			Object.Destroy(spawnedWantedItem);
		}
		string text = base.transform.parent.gameObject.name;
		GameObject gameObject = GameObject.Find(text.EndsWith("1") ? (text.Remove(text.Length - 1) + "2") : (text.Remove(text.Length - 1) + "1"));
		if (gameObject != null)
		{
			MebeebEnemy component2 = gameObject.GetComponent<MebeebEnemy>();
			if (component2 != null && component2.Plush != null)
			{
				component2.BearIsHappy = true;
				component2.Plush.GetComponent<Animator>().Play("bearFade");
				if (component2.Plush.spawnedWantedItem != null)
				{
					Object.Destroy(component2.Plush.spawnedWantedItem);
				}
			}
		}
		component.Plush.GetComponent<Animator>().Play("bearFade");
		Player.GetInstance().UpdateSelectedObjectUI();
	}

	public IEnumerator FadeOutSprite()
	{
		float duration = 5f;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float a = Mathf.Lerp(1f, 0f, elapsedTime / duration);
			Color color = spriteRenderer.color;
			color.a = a;
			spriteRenderer.color = color;
			yield return null;
		}
		base.transform.parent.gameObject.GetComponent<MebeebEnemy>().MebeebGone();
	}
}
