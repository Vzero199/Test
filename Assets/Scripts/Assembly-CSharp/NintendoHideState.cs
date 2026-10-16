using UnityEngine;

public class NintendoHideState : INintendoState
{
	public void EnterState(Nintendo nintendo)
	{
		nintendo.GetAnimator().Play("Empty");
		nintendo.isReachedMainGoal = true;
		nintendo.CreateAudioSource();
		if (nintendo.HideCabin == null)
		{
			Debug.LogError("HideCabin Is Null");
		}
		if (nintendo.InteractObject.name == "OpenBox")
		{
			SpriteRenderer component = nintendo.InteractObject.GetComponent<SpriteRenderer>();
			Sprite sprite = Resources.Load<Sprite>("Sprites/CloseBox");
			component.sprite = sprite;
		}
		else if (nintendo.InteractObject.transform.childCount > 0)
		{
			SpriteRenderer component2 = nintendo.InteractObject.GetComponent<SpriteRenderer>();
			Sprite sprite2 = Resources.Load<Sprite>("Sprites/TechiesCart5Nin");
			component2.sprite = sprite2;
		}
		else if (!(nintendo.InteractObject.name == "CurtainsHide"))
		{
			SpriteRenderer component3 = nintendo.InteractObject.GetComponent<SpriteRenderer>();
			Sprite sprite3 = Resources.Load<Sprite>("Sprites/CabinClose");
			component3.sprite = sprite3;
		}
	}

	public void UpdateState(Nintendo nintendo)
	{
		nintendo.HandleTimers();
	}

	public void ExitState(Nintendo nintendo)
	{
		nintendo.DestroyAudioSource();
		if (nintendo.InteractObject.name == "OpenBox")
		{
			SpriteRenderer component = nintendo.InteractObject.GetComponent<SpriteRenderer>();
			Sprite sprite = Resources.Load<Sprite>("Sprites/OpenBox");
			component.sprite = sprite;
		}
		else if (nintendo.InteractObject.transform.childCount > 0)
		{
			SpriteRenderer component2 = nintendo.InteractObject.GetComponent<SpriteRenderer>();
			Sprite sprite2 = Resources.Load<Sprite>("Sprites/TechiesCart3");
			component2.sprite = sprite2;
		}
		else if (!(nintendo.InteractObject.name == "CurtainsHide"))
		{
			SpriteRenderer component3 = nintendo.InteractObject.GetComponent<SpriteRenderer>();
			Sprite sprite3 = Resources.Load<Sprite>("Sprites/CabinOpen");
			component3.sprite = sprite3;
		}
		Transform transform = nintendo.transform.Find("ShadowObject");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: true);
		}
		nintendo.isReachedMainGoal = false;
		nintendo.HideCabin.ForceExit();
		nintendo.HideCabin = null;
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
