using UnityEngine;

public class DemoBlocker : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.SetActive(value: true);
	}
}
