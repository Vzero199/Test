public abstract class ItemToUse : Item
{
	public virtual bool CanBeUsed()
	{
		return true;
	}

	public virtual void UseItem()
	{
		Player.GetInstance().ManualTriggerCheck();
		Player.GetInstance().HandlePlayerInteractUI();
	}
}
