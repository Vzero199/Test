using System;
using System.Collections.Generic;
using SmallHedge.SoundManager;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class AnimationSoundSwitcher : MonoBehaviour
{
	[Serializable]
	public struct AnimationSoundPair
	{
		public string animationName;

		public AudioClip audioClip;
	}

	[SerializeField]
	private List<AnimationSoundPair> animationSounds = new List<AnimationSoundPair>();

	private Animator animator;

	private AudioSource audioSource;

	private string currentAnimation = "";

	private Dictionary<string, AudioClip> animationSoundDict = new Dictionary<string, AudioClip>();

	public void LickLoopSound()
	{
		SoundManager.PlaySound(SoundType.LickLoop, null, 0.8f);
	}

	public void Hloop2Sound()
	{
		SoundManager.PlaySound(SoundType.HSlowLoop);
	}

	public void Hloop3Sound()
	{
		SoundManager.PlaySound(SoundType.Hloop3);
	}

	public void ClimaxSound()
	{
		SoundManager.PlaySound(SoundType.HClimax);
	}

	private void Awake()
	{
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		foreach (AnimationSoundPair animationSound in animationSounds)
		{
			if (!animationSoundDict.ContainsKey(animationSound.animationName))
			{
				animationSoundDict.Add(animationSound.animationName, animationSound.audioClip);
			}
		}
		audioSource.loop = true;
	}

	private void Update()
	{
		if (!(animator == null) && !animator.GetCurrentAnimatorStateInfo(0).IsName(currentAnimation))
		{
			string currentAnimationName = GetCurrentAnimationName();
			if (currentAnimationName != currentAnimation)
			{
				currentAnimation = currentAnimationName;
				SwitchSound(currentAnimationName);
			}
		}
	}

	private string GetCurrentAnimationName()
	{
		AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.length > 0f)
		{
			foreach (KeyValuePair<string, AudioClip> item in animationSoundDict)
			{
				if (currentAnimatorStateInfo.IsName(item.Key))
				{
					return item.Key;
				}
			}
		}
		return "";
	}

	private void SwitchSound(string animationName)
	{
		if (animationSoundDict.TryGetValue(animationName, out var value))
		{
			if (value != null)
			{
				audioSource.clip = value;
				audioSource.Play();
			}
			else
			{
				audioSource.Stop();
			}
		}
		else
		{
			audioSource.Stop();
		}
	}
}
