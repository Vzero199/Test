using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSaveManager : MonoBehaviour
{
	private const string SaveFileName = "savegame.json";

	public static bool IsLoading = false;

	private static readonly byte[] Key = Encoding.UTF8.GetBytes("ChlomakiBestWaifu".Substring(0, 16));

	private static readonly byte[] IV = Encoding.UTF8.GetBytes("ItsFuckinBadGame".Substring(0, 16));

	public static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

	public static string CurrentGameVersion => Application.version;

	public static bool IsNightSaveExists()
	{
		if (!File.Exists(SavePath))
		{
			return false;
		}
		GameData gameData = JsonConvert.DeserializeObject<GameData>(Decrypt(File.ReadAllText(SavePath)));
		if (!string.IsNullOrEmpty(gameData.sceneName))
		{
			return gameData.sceneName.Contains("Night");
		}
		return false;
	}

	public static void SaveGame(string sceneName, bool isNight)
	{
		GameData gameData = new GameData
		{
			sceneName = sceneName
		};
		gameData.version = Application.version;
		if (!isNight)
		{
			Player player = UnityEngine.Object.FindObjectOfType<Player>();
			if (player != null)
			{
				gameData.playerPosition = new SerializableVector3(player.transform.position);
				gameData.playerRotation = new SerializableQuaternion(player.transform.rotation);
			}
			ExtraTVSceneOnEnter extraTVSceneOnEnter = UnityEngine.Object.FindObjectOfType<ExtraTVSceneOnEnter>();
			if (extraTVSceneOnEnter != null)
			{
				gameData.extraTVTriggered = extraTVSceneOnEnter.isTriggered;
			}
			AnimatorTriggerOnStart[] array = UnityEngine.Object.FindObjectsOfType<AnimatorTriggerOnStart>();
			foreach (AnimatorTriggerOnStart animatorTriggerOnStart in array)
			{
				gameData.animatorTriggersState[animatorTriggerOnStart.gameObject.name] = animatorTriggerOnStart.hasTriggered;
			}
			TimelineSceneOnEnter[] array2 = UnityEngine.Object.FindObjectsOfType<TimelineSceneOnEnter>();
			foreach (TimelineSceneOnEnter timelineSceneOnEnter in array2)
			{
				gameData.timelineTriggered[timelineSceneOnEnter.name] = timelineSceneOnEnter.isTriggered;
			}
			DoorTransit[] array3 = UnityEngine.Object.FindObjectsOfType<DoorTransit>();
			foreach (DoorTransit doorTransit in array3)
			{
				gameData.doorKeys[doorTransit.name] = doorTransit.KeyItemName;
			}
			NPCDialogueTrigger[] array4 = UnityEngine.Object.FindObjectsOfType<NPCDialogueTrigger>(includeInactive: true);
			foreach (NPCDialogueTrigger nPCDialogueTrigger in array4)
			{
				gameData.npcDialogueKeys[nPCDialogueTrigger.name] = nPCDialogueTrigger.KeyItemName;
			}
			QuestShine[] array5 = UnityEngine.Object.FindObjectsOfType<QuestShine>(includeInactive: true);
			foreach (QuestShine questShine in array5)
			{
				gameData.questShines.Add(new QuestShineData
				{
					id = questShine.name,
					position = new SerializableVector3(questShine.transform.position),
					isActive = questShine.gameObject.activeSelf,
					keyItemName = questShine.KeyItemName
				});
			}
			DialogueTrigger[] array6 = UnityEngine.Object.FindObjectsOfType<DialogueTrigger>();
			foreach (DialogueTrigger dialogueTrigger in array6)
			{
				gameData.dialogueTriggers[dialogueTrigger.name] = dialogueTrigger.DialogueName;
			}
			Transform transform = GameObject.Find("Canvas/TaskUI/TaskPanel/TaskGroup")?.transform;
			if (transform != null)
			{
				Transform[] componentsInChildren = transform.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform2 in componentsInChildren)
				{
					if (transform2.name.Contains("questTask"))
					{
						GameObject gameObject = transform2.gameObject;
						(bool, float)? questTargetState = QuestManager.GetInstance().GetQuestTargetState(gameObject.name);
						float num = 1f;
						float num2 = 1f;
						if (questTargetState.HasValue)
						{
							num = questTargetState.Value.Item2;
							num2 = questTargetState.Value.Item2;
						}
						else
						{
							TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
							Image componentInChildren = gameObject.GetComponentInChildren<Image>(includeInactive: true);
							num = ((component != null) ? component.color.a : 1f);
							num2 = ((componentInChildren != null) ? componentInChildren.color.a : 1f);
						}
						int siblingIndex = gameObject.transform.GetSiblingIndex();
						bool activeSelf = gameObject.activeSelf;
						gameData.questTasks.Add(new QuestTaskData
						{
							id = gameObject.name,
							textAlpha = num,
							imageAlpha = num2,
							siblingIndex = siblingIndex,
							isActive = activeSelf
						});
					}
				}
			}
			else
			{
				Debug.LogWarning("Не найден Canvas/TaskUI/TaskPanel/TaskGroup при сохранении questTask");
			}
			GameObject[] array7 = FindGameObjectsWithTagIncludingInactive("NPC");
			foreach (GameObject gameObject2 in array7)
			{
				Animator component2 = gameObject2.GetComponent<Animator>();
				string currentAnimation = "";
				float normalizedTime = 0f;
				if (component2 != null)
				{
					component2.Update(0f);
					if (component2.IsInTransition(0))
					{
						AnimatorStateInfo nextAnimatorStateInfo = component2.GetNextAnimatorStateInfo(0);
						currentAnimation = nextAnimatorStateInfo.fullPathHash.ToString();
						normalizedTime = nextAnimatorStateInfo.normalizedTime;
					}
					else
					{
						AnimatorStateInfo currentAnimatorStateInfo = component2.GetCurrentAnimatorStateInfo(0);
						currentAnimation = currentAnimatorStateInfo.fullPathHash.ToString();
						normalizedTime = currentAnimatorStateInfo.normalizedTime;
					}
				}
				NPCData nPCData = new NPCData
				{
					id = gameObject2.name,
					position = new SerializableVector3(gameObject2.transform.position),
					rotation = new SerializableQuaternion(gameObject2.transform.rotation),
					isActive = gameObject2.activeSelf,
					currentAnimation = currentAnimation,
					normalizedTime = normalizedTime
				};
				if (component2 != null)
				{
					AnimatorControllerParameter[] parameters = component2.parameters;
					foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
					{
						AnimatorParameterData animatorParameterData = new AnimatorParameterData
						{
							name = animatorControllerParameter.name,
							type = animatorControllerParameter.type
						};
						switch (animatorControllerParameter.type)
						{
						case AnimatorControllerParameterType.Float:
							animatorParameterData.value = component2.GetFloat(animatorControllerParameter.name).ToString();
							break;
						case AnimatorControllerParameterType.Int:
							animatorParameterData.value = component2.GetInteger(animatorControllerParameter.name).ToString();
							break;
						case AnimatorControllerParameterType.Bool:
							animatorParameterData.value = component2.GetBool(animatorControllerParameter.name).ToString();
							break;
						case AnimatorControllerParameterType.Trigger:
							animatorParameterData.value = "false";
							break;
						}
						nPCData.animatorParameters.Add(animatorParameterData);
					}
				}
				gameData.npcs.Add(nPCData);
			}
			PlayableDirector[] array8 = UnityEngine.Object.FindObjectsOfType<PlayableDirector>();
			foreach (PlayableDirector playableDirector in array8)
			{
				gameData.playedTimelines[playableDirector.name] = !playableDirector.playOnAwake;
				gameData.assignedTimelines[playableDirector.name] = ((playableDirector.playableAsset != null) ? playableDirector.playableAsset.name : "");
			}
		}
		gameData.buildType = "full";
		string contents = Encrypt(JsonConvert.SerializeObject(gameData, Formatting.Indented));
		File.WriteAllText(SavePath, contents);
	}

	public static GameObject[] FindGameObjectsWithTagIncludingInactive(string tag)
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		List<GameObject> list = new List<GameObject>();
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.scene.IsValid() && gameObject.CompareTag(tag))
			{
				list.Add(gameObject);
			}
		}
		return list.ToArray();
	}

	public static bool LoadGame()
	{
		if (!File.Exists(SavePath))
		{
			return false;
		}
		IsLoading = true;
		string value = Decrypt(File.ReadAllText(SavePath));
		GameData data = JsonConvert.DeserializeObject<GameData>(value);
		string text = data.version ?? "1.0.0";
		if (text != Application.version)
		{
			Debug.LogWarning("Несовместимая версия сохранения: " + text + " (текущая " + Application.version + ")");
			return false;
		}
		if (data.buildType != "full")
		{
			Debug.LogWarning("Несовместимое сохранение: другой тип билда.");
			return false;
		}
		SceneManager.LoadSceneAsync(data.sceneName, LoadSceneMode.Single).completed += (AsyncOperation operation) =>
		{
			RestoreGameState(data);
		};
		return true;
	}

	private static void RestoreGameState(GameData data)
	{
		Player player = UnityEngine.Object.FindObjectOfType<Player>();
		if (player != null && data.playerPosition != null && data.playerRotation != null)
		{
			player.transform.position = data.playerPosition.ToVector3();
			player.transform.rotation = data.playerRotation.ToQuaternion();
		}
		ExtraTVSceneOnEnter extraTVSceneOnEnter = UnityEngine.Object.FindObjectOfType<ExtraTVSceneOnEnter>();
		if (extraTVSceneOnEnter != null)
		{
			extraTVSceneOnEnter.isTriggered = data.extraTVTriggered;
		}
		AnimatorTriggerOnStart[] array = UnityEngine.Object.FindObjectsOfType<AnimatorTriggerOnStart>();
		foreach (AnimatorTriggerOnStart animatorTriggerOnStart in array)
		{
			if (data.animatorTriggersState.TryGetValue(animatorTriggerOnStart.name, out var value))
			{
				animatorTriggerOnStart.hasTriggered = value;
			}
		}
		TimelineSceneOnEnter[] array2 = UnityEngine.Object.FindObjectsOfType<TimelineSceneOnEnter>();
		foreach (TimelineSceneOnEnter timelineSceneOnEnter in array2)
		{
			if (data.timelineTriggered.TryGetValue(timelineSceneOnEnter.name, out var value2))
			{
				timelineSceneOnEnter.isTriggered = value2;
			}
		}
		DoorTransit[] array3 = UnityEngine.Object.FindObjectsOfType<DoorTransit>();
		foreach (DoorTransit doorTransit in array3)
		{
			if (data.doorKeys.TryGetValue(doorTransit.name, out var value3))
			{
				doorTransit.KeyItemName = value3;
			}
		}
		NPCDialogueTrigger[] array4 = UnityEngine.Object.FindObjectsOfType<NPCDialogueTrigger>(includeInactive: true);
		foreach (NPCDialogueTrigger nPCDialogueTrigger in array4)
		{
			if (data.npcDialogueKeys.TryGetValue(nPCDialogueTrigger.name, out var value4))
			{
				nPCDialogueTrigger.KeyItemName = value4;
			}
		}
		DialogueTrigger[] array5 = UnityEngine.Object.FindObjectsOfType<DialogueTrigger>();
		foreach (DialogueTrigger dialogueTrigger in array5)
		{
			if (data.dialogueTriggers.TryGetValue(dialogueTrigger.name, out var value5))
			{
				dialogueTrigger.DialogueName = value5;
			}
		}
		foreach (QuestShineData shineData in data.questShines)
		{
			QuestShine questShine = UnityEngine.Object.FindObjectsOfType<QuestShine>(includeInactive: true).FirstOrDefault((QuestShine s) => s.gameObject.name == shineData.id || s.name == shineData.id);
			if (questShine == null)
			{
				questShine = Resources.FindObjectsOfTypeAll<QuestShine>().FirstOrDefault((QuestShine s) => s.gameObject.name == shineData.id || s.name == shineData.id);
			}
			if (questShine != null)
			{
				questShine.transform.position = shineData.position.ToVector3();
				questShine.gameObject.SetActive(shineData.isActive);
				questShine.KeyItemName = shineData.keyItemName;
			}
			else
			{
				Debug.LogWarning("QuestShine с именем " + shineData.id + " не найден при загрузке");
			}
		}
		foreach (NPCData npcData in data.npcs)
		{
			GameObject gameObject = UnityEngine.Object.FindObjectsOfType<GameObject>(includeInactive: true).FirstOrDefault((GameObject obj) => obj.name == npcData.id);
			if (gameObject == null)
			{
				gameObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault((GameObject obj) => obj.name == npcData.id);
			}
			if (gameObject != null)
			{
				gameObject.transform.position = npcData.position.ToVector3();
				gameObject.transform.rotation = npcData.rotation.ToQuaternion();
				gameObject.SetActive(npcData.isActive);
				Animator component = gameObject.GetComponent<Animator>();
				if (!(component != null))
				{
					continue;
				}
				foreach (AnimatorParameterData animatorParameter in npcData.animatorParameters)
				{
					try
					{
						switch (animatorParameter.type)
						{
						case AnimatorControllerParameterType.Float:
						{
							if (float.TryParse(animatorParameter.value, out var result4))
							{
								component.SetFloat(animatorParameter.name, result4);
							}
							break;
						}
						case AnimatorControllerParameterType.Int:
						{
							if (int.TryParse(animatorParameter.value, out var result2))
							{
								component.SetInteger(animatorParameter.name, result2);
							}
							break;
						}
						case AnimatorControllerParameterType.Bool:
						{
							if (bool.TryParse(animatorParameter.value, out var result3))
							{
								component.SetBool(animatorParameter.name, result3);
							}
							break;
						}
						case AnimatorControllerParameterType.Trigger:
						{
							if (bool.TryParse(animatorParameter.value, out var result) & result)
							{
								component.SetTrigger(animatorParameter.name);
							}
							break;
						}
						}
					}
					catch (Exception ex)
					{
						Debug.LogWarning("Ошибка восстановления параметра " + animatorParameter.name + " для " + npcData.id + ": " + ex.Message);
					}
				}
				if (!string.IsNullOrEmpty(npcData.currentAnimation) && int.TryParse(npcData.currentAnimation, out var result5))
				{
					component.Play(result5, 0, npcData.normalizedTime);
					component.Update(0f);
				}
			}
			else
			{
				Debug.LogWarning("NPC с именем " + npcData.id + " не найден при загрузке");
			}
		}
		PlayableDirector[] array6 = UnityEngine.Object.FindObjectsOfType<PlayableDirector>();
		foreach (PlayableDirector playableDirector in array6)
		{
			if (data.playedTimelines.TryGetValue(playableDirector.name, out var value6))
			{
				playableDirector.playOnAwake = !value6;
			}
			if (!data.assignedTimelines.TryGetValue(playableDirector.name, out var value7))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(value7))
			{
				PlayableAsset[] array7 = Resources.FindObjectsOfTypeAll<PlayableAsset>();
				foreach (PlayableAsset playableAsset in array7)
				{
					if (playableAsset.name == value7)
					{
						playableDirector.playableAsset = playableAsset;
						break;
					}
				}
			}
			else
			{
				playableDirector.playableAsset = null;
			}
		}
		Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
		if (canvas == null)
		{
			Debug.LogWarning("Canvas не найден при восстановлении questTask");
		}
		foreach (QuestTaskData taskData in data.questTasks)
		{
			GameObject gameObject2 = null;
			if (canvas != null)
			{
				Transform transform = canvas.transform.Find("TaskUI/TaskPanel/TaskGroup/" + taskData.id);
				if (transform != null)
				{
					gameObject2 = transform.gameObject;
				}
			}
			if (gameObject2 == null)
			{
				gameObject2 = UnityEngine.Object.FindObjectsOfType<GameObject>(includeInactive: true).FirstOrDefault((GameObject obj) => obj.name == taskData.id);
			}
			if (gameObject2 == null)
			{
				Debug.LogWarning("Task " + taskData.id + " не найден при загрузке");
				continue;
			}
			gameObject2.SetActive(taskData.isActive);
			TextMeshProUGUI component2 = gameObject2.GetComponent<TextMeshProUGUI>();
			if (component2 != null)
			{
				Color color = component2.color;
				color.a = taskData.textAlpha;
				component2.color = color;
			}
			Image componentInChildren = gameObject2.GetComponentInChildren<Image>(includeInactive: true);
			if (componentInChildren != null)
			{
				Color color2 = componentInChildren.color;
				color2.a = taskData.imageAlpha;
				componentInChildren.color = color2;
			}
			if (gameObject2.transform.parent != null)
			{
				int siblingIndex = Mathf.Clamp(taskData.siblingIndex, 0, gameObject2.transform.parent.childCount - 1);
				gameObject2.transform.SetSiblingIndex(siblingIndex);
			}
		}
		IsLoading = false;
	}

	public static void DeleteSave()
	{
		if (File.Exists(SavePath))
		{
			File.Delete(SavePath);
			Debug.Log("Сохранение удалено.");
		}
		else
		{
			Debug.LogWarning("Файл сохранения не найден.");
		}
	}

	private static string Encrypt(string plainText)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.IV = IV;
		ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream stream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(plainText);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	private static string Decrypt(string cipherText)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.IV = IV;
		ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(cipherText));
		using CryptoStream stream2 = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}
}
