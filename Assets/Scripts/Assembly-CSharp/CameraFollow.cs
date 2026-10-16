using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CameraFixedAspect))]
public class CameraFollow : MonoBehaviour
{
	private enum CameraMode
	{
		Default = 0,
		Tab = 1,
		ScreenMove = 2
	}

	[SerializeField]
	private float yOffset = 1f;

	[SerializeField]
	private Transform target;

	[SerializeField]
	private float forwardOffset = 2f;

	[SerializeField]
	private float followSpeed = 5f;

	private CameraMode currentMode;

	private bool isMoving;

	private bool isTabPressed;

	private bool isScreenMovePressed;

	private Vector3 step = Vector3.one;

	private Vector3 newPosition;

	private float timer;

	private SpriteRenderer playerSpriteRenderer;

	private float ppu = 1f;

	[SerializeField]
	private Camera mainCamera;

	private Vector3 initialPosition;

	private Vector3 targetPosition;

	private static CameraFollow instance;

	private float holdStartTime;

	private bool isHoldTimerRunning;

	private Coroutine holdCoroutine;

	private Coroutine tabCoroutine;

	[SerializeField]
	private float screenMoveDelay = 0.001f;

	private float directionChangeTime;

	private bool isFacingRight = true;

	private const float directionChangeDuration = 0.5f;

	public Transform FollowTarget => target;

	public bool IsTabPressed => isTabPressed;

	public bool IsScreenMovePressed => isScreenMovePressed;

	public void SetFollowTarget(Transform newTarget)
	{
		base.transform.position = new Vector3(newTarget.position.x, newTarget.position.y, -50f);
		target = newTarget;
	}

