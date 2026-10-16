using System;
using UnityEngine;

[Serializable]
public class ObjectState
{
	public string objectName;

	public Vector3 position;

	public Quaternion rotation;

	public ObjectState(GameObject obj)
	{
		objectName = obj.name;
		position = obj.transform.position;
		rotation = obj.transform.rotation;
	}
}
