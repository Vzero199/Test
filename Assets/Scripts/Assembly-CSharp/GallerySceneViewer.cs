using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;

public class GallerySceneViewer : InteractionObject
{
	private Animator animator;

	[SerializeField]
	private bool startFromLoop;

	protected int loopCount;

	public static bool isGalleryScene;

	public static GallerySceneViewer instance;

	private const string DEFAULT_ANIMATION = "SexMode";

	[Header("Gallery Offset")]
	[SerializeField]
	private Vector2 galleryOffset;

	[SerializeField]
	private Vector2 visualOffset;

	private Vector3 originalAnimatorPosition;

	private Vector3 originalVisualLocalPosition;

	private bool offsetApplied;

	protected string currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";

	protected string previousWWpath;

	protected int counterForBreathe;

	protected int counterForWierdWord;

	private Transform VisualRoot
	{
		get
		{
			if (!(animator != null) || animator.transform.childCount <= 0)
			{
				return null;
			}
			return animator.transform.GetChild(0);
		}
	}

	private BlinkManager blinkManager => BlinkManager.GetInstance();

	protected virtual string BEpath => "";

	protected virtual string WWpath => currentWWpath;

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInParent<Animator>();
		if (string.IsNullOrEmpty(keyItemName))
		{
			keyItemName = "SexMode";
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

	public void LetsShakeElevator()
	{
	}

	private void ApplyGalleryOffset()
	{
		if (!offsetApplied && !(animator == null))
		{
			originalAnimatorPosition = animator.transform.position;
			Vector3 vector = originalAnimatorPosition + new Vector3(galleryOffset.x, galleryOffset.y, 0f);
			animator.transform.position = vector;
			Debug.Log($"[GalleryOffset] Applied animator offset: original={originalAnimatorPosition}, offset={galleryOffset}, new={vector}");
			if (VisualRoot != null)
			{
				originalVisualLocalPosition = VisualRoot.localPosition;
				Vector3 vector2 = originalVisualLocalPosition + new Vector3(visualOffset.x, visualOffset.y, 0f);
				VisualRoot.localPosition = vector2;
				Debug.Log($"[GalleryOffset] Applied visual offset: original local={originalVisualLocalPosition}, offset={visualOffset}, new local={vector2}");
			}
			offsetApplied = true;
		}
	}

	private void ResetGalleryOffset()
	{
		if (offsetApplied && !(animator == null))
		{
			animator.transform.position = originalAnimatorPosition;
			Debug.Log($"[GalleryOffset] Reset animator position to original: {originalAnimatorPosition}");
			if (VisualRoot != null)
			{
				VisualRoot.localPosition = originalVisualLocalPosition;
				Debug.Log($"[GalleryOffset] Reset visual local position to original: {originalVisualLocalPosition}");
			}
			offsetApplied = false;
		}
	}

	protected override void InteractComplete()
	{
		base.InteractComplete();
		instance = this;
		ApplyGalleryOffset();
		string stateName = (string.IsNullOrEmpty(keyItemName) ? "SexMode" : keyItemName);
		animator.Play(stateName);
		isGalleryScene = true;
		Player.GetInstance().gameObject.SetActive(value: false);
		CameraFollow.GetInstance().SetFollowTarget(animator.transform.GetChild(0));
	}

	public void ExitSexMode()
	{
		if (GallerySceneToggle.LoopScene)
		{
			ResetGalleryOffset();
			ApplyGalleryOffset();
			loopCount = 0;
			animator.SetInteger("HloopCount", loopCount);
			if (startFromLoop)
			{
				animator.Play("Hloop1");
				return;
			}
			string stateName = (string.IsNullOrEmpty(keyItemName) ? "SexMode" : keyItemName);
			animator.Play(stateName);
		}
		else
		{
			ResetGalleryOffset();
			EnableSkipButton();
			loopCount = 0;
			animator.SetInteger("HloopCount", loopCount);
			instance = null;
			isGalleryScene = false;
			animator.Play("Idle");
			Player.GetInstance().gameObject.SetActive(value: true);
			CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
			StartCoroutine(CheckPlayerUIButtons());
		}
	}

	private IEnumerator CheckPlayerUIButtons()
	{
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(Player.GetInstance().CheckInNextUpdate());
	}

	public void DisableSkipButton()
	{
		EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: false);
	}

