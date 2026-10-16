using System;
using SmallHedge.SoundManager;
using UnityEngine;

public class CrossItem : ItemToUse
{
	private static bool nunIsExists;

	private Animator crossAnimator;

	public static bool NunIsExists
	{
		get
		{
			return nunIsExists;
		}
		set
		{
			if (nunIsExists != value)
			{
				nunIsExists = value;
				NunExistsChanged?.Invoke(nunIsExists);
			}
		}
	}

	public static event Action<bool> NunExistsChanged;

	private void Awake()
	{
		crossAnimator = GetComponent<Animator>();
		if (crossAnimator == null)
		{
			crossAnimator = base.gameObject.AddComponent<Animator>();
			RuntimeAnimatorController runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("CrossController");
			crossAnimator.runtimeAnimatorController = runtimeAnimatorController;
		}
	}

	private void OnEnable()
	{
		NunExistsChanged += NunExistsHandler;
		UpdateAnimation(nunIsExists);
	}

	private void NunExistsHandler(bool newValue)
	{
		UpdateAnimation(newValue);
	}

	private void UpdateAnimation(bool nunIsExists)
	{
		if (!(crossAnimator == null))
		{
			crossAnimator.Play(nunIsExists ? "CrossShine" : "CrossIdle");
		}
	}

	private void OnDisable()
	{
		NunExistsChanged -= NunExistsHandler;
	}

	public override bool CanBeUsed()
	{
		return NunIsExists;
	}

	public override void UseItem()
	{
		NunEnemy nunEnemy = UnityEngine.Object.FindObjectOfType<NunEnemy>();
		SoundManager.PlaySound(SoundType.Cross);
		if (nunEnemy != null)
		{
			nunEnemy.NunDisapper();
		}
	}
}
