using System;
using System.Collections;
using System.Linq;
using SmallHedge.SoundManager;
using UnityEngine;

[RequireComponent(typeof(elevatorChoicesText))]
public class Elevator : InteractionObject
{
	[Header("Ink JSON")]
	public TextAsset inkJSON;

	[Header("Emote Animator")]
	[SerializeField]
	private Animator emoteAnimator;

	[Header("Elevator Settings")]
	[SerializeField]
	private float openDuration = 15f;

	private Coroutine closeElevatorCoroutine;

	private Coroutine playCloseAnimationCoroutine;

	public bool nintendoInElevator;

	private Animator animator;

	private bool isInOpenMode = true;

	private GameObject lockElevatorUI;

	private SpriteRenderer LockElevatorUIRenderer;

	private Coroutine fadeCoroutine;

	private bool isInteractable = true;

	private bool isInElevatorChoice;

	private bool playerInChoice;

	private static bool triggerFlag;

	private static Elevator triggerElevetorInstance;

	public Animator Animator
	{
		get
		{
			return animator;
		}
		set
		{
			animator = value;
		}
	}

	public GameObject LockElevatorUI => lockElevatorUI;

	public bool IsInElevatorChoice
	{
		get
		{
			return isInElevatorChoice;
		}
		set
		{
			isInElevatorChoice = value;
			triggerFlag = value;
			triggerElevetorInstance = this;
			StartCoroutine(resetTrigger());
		}
	}

	public static bool IsLockUIVisible()
	{
		if (triggerElevetorInstance == null)
		{
			return false;
		}
		return triggerElevetorInstance.LockElevatorUIRenderer.color.a > 0f;
	}

	public void UpdateLockUIVisibility(bool isShow)
	{
		if (lockElevatorUI == null)
		{
			Debug.LogWarning("Elevator Lock not set");
			return;
		}
		foreach (Elevator item in UnityEngine.Object.FindObjectsOfType<Elevator>().ToList())
		{
			item.FadeToAlpha(isShow ? 0.62f : 0f);
		}
	}

