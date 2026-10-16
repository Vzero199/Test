using UnityEngine;

public class TargetPoint : MonoBehaviour
{
	[SerializeField]
	private string requiredKey;

	public string RequiredKey => requiredKey;
}
