using UnityEngine;
using UnityEngine.InputSystem;

public class UIDragWithMouse : MonoBehaviour
{
	public RectTransform imageRectTransform;

	public Canvas canvas;

	private Vector2 mousePosition;

	private float minY;

	private float maxY;

	private void Start()
	{
		minY = 0f;
		maxY = canvas.GetComponent<RectTransform>().rect.height - imageRectTransform.rect.height;
	}

	private void Update()
	{
		mousePosition = Mouse.current.position.ReadValue();
		Vector2 vector = canvas.transform.InverseTransformPoint(mousePosition);
		vector.y = Mathf.Clamp(vector.y, minY, maxY);
		imageRectTransform.localPosition = new Vector3(imageRectTransform.localPosition.x, vector.y, imageRectTransform.localPosition.z);
	}
}
