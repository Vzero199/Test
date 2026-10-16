using UnityEngine;
using UnityEngine.UI;

public class ProgressBasedActivator : MonoBehaviour
{
	[Tooltip("Ключ прогресса из GlobalSaveKeys, который требуется проверить")]
	[SerializeField]
	private string requiredProgressKey;

	[Tooltip("Инвертировать условие (отключать объект если прогресс ЕСТЬ)")]
	[SerializeField]
	private bool invertCondition;

	[Tooltip("Отключить объект полностью (SetActive(false)) или просто сделать его неинтерактивным")]
	[SerializeField]
	private bool deactivateCompletely = true;

	private void Start()
	{
		CheckProgress();
	}

	private void CheckProgress()
	{
		bool progress = GlobalSaveSystem.GetProgress(requiredProgressKey);
		bool flag = (invertCondition ? (!progress) : progress);
		if (deactivateCompletely)
		{
			base.gameObject.SetActive(flag);
			return;
		}
		Selectable component = GetComponent<Selectable>();
		if (component != null)
		{
			component.interactable = flag;
			return;
		}
		Collider component2 = GetComponent<Collider>();
		if (component2 != null)
		{
			component2.enabled = flag;
		}
		Renderer component3 = GetComponent<Renderer>();
		if (component3 != null)
		{
			component3.enabled = flag;
		}
	}
}
