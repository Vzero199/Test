using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

public abstract class Entities : MonoBehaviour
{
	[SerializeField]
	protected RuntimeAnimatorController animatorController;

	[SerializeField]
	private GameObject sexStateLightPrefab;

	[SerializeField]
	private GameObject sexStateMaskPrefab;

	protected Dictionary<string, float> speedModifiers = new Dictionary<string, float>();

	private GameObject lightPrefabInstance;

	private float moveSpeedCup;

	private float interactionSpeedCup;

	private GameObject maskPrefabInstance;

	[SerializeField]
	protected float currentMoveSpeed = 200f;

	[SerializeField]
	protected float currentInteractionSpeed = 1f;

	[Range(0f, 5f)]
	[SerializeField]
	private float interactionCooldown = 1f;

	private PlayableDirector timeline;

	private static readonly Vector3 DefaultRoomSize = new Vector3(2f, 2f, 1000f);

	private float baseMoveSpeed;

	private float baseInteractionSpeed;

	private GameObject currentRoomObject;

	protected string currentStageName;

	protected Vector3 moveDirection;

	private bool faceRight = true;

	private Coroutine cooldownCoroutine;

	protected Animator animator;

	private BoxCollider entityCollider;

	protected Rigidbody rb;

	protected StageManager stageManager;

	protected BlinkManager blinkManager;

	protected SpriteRenderer spriteRenderer;

	private bool canInteract = true;

	protected bool inTrigger;

	protected InteractionObject interactObject;

	protected bool isMoving;

	protected float roamTime;

	protected string currentRoom;

	public GameObject SexStateLightPrefab
	{
		get
		{
			return sexStateLightPrefab;
		}
		set
		{
			sexStateLightPrefab = value;
		}
	}

	public GameObject SexStateMaskPrefab
	{
		get
		{
			return sexStateMaskPrefab;
		}
		set
		{
			sexStateMaskPrefab = value;
		}
	}

	public float MoveSpeedCup
	{
		get
		{
			return moveSpeedCup;
		}
		set
		{
			moveSpeedCup = value;
		}
	}

	public float InteractionSpeedCup
	{
		get
		{
			return interactionSpeedCup;
		}
		set
		{
			interactionSpeedCup = value;
		}
	}

	public GameObject LightPrefabInstance
	{
		get
		{
			return lightPrefabInstance;
		}
		set
		{
			lightPrefabInstance = value;
		}
	}

	public GameObject MaskPrefabInstance
	{
		get
		{
			return maskPrefabInstance;
		}
		set
		{
			maskPrefabInstance = value;
		}
	}

	public float BaseInteractionSpeed => baseInteractionSpeed;

	public float BaseMoveSpeed => baseMoveSpeed;

	public float InteractionCooldown
	{
		get
		{
			return interactionCooldown;
		}
		set
		{
			interactionCooldown = value;
		}
	}

	public virtual string CurrentStageName
	{
		get
		{
			if (currentStageName == null)
			{
				InitializeCurrentRoom();
			}
			return currentStageName;
		}
		set
		{
			currentStageName = value;
		}
	}

	public GameObject CurrentRoomObject => currentRoomObject;

	public bool FaceRight
	{
		get
		{
			return faceRight;
		}
		set
		{
			FaceRightSet(value);
		}
	}

	public BoxCollider EntityCollider
	{
		get
		{
			return entityCollider;
		}
		set
		{
			entityCollider = value;
		}
	}

	public bool CanInteract
	{
		get
		{
			return canInteract;
		}
		set
		{
			canInteract = value;
		}
	}

	public InteractionObject InteractObject
	{
		get
		{
			return interactObject;
		}
		set
		{
			Debug.Log($"Object Change For {base.gameObject} On {value}");
			interactObject = value;
		}
	}

	public virtual string CurrentRoom
	{
		get
		{
			return currentRoom;
		}
		set
		{
			if (currentRoom != value)
			{
				currentRoom = value;
			}
		}
	}

	public virtual float CurrentMoveSpeed
	{
		get
		{
			return currentMoveSpeed;
		}
		set
		{
			currentMoveSpeed = value;
		}
	}

	public virtual float CurrentInteractionSpeed
	{
		get
		{
			return currentInteractionSpeed;
		}
		set
		{
			currentInteractionSpeed = value;
		}
	}

	public Vector3 MoveDirection
	{
		get
		{
			return moveDirection;
		}
		set
		{
			moveDirection = value;
		}
	}

	public bool InTrigger
	{
		get
		{
			return inTrigger;
		}
		set
		{
			inTrigger = value;
		}
	}

	public bool IsMoving
	{
		get
		{
			return isMoving;
		}
		set
		{
			isMoving = value;
		}
	}

