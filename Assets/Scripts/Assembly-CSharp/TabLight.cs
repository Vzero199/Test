using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TabLight : InteractionObject
{
	private GameObject guardLight;

	private GameObject lampObject;

	private string lampType;

	private Light2D guardScript;

	private Color baseColor;

	private bool trapIsSet;

	private bool lightEffect;

	private bool lampIsBroken;

	private BoxCollider ultraLightCollider;

	private Animator lampAnimator;

	private AudioSource guardLightAudioSource;

	private Coroutine currentSoundCoroutine;

	private HorrorLightFlicker flickerScript;

	private Coroutine volumeFadeCoroutine;

	private float currentBaseVolume = 0.6f;

	private bool wasSceneBlocked;

	private BoxCollider additionalColl;

	protected override void Start()
	{
		base.Start();
		guardLight = base.transform.GetChild(0).gameObject;
		if (guardLight == null)
		{
			Debug.LogError("guardLight not Found");
			return;
		}
		flickerScript = guardLight.GetComponent<HorrorLightFlicker>();
		guardScript = guardLight.GetComponent<Light2D>();
		if (guardScript == null)
		{
			Debug.LogError("component Light2D not found");
			return;
		}
		baseColor = guardScript.color;
		GameObject gameObject = guardLight.transform.GetChild(0)?.gameObject;
		if (gameObject == null)
		{
			Debug.LogError("ultraLight object not found");
			return;
		}
		GameObject gameObject2 = gameObject.transform.GetChild(0)?.gameObject;
		if (gameObject2 == null)
		{
			Debug.LogError("additionalCollObject object not found");
			return;
		}
		additionalColl = gameObject2.GetComponent<BoxCollider>();
		ultraLightCollider = gameObject.GetComponent<BoxCollider>();
		lampObject = base.transform.GetChild(1).gameObject;
		if (lampObject == null)
		{
			Debug.LogError("lampObject not Found");
			return;
		}
		lampType = lampObject.GetComponent<SpriteRenderer>().sprite.name;
		Debug.Log("lampType is " + lampType);
		lampAnimator = lampObject.GetComponent<Animator>();
		if (lampAnimator == null)
		{
			Debug.LogError("lampAnimator not Found");
			return;
		}
		guardLightAudioSource = guardLight.GetComponent<AudioSource>();
		if (guardLightAudioSource == null)
		{
			guardLightAudioSource = guardLight.AddComponent<AudioSource>();
			guardLightAudioSource.spatialBlend = 1f;
			guardLightAudioSource.minDistance = 50f;
			guardLightAudioSource.maxDistance = 500f;
			guardLightAudioSource.spread = 180f;
		}
		if (lampType == "upLamp2")
		{
			lampAnimator.Play("lamp2Inactive");
		}
		else
		{
			lampAnimator.Play("lamp1Inactive");
		}
		PlayLampSound(SoundType.LampRepeat, 0f);
	}

	protected override void Update()
	{
		base.Update();
		if (guardLightAudioSource == null)
		{
			return;
		}
		bool flag = IsSceneBlocked();
		if (flag != wasSceneBlocked)
		{
			wasSceneBlocked = flag;
			if (flag)
			{
				FadeTo(0f);
			}
			else
			{
				FadeTo(currentBaseVolume);
			}
		}
	}

	private void FadeTo(float volume, float duration = 0.5f)
	{
		if (!(Mathf.Abs(guardLightAudioSource.volume - volume) < 0.01f))
		{
			if (volumeFadeCoroutine != null)
			{
				StopCoroutine(volumeFadeCoroutine);
			}
			volumeFadeCoroutine = StartCoroutine(FadeVolumeRoutine(volume, duration));
		}
	}

	private IEnumerator FadeVolumeRoutine(float target, float duration)
	{
		float start = guardLightAudioSource.volume;
		float time = 0f;
		while (time < duration)
		{
			time += Time.unscaledDeltaTime;
			guardLightAudioSource.volume = Mathf.Lerp(start, target, time / duration);
			yield return null;
		}
		guardLightAudioSource.volume = target;
	}

	private bool IsSceneBlocked()
	{
		if (EndEvent.GetInstance() != null)
		{
			return EndEvent.GetInstance().IsAnyScene();
		}
		return false;
	}

	public override bool CheckConditions()
	{
		if (Player.GetInstance().CanInteract && base.CheckConditions() && !trapIsSet)
		{
			return !lampIsBroken;
		}
		return false;
	}

	protected override bool CheckInteractButton()
	{
		if (player.IsActionButton)
		{
			if (!CheckConditions())
			{
				if (interactableObject != null)
				{
					return interactableObject.GetComponent<Nintendo>() != null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void InteractComplete()
	{
		if (!guardLight.activeSelf)
		{
			guardLight.SetActive(value: true);
		}
		if (flickerScript != null)
		{
			flickerScript.enabled = false;
			if (guardScript != null)
			{
				guardScript.enabled = true;
			}
		}
		additionalColl.enabled = true;
		guardScript.color = new Color32(107, 196, 234, byte.MaxValue);
		trapIsSet = true;
		Object.Destroy(Player.GetInstance().CurrentSelectedItem.gameObject);
		Player.GetInstance().InteractObject = null;
		ManualTriggerCheck();
		if (lampType == "upLamp2")
		{
			lampAnimator.Play("lamp2SetTrap");
		}
		else
		{
			lampAnimator.Play("lamp1SetTrap");
		}
		StartLampRepeatSound();
		CheckForExistingEnemiesInCollider();
		InteractionManager.Instance.StartInteractionCooldown(interactableObject.GetComponent<Entities>());
	}

	private void CheckForExistingEnemiesInCollider()
	{
		if (!trapIsSet || ultraLightCollider == null || !ultraLightCollider.enabled)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(ultraLightCollider.bounds.center, ultraLightCollider.bounds.extents, ultraLightCollider.transform.rotation);
		int num = 0;
		if (num < array.Length)
		{
			Enemy component = array[num].GetComponent<Enemy>();
			if (!(component == null) && !(component.CurrentState is SexModeState) && !component.IsCatch)
			{
				StartCoroutine(TriggerLightTrap(component));
			}
		}
	}

	private void PlayLampSound(SoundType soundType, float volume = 0.3f, bool loop = true)
	{
		if (!(guardLightAudioSource == null))
		{
			AudioClip clip = SoundManager.GetClip(soundType);
			if (!(clip == null))
			{
				SoundList soundList = SoundManager.GetSoundList(soundType);
				guardLightAudioSource.clip = clip;
				guardLightAudioSource.rolloffMode = AudioRolloffMode.Linear;
				guardLightAudioSource.loop = loop;
				guardLightAudioSource.outputAudioMixerGroup = soundList.mixer;
				currentBaseVolume = volume;
				guardLightAudioSource.volume = volume;
				guardLightAudioSource.Play();
			}
		}
	}

	private void StartLampRepeatSound()
	{
		if (!(guardLightAudioSource == null))
		{
			if (currentSoundCoroutine != null)
			{
				StopCoroutine(currentSoundCoroutine);
			}
			currentSoundCoroutine = StartCoroutine(PlayLampOnThenRepeat());
		}
	}

	private IEnumerator PlayLampOnThenRepeat()
	{
		PlayLampSound(SoundType.LampOn, 0.6f);
		if (guardLightAudioSource.clip != null)
		{
			yield return new WaitForSecondsRealtime(guardLightAudioSource.clip.length - 1f);
		}
		else
		{
			yield return new WaitForSecondsRealtime(1f);
		}
		if (!lampIsBroken && guardLightAudioSource != null)
		{
			guardLightAudioSource.loop = true;
			PlayLampSound(SoundType.LampRepeat, 0.6f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!trapIsSet && !lightEffect)
		{
			return;
		}
		Enemy component = other.GetComponent<Enemy>();
		if (!(component == null) && !(component.CurrentState is SexModeState) && !component.IsCatch && !(component.tag == "Poster") && !(component is PosterEnemy { name: "SpriteParent", IsChaseMode: false }) && !(component is MebeebEnemy))
		{
			component.ReactOnLightTrap();
			if (trapIsSet)
			{
				StartCoroutine(TriggerLightTrap(component));
			}
		}
	}

	public void OnTriggerTrap(Collider other)
	{
		OnTriggerEnter(other);
	}

	public void ManualTriggerCheck()
	{
		if (!(ultraLightCollider == null) && base.gameObject.activeInHierarchy)
		{
			Collider[] array = Physics.OverlapBox(base.transform.position, ultraLightCollider.size / 2f, base.transform.rotation);
			Debug.Log($"Collider Count: {array.Length}");
			Collider[] array2 = array;
			foreach (Collider other in array2)
			{
				OnTriggerEnter(other);
			}
		}
	}

	private IEnumerator TriggerLightTrap(Enemy enemy)
	{
		lightEffect = true;
		ultraLightCollider.enabled = true;
		float originalIntensity = guardScript.intensity;
		float targetIntensity = 2f;
		float duration = 0.25f;
		trapIsSet = false;
		lampIsBroken = true;
		if (currentSoundCoroutine != null)
		{
			StopCoroutine(currentSoundCoroutine);
			currentSoundCoroutine = null;
		}
		if (guardLightAudioSource != null && guardLightAudioSource.isPlaying)
		{
			guardLightAudioSource.Stop();
		}
		try
		{
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				guardScript.intensity = Mathf.Lerp(originalIntensity, targetIntensity, elapsed / duration);
				yield return null;
			}
			guardScript.intensity = targetIntensity;
			yield return new WaitForSeconds(0.25f);
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				guardScript.intensity = Mathf.Lerp(targetIntensity, originalIntensity, elapsed / duration);
				yield return null;
			}
		}
		finally
		{
			TabLight tabLight = this;
			tabLight.ultraLightCollider.enabled = false;
			tabLight.additionalColl.enabled = false;
			tabLight.lightEffect = false;
			tabLight.PlayLampSound(SoundType.LampBroken, 0.6f, loop: false);
			if (tabLight.lampType == "upLamp2")
			{
				tabLight.lampAnimator.Play("lamp2TrapOffBroken");
			}
			else
			{
				tabLight.lampAnimator.Play("lamp1TrapOffBroken");
			}
			if (tabLight.guardLight.activeSelf)
			{
				tabLight.StartCoroutine(tabLight.BrokeLightDelay());
			}
		}
	}

	private IEnumerator BrokeLightDelay()
	{
		yield return new WaitForSeconds(1f);
		if (guardLightAudioSource != null && guardLightAudioSource.isPlaying)
		{
			guardLightAudioSource.Stop();
		}
		guardLight.SetActive(value: false);
	}

	private Transform FindGuardLightByNamePrefix(string prefix)
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name.StartsWith(prefix))
			{
				return transform;
			}
		}
		return null;
	}
}
