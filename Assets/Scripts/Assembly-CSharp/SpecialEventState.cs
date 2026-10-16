public class SpecialEventState : IEnemyState
{
	public void EnterState(Enemy enemy)
	{
		enemy.PlayAnimation("SpecialEvent");
	}

	public void UpdateState(Enemy enemy)
	{
	}

	public void ExitState(Enemy enemy)
	{
	}
}
