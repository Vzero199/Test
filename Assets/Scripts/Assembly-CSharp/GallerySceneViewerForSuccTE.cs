using System.Collections;
using UnityEngine;

public class GallerySceneViewerForSuccTE : InteractionObject
{
	[SerializeField]
	private GameObject funScene;

	[SerializeField]
	private CanvasGroup altGroup;

	public static bool isGalleryScene;

	public static GallerySceneViewerForSuccTE instance;

	private GameObject loopToggleObj;

	private Coroutine fadeCoroutine;

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

	protected override void Start()
	{
		base.Start();
		loopToggleObj = GameObject.Find("LoopToggle");
	}

	public void FadeIn(float duration)
	{
		StartFade(1f, duration, enableInteraction: true);
	}

	public void FadeOut(float duration)
	{
		StartFade(0f, duration, enableInteraction: false);
	}

	private void StartFade(float targetAlpha, float duration, bool enableInteraction)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, duration, enableInteraction));
	}

	private IEnumerator FadeCoroutine(float targetAlpha, float duration, bool enableInteraction)
	{
		float startAlpha = altGroup.alpha;
		float time = 0f;
		altGroup.blocksRaycasts = enableInteraction;
		altGroup.interactable = enableInteraction;
		while (time < duration)
		{
			time += Time.deltaTime;
			altGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
			yield return null;
		}
		altGroup.alpha = targetAlpha;
		if (!enableInteraction)
		{
			altGroup.blocksRaycasts = false;
			altGroup.interactable = false;
		}
		fadeCoroutine = null;
	}

	protected override void InteractComplete()
	{
		base.InteractComplete();
		instance = this;
		isGalleryScene = true;
		funScene.SetActive(value: true);
		Player.GetInstance().SetPause(isPaused: true);
		if (loopToggleObj != null)
		{
			loopToggleObj.SetActive(value: false);
		}
		else
		{
			Debug.LogWarning("LoopToggle объект не найден в сцене");
		}
		FadeOut(1f);
	}

	public void ExitSexMode()
	{
		FadeIn(1f);
		EnableSkipButton();
		funScene.SetActive(value: false);
		Player.GetInstance().SetPause(isPaused: false);
		instance = null;
		isGalleryScene = false;
		if (loopToggleObj != null)
		{
			loopToggleObj.SetActive(value: true);
		}
		else
		{
			Debug.LogWarning("LoopToggle объект не найден в сцене");
		}
		StartCoroutine(CheckPlayerUIButtons());
	}

	private IEnumerator CheckPlayerUIButtons()
	{
		yield return null;
		Player.GetInstance().ChangeState(new PlayerIdleState());
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

	private void OnDestroy()
	{
		isGalleryScene = false;
		instance = null;
		if (EndEvent.GetInstance() != null)
		{
			EnableSkipButton();
		}
	}
}
