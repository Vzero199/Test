using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSpeedCycler : MonoBehaviour
{
	private readonly float[] speeds = new float[3] { 1f, 1.5f, 2f };

	private int currentIndex;

	private Coroutine cycleCoroutine;

	private void OnEnable()
	{
		cycleCoroutine = StartCoroutine(CycleSpeed());
	}

	private void OnDisable()
	{
		if (cycleCoroutine != null)
		{
			StopCoroutine(cycleCoroutine);
			cycleCoroutine = null;
		}
		PlayableDirector playableDirector = TimelineManager.GetInstance()?.Director;
		if (playableDirector != null && playableDirector.playableGraph.IsValid())
		{
			playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1.0);
		}
	}

	private IEnumerator CycleSpeed()
	{
		PlayableDirector director = TimelineManager.GetInstance()?.Director;
		if (director == null || !director.playableGraph.IsValid())
		{
			yield break;
		}
		yield return new WaitForSeconds(5f);
		while (true)
		{
			float num = speeds[currentIndex];
			director.playableGraph.GetRootPlayable(0).SetSpeed(num);
			currentIndex = (currentIndex + 1) % speeds.Length;
			yield return new WaitForSeconds(5f);
		}
	}
}
