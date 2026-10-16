using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.SoundManager
{
	[Serializable]
	public struct SoundList
	{
		[HideInInspector]
		public string name;

		[Range(0f, 1f)]
		public float volume;

		public AudioMixerGroup mixer;

		public AudioClip[] sounds;
	}
}
