using UnityEngine.EventSystems;

public class NullInputModule : BaseInput
{
	public override bool GetMouseButtonDown(int button)
	{
		return false;
	}

	public override bool GetMouseButton(int button)
	{
		return false;
	}

	public override bool GetMouseButtonUp(int button)
	{
		return false;
	}
}
