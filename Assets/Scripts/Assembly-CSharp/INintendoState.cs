using UnityEngine;

public interface INintendoState
{
	void EnterState(Nintendo nintendo);

	void UpdateState(Nintendo nintendo);

	void ExitState(Nintendo nintendo);

	void EnterTriggerState(Nintendo nintendo, Collider collider);

	void StayTriggerState(Nintendo nintendo, Collider collider);

	void ExitTriggerState(Nintendo nintendo, Collider collider);
}
