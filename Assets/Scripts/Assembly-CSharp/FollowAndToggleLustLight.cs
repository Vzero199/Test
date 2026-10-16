using UnityEngine;

public class FollowAndToggleLustLight : MonoBehaviour
{
	[SerializeField]
	private GameObject lustLight;

	[SerializeField]
	private Transform followTransform;

	private static FollowAndToggleLustLight instance;

	public GameObject LustLight => lustLight;

	public static FollowAndToggleLustLight GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<FollowAndToggleLustLight>();
			if (instance == null)
			{
				Debug.LogWarning("FollowAndToggleLustLight instance not found in the scene.");
			}
		}
		return instance;
	}

	private void Start()
	{
		if (followTransform == null)
		{
			followTransform = Player.GetInstance().transform;
		}
		if (lustLight == null)
		{
			lustLight = base.transform.Find("LustLight")?.gameObject;
		}
		UpdateLustLight();
	}

	private void Update()
	{
		FollowPlayer();
		UpdateLustLight();
	}

	private void FollowPlayer()
	{
		if (followTransform != null)
		{
			base.transform.position = new Vector3(followTransform.position.x, followTransform.position.y, base.transform.position.z);
		}
	}

	private void UpdateLustLight()
	{
		if (!(Player.GetInstance() == null))
		{
			bool flag = Player.GetInstance().gameObject.activeSelf && !(Player.GetInstance().CurrentState is PlayerSexModeState);
			if (lustLight != null)
			{
				lustLight.SetActive(!flag);
			}
		}
	}
}
