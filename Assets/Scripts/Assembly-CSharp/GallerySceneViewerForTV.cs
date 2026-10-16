using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class GallerySceneViewerForTV : InteractionObject
{
	public static bool isGalleryScene;

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
		base.InteractComplete();
		isGalleryScene = true;
		LoadAndPlayTimeline();
	}

	private void LoadAndPlayTimeline()
	{
		string text = "Timelines/TVSceneGallery";
		PlayableAsset playableAsset = Resources.Load<PlayableAsset>(text);
		TimelineManager instance = TimelineManager.GetInstance();
		if (playableAsset != null && instance != null && instance.Director != null)
		{
			instance.Director.playableAsset = playableAsset;
			instance.Director.Play();
		}
		else
		{
			Debug.LogError("Не удалось загрузить Timeline: " + text);
		}
	}

	public void OnDestroy()
	{
		isGalleryScene = false;
	}

	public void ExitSexMode()
	{
		StartCoroutine(ExitSexModeDelay());
	}

	private IEnumerator ExitSexModeDelay()
	{
		yield return null;
		EnableSkipButton();
		isGalleryScene = false;
		Player.GetInstance().ChangeState(new PlayerIdleState());
		StartCoroutine(CheckPlayerUIButtons());
	}

	private IEnumerator CheckPlayerUIButtons()
	{
		yield return null;
		Player.GetInstance().ChangeState(new PlayerIdleState());
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(Player.GetInstance().CheckInNextUpdate());
	}

	public void RewindLine()
	{
		if (GallerySceneToggle.LoopScene)
		{
			PlayableDirector playableDirector = TimelineManager.GetInstance()?.Director;
			if (playableDirector == null)
			{
				Debug.LogError("PlayableDirector не найден");
				return;
			}
			double time = 4.666666666666667;
			playableDirector.time = time;
			playableDirector.Evaluate();
		}
	}

	public void DisableSkipButton()
	{
		EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: false);
	}

	public void EnableSkipButton()
	{
		EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: true);
	}
}
