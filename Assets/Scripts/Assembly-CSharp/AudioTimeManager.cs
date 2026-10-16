using UnityEngine;

public class AudioTimeManager : MonoBehaviour
{
	public static float CicadasTime;

	public static void SaveCicadasTime(AudioSource cicadasSource)
	{
		if (cicadasSource != null && cicadasSource.isPlaying)
		{
			CicadasTime = cicadasSource.time;
		}
	}
}
