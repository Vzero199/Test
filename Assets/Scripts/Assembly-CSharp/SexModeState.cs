using UnityEngine;

public class SexModeState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
		HandlePlayerHiding();
		enemy.DestroyAudio();
		Nintendo.HideNintendo();
		DialogueManager.GetInstance().StopDialogueMode();
		if (!(enemy is SisSupEnemy))
		{
			CameraFollow.GetInstance().SetFollowTarget(enemy.transform);
		}
		Player.GetInstance().gameObject.SetActive(value: false);
		if (enemy is WhiteWomanEnemy)
		{
			enemy.ToggleRenderers(state: false);
		}
		if (enemy.SexStateLightPrefab != null)
		{
			enemy.LightPrefabInstance = Object.Instantiate(enemy.SexStateLightPrefab);
			enemy.LightPrefabInstance.transform.parent = enemy.transform;
			enemy.LightPrefabInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
			if (enemy is SisSupEnemy)
			{
				enemy.LightPrefabInstance.transform.localPosition = new Vector3(50f, -50f, 0f);
				enemy.FixAnimator();
			}
			else if (enemy is NunEnemy)
			{
				enemy.LightPrefabInstance.transform.localPosition = new Vector3(0f, 150f, 0f);
			}
		}
		if (enemy.SexStateMaskPrefab != null && (enemy is SisEnemy || enemy is HoodieCatEnemy || enemy is PosterEnemy || enemy is VampireEnemy || enemy is WhisperEnemy || enemy is NunEnemy))
		{
			enemy.MaskPrefabInstance = Object.Instantiate(enemy.SexStateMaskPrefab);
			enemy.MaskPrefabInstance.transform.parent = enemy.transform;
			enemy.MaskPrefabInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		if (!(enemy is WhisperEnemy))
		{
			enemy.PlayAnimation("SexMode");
		}
		else if (Random.value <= 0.5f)
		{
			enemy.PlayAnimation("SecondSexMode");
			Vector3 position = enemy.transform.position;
			enemy.transform.position = new Vector3(position.x, position.y - 50f, position.z);
		}
		else
		{
			enemy.PlayAnimation("SexMode");
		}
		if (enemy is SisEnemy)
		{
			Vector3 position2 = enemy.transform.position;
			enemy.transform.position = new Vector3(position2.x, position2.y - 20f, position2.z);
		}
		if (enemy is PosterEnemy || enemy is VampireEnemy || enemy is SisEnemy || enemy is HoodieCatEnemy)
		{
			NightManager.GetInstance().HandleCatchMode(enemy);
		}
	}

	protected void HandlePlayerHiding()
	{
		if (Player.GetInstance() != null && Player.GetInstance().CurrentState is PlayerHideState)
		{
			HideThing[] array = Object.FindObjectsOfType<HideThing>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ForceExit();
			}
		}
	}

	public void UpdateState(Enemy enemy)
	{
		if (enemy.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName("ClimaxRepeat") && enemy.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime >= 6f)
		{
			enemy.EndSexMode();
		}
	}

	public void ExitState(Enemy enemy)
	{
		HandlePlayerHiding();
		if (!(enemy is PosterEnemy))
		{
			Debug.LogError($"SexMode Has Been Changed In ExtraState, Enemy Error Name: {enemy}");
		}
	}
}
