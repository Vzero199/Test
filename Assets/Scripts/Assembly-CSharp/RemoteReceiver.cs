using UnityEngine;

public class RemoteReceiver : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && base.gameObject.GetComponentInChildren<TVEnemy>() != null)
		{
			RemoteItem.CloseToTv = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player" && base.gameObject.GetComponentInChildren<TVEnemy>() != null)
		{
			RemoteItem.CloseToTv = false;
		}
	}
}
