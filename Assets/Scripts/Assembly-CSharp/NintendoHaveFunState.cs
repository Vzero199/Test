using UnityEngine;

public class NintendoHaveFunState : INintendoState
{
	public void EnterState(Nintendo nintendo)
	{
		nintendo.GetAnimator().Play("FunMode");
		if (nintendo.InteractObject.name == "OpenBox")
		{
			BoxCollider component = nintendo.InteractObject.GetComponent<BoxCollider>();
			if (component != null)
			{
				Vector3 center = component.bounds.center;
				nintendo.transform.position = new Vector3(center.x + 585f, center.y - 7f, center.z);
			}
			else
			{
				Debug.LogError("BoxCollider у дочернего объекта отсутствует!");
			}
		}
		else if (nintendo.InteractObject.transform.childCount > 0)
		{
			BoxCollider component2 = nintendo.InteractObject.transform.GetChild(0).GetComponent<BoxCollider>();
			if (component2 != null)
			{
				Vector3 center2 = component2.bounds.center;
				float y = component2.bounds.max.y;
				nintendo.transform.position = new Vector3(center2.x, y + 25f, center2.z);
			}
			else
			{
				Debug.LogError("BoxCollider у дочернего объекта отсутствует!");
			}
		}
		else if (nintendo.InteractObject.name == "CurtainsHide")
		{
			Vector3 position = nintendo.InteractObject.transform.position;
			nintendo.transform.position = new Vector3(position.x - 60f, position.y - 165f, nintendo.transform.position.z);
		}
		else
		{
			BoxCollider component3 = nintendo.InteractObject.transform.parent.GetComponent<BoxCollider>();
			Vector3 vector = nintendo.InteractObject.transform.parent.InverseTransformPoint(component3.bounds.center);
			Transform parent = nintendo.InteractObject.transform.parent;
			nintendo.transform.position = new Vector3(parent.position.x - vector.x, nintendo.transform.position.y + 10f, nintendo.transform.position.z);
			nintendo.InteractObject.gameObject.SetActive(value: false);
		}
		if (nintendo.SexStateLightPrefab != null)
		{
			nintendo.LightPrefabInstance = Object.Instantiate(nintendo.SexStateLightPrefab);
			nintendo.LightPrefabInstance.transform.parent = nintendo.transform;
			nintendo.LightPrefabInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		if (nintendo.SexStateMaskPrefab != null)
		{
			nintendo.MaskPrefabInstance = Object.Instantiate(nintendo.SexStateMaskPrefab);
			nintendo.MaskPrefabInstance.transform.parent = nintendo.transform;
			nintendo.MaskPrefabInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		nintendo.isReachedMainGoal = false;
		Player.GetInstance().gameObject.SetActive(value: false);
		CameraFollow instance = CameraFollow.GetInstance();
		instance.SetFollowTarget(nintendo.transform);
		instance.enabled = false;
		nintendo.GetAnimator().enabled = true;
		nintendo.GetComponent<SpriteRenderer>().enabled = true;
	}

	public void UpdateState(Nintendo nintendo)
	{
		nintendo.HandleTimers();
	}

	public void ExitState(Nintendo nintendo)
	{
		nintendo.DestroyInstances();
		nintendo.EnableSkipButton();
		Player instance = Player.GetInstance();
		if (instance.CurrentHeartNumber > 0)
		{
			if ((nintendo.HideCabin != null && Player.GetInstance().CurrentState is PlayerHideState) || Player.GetInstance().CurrentState is PlayerIdleState)
			{
				nintendo.HideCabin.ForceExit();
			}
			else if ((instance.HideCabin != null && Player.GetInstance().CurrentState is PlayerHideState) || Player.GetInstance().CurrentState is PlayerIdleState)
			{
				instance.HideCabin.GetComponent<HideThing>().ForceExit();
			}
			Transform transform = nintendo.transform.Find("ShadowObject");
			if (transform != null)
			{
				transform.gameObject.SetActive(value: true);
			}
			nintendo.targetChase.SetActive(value: true);
			nintendo.GuardCoroutine = null;
			nintendo.ShowRoflTimer = nintendo.ShowRoflCooldown;
			instance.gameObject.SetActive(value: true);
			instance.IsPaused = false;
			CameraFollow.GetInstance().enabled = true;
			if (nintendo.InteractObject != null)
			{
				nintendo.InteractObject.gameObject.SetActive(value: true);
			}
			nintendo.InteractObject = null;
			CameraFollow.GetInstance().SetFollowTarget(Player.GetInstance().transform);
			instance.AlignToRoomLine();
		}
	}

	public void EnterTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void StayTriggerState(Nintendo nintendo, Collider collider)
	{
	}

	public void ExitTriggerState(Nintendo nintendo, Collider collider)
	{
	}
}