	public void EnableSkipButton()
	{
		EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: true);
	}

	public void CheckToExitState()
	{
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("ClimaxRepeat") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 6f)
		{
			ExitSexMode();
		}
	}

	public void AnimationLoopFinished()
	{
		loopCount++;
		animator.SetInteger("HloopCount", loopCount);
	}

	public void NunFuckSound()
	{
		SoundManager.PlaySoundAtObject(SoundType.NunFuck, base.gameObject);
	}

	public void NunSingSound()
	{
	}

	public void CatCapSound()
	{
	}

	public void CatCapGallerySound()
	{
		SoundManager.PlaySound(SoundType.HoodieCatCap);
	}

	public void CatFuckSound()
	{
		SoundManager.PlaySound(SoundType.HoodieCatFuck);
	}

	public void Nurse2BJSound()
	{
		SoundManager.PlaySound(SoundType.Nurse2BJ);
	}

	public void Nurse2DoggySound()
	{
		SoundManager.PlaySound(SoundType.Hloop1n2);
	}

	public void Nurse2MissionerSound()
	{
		SoundManager.PlaySound(SoundType.Hloop3);
	}

	public void Nurse2MasturbationSound()
	{
	}

	public void PortalSound()
	{
		SoundManager.PlaySound(SoundType.Portal);
	}

	public void TentacleCatchSound()
	{
		SoundManager.PlaySound(SoundType.TVCatch);
	}

	public void Hloop1Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop1n2);
	}

	public void Hloop2Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop1n2);
	}

	public void Hloop3Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop3);
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

	public void Nurse1StepSound()
	{
		SoundManager.PlaySound(SoundType.Nurse1Step);
	}

	public void Nurse1FuckSound()
	{
		SoundManager.PlaySound(SoundType.Nurse1Fuck);
	}

	public void Nurse2ClimaxSound()
	{
		SoundManager.PlaySound(SoundType.HClimax);
	}

	public void MattressPressSound()
	{
		SoundManager.PlaySound(SoundType.MattressPress);
	}

	public void HJobloop1Sound()
	{
		SoundManager.PlaySound(SoundType.HJLoop);
	}

	public void HLickloopSound()
	{
		SoundManager.PlaySound(SoundType.LickLoop);
	}

	public void DoppPuffSound()
	{
		SoundManager.PlaySound(SoundType.DoppPoof, null, 0.2f);
	}

	public void ForceBlink()
	{
		blinkManager.FadeToBlink(Player.GetInstance().gameObject);
	}

	public void EndSexMode()
	{
		blinkManager.FadeToBlink(base.gameObject, ExitSexMode, allowVerticalMovement: false, 0f, 0f, base.gameObject);
	}

	public void EnableFastWWEffect()
	{
		ChangeWWPath("prefabs/WierdWordEffects/WWFuckFast");
	}

	public void EnableSlurpWWEffect()
	{
		ChangeWWPath("prefabs/WierdWordEffects/WWFuckSlurp");
	}

	public void ResetWWEffect()
	{
		ChangeWWPath("prefabs/WierdWordEffects/WWFuckStandart");
	}

	private void ChangeWWPath(string newPath)
	{
		previousWWpath = currentWWpath;
		currentWWpath = newPath;
	}

	private void CreateEffect(string path, Vector3? position = null)
	{
		GameObject gameObject = Resources.Load<GameObject>(path);
		if (gameObject != null)
		{
			Object.Instantiate(gameObject, position ?? base.transform.position, Quaternion.identity, base.transform).name = gameObject.name;
		}
		else
		{
			Debug.LogError("Не удалось загрузить эффект по пути: " + path);
		}
	}

	public void CreateCommonEffect(string effectName)
	{
		string path = "prefabs/CommonEffects/" + effectName;
		switch (effectName)
		{
		case "LostBoySignEffect":
			CreateEffect(path, base.transform.position + new Vector3(15f, 80f, 0f));
			break;
		case "DetectionTriggerEffect":
			CreateEffect(path, base.transform.position + new Vector3(-20f, 150f, 0f));
			break;
		case "LustEffect":
			CreateEffect(path, base.transform.position + new Vector3(60f, 100f, 0f));
			break;
		default:
			CreateEffect(path);
			break;
		}
	}

	public void EnableRandomWWEffect()
	{
		previousWWpath = currentWWpath;
		if (currentWWpath == "prefabs/WierdWordEffects/WWFuckSlurp")
		{
			currentWWpath = previousWWpath;
		}
		else
		{
			currentWWpath = ((Random.value < 0.5f) ? "prefabs/WierdWordEffects/WWFuckSlurp" : ((currentWWpath == "prefabs/WierdWordEffects/WWFuckStandart") ? "prefabs/WierdWordEffects/WWFuckFast" : "prefabs/WierdWordEffects/WWFuckStandart"));
		}
	}

	public virtual void CreateBreatheEffect()
	{
		if (!string.IsNullOrEmpty(BEpath))
		{
			if (counterForBreathe <= 0)
			{
				CreateEffect(BEpath);
				counterForBreathe = Random.Range(1, 4);
			}
			else
			{
				counterForBreathe--;
			}
		}
	}

	public virtual void CreateWierdWordEffect()
	{
		if (!string.IsNullOrEmpty(WWpath))
		{
			if (counterForWierdWord <= 0)
			{
				Vector3 vector = Random.insideUnitSphere.normalized * 50f;
				CreateEffect(WWpath, base.transform.position + vector);
				counterForWierdWord = Random.Range(1, 4);
			}
			else
			{
				counterForWierdWord--;
			}
		}
	}

	private void OnDestroy()
	{
		ResetGalleryOffset();
		isGalleryScene = false;
		instance = null;
		if (EndEvent.GetInstance() != null)
		{
			EnableSkipButton();
		}
	}
}
