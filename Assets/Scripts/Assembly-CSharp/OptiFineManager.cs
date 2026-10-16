using System;
using UnityEngine;

public class OptiFineManager : MonoBehaviour
{
	public Camera mainCamera;

	public float _visibilityMultiplier = 50f;

	public float updateInterval = 0.5f;

	public int groupCount = 16;

	private float originalVisibilityMultiplier;

	private OptiFineGroup[] groups;

	private static OptiFineManager instance;

	public float VisibilityMultiplier
	{
		get
		{
			return _visibilityMultiplier;
		}
		set
		{
			_visibilityMultiplier = value;
			UpdateVisibilityMultiplierForGroups(_visibilityMultiplier);
			if (!(NightManager.GetInstance() != null) || NightManager.GetInstance().StopGameCoroutine == null)
			{
				ForceUpdateRenderersVisibility();
			}
		}
	}

	public static OptiFineManager GetInstance()
	{
		if (instance == null)
		{
			instance = UnityEngine.Object.FindObjectOfType<OptiFineManager>();
			if (instance == null)
			{
				return null;
			}
		}
		return instance;
	}

	private void Awake()
	{
		originalVisibilityMultiplier = _visibilityMultiplier;
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		groups = new OptiFineGroup[groupCount];
		for (int i = 0; i < groupCount; i++)
		{
			GameObject obj = new GameObject("OptiFineGroup" + i);
			obj.transform.parent = base.transform;
			OptiFineGroup optiFineGroup = obj.AddComponent<OptiFineGroup>();
			optiFineGroup.Initialize(i, groupCount, mainCamera, _visibilityMultiplier, updateInterval);
			groups[i] = optiFineGroup;
		}
	}

	private void Start()
	{
		GC.Collect();
	}

	public void ForceUpdateRenderersVisibility()
	{
		OptiFineGroup[] array = groups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ForceUpdateRenderersVisibility();
		}
	}

	public void UpdateVisibilityMultiplierForGroups(float newMultiplier)
	{
		OptiFineGroup[] array = groups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateVisibilityMultiplier(newMultiplier);
		}
	}
}
