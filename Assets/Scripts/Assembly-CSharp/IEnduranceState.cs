public interface IEnduranceState
{
	void Enter(EnduranceStateMachine sm);

	void Update(EnduranceStateMachine sm);

	void Exit(EnduranceStateMachine sm);
}
