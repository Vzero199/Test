using UnityEngine;

public class FirstDayAudioManager : MonoBehaviour
{
	[SerializeField]
	private AudioSource cicadasAudioSource;

	private void Start()
	{
		if (cicadasAudioSource != null)
		{
			cicadasAudioSource.time = AudioTimeManager.CicadasTime;
			cicadasAudioSource.Play();
		}
	}
}
