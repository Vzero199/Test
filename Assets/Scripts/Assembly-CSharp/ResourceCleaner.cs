using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResourceCleaner : MonoBehaviour
{
	private static ResourceCleaner instance;

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		StartCoroutine(CleanupResources());
	}

	private IEnumerator CleanupResources()
	{
		yield return Resources.UnloadUnusedAssets();
	}
}