	public float TotalSpeedModifier
	{
		get
		{
			float num = 1f;
			foreach (float value in speedModifiers.Values)
			{
				num += value;
			}
			return Mathf.Clamp(num, 0.1f, 3f);
		}
	}

	public void FixAnimator()
	{
		Animator component = GetComponent<Animator>();
		component.Rebind();
		component.Update(0f);
	}

	public void SetAnimatorController(RuntimeAnimatorController value)
	{
		animatorController = value;
	}

	private void FaceRightSet(bool value)
	{
		UpdateRotation(value);
	}

	public void SetAnimator(Animator anim)
	{
		animator = anim;
	}

	public Animator GetAnimator()
	{
		return animator;
	}

	protected float GetCurrentMoveSpeed()
	{
		return currentMoveSpeed;
	}

	protected float GetCurrentInteractionSpeed()
	{
		return currentInteractionSpeed;
	}

	protected bool IsTimelinePlaying()
	{
		if (timeline != null)
		{
			return timeline.state == PlayState.Playing;
		}
		return false;
	}

	public abstract void EnterHideState();

	public abstract void ExitHideState();

	public void SetBaseSpeed(float speed, float interactionSpeed)
	{
		baseMoveSpeed = speed;
		baseInteractionSpeed = interactionSpeed;
		if (!(this is Player))
		{
			CurrentMoveSpeed = baseMoveSpeed;
			CurrentInteractionSpeed = baseInteractionSpeed;
		}
	}

	public void AddSpeedModifier(string key, float modifier)
	{
		if (!speedModifiers.ContainsKey(key))
		{
			speedModifiers[key] = modifier;
			RecalculateSpeed();
		}
	}

	public void RemoveSpeedModifier(string key)
	{
		if (speedModifiers.ContainsKey(key))
		{
			speedModifiers.Remove(key);
			RecalculateSpeed();
		}
	}

	public void ResetSpeedModifiers()
	{
		speedModifiers.Clear();
		RecalculateSpeed();
	}

	private void RecalculateSpeed()
	{
		float num = 1f;
		foreach (float value in speedModifiers.Values)
		{
			num += value;
		}
		num = Mathf.Clamp(num, 0.1f, 3f);
		CurrentMoveSpeed = BaseMoveSpeed * num;
		CurrentInteractionSpeed = BaseInteractionSpeed * num;
		float num2 = 1f;
		foreach (float value2 in speedModifiers.Values)
		{
			if (value2 < 0f)
			{
				num2 += value2;
			}
		}
		num2 = Mathf.Clamp(num2, 0.1f, 1f);
		if (animator != null)
		{
			animator.speed = num2;
		}
		HandleAnimBySpeed();
	}

	protected virtual void HandleAnimBySpeed()
	{
	}

	public string GetCurrentRoom()
	{
		return currentRoom;
	}

	protected virtual void Start()
	{
		RigidBodyInitialize();
		SpriteRendererInitialize();
		if (!(this is WhiteWomanEnemy))
		{
			BoxColliderInitialize();
		}
		AnimatorInitialize();
		if (timeline == null)
		{
			timeline = Object.FindObjectOfType<PlayableDirector>();
		}
		stageManager = StageManager.GetInstance();
		blinkManager = BlinkManager.GetInstance();
		SetBaseSpeed(CurrentMoveSpeed, CurrentInteractionSpeed);
		sexStateLightPrefab = Resources.Load<GameObject>("prefabs/SexStateLight");
		sexStateMaskPrefab = Resources.Load<GameObject>("prefabs/SexStateMask");
		InitializeCurrentRoom();
	}

