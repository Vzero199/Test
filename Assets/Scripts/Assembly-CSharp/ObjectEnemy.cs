using UnityEngine;

public abstract class ObjectEnemy : Enemy
{
	protected BoxCollider objectCollider;

	public RuntimeAnimatorController AnimatorController
	{
		set
		{
			animatorController = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	protected override void Initialize()
	{
		blinkManager = BlinkManager.GetInstance();
		player = Player.GetInstance().transform;
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			animator = base.gameObject.AddComponent<Animator>();
		}
		objectCollider = GetComponent<BoxCollider>();
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = base.gameObject.AddComponent<Rigidbody>();
		}
		rb.useGravity = false;
		rb.isKinematic = true;
	}

	protected override void Update()
	{
	}

	protected void UpdateEnemy()
	{
		base.Update();
	}

	protected void CloseCatch()
	{
		ChangeState(new SexModeState());
	}
}
