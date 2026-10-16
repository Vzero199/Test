using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
	public float rotationSpeed;

	private Vector2 direction;

	public float moveSpeed;

	private void Update()
	{
		direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - base.transform.position;
		Quaternion b = Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * 57.29578f, Vector3.forward);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, rotationSpeed * Time.deltaTime);
		Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		base.transform.position = Vector2.MoveTowards(base.transform.position, target, moveSpeed * Time.deltaTime);
	}
}
