using UnityEngine;

public class EndurancePausedState : IEnduranceState
{
	public void Enter(EnduranceStateMachine sm)
	{
		sm.player.SetEnduranceBarColor(Color.red);
	}

	public void Update(EnduranceStateMachine sm)
	{
	}

	public void Exit(EnduranceStateMachine sm)
	{
	}
}
