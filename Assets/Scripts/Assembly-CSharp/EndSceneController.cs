using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndSceneController : MonoBehaviour
{
	private enum HoldAction
	{
		None = 0,
		Fuck1ToIdle2 = 1,
		IdleToFuck2 = 2,
		Fuck2ToIdle = 3,
		FinalConfirm = 4,
		Restart = 5
	}

	[Header("Animation And Progress")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private ProgressBar progressBar;

	[SerializeField]
	private float progressTime = 30f;

	[Header("UI")]
	[SerializeField]
	private CanvasGroup confirmGroup;

	[SerializeField]
	private Image leftConfirmIcon;

	[SerializeField]
	private Image rightConfirmIcon;

	[SerializeField]
	private float holdDuration = 1f;

	[SerializeField]
	private float fillDecaySpeed = 2f;

	private bool interactHint;

	private Coroutine progressCoroutine;

	private float startTime;

	private float postPauseDelay = 0.3f;

	private float lastUnpauseTime = -999f;

	private bool isLeftHolding;

	private float leftHoldTimer;

	private bool isRightHolding;

	private float rightHoldTimer;

	[Header("Effects")]
	[SerializeField]
	private CanvasGroup screenFader;

	[SerializeField]
	private float fadeDuration = 0.3f;

	[Header("Intro")]
	[SerializeField]
	private CanvasGroup introGroup;

	[SerializeField]
	private float introFadeDuration = 0.5f;

	[SerializeField]
	private float introDisplayTime = 5f;

	[Header("Switch Guide")]
	[SerializeField]
	private CanvasGroup switchGuideGroup;

	[SerializeField]
	private float switchGuideDelay = 10f;

	[SerializeField]
	private float switchGuideFadeDuration = 0.5f;

	[SerializeField]
	private TMP_Text guideText;

	[SerializeField]
	private CanvasGroup guidesGroup;

	[SerializeField]
	private GameObject rightClickHintObject;

	private Coroutine textFadeCoroutine;

	private bool firstConfirmStarted;

	private bool canConfirm;

	private Coroutine switchGuideCoroutine;

	private bool isIntroPlaying;

	private HoldAction pendingAction;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private RectTransform contentChild;

	[SerializeField]
	private Vector3 zoomedOutScale = new Vector3(0.5f, 0.5f, 1f);

	[SerializeField]
	private Vector3 defaultScale = Vector3.one;

	[SerializeField]
	private float zoomedOutXOffset = 200f;

	private Vector2 originalChildOffsetMin;

	private Vector2 originalChildOffsetMax;

	private bool childRectCached;

	private bool usingAltChildRect;

	private bool zoomedOut;

	public void OnZoomToggle(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			zoomedOut = !zoomedOut;
			content.localScale = (zoomedOut ? zoomedOutScale : defaultScale);
			LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
			if (zoomedOut)
			{
				SetContentOffset(zoomedOutXOffset);
			}
			else
			{
				SetContentOffset(0f);
			}
			scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	private void CacheChildRect()
	{
		if (!childRectCached && !(contentChild == null))
		{
			originalChildOffsetMin = contentChild.offsetMin;
			originalChildOffsetMax = contentChild.offsetMax;
			childRectCached = true;
		}
	}

	private void ApplyAltChildRect()
	{
		if (!usingAltChildRect && !(contentChild == null))
		{
			contentChild.offsetMin = new Vector2(-66f, -62f);
			contentChild.offsetMax = new Vector2(727f, 62f);
			usingAltChildRect = true;
		}
	}

	private void RestoreChildRect()
	{
		if (usingAltChildRect && !(contentChild == null))
		{
			contentChild.offsetMin = originalChildOffsetMin;
			contentChild.offsetMax = originalChildOffsetMax;
			usingAltChildRect = false;
		}
	}

	private void SetContentOffset(float xOffset)
	{
		Vector2 anchoredPosition = content.anchoredPosition;
		anchoredPosition.x = xOffset;
		content.anchoredPosition = anchoredPosition;
	}

	private void OnEnable()
	{
		switchGuideGroup.alpha = 0f;
		CacheChildRect();
		guideText.text = "";
		StartCoroutine(FadeText(guideText, GetLocalizedText("Start")));
		if (!PauseMenu.IsPaused)
		{
			lastUnpauseTime = Time.unscaledTime;
		}
		startTime = Time.time;
		ResetHold();
		SetConfirmGroupVisible(visible: false);
		StartCoroutine(PlayIntroUI());
	}

	private IEnumerator PlayIntroUI()
	{
		isIntroPlaying = true;
		introGroup.gameObject.SetActive(value: true);
		introGroup.blocksRaycasts = true;
		introGroup.interactable = true;
		yield return new WaitForSeconds(1.5f);
		float t = 0f;
		while (t < introFadeDuration)
		{
			t += Time.unscaledDeltaTime;
			introGroup.alpha = Mathf.Clamp01(t / introFadeDuration);
			yield return null;
		}
		introGroup.alpha = 1f;
		yield return new WaitForSecondsRealtime(introDisplayTime);
		t = introFadeDuration;
		while (t > 0f)
		{
			t -= Time.unscaledDeltaTime;
			introGroup.alpha = Mathf.Clamp01(t / introFadeDuration);
			yield return null;
		}
		introGroup.alpha = 0f;
		introGroup.blocksRaycasts = false;
		introGroup.interactable = false;
		introGroup.gameObject.SetActive(value: false);
		isIntroPlaying = false;
		StartCoroutine(ShowSwitchGuideWithDelay());
	}

	private IEnumerator ShowSwitchGuideWithDelay()
	{
		canConfirm = false;
		yield return new WaitForSecondsRealtime(switchGuideDelay);
		switchGuideGroup.gameObject.SetActive(value: true);
		switchGuideGroup.alpha = 0f;
		switchGuideGroup.blocksRaycasts = true;
		switchGuideGroup.interactable = true;
		float t = 0f;
		while (t < switchGuideFadeDuration)
		{
			t += Time.unscaledDeltaTime;
			switchGuideGroup.alpha = Mathf.Clamp01(t / switchGuideFadeDuration);
			yield return null;
		}
		switchGuideGroup.alpha = 1f;
		canConfirm = true;
	}

	public void OnLeftClick(InputAction.CallbackContext context)
	{
		if (!isIntroPlaying && canConfirm && base.gameObject.activeSelf && !PauseMenu.IsPaused && !(Time.unscaledTime - lastUnpauseTime < postPauseDelay))
		{
			if (context.started)
			{
				isLeftHolding = true;
			}
			else if (context.canceled)
			{
				isLeftHolding = false;
			}
		}
	}

	public void OnRightClick(InputAction.CallbackContext context)
	{
		if (!isIntroPlaying && canConfirm && firstConfirmStarted && base.gameObject.activeSelf && !PauseMenu.IsPaused && !(Time.unscaledTime - lastUnpauseTime < postPauseDelay))
		{
			if (context.started)
			{
				isRightHolding = true;
			}
			else if (context.canceled)
			{
				isRightHolding = false;
			}
		}
	}

	private void Update()
	{
		UpdateChildRectByState();
		if (!isIntroPlaying && canConfirm)
		{
			HandleLeftHold();
			HandleRightHold();
			UpdateConfirmGroupAlpha();
		}
	}

	private void UpdateChildRectByState()
	{
		if (GetCurrentAnimationState() == "SuccubusFuck1")
		{
			RestoreChildRect();
		}
		else
		{
			ApplyAltChildRect();
		}
	}

	private void HandleLeftHold()
	{
		if (isIntroPlaying || !canConfirm)
		{
			return;
		}
		string currentAnimationState = GetCurrentAnimationState();
		if (isLeftHolding && !PauseMenu.IsPaused && !isRightHolding)
		{
			if (currentAnimationState == "SuccubusFuck1")
			{
				pendingAction = HoldAction.Fuck1ToIdle2;
			}
			else if (currentAnimationState.StartsWith("SuccubusFuckIdle"))
			{
				pendingAction = HoldAction.IdleToFuck2;
			}
			else if (currentAnimationState.StartsWith("SuccubusFuck2"))
			{
				pendingAction = HoldAction.Fuck2ToIdle;
			}
			else if (currentAnimationState == "SuccubusIdle2Cum" && interactHint)
			{
				pendingAction = HoldAction.FinalConfirm;
			}
			else
			{
				pendingAction = HoldAction.None;
			}
			if (pendingAction != HoldAction.None)
			{
				leftHoldTimer += Time.unscaledDeltaTime;
				leftConfirmIcon.fillAmount = Mathf.Clamp01(leftHoldTimer / holdDuration);
				if (leftHoldTimer >= holdDuration)
				{
					ExecuteLeftAction(currentAnimationState);
					ResetLeftHold();
				}
			}
		}
		else
		{
			if (PauseMenu.IsPaused)
			{
				isLeftHolding = false;
			}
			leftHoldTimer -= Time.unscaledDeltaTime * fillDecaySpeed;
			if (leftHoldTimer < 0f)
			{
				leftHoldTimer = 0f;
			}
			leftConfirmIcon.fillAmount = Mathf.Clamp01(leftHoldTimer / holdDuration);
		}
	}

	private void HandleRightHold()
	{
		if (isIntroPlaying || !canConfirm || !firstConfirmStarted)
		{
			return;
		}
		if (!interactHint || GetCurrentAnimationState() != "SuccubusIdle2Cum")
		{
			rightHoldTimer -= Time.unscaledDeltaTime * fillDecaySpeed;
			if (rightHoldTimer < 0f)
			{
				rightHoldTimer = 0f;
			}
			return;
		}
		if (isRightHolding && !PauseMenu.IsPaused && !isLeftHolding)
		{
			rightHoldTimer += Time.unscaledDeltaTime;
			rightConfirmIcon.fillAmount = Mathf.Clamp01(rightHoldTimer / holdDuration);
			if (rightHoldTimer >= holdDuration)
			{
				RestartProcess();
				ResetRightHold();
			}
			return;
		}
		if (PauseMenu.IsPaused)
		{
			isRightHolding = false;
		}
		rightHoldTimer -= Time.unscaledDeltaTime * fillDecaySpeed;
		if (rightHoldTimer < 0f)
		{
			rightHoldTimer = 0f;
		}
		rightConfirmIcon.fillAmount = Mathf.Clamp01(rightHoldTimer / holdDuration);
	}

	private void ExecuteLeftAction(string currentState)
	{
		float progress = progressBar.GetProgress();
		switch (pendingAction)
		{
		case HoldAction.Fuck1ToIdle2:
			ShowGuideText("Start");
			break;
		case HoldAction.IdleToFuck2:
			ShowGuideText("SlowDown");
			break;
		case HoldAction.Fuck2ToIdle:
			ShowGuideText("SpeedUp");
			break;
		case HoldAction.FinalConfirm:
			ShowGuideText("Finish");
			break;
		}
		switch (pendingAction)
		{
		case HoldAction.Fuck1ToIdle2:
			StartCoroutine(FadeAndPlay("SuccubusFuck1", "SuccubusFuckIdle2"));
			progressBar.gameObject.SetActive(value: true);
			progressBar.ShowBar();
			progressBar.UpdateProgress(0f);
			break;
		case HoldAction.IdleToFuck2:
			if (progress < 0.33f)
			{
				animator.Play("SuccubusFuck2_1P");
			}
			else if (progress < 0.66f)
			{
				animator.Play("SuccubusFuck2_2P");
			}
			else
			{
				animator.Play("SuccubusFuck2_3P");
			}
			if (progressCoroutine != null)
			{
				StopCoroutine(progressCoroutine);
			}
			progressCoroutine = StartCoroutine(FillProgressBar());
			break;
		case HoldAction.Fuck2ToIdle:
			if (progress < 0.5f)
			{
				animator.Play("SuccubusFuckIdle2");
			}
			else
			{
				animator.Play("SuccubusFuckIdle1");
			}
			if (progressCoroutine != null)
			{
				StopCoroutine(progressCoroutine);
			}
			progressCoroutine = StartCoroutine(DrainProgressBar());
			break;
		case HoldAction.FinalConfirm:
			if (!firstConfirmStarted)
			{
				firstConfirmStarted = true;
				StartCoroutine(FadeCanvasGroup(guidesGroup, 1f, fadeDuration));
				rightClickHintObject.SetActive(value: true);
			}
			else
			{
				ConfirmEnd();
			}
			break;
		}
	}

	private string GetLocalizedText(string key)
	{
		string currentLanguage = GameSettings.CurrentLanguage;
		return key switch
		{
			"SpeedUp" => (currentLanguage == "JP") ? "速度を上げる" : ((!(currentLanguage == "CN")) ? "Speed Up" : "加速"), 
			"SlowDown" => (currentLanguage == "JP") ? "速度を落とす" : ((!(currentLanguage == "CN")) ? "Slow Down" : "减慢"), 
			"Finish" => (currentLanguage == "JP") ? "終了" : ((!(currentLanguage == "CN")) ? "Finish" : "结束"), 
			"Restart" => (currentLanguage == "JP") ? "シーンをリスタート" : ((!(currentLanguage == "CN")) ? "Restart the scene" : "重新开始本场景"), 
			"Start" => (currentLanguage == "JP") ? "スタート" : ((!(currentLanguage == "CN")) ? "Start" : "开始"), 
			_ => key, 
		};
	}

	private void ShowGuideText(string key)
	{
		if (textFadeCoroutine != null)
		{
			StopCoroutine(textFadeCoroutine);
		}
		textFadeCoroutine = StartCoroutine(FadeText(guideText, GetLocalizedText(key)));
	}

	private IEnumerator FadeAndPlay(string fromState, string toState)
	{
		string currentAnimationState = GetCurrentAnimationState();
		bool requiresFade = currentAnimationState == fromState || toState == fromState;
		if (requiresFade)
		{
			yield return StartCoroutine(FadeScreen(fadeIn: true));
		}
		animator.Play(toState);
		if (requiresFade)
		{
			yield return StartCoroutine(FadeScreen(fadeIn: false));
		}
	}

	private IEnumerator FadeScreen(bool fadeIn)
	{
		float time = 0f;
		float start = (fadeIn ? 0f : 1f);
		float end = (fadeIn ? 1f : 0f);
		screenFader.blocksRaycasts = true;
		while (time < fadeDuration)
		{
			time += Time.unscaledDeltaTime;
			float t = time / fadeDuration;
			screenFader.alpha = Mathf.Lerp(start, end, t);
			yield return null;
		}
		screenFader.alpha = end;
		screenFader.blocksRaycasts = fadeIn;
	}

	private void ConfirmEnd()
	{
		if (canConfirm)
		{
			Debug.Log("Продолжить");
			leftConfirmIcon.fillAmount = 0f;
			rightConfirmIcon.fillAmount = 0f;
			StartCoroutine(FadeCanvasGroup(guidesGroup, 0f, fadeDuration));
			if (GallerySceneViewerForSuccTE.isGalleryScene)
			{
				GallerySceneViewerForSuccTE.instance.ExitSexMode();
				RestartProcess();
			}
			else
			{
				StartCoroutine(LoadFinalSceneWithTransition());
			}
		}
	}

	private IEnumerator LoadFinalSceneWithTransition()
	{
		Time.timeScale = 0f;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("FinalTrueScene", LoadSceneMode.Single);
		asyncLoad.allowSceneActivation = false;
		GameObject gameObject = BlinkManager.GetInstance().gameObject;
		Image component = gameObject.GetComponent<Image>();
		if (component == null)
		{
			Debug.LogError("BlinkManager не содержит Image компонент.");
			yield break;
		}
		BlinkManager.GetInstance().enabled = false;
		gameObject.SetActive(value: true);
		yield return StartCoroutine(FadeImageAlpha(component, 0f, 1f, 1f));
		yield return new WaitForSecondsRealtime(3f);
		Time.timeScale = 1f;
		asyncLoad.allowSceneActivation = true;
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	private IEnumerator FadeImageAlpha(Image image, float from, float to, float duration)
	{
		float elapsed = 0f;
		Color color = image.color;
		while (elapsed < duration)
		{
			float a = Mathf.Lerp(from, to, elapsed / duration);
			image.color = new Color(color.r, color.g, color.b, a);
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		image.color = new Color(color.r, color.g, color.b, to);
	}

	private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
	{
		group.gameObject.SetActive(value: true);
		float startAlpha = group.alpha;
		float time = 0f;
		while (time < duration)
		{
			time += Time.unscaledDeltaTime;
			group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
			yield return null;
		}
		group.alpha = targetAlpha;
		group.blocksRaycasts = targetAlpha > 0f;
		group.interactable = targetAlpha > 0f;
		if (targetAlpha == 0f)
		{
			group.gameObject.SetActive(value: false);
		}
	}

	private void ResetLeftHold()
	{
		isLeftHolding = false;
		leftHoldTimer = 0f;
		leftConfirmIcon.fillAmount = 0f;
		rightConfirmIcon.fillAmount = 0f;
		pendingAction = HoldAction.None;
	}

	private void ResetRightHold()
	{
		isRightHolding = false;
		rightHoldTimer = 0f;
		leftConfirmIcon.fillAmount = 0f;
		rightConfirmIcon.fillAmount = 0f;
	}

	private void ResetHold()
	{
		ResetLeftHold();
		ResetRightHold();
	}

	public void NotifyUnpaused()
	{
		lastUnpauseTime = Time.unscaledTime;
	}

	private string GetCurrentAnimationState()
	{
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusFuckIdle2"))
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusFuck1"))
			{
				if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusFuckIdle1"))
				{
					if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusFuck2_1P"))
					{
						if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusFuck2_2P"))
						{
							if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusFuck2_3P"))
							{
								if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusClimax"))
								{
									if (!animator.GetCurrentAnimatorStateInfo(0).IsName("SuccubusIdle2Cum"))
									{
										return "unknown";
									}
									return "SuccubusIdle2Cum";
								}
								return "SuccubusClimax";
							}
							return "SuccubusFuck2_3P";
						}
						return "SuccubusFuck2_2P";
					}
					return "SuccubusFuck2_1P";
				}
				return "SuccubusFuckIdle1";
			}
			return "SuccubusFuck1";
		}
		return "SuccubusFuckIdle2";
	}

	private void UpdateConfirmGroupAlpha()
	{
		float num = Mathf.Clamp01(Mathf.Max(leftHoldTimer, rightHoldTimer) / holdDuration);
		confirmGroup.alpha = Mathf.Clamp01(num * 4f);
		confirmGroup.interactable = confirmGroup.alpha > 0f;
		confirmGroup.blocksRaycasts = confirmGroup.alpha > 0f;
	}

	private IEnumerator FillProgressBar()
	{
		string currentAnimation = "";
		while (progressBar.GetProgress() < 1f)
		{
			if (PauseMenu.IsPaused)
			{
				yield return null;
				continue;
			}
			progressBar.UpdateProgress(progressBar.GetProgress() + Time.unscaledDeltaTime / progressTime);
			float progress = progressBar.GetProgress();
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				string text = ((progress < 0.33f) ? "SuccubusFuck2_1P" : ((!(progress < 0.66f)) ? "SuccubusFuck2_3P" : "SuccubusFuck2_2P"));
				if (text != currentAnimation)
				{
					animator.Play(text);
					currentAnimation = text;
				}
			}
			yield return null;
		}
		if (progressBar.GetProgress() >= 1f)
		{
			animator.Play("SuccubusClimax");
			interactHint = true;
			StartCoroutine(FadeOutBar());
			StartCoroutine(DelayBeforeConfirm());
		}
	}

	private IEnumerator DelayBeforeConfirm()
	{
		yield return new WaitForSecondsRealtime(1f);
		ShowGuideText("Continue");
		guidesGroup.alpha = 1f;
		guidesGroup.gameObject.SetActive(value: true);
	}

	private IEnumerator DrainProgressBar()
	{
		string currentAnimation = "";
		while (progressBar.GetProgress() > 0f)
		{
			if (PauseMenu.IsPaused)
			{
				yield return null;
				continue;
			}
			progressBar.UpdateProgress(progressBar.GetProgress() - Time.unscaledDeltaTime / progressTime);
			float progress = progressBar.GetProgress();
			if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
			{
				string text = ((!(progress < 0.5f)) ? "SuccubusFuckIdle1" : "SuccubusFuckIdle2");
				if (text != currentAnimation)
				{
					animator.Play(text);
					currentAnimation = text;
				}
			}
			yield return null;
		}
	}

	private IEnumerator FadeOutBar()
	{
		yield return progressBar.FadeOutBar();
		progressBar.UpdateProgress(0f);
	}

	private IEnumerator FadeOutUI(GameObject uiObject)
	{
		interactHint = false;
		CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
		float elapsedTime = 1f;
		while (elapsedTime > 0f)
		{
			elapsedTime -= Time.unscaledDeltaTime;
			canvasGroup.alpha = Mathf.Clamp01(elapsedTime);
			yield return null;
		}
		uiObject.SetActive(value: false);
	}

	private void SetConfirmGroupVisible(bool visible)
	{
		confirmGroup.alpha = (visible ? 1f : 0f);
		confirmGroup.blocksRaycasts = visible;
		confirmGroup.interactable = visible;
	}

	private void RestartProcess()
	{
		firstConfirmStarted = false;
		if (switchGuideCoroutine != null)
		{
			StopCoroutine(switchGuideCoroutine);
		}
		if (textFadeCoroutine != null)
		{
			StopCoroutine(textFadeCoroutine);
		}
		textFadeCoroutine = StartCoroutine(FadeText(guideText, GetLocalizedText("Start")));
		rightClickHintObject.SetActive(value: false);
		switchGuideGroup.alpha = 0f;
		canConfirm = false;
		switchGuideCoroutine = StartCoroutine(ShowSwitchGuideWithDelay());
		StartCoroutine(FadeAndPlay(GetCurrentAnimationState(), "SuccubusFuck1"));
		progressBar.UpdateProgress(0f);
		ResetHold();
	}

	private IEnumerator FadeText(TMP_Text textComponent, string newText, float duration = 0.3f)
	{
		Color originalColor = textComponent.color;
		float t = 0f;
		while (t < duration)
		{
			t += Time.unscaledDeltaTime;
			float a = Mathf.Lerp(1f, 0f, t / duration);
			textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, a);
			yield return null;
		}
		textComponent.text = newText;
		t = 0f;
		while (t < duration)
		{
			t += Time.unscaledDeltaTime;
			float a2 = Mathf.Lerp(0f, 1f, t / duration);
			textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, a2);
			yield return null;
		}
		textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
	}
}