	public static CameraFollow GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<CameraFollow>();
		}
		return instance;
	}

	private void Start()
	{
		playerSpriteRenderer = target.GetComponent<SpriteRenderer>();
		base.transform.position = target.position;
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		SmoothMove();
	}

	private void Update()
	{
		if (target == null)
		{
			return;
		}
		if (target.gameObject.HasScript("Player"))
		{
			if (Player.GetInstance().IsCaught)
			{
				base.transform.position = new Vector3(target.position.x, base.transform.position.y, base.transform.position.z);
				return;
			}
			if (Player.GetInstance().IsMoving)
			{
				currentMode = CameraMode.Default;
			}
			switch (currentMode)
			{
			case CameraMode.Default:
				StepUpdate();
				break;
			case CameraMode.Tab:
				StepUpdateTab();
				break;
			case CameraMode.ScreenMove:
				StepUpdateScreenMove();
				break;
			}
			if (isMoving)
			{
				FixedMovePosition();
			}
			if (!Player.GetInstance().IsMoving && isScreenMovePressed && currentMode != CameraMode.ScreenMove && holdCoroutine == null)
			{
				holdCoroutine = StartCoroutine(ScreenMoveDelayCoroutine());
			}
		}
		else
		{
			base.transform.position = new Vector3(target.position.x, base.transform.position.y, base.transform.position.z);
		}
	}

	private void CheckPlayerDir()
	{
	}

	private void FixedMovePosition()
	{
		if (timer > 0f && currentMode != CameraMode.Default)
		{
			timer -= Time.deltaTime;
			base.transform.position += step * ((timer > 0.5f) ? 2 : 3);
			base.transform.position = RoundToPPU(base.transform.position);
		}
		else
		{
			isMoving = false;
			step = Vector3.one;
		}
	}

	public Vector3 CalculateTargetPosition()
	{
		CheckPlayerDir();
		bool faceRight = Player.GetInstance().FaceRight;
		if (faceRight != isFacingRight)
		{
			isFacingRight = faceRight;
			directionChangeTime = 0f;
		}
		float num = (faceRight ? forwardOffset : (0f - forwardOffset));
		return new Vector3(target.position.x + num, target.position.y + yOffset / 10f, target.position.z - 10f);
	}

	private void StepUpdate()
	{
		newPosition = CalculateTargetPosition();
		if (directionChangeTime < 0.5f)
		{
			directionChangeTime += Time.deltaTime;
		}
		float t = directionChangeTime / 0.5f;
		Vector3 position = Vector3.Lerp(base.transform.position, newPosition, t);
		base.transform.position = position;
		base.transform.position = RoundToPPU(base.transform.position);
	}

	private void StepUpdateTab()
	{
		if (!Player.GetInstance().IsMoving)
		{
			newPosition = CalculateNearestTabLightPosition();
			step = (newPosition - base.transform.position) * followSpeed * Time.deltaTime;
		}
	}

	private void StepUpdateScreenMove()
	{
		step = (targetPosition - base.transform.position) * followSpeed * Time.deltaTime;
	}

	public void SmoothMove()
	{
		CheckPlayerDir();
		isMoving = true;
		timer = 2f;
	}

	public void MoveToTargetSmoothly(Transform newTarget)
	{
		if (newTarget != null)
		{
			target = newTarget;
			Vector3 targetPos = CalculateTargetPosition();
			targetPos.z = base.transform.position.z;
			StartCoroutine(SmoothMoveCoroutine(targetPos, 1f));
		}
	}

	private IEnumerator SmoothMoveCoroutine(Vector3 targetPos, float duration)
	{
		isMoving = true;
		Vector3 startPos = base.transform.position;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
			base.transform.position = Vector3.Lerp(startPos, targetPos, t);
			yield return null;
		}
		isMoving = false;
		base.transform.position = targetPos;
	}

	private Vector3 RoundToPPU(Vector3 position)
	{
		float x = Mathf.Round(position.x * ppu) / ppu;
		float y = Mathf.Round(position.y * ppu) / ppu;
		return new Vector3(x, y, position.z);
	}

	public void OnTab(InputAction.CallbackContext context)
	{
		if (Player.GetInstance() == null || PauseMenu.IsPaused || Timer.IsTransitioning)
		{
			return;
		}
		if (context.performed && currentMode == CameraMode.ScreenMove)
		{
			isTabPressed = true;
		}
		else if (context.canceled && currentMode == CameraMode.ScreenMove)
		{
			isTabPressed = false;
		}
		else if (context.performed && !Player.GetInstance().IsMoving && !(Player.GetInstance().CurrentState is PlayerHideState))
		{
			if (tabCoroutine == null)
			{
				tabCoroutine = StartCoroutine(TabDelayCoroutine());
			}
		}
		else if (context.canceled)
		{
			if (tabCoroutine != null)
			{
				StopCoroutine(tabCoroutine);
				tabCoroutine = null;
			}
			isTabPressed = false;
			currentMode = CameraMode.Default;
			SmoothMove();
		}
	}

	private IEnumerator TabDelayCoroutine()
	{
		yield return new WaitForSeconds(0.5f);
		if (!Player.GetInstance().IsMoving && currentMode != CameraMode.Tab)
		{
			currentMode = CameraMode.Tab;
			isTabPressed = true;
			initialPosition = base.transform.position;
			SmoothMoveToNearestTabLight();
		}
		tabCoroutine = null;
	}

	private void SmoothMoveToNearestTabLight()
	{
		isMoving = true;
		timer = 2f;
		targetPosition = CalculateNearestTabLightPosition();
	}

	private Vector3 CalculateNearestTabLightPosition()
	{
		CheckPlayerDir();
		Vector3 result = base.transform.position;
		float num = float.PositiveInfinity;
		bool faceRight = Player.GetInstance().FaceRight;
		GameObject[] array = GameObject.FindGameObjectsWithTag("TabLight");
		foreach (GameObject gameObject in array)
		{
			GameObject roomContainingObject = GetRoomContainingObject(gameObject);
			if (roomContainingObject != null && roomContainingObject.name != Player.GetInstance().GetCurrentRoom())
			{
				continue;
			}
			Vector3 vector = gameObject.transform.position - base.transform.position;
			if ((faceRight && vector.x > 0f) || (!faceRight && vector.x < 0f))
			{
				float num2 = Mathf.Abs(vector.x);
				if (num2 < num)
				{
					num = num2;
					result = new Vector3(gameObject.transform.position.x, base.transform.position.y, base.transform.position.z);
				}
			}
		}
		return result;
	}

	private GameObject GetRoomContainingObject(GameObject obj)
	{
		StaticObjectRoom component = obj.GetComponent<StaticObjectRoom>();
		if (component != null)
		{
			return component.roomName;
		}
		Vector3 vector = CalculateRoomBounds(obj);
		Collider[] array = Physics.OverlapBox(CalculateRoomCenter(obj), vector / 2f);
		foreach (Collider collider in array)
		{
			if (collider.CompareTag("RoomNamer"))
			{
				return collider.gameObject;
			}
		}
		Debug.LogError("Room Not Found! For Object: " + obj.name);
		return null;
	}

	protected Vector3 CalculateRoomBounds(GameObject roomObject)
	{
		BoxCollider component = roomObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			return component.size;
		}
		Renderer component2 = roomObject.GetComponent<Renderer>();
		if (component2 != null)
		{
			return component2.bounds.size;
		}
		return new Vector3(2f, 2f, 1000f);
	}

	protected Vector3 CalculateRoomCenter(GameObject roomObject)
	{
		BoxCollider component = roomObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			return roomObject.transform.position + roomObject.transform.TransformVector(component.center);
		}
		Renderer component2 = roomObject.GetComponent<Renderer>();
		if (component2 != null)
		{
			return component2.bounds.center;
		}
		return roomObject.transform.position;
	}

	public void OnScreenMove(InputAction.CallbackContext context)
	{
		if (PauseMenu.IsPaused || Timer.IsTransitioning)
		{
			return;
		}
		if (context.performed && Player.GetInstance().CurrentBatteryLevel > 0 && !(Player.GetInstance().CurrentState is PlayerHideState))
		{
			isScreenMovePressed = true;
			if (FlashlightSword.GetInstance().gameObject.activeSelf && holdCoroutine == null)
			{
				holdCoroutine = StartCoroutine(ScreenMoveDelayCoroutine());
			}
		}
		else
		{
			if (!context.canceled && Player.GetInstance().CurrentBatteryLevel != 0)
			{
				return;
			}
			isScreenMovePressed = false;
			if (holdCoroutine != null)
			{
				StopCoroutine(holdCoroutine);
				holdCoroutine = null;
			}
			isHoldTimerRunning = false;
			if (isTabPressed)
			{
				if (tabCoroutine == null)
				{
					tabCoroutine = StartCoroutine(TabDelayCoroutine());
				}
			}
			else
			{
				currentMode = CameraMode.Default;
				SmoothMove();
			}
		}
	}

	private IEnumerator ScreenMoveDelayCoroutine()
	{
		yield return new WaitForSeconds(screenMoveDelay);
		holdStartTime = Time.time;
		isHoldTimerRunning = true;
		while (isHoldTimerRunning)
		{
			if (Time.time - holdStartTime >= 0.001f)
			{
				if (currentMode != CameraMode.ScreenMove)
				{
					currentMode = CameraMode.ScreenMove;
					SmoothMoveToScreenSide();
				}
				isHoldTimerRunning = false;
				holdCoroutine = null;
				break;
			}
			yield return null;
		}
	}

	private void SmoothMoveToScreenSide()
	{
		isMoving = true;
		timer = 2f;
		bool faceRight = Player.GetInstance().FaceRight;
		Vector3 position = Player.GetInstance().transform.position;
		float num = mainCamera.orthographicSize * mainCamera.aspect * 1.5f;
		float b = position.x - num;
		float b2 = position.x + num;
		if (faceRight)
		{
			targetPosition = new Vector3(Mathf.Min(base.transform.position.x + num, b2), base.transform.position.y, base.transform.position.z);
		}
		else
		{
			targetPosition = new Vector3(Mathf.Max(base.transform.position.x - num, b), base.transform.position.y, base.transform.position.z);
		}
	}

	public void AlignCameraToTarget()
	{
		if (target != null)
		{
			base.transform.position = new Vector3(target.position.x, target.position.y, -50f);
		}
	}
}
