using SmallHedge.SoundManager;
using UnityEngine;

public class DayFuckEvents : MonoBehaviour
{
	protected string currentWWpath = "prefabs/WierdWordEffects/WWFuckStandart";

	protected string previousWWpath;

	protected int counterForBreathe;

	protected int counterForWierdWord;

	protected virtual string BEpath => "";

	protected virtual string WWpath => currentWWpath;

	private void PlaySoundSafe(SoundType type)
	{
		if (Application.isPlaying)
		{
			SoundManager.PlaySound(type);
		}
	}

	public void Hloop1n2Sound()
	{
		PlaySoundSafe(SoundType.Hloop1n2);
	}

	public void Hloop3Sound()
	{
		PlaySoundSafe(SoundType.Hloop3);
	}

	public void ClimaxSound()
	{
		PlaySoundSafe(SoundType.HClimax);
	}

	public void HJobloop1Sound()
	{
		SoundManager.PlaySound(SoundType.HJLoop);
	}

	public void HLickloopSound()
	{
		SoundManager.PlaySound(SoundType.LickLoop);
	}

	public virtual void CreateWierdWordEffect()
	{
		if (Application.isPlaying && !string.IsNullOrEmpty(WWpath))
		{
			if (counterForWierdWord <= 0)
			{
				Vector3 vector = Random.insideUnitSphere.normalized * 50f;
				Vector3 value = base.transform.position + vector;
				CreateEffect(WWpath, value);
				counterForWierdWord = Random.Range(1, 4);
			}
			else
			{
				counterForWierdWord--;
			}
		}
	}

	private void CreateEffect(string path, Vector3? position = null)
	{
		GameObject gameObject = Resources.Load<GameObject>(path);
		if (gameObject != null)
		{
			GameObject obj = Object.Instantiate(gameObject, position ?? base.transform.position, Quaternion.identity, base.transform);
			obj.transform.localRotation = Quaternion.identity;
			obj.name = gameObject.name;
		}
		else
		{
			Debug.LogError("Не удалось загрузить эффект по пути: " + path);
		}
	}
}
