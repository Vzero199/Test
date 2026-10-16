using UnityEngine;
using UnityEngine.Playables;

public class TimelineSceneOnEnter : MonoBehaviour
{
	public bool isTriggered;

	[SerializeField]
	private string timelineName;

	private TimelineManager timelineManager => TimelineManager.GetInstance();

	private BlinkManager blinkManager => BlinkManager.GetInstance();

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && !isTriggered)
		{
			isTriggered = true;
			Player.GetInstance().ChangeState(new PlayerIdleState());
			blinkManager.FadeToBlink(base.gameObject, () =>
			{
				LoadAndPlayTimeline(timelineName);
			}, allowVerticalMovement: true, 0f, 0f, base.gameObject);
		}
	}

	private void LoadAndPlayTimeline(string timelineName)
	{
		if (timelineName == "SecondDayMainTimeline")
		{
			QuestManager.SetDialogueForObject("SignalReceiver", "2Day/startMainQuest");
		}
		string text = "Timelines/" + timelineName;
		PlayableAsset playableAsset = Resources.Load<PlayableAsset>(text);
		if (playableAsset != null && timelineManager != null && timelineManager.Director != null)
		{
			timelineManager.Director.playableAsset = playableAsset;
			timelineManager.Director.Play();
		}
		else
		{
			Debug.LogError("Не удалось загрузить Timeline: " + text);
		}
	}
}
