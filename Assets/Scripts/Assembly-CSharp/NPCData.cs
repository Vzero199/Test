using System;
using System.Collections.Generic;

[Serializable]
public class NPCData
{
	public string id;

	public SerializableVector3 position;

	public SerializableQuaternion rotation;

	public bool isActive;

	public string currentAnimation;

	public float normalizedTime;

	public List<AnimatorParameterData> animatorParameters = new List<AnimatorParameterData>();
}
