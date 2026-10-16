using UnityEngine;

namespace SmallHedge.SoundManager
{
	[CreateAssetMenu(menuName = "Small Hedge/Sounds SO", fileName = "Sounds SO")]
	public class SoundsSO : ScriptableObject
	{
		public SoundList[] sounds;
	}
}
