using UnityEngine;

public class DestroyOnAnimationEnd : MonoBehaviour
{
	public void DestroyObject()
	{
		Object.Destroy(base.gameObject);
	}
}
