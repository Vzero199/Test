using UnityEngine;

public class EnduranceNormalState : IEnduranceState
{
	public void Enter(EnduranceStateMachine sm)
	{
		if (!sm.IsConsumptionPaused)
		{
			sm.SetTargetColor(Color.white);
		}
	}

	public void Update(EnduranceStateMachine sm)
	{
		if (!sm.BlockRegen && !BlinkManager.GetInstance().IsFade())
		{
			if (!sm.player.IsMoving || sm.player.CurrentState is PlayerInteractState)
			{
				sm.Endurance += sm.regenSpeed * 1.5f * Time.deltaTime;
			}
			else if (sm.player.IsShift)
			{
				sm.ConsumeRunning();
			}
			else
			{
				sm.Endurance += sm.regenSpeed * 0.75f * Time.deltaTime;
			}
		}
	}

	public void Exit(EnduranceStateMachine sm)
	{
	}
}
