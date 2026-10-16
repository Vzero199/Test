using UnityEngine;

public class EnduranceStateMachine
{
	public Player player;

	public IEnduranceState currentState;

	public float rollbackTarget;

	public float rollbackSpeed = 0.06f;

	public float regenSpeed = 0.06f;

	public float runDrain = 0.08f;

	public float walkDrain;

	private float pauseTimer;

	private bool isPaused;

	private const float CriticalThreshold = 0.3f;

	private float blinkTimer;

	private const float BlinkSpeed = 8f;

	private Color currentColor = Color.white;

	private Color targetColor = Color.white;

	private const float ColorLerpSpeed = 5f;

	private const float CriticalEnterDuration = 0.25f;

	private EnduranceModifier modifiers;

	private bool pendingRollback;

	public float Endurance
	{
		get
		{
			return player.EndurancePercent;
		}
		set
		{
			player.EndurancePercent = Mathf.Clamp01(value);
		}
	}

	public bool BlockRegen { get; set; }

	public bool IsExternallyDraining { get; private set; }

	public bool IsConsumptionPaused => isPaused;

	public void SetTargetColor(Color color)
	{
		if (!(targetColor == color))
		{
			Debug.Log($"Set Current target color {targetColor} To: {color}");
			targetColor = color;
		}
	}

	public EnduranceStateMachine(Player p)
	{
		player = p;
		ChangeState(new EnduranceNormalState());
	}

	public void AddModifier(EnduranceModifier mod)
	{
		modifiers |= mod;
		RecalculateLocks();
	}

	public void RemoveModifier(EnduranceModifier mod)
	{
		modifiers &= ~mod;
		RecalculateLocks();
	}

	public bool HasModifier(EnduranceModifier mod)
	{
		return (modifiers & mod) != 0;
	}

	private void RecalculateLocks()
	{
		bool flag = (BlockRegen = HasModifier(EnduranceModifier.TVDrain));
		SetExternalDrain(flag && !IsConsumptionPaused);
	}

	public void SetExternalDrain(bool value)
	{
		if (IsExternallyDraining != value)
		{
			IsExternallyDraining = value;
			RecalculateVisuals();
		}
	}

	private void RecalculateVisuals()
	{
		blinkTimer = 0f;
		if (IsConsumptionPaused)
		{
			SetTargetColor(Color.green);
		}
		else if (IsExternallyDraining)
		{
			SetTargetColor(Color.red);
		}
		else
		{
			SetTargetColor(Color.white);
		}
	}

	public void Update()
	{
		if (isPaused)
		{
			pauseTimer -= Time.deltaTime;
			if (pauseTimer <= 0f)
			{
				isPaused = false;
				RecalculateVisuals();
				if (pendingRollback)
				{
					ChangeState(new EndurancePoisonRollbackState());
					pendingRollback = false;
				}
			}
		}
		currentState.Update(this);
		UpdateBlinking();
		UpdateColorLerp();
	}

	private void UpdateColorLerp()
	{
		float num = (IsExternallyDraining ? 5f : 8f);
		currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * num);
		player.SetEnduranceBarColor(currentColor);
	}

	private void UpdateBlinking()
	{
		if (player.isTired || IsConsumptionPaused)
		{
			return;
		}
		if (Endurance > 0.3f)
		{
			if (IsExternallyDraining)
			{
				SetTargetColor(Color.red);
			}
			else
			{
				SetTargetColor(Color.white);
			}
		}
		else
		{
			blinkTimer += Time.deltaTime * 8f;
			float t = (Mathf.Cos(blinkTimer) + 1f) * 0.5f;
			Color color = Color.Lerp(Color.white, Color.red, t);
			SetTargetColor(color);
		}
	}

	public void ChangeState(IEnduranceState newState)
	{
		currentState?.Exit(this);
		Debug.Log("Change State From: " + (currentState?.GetType().Name ?? "null") + " To: " + newState.GetType().Name);
		currentState = newState;
		currentState.Enter(this);
	}

	public void PauseConsumption(float t)
	{
		player.EnduranceBar.SetRollback(0f);
		isPaused = true;
		pauseTimer = Mathf.Max(pauseTimer, t);
		RecalculateVisuals();
	}

	public void ConsumeRunning()
	{
		if (!isPaused)
		{
			Endurance -= runDrain * Time.deltaTime;
		}
		if (Endurance <= 0f)
		{
			ChangeState(new EnduranceTiredState());
		}
	}

	public void ConsumeWalking()
	{
		if (!isPaused)
		{
			Endurance -= walkDrain * Time.deltaTime;
		}
		if (Endurance <= 0f)
		{
			ChangeState(new EnduranceTiredState());
		}
	}

	public void SetConsumptionPause(bool value)
	{
		isPaused = value;
	}

	public void RestoreFull()
	{
		Endurance = 1f;
		rollbackTarget = Endurance;
		pendingRollback = false;
	}

	public void ApplyTemporaryBoost()
	{
		rollbackTarget = Endurance;
		Endurance = 1f;
		PauseConsumption(1f);
		if (!HasModifier(EnduranceModifier.TVDrain))
		{
			pendingRollback = true;
		}
	}
}