	protected void AnimatorInitialize()
	{
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			animator = base.gameObject.AddComponent<Animator>();
		}
		if (animator.runtimeAnimatorController == null)
		{
			animator.runtimeAnimatorController = animatorController;
		}
	}

	protected void RigidBodyInitialize()
	{
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = base.gameObject.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.isKinematic = true;
		}
	}

	protected void SpriteRendererInitialize()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null)
		{
			spriteRenderer = base.gameObject.AddComponent<SpriteRenderer>();
		}
		if (!(this is ObjectEnemy))
		{
			spriteRenderer.sortingLayerName = "Entity";
			if (!(this is Player))
			{
				spriteRenderer.sortingOrder = 59;
			}
			if (this is Nintendo)
			{
				spriteRenderer.sortingOrder = 90;
			}
			Material material = Resources.Load<Material>("materials/Sprite-Lit-Default");
			if (material != null)
			{
				spriteRenderer.material = material;
			}
		}
	}

	protected void BoxColliderInitialize()
	{
		BoxCollider boxCollider = GetComponent<BoxCollider>();
		if (boxCollider == null)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		boxCollider.isTrigger = true;
		entityCollider = boxCollider;
		boxCollider.size = new Vector3(100f, 200f, 500f);
		if (this is Nintendo)
		{
			boxCollider.size = new Vector3(100f, 180f, 500f);
		}
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		HandleCurrentRoom(other);
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (other.gameObject.name == "Stage3" && currentRoom == "Stage3" && other is BoxCollider boxCollider && !(this is Player) && Time.timeScale != 0f && BlinkManager.GetInstance() != null && !BlinkManager.GetInstance().IsFade())
		{
			float num = boxCollider.center.x - base.transform.position.x;
			Vector3 position = base.transform.position;
			if (num <= 0f)
			{
				position.x -= StageManager.chunkWidth * 12f;
			}
			else
			{
				position.x += StageManager.chunkWidth * 12f;
			}
			base.transform.position = position;
		}
	}

	protected virtual void HandleCurrentRoom(Collider other)
	{
		if (other.CompareTag("RoomNamer"))
		{
			HospitalManager.GetInstance().GetRoomContainingObject(base.gameObject);
			currentRoomObject = other.gameObject;
			currentRoom = currentRoomObject.name;
			CurrentStageName = HospitalManager.GetInstance().FindNearestRoomStartingWith(currentRoomObject, "Stage");
			if (CurrentStageName != null)
			{
				Debug.Log("CurrentStage Is: " + CurrentStageName);
			}
			else
			{
				Debug.LogWarning("CurrentStage Is Null");
			}
			Debug.Log(base.gameObject.name + " entered " + currentRoom);
		}
	}

	protected void Flip()
	{
		float num = Mathf.Round(base.transform.rotation.eulerAngles.y);
		if (moveDirection.x > 0f && num == 180f)
		{
			UpdateRotation(isRight: true);
		}
		else if (moveDirection.x < 0f && num == 0f)
		{
			UpdateRotation(isRight: false);
		}
	}

	private void UpdateRotation(bool isRight)
	{
		faceRight = isRight;
		base.transform.rotation = Quaternion.Euler(0f, (!faceRight) ? 180 : 0, 0f);
	}

	public virtual void InitializeCurrentRoom()
	{
		if (!(NightManager.GetInstance() == null))
		{
			GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(base.gameObject);
			if (roomContainingObject != null)
			{
				currentRoom = roomContainingObject.name;
				currentRoomObject = roomContainingObject;
				CurrentStageName = HospitalManager.GetInstance().FindNearestRoomStartingWith(currentRoomObject, "Stage");
				Debug.Log(base.gameObject.name + "in the: " + currentRoom);
			}
			else
			{
				Debug.LogError("Can't Get Room For: " + base.gameObject.name);
			}
		}
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
		return DefaultRoomSize;
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

	protected virtual void FixedUpdate()
	{
		if (!(BlinkManager.GetInstance() != null) || (!BlinkManager.GetInstance().IsFadingIn && (!(this is Player player) || !(player.CurrentState is PlayerHideState)) && (!(this is Nintendo nintendo) || !(nintendo.CurrentState is NintendoHideState))))
		{
			moveDirection.x = GetDirection();
			Flip();
			Move();
		}
	}

	public void AlignToRoomLine()
	{
		if (!(this is SisSupEnemy))
		{
			float holdLineY = GetHoldLineY();
			float entityBottomY = GetEntityBottomY();
			base.transform.position = new Vector3(base.transform.position.x, holdLineY + (base.transform.position.y - entityBottomY), base.transform.position.z);
		}
	}

	private float GetHoldLineY()
	{
		GameObject roomContainingObject = HospitalManager.GetInstance().GetRoomContainingObject(base.gameObject);
		if (roomContainingObject == null)
		{
			if (CurrentRoomObject == null)
			{
				Debug.LogError("RoomNamer not found for " + base.gameObject.name);
				return 0f;
			}
			roomContainingObject = CurrentRoomObject;
		}
		Transform transform = roomContainingObject.transform.Find("HoldTheLine");
		if (transform == null)
		{
			Debug.LogError("Line object not found in RoomNamer " + roomContainingObject.name);
			return 0f;
		}
		BoxCollider component = transform.GetComponent<BoxCollider>();
		if (component == null)
		{
			Debug.LogError("No BoxCollider found on Line object in RoomNamer " + roomContainingObject.name);
			return 0f;
		}
		return transform.position.y + component.center.y - component.size.y / 2f;
	}

	private float GetEntityBottomY()
	{
		Transform transform = base.transform.Find("CockCollider");
		BoxCollider boxCollider = null;
		boxCollider = ((!(transform != null)) ? base.transform.GetComponent<BoxCollider>() : transform.GetComponent<BoxCollider>());
		return base.transform.position.y + boxCollider.center.y - boxCollider.size.y / 2f;
	}

	protected virtual void Move()
	{
		if (moveDirection.x != 0f)
		{
			isMoving = true;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Vector3 position = rb.position + moveDirection * CurrentMoveSpeed * Time.fixedDeltaTime;
			Vector3 extents = entityCollider.bounds.extents;
			if (Physics.BoxCast(rb.position, extents, moveDirection, out var hitInfo, Quaternion.identity, CurrentMoveSpeed * Time.fixedDeltaTime) && hitInfo.collider.CompareTag("Wall"))
			{
				isMoving = false;
				return;
			}
			rb.MovePosition(position);
			if (isMoving)
			{
				AlignToRoomLine();
			}
		}
		else if (isMoving)
		{
			isMoving = false;
		}
	}

	public void FixPixelPosition()
	{
		Vector2 vector = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
		rb.position = vector;
	}

	public virtual void ForceBlink(float speed)
	{
		BlinkManager.GetInstance().BlinkWithCustomSpeed(speed);
	}

	public void ToggleRenderers(bool state)
	{
		Debug.Log($"Renderers Toggle Status is: {state}");
		SetLightAndRendererState(state);
		ToggleRoomRenderers(state);
		ToggleUIParty(state);
	}

	private void SetLightAndRendererState(bool state)
	{
		if (OptiFineManager.GetInstance() != null)
		{
			OptiFineManager.GetInstance().gameObject.SetActive(state);
		}
		if ((this is WhiteWomanEnemy) & state)
		{
			Player.GetInstance().gameObject.SetActive(state);
			Player.GetInstance().ElevatorInstance.Animator.Play("Closed");
		}
		Light2D[] array = Object.FindObjectsOfType<Light2D>();
		foreach (Light2D light2D in array)
		{
			if (light2D.lightType != Light2D.LightType.Global)
			{
				light2D.enabled = state;
			}
		}
	}

	private void ToggleRoomRenderers(bool state)
	{
		Debug.Log($"Is Renderer caller: Current State is {state}");
		GameObject gameObject = null;
		if (this is Enemy enemy && !(enemy is WhiteWomanEnemy))
		{
			if (enemy.CurrentState is SexModeState)
			{
				gameObject = base.gameObject;
			}
		}
		else if (this is Player player)
		{
			if (player.CurrentState is PlayerSexModeState)
			{
				gameObject = base.gameObject;
			}
		}
		else
		{
			gameObject = Elevator.GetTriggeredInstance().transform.parent.gameObject;
		}
		Renderer[] array = Object.FindObjectsOfType<Renderer>();
		foreach (Renderer renderer in array)
		{
			if ((!renderer.transform.IsChildOf(gameObject.transform) && renderer != spriteRenderer) | state)
			{
				renderer.enabled = state;
			}
		}
	}

	private void ToggleUIParty(bool state)
	{
		GameObject.Find("Canvas").transform.Find("HideUI/UI party").gameObject.SetActive(state);
		if ((this is Enemy enemy && enemy.CurrentState is SexModeState) || (this is Player player && player.CurrentState is PlayerSexModeState))
		{
			return;
		}
		Elevator triggeredInstance = Elevator.GetTriggeredInstance();
		if (triggeredInstance == null)
		{
			return;
		}
		Light2D[] array = Object.FindObjectsOfType<Light2D>();
		foreach (Light2D light2D in array)
		{
			if (light2D.lightType != Light2D.LightType.Global)
			{
				light2D.enabled = state;
			}
		}
		foreach (Transform item in triggeredInstance.transform)
		{
			Light2D component = item.GetComponent<Light2D>();
			if (component != null)
			{
				item.gameObject.SetActive(value: true);
				component.enabled = true;
			}
		}
		Transform transform2 = triggeredInstance.transform.parent?.Find("BlackBlock");
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(state);
		}
	}

	private static List<GameObject> ReconstructPath(Dictionary<GameObject, GameObject> previousRoom, GameObject startRoom, GameObject targetRoom)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject gameObject = targetRoom;
		while (gameObject != null && gameObject != startRoom)
		{
			list.Add(gameObject);
			gameObject = (previousRoom.ContainsKey(gameObject) ? previousRoom[gameObject] : null);
		}
		list.Reverse();
		return list;
	}

	protected virtual void OnDestroy()
	{
		if (lightPrefabInstance != null)
		{
			Object.Destroy(lightPrefabInstance);
		}
		if (maskPrefabInstance != null)
		{
			Object.Destroy(maskPrefabInstance);
		}
	}

	protected abstract float GetDirection();

	public virtual AudioClip GetMovementSound()
	{
		return null;
	}

	public void DisableSkipButton()
	{
		EndEvent.GetInstance().transform.parent.gameObject.SetActive(value: false);
	}

	public void EnableSkipButton()
	{
		EndEvent.GetInstance().UnblockSkip();
	}
}
