using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;

public class WhisperEnemy : Enemy
{
	[SerializeField]
	private float spookyHideTime = 0.25f;

	[SerializeField]
	private float spookyHideTimer;

	[SerializeField]
	private Vector3 colliderSize;

	[SerializeField]
	private float proximityXThreshold = 350f;

	[SerializeField]
	private float proximitySoundCooldown = 15f;

	private float lastProximitySoundTime = float.NegativeInfinity;

	private bool hasPlayedProximitySound;

	public float SpookyHideTime => spookyHideTime;

	public float SpookyHideTimer
	{
		get
		{
			return spookyHideTimer;
		}
		set
		{
			spookyHideTimer = value;
		}
	}

	public Vector3 ColliderSize
	{
		set
		{
			colliderSize = value;
		}
	}

	protected override string WWpath => "prefabs/WierdWordEffects/WWFuckSlurp";

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public void CheckProximitySound()
	{
		Player instance = Player.GetInstance();
		if (instance == null || !PlayerInCurrentRoom() || SpookyHideTimer > 0f)
		{
			return;
		}
		if (Mathf.Abs(base.transform.position.x - instance.transform.position.x) <= proximityXThreshold)
		{
			if (!hasPlayedProximitySound || Time.time - lastProximitySoundTime >= proximitySoundCooldown)
			{
				SoundManager.PlaySoundAtObject(SoundType.WhisperWoo, base.gameObject);
				lastProximitySoundTime = Time.time;
				hasPlayedProximitySound = true;
			}
		}
		else
		{
			hasPlayedProximitySound = false;
		}
	}

	public void HJobloop1Sound()
	{
		SoundManager.PlaySound(SoundType.HJLoop);
	}

	public void HLickloopSound()
	{
		SoundManager.PlaySound(SoundType.LickLoop);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySound(SoundType.HClimax);
	}

	public void UpdateSpookyHideTimer()
	{
		if (spookyHideTimer > 0f)
		{
			spookyHideTimer -= Time.deltaTime;
		}
	}

	protected override void SetSexMode()
	{
		if (!GlobalSaveSystem.GetProgress("HintNWhisper") && !GlobalSaveSystem.GetProgress("HintNWhisperReady"))
		{
			GlobalSaveSystem.SetProgress("HintNWhisperReady", value: true);
		}
		base.SetSexMode();
	}

	protected override void Initialize()
	{
		Collider component = GetComponent<Collider>();
		GetComponent<SpriteRenderer>().sortingLayerName = "Entity";
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
			boxCollider.size = colliderSize;
		}
		base.Initialize();
	}

	public override void LightReact()
	{
		if (!(base.CurrentState is SexModeState))
		{
			SoundManager.PlaySoundAtObject(SoundType.WhisperWoo, base.gameObject);
			if (!GlobalSaveSystem.GetProgress("HintNWhisper"))
			{
				GlobalSaveSystem.SetProgress("HintNWhisper", value: true);
				GlobalSaveSystem.SetProgress("HintNWhisperReady", value: false);
			}
			StartCoroutine(Dissapearing());
		}
	}

	private IEnumerator Dissapearing()
	{
		Debug.Log("Whisper Dissapear");
		SpookyHideTimer = 1f;
		BoxCollider boxCollider = base.gameObject.GetComponent<BoxCollider>();
		boxCollider.enabled = false;
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			Debug.LogWarning("Dissapearing: У объекта нет SpriteRenderer!");
			yield break;
		}
		float elapsedTime = 0f;
		Color originalColor = spriteRenderer.color;
		while (elapsedTime < SpookyHideTimer)
		{
			float a = Mathf.Lerp(1f, 0f, elapsedTime / SpookyHideTimer);
			spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
			elapsedTime += Time.deltaTime;
			yield return null;
			if (base.CurrentState is SexModeState)
			{
				boxCollider.enabled = true;
				spriteRenderer.color = originalColor;
				yield break;
			}
		}
		Object.Destroy(base.gameObject);
	}

	public override bool CanBeCleared()
	{
		return false;
	}

	public override void ReactOnLightTrap()
	{
		LightReact();
	}

	public override bool IsLightSwordWeakness()
	{
		return true;
	}
}
