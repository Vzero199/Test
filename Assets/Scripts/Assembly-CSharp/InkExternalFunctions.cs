using Ink.Runtime;
using SmallHedge.SoundManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InkExternalFunctions
{
	public void Bind(Story story, Animator emoteAnimator)
	{
		story.BindExternalFunction("playEmote", (string emoteName) =>
		{
			PlayEmote(emoteName, emoteAnimator);
		});
		story.BindExternalFunction("playPlayerAnim", (string animName) =>
		{
			playPlayerAnim(animName);
		});
		story.BindExternalFunction("elevatorUse", (string elevatorName) =>
		{
			ElevatorUse(elevatorName);
		});
		story.BindExternalFunction("getElevatorName", () => GetElevatorName());
		story.BindExternalFunction("setTypingSpeed", (float value) =>
		{
			SetTypeSpeed(value);
		});
		story.BindExternalFunction("setKeyItemName", (string objectName, string newKeyItemName) =>
		{
			SetKeyItemName(objectName, newKeyItemName);
		});
		story.BindExternalFunction("runQuestMethod", (string methodName) =>
		{
			RunQuestMethod(methodName);
		});
		story.BindExternalFunction("SetKarmaProgress", (string key, bool value) =>
		{
			SetKarmaProgress(key, value);
		});
		story.BindExternalFunction("PauseGame", () =>
		{
			PauseGame();
		});
		story.BindExternalFunction("UnPauseGame", () =>
		{
			UnPauseGame();
		});
		story.BindExternalFunction("SceneIsFirstNight", () => SceneIsFirstNight());
		story.BindExternalFunction("SoftMute", (float time) =>
		{
			SoftMute(time);
		});
	}

	public void Unbind(Story story)
	{
	}

	public bool SceneIsFirstNight()
	{
		string name = SceneManager.GetActiveScene().name;
		if (!name.Contains("FirstNight"))
		{
			return name.Contains("FirstNIght");
		}
		return true;
	}

	public void SoftMute(float time)
	{
		SoundManager.FadeOutAndStopBGM(time);
	}

	public void PlayEmote(string emoteName, Animator emoteAnimator)
	{
		if (emoteAnimator != null)
		{
			emoteAnimator.Play(emoteName);
		}
		else
		{
			Debug.LogWarning("Tried to play emote, but emote animator was not initialized when entering dialogue mode.");
		}
	}

	public void SetKarmaProgress(string key, bool value)
	{
		if (value)
		{
			SoundManager.PlaySound(SoundType.questComplete);
		}
		GlobalSaveSystem.SetProgress(key, value);
	}

	public void playPlayerAnim(string animName)
	{
		Player.GetInstance().PlayAnimation(animName);
	}

	public void PauseGame()
	{
	}

	public void UnPauseGame()
	{
	}

	public void ElevatorUse(string elevatorName)
	{
		if (Player.GetInstance().ElevatorInstance.GetComponentInParent<WhiteWomanEnemy>() != null)
		{
			Player.GetInstance().ElevatorInstance.transform.Find("WW").GetComponent<WhiteWomanEnemy>().ElevatorName = elevatorName;
		}
		Player.GetInstance().ElevatorInstance.IsInElevatorChoice = true;
		if (Player.GetInstance().ElevatorInstance.nintendoInElevator)
		{
			Nintendo.GetInstance().StartLiftAnim();
			Nintendo.elevatorName = elevatorName;
		}
		else
		{
			Player.GetInstance().ElevatorInstance.GoToFloor(elevatorName, CheckForNintendoChase);
			Nintendo.elevatorName = elevatorName;
		}
	}

	private void CheckForNintendoChase()
	{
		Nintendo instance = Nintendo.GetInstance();
		if (instance != null && Player.GetInstance() != null && instance.targetChase != null && instance.MainGoal == Player.GetInstance().gameObject)
		{
			instance.transform.position = Player.GetInstance().transform.position + new Vector3(-50f, 0f, 0f);
			instance.AlignToRoomLine();
		}
	}

	public void SetKeyItemName(string objectName, string newKeyItemName)
	{
		InteractionObject interactionObject = GameObject.Find(objectName)?.GetComponentInChildren<InteractionObject>();
		if (interactionObject != null)
		{
			interactionObject.KeyItemName = newKeyItemName;
		}
		else
		{
			Debug.LogWarning("InteractionObject с именем " + objectName + " не найден.");
		}
	}

	public string GetElevatorName()
	{
		return Player.GetInstance().ElevatorInstance.name;
	}

	public void SetTypeSpeed(float value)
	{
		DialogueManager.GetInstance().SetTypingSpeed(value);
	}

	public void RunQuestMethod(string methodName)
	{
		QuestManager.GetInstance()?.RunMethod(methodName);
	}
}
