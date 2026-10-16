using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;

public class HoodieCatEnemy : Enemy
{
	[SerializeField]
	private Vector3 colliderSize;

	[SerializeField]
	private GameObject stepPrefab;

	private float stepDistance = 64f;

	private float lastStepPosition;

	private int currentStepAnimation = 1;

	private bool reverseStepSequence;

	private GameObject soundObject;

	public Vector3 ColliderSize
	{
		set
		{
			colliderSize = value;
		}
	}

	public GameObject StepPrefab
	{
		get
		{
			return stepPrefab;
		}
		set
		{
			stepPrefab = value;
		}
	}

	protected override string BEpath => "prefabs/BreatheEffects/BECat";

	public override void ForceBlink(float speed)
	{
		BlinkManager.GetInstance().FadeToBlink(Player.GetInstance().gameObject);
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
		spriteRenderer.sortingOrder = 59;
		lastStepPosition = base.transform.position.x;
		CreateAudioSource();
	}

	public void CatCapSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.HoodieCatCap, base.gameObject);
	}

	public void FJHloop1Sound()
	{
		SoundManager.PlaySound(SoundType.FJHloop1);
	}

	public void FJHloop2Sound()
	{
		SoundManager.PlaySound(SoundType.FJHloop2n3);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySound(SoundType.HClimax);
	}

	protected override void Initialize()
	{
		base.Initialize();
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
		if (base.IsAngry)
		{
			ChangeState(new ChaseState());
			base.ItKnow = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (source != null)
		{
			PauseMenu.UnregisterPausableAudio(source);
		}
		if (soundObject != null)
		{
			Object.Destroy(soundObject);
		}
	}

	private void CreateAudioSource()
	{
		string text = "cat purring";
		source = base.gameObject.AddComponent<AudioSource>();
		source.gameObject.tag = "enemyNoise";
		AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
		if (sFXGroup != null)
		{
			source.outputAudioMixerGroup = sFXGroup;
		}
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/" + text);
		if (audioClip == null)
		{
			Debug.LogWarning("[Audio] Клип '" + text + "' не найден в Resources/Audio!");
			return;
		}
		source.clip = audioClip;
		source.volume = 0.1f;
		source.spatialBlend = 1f;
		source.loop = true;
		AudioMixer audioMixer = SoundManager.GetInstance().GetAudioMixer();
		if (audioMixer != null)
		{
			AudioMixerGroup[] array = audioMixer.FindMatchingGroups("SFX");
			if (array.Length != 0)
			{
				source.outputAudioMixerGroup = array[0];
			}
			else
			{
				Debug.LogWarning("[Audio] Группа 'SFX' не найдена в миксере!");
			}
		}
		else
		{
			Debug.LogWarning("[Audio] AudioMixer не найден!");
		}
		source.dopplerLevel = 0f;
		source.minDistance = 100f;
		source.maxDistance = 400f;
		source.rolloffMode = AudioRolloffMode.Linear;
		source.Play();
		PauseMenu.RegisterPausableAudio(source);
	}

	protected override void Update()
	{
		base.Update();
		if (Vector3.Distance(new Vector3(base.transform.position.x, 0f, 0f), new Vector3(lastStepPosition, 0f, 0f)) >= stepDistance)
		{
			CreateStep();
			lastStepPosition = base.transform.position.x;
		}
	}

	private void CreateStep()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		Vector3 position = base.transform.position;
		if (component != null)
		{
			position.y -= component.size.y * 0.5f;
			position.y += component.center.y;
		}
		StepAnimation component2 = Object.Instantiate(stepPrefab, position, Quaternion.identity, null).GetComponent<StepAnimation>();
		if (component2 != null)
		{
			component2.SetStepAnimation(currentStepAnimation);
			component2.SetDirection(moveDirection.x);
		}
		UpdateStepAnimation();
	}

	private void UpdateStepAnimation()
	{
		if (reverseStepSequence)
		{
			currentStepAnimation--;
			if (currentStepAnimation < 1)
			{
				currentStepAnimation = 2;
				reverseStepSequence = false;
			}
		}
		else
		{
			currentStepAnimation++;
			if (currentStepAnimation > 3)
			{
				currentStepAnimation = 2;
				reverseStepSequence = true;
			}
		}
	}

	public override AudioClip GetMovementSound()
	{
		string text = "cat purring";
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

	public override bool IsLightSwordWeakness()
	{
		return true;
	}

	public override void CreateStunEffect()
	{
		CreateCommonEffect("StunEffect", new Vector3(0f, 110f, 0f));
	}
}
