using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	private void Update()
	{
		if (Player.GetInstance() != null)
		{
			base.transform.position = Player.GetInstance().transform.position;
		}
	}
}
