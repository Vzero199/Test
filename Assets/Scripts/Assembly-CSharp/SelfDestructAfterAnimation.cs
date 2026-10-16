using UnityEngine;

public class SelfDestructAfterAnimation : MonoBehaviour
{
	public void DestroySelf()
	{
		Object.Destroy(base.gameObject);
	}
}
