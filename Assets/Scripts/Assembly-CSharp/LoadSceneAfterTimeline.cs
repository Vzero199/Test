using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class LoadSceneAfterTimeline : MonoBehaviour
{
	[SerializeField]
	private string sceneToLoad = "Main Menu";

	private PlayableDirector playableDirector;

	private AsyncOperation sceneLoadOperation;

	private void Awake()
	{
		playableDirector = GetComponent<PlayableDirector>();
		if (playableDirector == null)
		{
			Debug.LogError("[LoadSceneAfterTimeline] PlayableDirector не найден!", this);
		}
	}

	private void Start()
	{
		StartCoroutine(LoadSceneAsync());
		if (playableDirector.state == PlayState.Playing)
		{
			playableDirector.stopped += OnTimelineFinished;
			return;
		}
		Debug.LogWarning("[LoadSceneAfterTimeline] PlayableDirector не запущен, загружаем сцену сразу.");
		ActivateScene();
	}

	private IEnumerator LoadSceneAsync()
	{
		sceneLoadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
		sceneLoadOperation.allowSceneActivation = false;
		yield return sceneLoadOperation;
	}

	private void OnTimelineFinished(PlayableDirector director)
	{
		ActivateScene();
	}

	private void ActivateScene()
	{
		if (sceneLoadOperation != null)
		{
			sceneLoadOperation.allowSceneActivation = true;
		}
		else
		{
			Debug.LogError("[LoadSceneAfterTimeline] Ошибка: сцена не была загружена!");
		}
	}

	private void OnDisable()
	{
		if (playableDirector != null)
		{
			playableDirector.stopped -= OnTimelineFinished;
		}
	}
}
