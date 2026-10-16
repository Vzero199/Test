using System.Collections;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.Audio;

public class GallerySceneViewerFoSuccGO : InteractionObject
{
	[SerializeField]
	private GameObject funScene;

	public static GallerySceneViewerFoSuccGO instance;

	public static bool isGalleryScene;

	private GameObject audioObject;

	private AudioSource hLoopSource;

	public void HloopSound()
	{
		SoundList soundList = SoundManager.GetSoundList(SoundType.SuccGallerSound);
		if (soundList.sounds != null && soundList.sounds.Length != 0)
		{
			AudioClip audioClip = soundList.sounds[Random.Range(0, soundList.sounds.Length)];
			if (!(audioClip == null))
			{
				GameObject obj = new GameObject("TempSound_SuccGallerSound");
				AudioSource audioSource = obj.AddComponent<AudioSource>();
				audioSource.clip = audioClip;
				audioSource.outputAudioMixerGroup = soundList.mixer;
				audioSource.volume = soundList.volume;
				audioSource.pitch = GallerySpeedController.CurrentSpeed;
				audioSource.Play();
				Object.Destroy(obj, audioClip.length / audioSource.pitch);
			}
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
		base.InteractComplete();
		instance = this;
		isGalleryScene = true;
		funScene.SetActive(value: true);
		AudioClip audioClip = Resources.Load<AudioClip>("Audio/GameOverBreath");
		if (audioClip != null)
		{
			audioObject = new GameObject("GameOverAudio");
			AudioSource audioSource = audioObject.AddComponent<AudioSource>();
			AudioMixerGroup sFXGroup = SoundManager.GetSFXGroup();
			if (sFXGroup != null)
			{
				audioSource.outputAudioMixerGroup = sFXGroup;
			}
			audioSource.clip = audioClip;
			audioSource.loop = true;
			audioSource.playOnAwake = false;
			audioSource.Play();
		}
	}

	public void ExitSexMode()
	{
		isGalleryScene = false;
		if (hLoopSource != null)
		{
			SoundManager.StopLoopingSound(hLoopSource);
			hLoopSource = null;
		}
		Object.Destroy(audioObject);
		EnableSkipButton();
		funScene.SetActive(value: false);
		instance = null;
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
		instance = null;
		isGalleryScene = false;
	}
}
