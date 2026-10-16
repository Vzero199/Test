using SmallHedge.SoundManager;
using UnityEngine;

public class EnduranceTiredState : IEnduranceState
{
	public void Enter(EnduranceStateMachine sm)
	{
		sm.player.isTired = true;
		sm.SetTargetColor(Color.red);
		SoundManager.PlaySound(SoundType.StaminaEnds);
		sm.player.IsShift = false;
		if (sm.player.CurrentState is PlayerRunState)
		{
			sm.player.ChangeState(new PlayerWalkState());
		}
	}

	public void Update(EnduranceStateMachine sm)
	{
		if (!sm.BlockRegen && !BlinkManager.GetInstance().IsFade())
		{
			if (sm.player.IsShift)
			{
				sm.player.IsShift = false;
			}
			sm.Endurance += sm.regenSpeed * Time.deltaTime * 1.5f;
			if (sm.Endurance >= 1f)
			{
				sm.ChangeState(new EnduranceNormalState());
			}
		}
	}

	public void Exit(EnduranceStateMachine sm)
	{
		sm.player.isTired = false;
		if (!sm.IsConsumptionPaused)
		{
			sm.SetTargetColor(Color.white);
		}
	}
}
