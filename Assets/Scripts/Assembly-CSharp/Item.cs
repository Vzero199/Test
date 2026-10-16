using UnityEngine;

public abstract class Item : MonoBehaviour
{
	[SerializeField]
	protected string itemName;

	public string ItemName
	{
		get
		{
			return itemName;
		}
		set
		{
			itemName = value;
		}
	}
}
