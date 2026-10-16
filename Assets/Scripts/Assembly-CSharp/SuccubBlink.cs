using System.Collections;
using UnityEngine;

public class SuccubBlink : MonoBehaviour
{
	[SerializeField]
	private GameObject teleportObject;

	private Animator animator;

	private BlinkManager blinkManager => BlinkManager.GetInstance();

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	protected void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject == Player.GetInstance().gameObject)
		{
			animator.Play("FingerClick");
			StartCoroutine(TeleportWithDelay());
		}
	}

	private IEnumerator TeleportWithDelay()
	{
		yield return new WaitForSeconds(1.2f);
		blinkManager.FadeToBlink(Player.GetInstance().gameObject, PlayerHoldTheLine, allowVerticalMovement: true, 0f, 0f, teleportObject);
	}

	public void PlayerHoldTheLine()
	{
		Player.GetInstance().AlignToRoomLine();
	}
}
