using System.Collections.Generic;
using UnityEngine;

public class RoomGraph
{
	public Dictionary<GameObject, List<GameObject>> Connections = new Dictionary<GameObject, List<GameObject>>();

	public void AddConnection(GameObject roomA, GameObject roomB)
	{
		if (!Connections.ContainsKey(roomA))
		{
			Connections[roomA] = new List<GameObject>();
		}
		if (!Connections.ContainsKey(roomB))
		{
			Connections[roomB] = new List<GameObject>();
		}
		Connections[roomA].Add(roomB);
		Connections[roomB].Add(roomA);
	}

	public List<GameObject> GetConnections(GameObject room)
	{
		if (!Connections.ContainsKey(room))
		{
			return new List<GameObject>();
		}
		return Connections[room];
	}
}
