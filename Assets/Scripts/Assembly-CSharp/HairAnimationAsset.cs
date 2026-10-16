using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HairAnimationAsset", menuName = "Hair/Hair Animation Asset")]
public class HairAnimationAsset : ScriptableObject
{
	public List<Texture2D> frames = new List<Texture2D>();

	public float frameRate = 30f;

	public Texture2D GetFrame(float time)
	{
		if (frames.Count == 0)
		{
			return null;
		}
		int index = Mathf.FloorToInt(time * frameRate) % frames.Count;
		return frames[index];
	}
}
