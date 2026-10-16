using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class BlinkManager : MonoBehaviour
{
	public Image panel;

	public float fadeSpeed = 2f;

	public float alpha;

	private bool isFadingIn;

	private bool isFadingOut;

	private bool holdAtMaxAlpha;

	private bool shouldBlinkAfterFade;

	private bool isBlinkingPending;

	private GameObject[] blinkObjects;

	private GameObject[] targets;

	private Entities[] targetClasses;

	private float offsetX;

	private float offsetY;

	private const float DEFAULT_MAX_ALPHA = 1f;

	private float maxAlpha = 1f;

	private float originalFadeSpeed;

	private float holdTime;

	private Action blinkAction;

	private bool allowVerticalMovement = true;

	private Collider blinkCollider;

	private static BlinkManager instance;

	private float fadeOutTime;

	public static BlinkManager Instance => instance;

	public bool IsFadingIn => isFadingIn;

	public bool IsFadingOut => isFadingOut;

	public bool IsFade()
	{
		return isFadingIn;
	}

	public static BlinkManager GetInstance()
	{
		return instance;
	}

	private void Awake()
	{
		instance = this;
		originalFadeSpeed = fadeSpeed;
	}

	private void Update()
	{
		HandleFading();
	}

	private void HandleFading()
	{
		if (isFadingIn)
		{
			PerformFadeIn();
		}
		if (isFadingOut)
		{
			PerformFadeOut();
		}
	}

	private void PerformFadeIn()
	{
		alpha += fadeSpeed * Time.deltaTime;
		alpha = Mathf.Min(alpha, maxAlpha);
		SetPanelAlpha();
		if (alpha == maxAlpha && !holdAtMaxAlpha)
		{
			StartCoroutine(HoldAtMaxAlpha());
		}
	}

	private void PerformFadeOut()
	{
		fadeOutTime += Time.deltaTime * fadeSpeed;
		float t = Mathf.SmoothStep(0f, 1f, fadeOutTime);
		alpha = Mathf.Lerp(maxAlpha, 0f, t);
		alpha = Mathf.Max(alpha, 0f);
		SetPanelAlpha();
		if (alpha <= 0f)
		{
			alpha = 0f;
			EndFadeOut();
		}
	}

	private void EndFadeOut()
	{
		if ((!(NightManager.GetInstance() != null) || NightManager.GetInstance().StopGameCoroutine == null) && (!(TimelineManager.GetInstance() != null) || !(TimelineManager.GetInstance().Director != null) || TimelineManager.GetInstance().Director.state == PlayState.Playing))
		{
			Timer.IsEnabled = true;
		}
		isFadingOut = false;
		fadeSpeed = originalFadeSpeed;
		maxAlpha = 1f;
		isBlinkingPending = false;
	}

	private void SetPanelAlpha()
	{
		Color color = panel.color;
		color.a = alpha;
		panel.color = color;
	}

	public void StartFadingIn()
	{
		if (!isFadingIn)
		{
			Debug.Log($"Alpha value: {alpha}, isFadingIn: {isFadingIn}, isFadingOut: {isFadingOut}");
			Timer.IsEnabled = false;
			isFadingIn = true;
			Player.GetInstance()?.DestroyCurrentArrow();
			isFadingOut = false;
		}
	}

	public void StartFadingOut()
	{
		if (!IsHScene())
		{
			Timer.IsEnabled = true;
		}
		isFadingOut = true;
		isFadingIn = false;
	}

	private bool IsHScene()
	{
		if (NightManager.GetInstance() != null && NightManager.GetInstance().StopGameCoroutine != null && !TVEnemy.IsTrapped && (WhiteWomanEnemy.isLoseEvent || !WhiteWomanEnemy.IsCharmGame))
		{
			if (SisSupEnemy.IsTrapped)
			{
				return SisSupEnemy.isCaptured;
			}
			return true;
		}
		return false;
	}

	public void FadeToBlink(GameObject movableObject, Action action = null, bool allowVerticalMovement = true, float offsetX = 0f, float offsetY = 0f, GameObject blinkTarget = null)
	{
		FadeToBlink(new GameObject[1] { movableObject }, action, allowVerticalMovement, offsetX, offsetY, blinkTarget);
	}

	public void FadeToBlink(GameObject[] movableObjects, Action action = null, bool allowVerticalMovement = true, float offsetX = 0f, float offsetY = 0f, GameObject blinkTarget = null)
	{
		if (isBlinkingPending)
		{
			Debug.LogWarning("BlinkManager: FadeToBlink вызван во время активного перехода. Игнор.");
			return;
		}
		isBlinkingPending = true;
		blinkObjects = new GameObject[1] { blinkTarget ?? Player.GetInstance().gameObject };
		SetBlinkParameters(movableObjects, action, allowVerticalMovement, offsetX, offsetY);
		PrepareFadeForBlink();
	}

	public void BlinkWithCustomSpeed(float customFadeSpeed, GameObject[] moveObjs = null, Action additionalAction = null, bool allowVerticalMovement = true, float offsetX = 0f, float offsetY = 0f, GameObject blinkTarget = null)
	{
		SetCustomFadeSpeed(customFadeSpeed);
		FadeToBlink(moveObjs ?? new GameObject[1] { Player.GetInstance().gameObject }, additionalAction, allowVerticalMovement, offsetX, offsetY, blinkTarget);
	}

	public void BlinkWithCustomSpeed(float customFadeSpeed, GameObject moveObj, Action additionalAction = null, bool allowVerticalMovement = true, float offsetX = 0f, float offsetY = 0f, GameObject blinkTarget = null)
	{
		BlinkWithCustomSpeed(customFadeSpeed, new GameObject[1] { moveObj }, additionalAction, allowVerticalMovement, offsetX, offsetY, blinkTarget);
	}

	private void SetBlinkParameters(GameObject[] movableObjects, Action action, bool allowVerticalMovement, float offsetX, float offsetY)
	{
		this.offsetX = offsetX;
		this.offsetY = offsetY;
		targets = movableObjects;
		targetClasses = new Entities[movableObjects.Length];
		blinkAction = action;
		this.allowVerticalMovement = allowVerticalMovement;
		blinkObjects = blinkObjects ?? new GameObject[1] { Player.GetInstance().gameObject };
	}

	private void PrepareFadeForBlink()
	{
		if (holdTime <= 0f)
		{
			holdTime = 0.1f;
		}
		shouldBlinkAfterFade = true;
		StartFadingIn();
	}

	private void SetCustomFadeSpeed(float customFadeSpeed)
	{
		fadeSpeed = customFadeSpeed;
		maxAlpha = 1f;
		holdTime = 1f / 3f * (1f / fadeSpeed);
		shouldBlinkAfterFade = false;
	}

	private void Blink()
	{
		if (targets == null || targets.Length == 0)
		{
			Debug.LogWarning("BlinkManager: No targets assigned for blink.");
			return;
		}
		GameObject[] array = targets;
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				MoveTarget(gameObject);
				ResetInteractionTimer(gameObject);
				if (gameObject.CompareTag("Player"))
				{
					MoveCamera();
				}
			}
		}
		isBlinkingPending = false;
		blinkAction?.Invoke();
		blinkAction = null;
	}

	private void MoveTarget(GameObject target)
	{
		if (blinkObjects == null || blinkObjects.Length == 0 || blinkObjects[0] == null)
		{
			Debug.LogWarning("BlinkManager: blinkObjects пуст или содержит null. Используется Player.GetInstance() вместо.");
			blinkObjects = new GameObject[1] { Player.GetInstance().gameObject };
		}
		GameObject gameObject = blinkObjects[0];
		blinkCollider = gameObject.GetComponent<Collider>();
		float blinkColliderBottom = ((blinkCollider != null) ? blinkCollider.bounds.min.y : gameObject.transform.position.y);
		if (blinkCollider == null)
		{
			Debug.LogWarning("BlinkManager: У объекта нет Collider. Используется transform.position.y.");
		}
		SpriteRenderer component = target.GetComponent<SpriteRenderer>();
		Vector3 position = CalculateNewPosition(target, gameObject, blinkColliderBottom, component);
		target.transform.position = position;
		targetClasses[Array.IndexOf(targets, target)] = target.GetComponent<Entities>();
	}

	private Vector3 CalculateNewPosition(GameObject target, GameObject referenceObject, float blinkColliderBottom, SpriteRenderer targetRenderer)
	{
		float num = ((targetRenderer != null) ? targetRenderer.bounds.size.y : target.transform.localScale.y);
		float num2 = ((blinkCollider != null) ? blinkCollider.bounds.center.x : referenceObject.transform.position.x);
		return new Vector3(z: referenceObject.transform.position.z, x: num2 + offsetX, y: allowVerticalMovement ? (blinkColliderBottom + num / 2f + offsetY) : target.transform.position.y);
	}

	private void ResetInteractionTimer(GameObject target)
	{
		if (!target.GetComponent<CameraFollow>())
		{
			target.GetComponent<Entities>();
			if (!(blinkObjects[0].GetComponent<Elevator>() == null))
			{
				Player.GetInstance().InteractionElevatorTimer = Player.GetInstance().ElevatorCooldown;
			}
		}
	}

	private void MoveCamera()
	{
		GameObject obj = Camera.main.gameObject;
		CameraFollow component = obj.GetComponent<CameraFollow>();
		Vector3 position = component.CalculateTargetPosition();
		obj.transform.position = position;
		component.SmoothMove();
	}

	private IEnumerator HoldAtMaxAlpha()
	{
		holdAtMaxAlpha = true;
		yield return new WaitForSecondsRealtime(holdTime);
		holdAtMaxAlpha = false;
		StartFadingOut();
		if (shouldBlinkAfterFade)
		{
			Blink();
		}
	}

	public void BlinkInstant(GameObject movableObject, Action onComplete = null, bool allowVerticalMovement = true, float offsetX = 0f, float offsetY = 0f, GameObject blinkTarget = null)
	{
		if (movableObject == null)
		{
			Debug.LogWarning("BlinkInstant: movableObject is null, using Player.GetInstance() instead.");
			movableObject = Player.GetInstance()?.gameObject;
			if (movableObject == null)
			{
				Debug.LogError("BlinkInstant: Player.GetInstance() returned null for movableObject. Aborting.");
				return;
			}
		}
		if (blinkTarget == null)
		{
			Debug.LogWarning("BlinkInstant: blinkTarget is null, using Player.GetInstance() instead.");
			blinkTarget = Player.GetInstance()?.gameObject;
			if (blinkTarget == null)
			{
				Debug.LogError("BlinkInstant: Player.GetInstance() returned null for blinkTarget. Aborting.");
				return;
			}
		}
		Vector3 position = blinkTarget.transform.position;
		Collider component = blinkTarget.GetComponent<Collider>();
		if (component != null)
		{
			position.y = component.bounds.min.y;
		}
		BoxCollider component2 = blinkTarget.GetComponent<BoxCollider>();
		if (component2 != null)
		{
			position.x = component2.bounds.min.x;
		}
		position.x += offsetX;
		if (allowVerticalMovement)
		{
			position.y += offsetY;
			Collider component3 = movableObject.GetComponent<Collider>();
			if (component3 != null)
			{
				float y = component3.bounds.extents.y;
				position.y += y;
			}
		}
		else
		{
			position.y = movableObject.transform.position.y;
		}
		movableObject.transform.position = position;
		Entities component4 = movableObject.GetComponent<Entities>();
		if (component4 != null)
		{
			component4.AlignToRoomLine();
		}
		onComplete?.Invoke();
	}
}
