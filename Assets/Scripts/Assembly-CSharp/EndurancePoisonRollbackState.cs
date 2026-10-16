using UnityEngine;

public class EndurancePoisonRollbackState : IEnduranceState
{
	public void Enter(EnduranceStateMachine sm)
	{
		sm.player.EnduranceBar.GetProgress();
		sm.player.EnduranceBar.SetRollback(1f - sm.rollbackTarget);
	}

	public void Update(EnduranceStateMachine sm)
	{
		sm.Endurance -= sm.rollbackSpeed * Time.deltaTime;
		if (!sm.BlockRegen && sm.Endurance <= sm.rollbackTarget)
		{
			sm.ChangeState(new EnduranceNormalState());
		}
	}

	public void Exit(EnduranceStateMachine sm)
	{
		sm.player.EnduranceBar.SetRollback(0f);
	}
}
