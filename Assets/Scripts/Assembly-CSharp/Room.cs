using UnityEngine;

public class Room : MonoBehaviour
{
	public float wallWidth = 1f;

	private void Start()
	{
		CreateInvisibleWall();
	}

	private void CreateInvisibleWall()
	{
		float y = GetComponent<BoxCollider>().bounds.size.y;
		float x = GetComponent<BoxCollider>().bounds.size.x;
		CreateWall(new Vector3(base.transform.position.x - x / 2f, base.transform.position.y, base.transform.position.z), new Vector3(wallWidth, y, 1f));
		CreateWall(new Vector3(base.transform.position.x + x / 2f, base.transform.position.y, base.transform.position.z), new Vector3(wallWidth, y, 1f));
	}

	private void CreateWall(Vector3 position, Vector3 size)
	{
		GameObject obj = new GameObject("InvisibleWall");
		obj.tag = "Wall";
		obj.transform.parent = base.transform;
		BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
		boxCollider.center = position;
		boxCollider.size = size;
	}
}
