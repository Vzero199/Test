using UnityEngine;

public class Wall : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		Entities component = other.GetComponent<Entities>();
		if (component != null)
		{
			component.MoveDirection = Vector3.zero;
			component.IsMoving = false;
			Debug.Log("Вы не можете пройти через стену!");
		}
	}
}
