using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
	public string sceneName;

	public SerializableVector3 playerPosition;

	public SerializableQuaternion playerRotation;

	public bool extraTVTriggered;

	public string version;

	public Dictionary<string, bool> timelineTriggered = new Dictionary<string, bool>();

	public Dictionary<string, string> doorKeys = new Dictionary<string, string>();

	public Dictionary<string, string> npcDialogueKeys = new Dictionary<string, string>();

	public List<QuestShineData> questShines = new List<QuestShineData>();

	public Dictionary<string, string> dialogueTriggers = new Dictionary<string, string>();

	public List<QuestTaskData> questTasks = new List<QuestTaskData>();

	public List<NPCData> npcs = new List<NPCData>();

	public Dictionary<string, bool> animatorTriggersState = new Dictionary<string, bool>();

	public Dictionary<string, bool> playedTimelines = new Dictionary<string, bool>();

	public Dictionary<string, string> assignedTimelines = new Dictionary<string, string>();

	public string buildType;
}
