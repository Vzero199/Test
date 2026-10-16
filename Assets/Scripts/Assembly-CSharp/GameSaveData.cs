using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
	public float karma;

	public string lastCheckpoint;

	public List<ObjectState> sceneObjects = new List<ObjectState>();
}
