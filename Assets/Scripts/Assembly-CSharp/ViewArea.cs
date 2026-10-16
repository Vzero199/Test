using UnityEngine;

public class ViewArea : MonoBehaviour
{
	private Enemy roamer;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !roamer.IsChaseMode)
		{
			roamer.ChangeState(new ChaseState());
		}
	}

	public void SetCreatorObject(GameObject creator)
	{
		roamer = creator.GetComponent<Enemy>();
	}
}
