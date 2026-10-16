using System.Collections;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
	public static InteractionManager Instance;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void StartInteractionCooldown(Entities target, float cooldownTime = 1f)
	{
		StartCoroutine(SetInteractionCooldown(target, cooldownTime));
	}

	private IEnumerator SetInteractionCooldown(Entities target, float cooldownTime = 1f)
	{
		Debug.Log("Coroutine SetInteractionCooldown started");
		if (!(target != null))
		{
			yield break;
		}
		target.CanInteract = false;
		if (target is Player player)
		{
			player.HandlePlayerInteractUI();
		}
		Debug.Log("Cooldown started for " + target.name);
		yield return new WaitForSeconds(cooldownTime);
		if (!(target == null))
		{
			Debug.Log("Coroutine SetInteractionCooldown After wait");
			target.CanInteract = true;
			if (target is Player player2)
			{
				player2.HandlePlayerInteractUI();
				player2.ManualTriggerCheck();
			}
			Debug.Log("Cooldown ended for " + target.name);
		}
	}
}
