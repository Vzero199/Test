using SmallHedge.SoundManager;
using UnityEngine;

public class SisEnemy : Enemy
{
	[SerializeField]
	private Vector3 colliderSize;

	private GameObject flashlight;

	public Vector3 ColliderSize
	{
		set
		{
			colliderSize = value;
		}
	}

	protected override string BEpath => "prefabs/BreatheEffects/BENurse1";

	protected override void Start()
	{
		base.Start();
		spriteRenderer.sortingOrder = 60;
		flashlight = GameObject.Find("sislight(Clone)");
	}

	public void Hloop1Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Hloop1n2, base.gameObject);
	}

	public void Hloop2Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Hloop1n2, base.gameObject);
	}

	public void Hloop3Sound()
	{
		SoundManager.PlaySoundAtObject(SoundType.Hloop3, base.gameObject);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.HClimax, base.gameObject);
	}

	protected override void SetSexMode()
	{
		base.SetSexMode();
		base.gameObject.transform.GetChild(0).gameObject.SetActive(value: false);
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
		CreateShadowObject();
		base.Initialize();
	}

	public void SetActiveFlashlight(bool active)
	{
		flashlight.SetActive(active);
	}

	private void CreateShadowObject()
	{
		GameObject obj = new GameObject("ShadowObject");
		obj.AddComponent<SpriteRenderer>().sortingOrder = 2;
		obj.transform.SetParent(base.transform);
		Animator component = GetComponent<Animator>();
		component.Rebind();
		component.Update(0f);
	}

	public override AudioClip GetMovementSound()
	{
		string[] array = new string[5] { "NurseSteps 1", "NurseSteps 2", "NurseSteps 3", "NurseSteps 4", "NurseSteps 5" };
		int num = Random.Range(0, array.Length);
		string text = array[num];
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/" + text);
		if (audioClip == null)
		{
			Debug.LogWarning("[Audio] Клип '" + text + "' не найден в Resources/Audio!");
			return null;
		}
		return audioClip;
	}

	public override void ReactOnLightTrap()
	{
		ChangeState(new StunState());
	}

	public override void CreateStunEffect()
	{
		CreateCommonEffect("StunEffect", new Vector3(0f, 110f, 0f));
	}
}