	private void FadeToAlpha(float targetAlpha)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
	}

	private IEnumerator FadeCoroutine(float targetAlpha)
	{
		float startAlpha = LockElevatorUIRenderer.color.a;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			float t2 = Mathf.SmoothStep(0f, 1f, t);
			float alpha = Mathf.Lerp(startAlpha, targetAlpha, t2);
			SetAlpha(alpha);
			yield return null;
		}
		SetAlpha(targetAlpha);
	}

	private void SetAlpha(float alpha)
	{
		Color color = LockElevatorUIRenderer.color;
		color.a = alpha;
		LockElevatorUIRenderer.color = color;
	}

	public void IsClose()
	{
		if (playerInChoice && DialogueManager.GetInstance().dialogueIsPlaying)
		{
			StartCoroutine(DialogueManager.GetInstance().ExitDialogueMode());
		}
		playerInChoice = false;
		isInOpenMode = false;
	}

	public void IsOpen()
	{
		isInOpenMode = true;
	}

	private IEnumerator resetTrigger()
	{
		yield return new WaitForSeconds(1f);
		ClearTrigger();
	}

	public static void ClearTrigger()
	{
		triggerFlag = false;
	}

	public static bool GetTriggered()
	{
		return triggerFlag;
	}

	public static Elevator GetTriggeredInstance()
	{
		return triggerElevetorInstance;
	}

	protected override void Start()
	{
		base.Start();
		triggerElevetorInstance = null;
		lockElevatorUI = base.transform.Find("LockElevatorUI").gameObject;
		if (lockElevatorUI != null)
		{
			LockElevatorUIRenderer = lockElevatorUI.GetComponent<SpriteRenderer>();
		}
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Animator not set!");
		}
	}

	private void OnDestroy()
	{
		triggerElevetorInstance = null;
	}

	public override bool CheckConditions()
	{
		if (isInteractable && Player.GetInstance().InteractionElevatorTimer == 0f)
		{
			return base.CheckConditions();
		}
		return false;
	}

	protected override bool CheckInteractButton()
	{
		if (player.IsActionButton)
		{
			return CheckConditions();
		}
		return false;
	}

	protected override void InteractComplete()
	{
		base.InteractComplete();
		if (interactableObject != null && interactableObject.GetComponent<Nintendo>() != null)
		{
			if (!isInOpenMode)
			{
				StartCoroutine(EnterElevator(isInOpenMode));
			}
			else
			{
				StartCoroutine(EnterElevator(isInOpenMode));
			}
		}
		else if (!isInOpenMode)
		{
			isInteractable = false;
			animator.Play("Opened");
			if (GetComponent<WhiteWomanEnemy>() != null)
			{
				SoundManager.PlaySoundWithDelay(SoundType.StaminaEnds, 0.5f);
			}
			if (closeElevatorCoroutine != null)
			{
				StopCoroutine(closeElevatorCoroutine);
			}
			closeElevatorCoroutine = StartCoroutine(CloseElevatorAfterDelay(animator.GetCurrentAnimatorStateInfo(0).length));
		}
		else
		{
			DialogueManager.GetInstance().EnterDialogueMode(inkJSON, emoteAnimator);
			Player.GetInstance().IsPaused = false;
			playerInChoice = true;
		}
	}

	private IEnumerator EnterElevator(bool isInOpenMode)
	{
		nintendoInElevator = true;
		if (!isInOpenMode)
		{
			animator.Play("Opened");
		}
		Nintendo nintendoScript = interactableObject.GetComponent<Nintendo>();
		nintendoScript.CanInteract = false;
		isInteractable = false;
		if (closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
		}
		closeElevatorCoroutine = StartCoroutine(CloseElevatorAfterDelay(animator.GetCurrentAnimatorStateInfo(0).length));
		yield return new WaitForSeconds(0.4f);
		float moveDuration = 1.5f;
		Vector3 startPosition = nintendoScript.transform.position;
		Vector3 targetPosition = new Vector3(base.transform.position.x + 110f, startPosition.y, startPosition.z);
		nintendoScript.GetComponent<Animator>().Play("Run");
		float elapsedTime = 0f;
		while (elapsedTime < moveDuration && !nintendoScript.EventTriggered)
		{
			nintendoScript.ChaseDirection = 0f;
			nintendoScript.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		nintendoScript.GetComponent<Animator>().Play("Idle");
		if (closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
		}
		closeElevatorCoroutine = StartCoroutine(CloseElevatorAfterDelay(animator.GetCurrentAnimatorStateInfo(0).length));
		if (!nintendoScript.EventTriggered)
		{
			yield return new WaitForSeconds(0.6f);
		}
		elapsedTime = 1f;
		targetPosition = nintendoScript.transform.position;
		startPosition = new Vector3(targetPosition.x, targetPosition.y + 60f, targetPosition.z);
		nintendoScript.GetComponent<Animator>().Play("Run");
		moveDuration = 0f;
		nintendoScript.GetComponent<SpriteRenderer>().sortingLayerName = "TV";
		while (moveDuration < elapsedTime && !nintendoScript.EventTriggered)
		{
			nintendoScript.ChaseDirection = 0f;
			nintendoScript.transform.position = Vector3.Lerp(targetPosition, startPosition, moveDuration / elapsedTime);
			moveDuration += Time.deltaTime;
			yield return null;
		}
		if (closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
		}
		closeElevatorCoroutine = StartCoroutine(CloseElevatorAfterDelay(animator.GetCurrentAnimatorStateInfo(0).length));
		nintendoScript.GetComponent<Animator>().Play("Idle");
		if (!nintendoScript.EventTriggered)
		{
			yield return new WaitForSeconds(0.6f);
		}
		if (closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
		}
		closeElevatorCoroutine = StartCoroutine(CloseElevatorAfterDelay(animator.GetCurrentAnimatorStateInfo(0).length));
		if (closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
		}
		if (Vector3.Distance(Player.GetInstance().transform.position, nintendoScript.transform.position) <= 900f)
		{
			moveDuration = 0f;
			elapsedTime = 5f;
			while (moveDuration < elapsedTime && !DialogueManager.GetInstance().dialogueIsPlaying && !nintendoScript.EventTriggered)
			{
				moveDuration += Time.deltaTime;
				nintendoScript.ChaseDirection = 0f;
				yield return null;
			}
		}
		closeElevatorCoroutine = StartCoroutine(CloseElevatorAfterDelay(animator.GetCurrentAnimatorStateInfo(0).length));
		while (DialogueManager.GetInstance().dialogueIsPlaying && !nintendoScript.EventTriggered)
		{
			nintendoScript.ChaseDirection = 0f;
			yield return null;
		}
		if (!nintendoScript.EventTriggered)
		{
			while (WhiteWomanEnemy.isLoseEvent || WhiteWomanEnemy.eventTriggered || WhiteWomanEnemy.isForceOpenDoor)
			{
				yield return new WaitForSeconds(1f);
			}
			animator.Play("Closed");
			IsClose();
		}
		if (!nintendoScript.EventTriggered)
		{
			yield return new WaitForSeconds(1f);
		}
		nintendoInElevator = false;
		if ((bool)nintendoScript && closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
		}
		while (nintendoScript.EventTriggered)
		{
			nintendoScript.ChaseDirection = 0f;
			yield return null;
		}
		StartCoroutine(GoNintendoToFloor(nintendoScript, nintendoScript.GetComponent<Nintendo>().currentRoomForObjective));
	}

	private IEnumerator CloseElevatorAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		isInteractable = true;
		yield return new WaitForSeconds(openDuration);
		while (WhiteWomanEnemy.isLoseEvent || WhiteWomanEnemy.eventTriggered || WhiteWomanEnemy.isForceOpenDoor)
		{
			yield return new WaitForSeconds(1f);
		}
		animator.Play("Closed");
		IsClose();
	}

	private IEnumerator GoNintendoToFloor(Nintendo nintendoScript, string ElevatorStageName)
	{
		if (ElevatorStageName == null)
		{
			Debug.LogError("Stage Name in NULL");
			yield break;
		}
		Transform transform = GameObject.Find(ElevatorStageName).transform;
		if (transform == null)
		{
			Debug.LogError("ElevatorStageObject in NULL");
			yield break;
		}
		Vector3 vector = CalculateRoomBounds(transform.gameObject);
		Collider[] array = Physics.OverlapBox(transform.position, vector / 2f);
		Elevator elevator = null;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if ((bool)collider.GetComponent<Elevator>())
			{
				elevator = collider.GetComponent<Elevator>();
				break;
			}
		}
		if (elevator == null)
		{
			Debug.LogError("Elevator Not Found");
			yield break;
		}
		nintendoScript.transform.position = new Vector3(elevator.transform.position.x + 70f, elevator.transform.position.y - 100f, 0f);
		nintendoScript.GetComponent<SpriteRenderer>().sortingLayerName = "TV";
		elevator.GetComponent<Animator>().Play("Opened");
		nintendoScript.GetComponent<Animator>().Play("Idle");
		float elapsedTime = 0f;
		float duration = 1.5f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			nintendoScript.ChaseDirection = 0f;
			yield return null;
		}
		duration = 1f;
		Vector3 startPosition = nintendoScript.transform.position;
		Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y - 60f, startPosition.z);
		nintendoScript.GetComponent<Animator>().Play("Run");
		elapsedTime = 0f;
		nintendoScript.GetComponent<SpriteRenderer>().sortingLayerName = "Entity";
		while (elapsedTime < duration)
		{
			nintendoScript.ChaseDirection = 0f;
			nintendoScript.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		nintendoScript.transform.position = targetPosition;
		nintendoScript.GetComponent<Animator>().Play("Idle");
		elapsedTime = 0f;
		duration = 1f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			nintendoScript.ChaseDirection = 0f;
			yield return null;
		}
		nintendoScript.ChangeState(new NintendoChaseState());
		nintendoScript.InitializeCurrentRoom();
		nintendoScript.SetTargetRace(nintendoScript.MainGoal);
		nintendoScript.CanInteract = true;
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

	public void GoToFloor(string ElevatorFloorName, Action action = null)
	{
		GameObject gameObject = GameObject.Find(ElevatorFloorName);
		interactableObject = Player.GetInstance().gameObject;
		SoundManager.PlaySound(SoundType.ElevatorButton);
		BlinkManager blinkManager = UnityEngine.Object.FindObjectOfType<BlinkManager>();
		if (blinkManager != null)
		{
			blinkManager.FadeToBlink(interactableObject, () =>
			{
				action?.Invoke();
				UpdateLockUIVisibility(isShow: true);
			}, allowVerticalMovement: true, 0f, 0f, gameObject);
			Animator component = gameObject.GetComponent<Animator>();
			component.Play("Opened");
			if (playCloseAnimationCoroutine != null)
			{
				StopCoroutine(playCloseAnimationCoroutine);
			}
			playCloseAnimationCoroutine = StartCoroutine(PlayCloseAnimationWithDelay(component, 0.5f));
		}
		else
		{
			Debug.LogError("BlinkManager not found in the scene.");
		}
	}

	private IEnumerator PlayCloseAnimationWithDelay(Animator gotoObjectAnimator, float delay)
	{
		yield return new WaitForSeconds(delay);
		while (WhiteWomanEnemy.isLoseEvent || WhiteWomanEnemy.eventTriggered || WhiteWomanEnemy.isForceOpenDoor)
		{
			yield return new WaitForSeconds(1f);
		}
		gotoObjectAnimator.Play("Closed");
	}

	public void StopElevatorCoroutines()
	{
		if (closeElevatorCoroutine != null)
		{
			StopCoroutine(closeElevatorCoroutine);
			isInteractable = true;
			Player.GetInstance().ManualTriggerCheck();
			Player.GetInstance().HandlePlayerInteractUI();
			Player.GetInstance().UpdateSelectedObjectUI();
			closeElevatorCoroutine = null;
		}
		if (playCloseAnimationCoroutine != null)
		{
			StopCoroutine(playCloseAnimationCoroutine);
			playCloseAnimationCoroutine = null;
		}
	}
}
