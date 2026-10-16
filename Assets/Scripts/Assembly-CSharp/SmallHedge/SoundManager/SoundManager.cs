using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.SoundManager
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundManager : MonoBehaviour
	{
		[SerializeField]
		private SoundsSO SO;

		[SerializeField]
		private AudioMixer audioMixer;

		private static SoundManager instance = null;

		private AudioSource audioSource;

		private static readonly string[] VolumeParameters = new string[4] { "Master_Volume", "BGM_Volume", "SFX_Volume", "UI_Volume" };

		public AudioMixer GetAudioMixer()
		{
			return audioMixer;
		}

		public static SoundManager GetInstance()
		{
			return instance;
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				audioSource = GetComponent<AudioSource>();
				Object.DontDestroyOnLoad(base.gameObject);
				Debug.Log("[SoundManager] Initializing and loading volume settings from PlayerPrefs");
				string[] volumeParameters = VolumeParameters;
				foreach (string text in volumeParameters)
				{
					float num = PlayerPrefs.GetFloat(text, 1f);
					Debug.Log($"[SoundManager] Loading {text} from PlayerPrefs: {num} ({num * 100f:F1}%)");
					float num2 = ((num > 0.0001f) ? (Mathf.Log10(num) * 20f) : (-80f));
					instance.audioMixer.SetFloat(text, num2);
					Debug.Log($"[SoundManager] Applied to mixer - {text}: Linear={num}, dB={num2}");
				}
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}

		public static AudioMixerGroup GetSFXGroup()
		{
			if (instance == null || instance.audioMixer == null)
			{
				Debug.LogWarning("GetSFXGroup: SoundManager или AudioMixer не инициализирован.");
				return null;
			}
			AudioMixerGroup[] array = instance.audioMixer.FindMatchingGroups("Master/SFX");
			if (array.Length != 0)
			{
				return array[0];
			}
			Debug.LogError("GetSFXGroup: Не найдена группа 'Master/SFX' в AudioMixer.");
			return null;
		}

		public static AudioMixerGroup GetUIGroup()
		{
			if (instance == null || instance.audioMixer == null)
			{
				Debug.LogWarning("GetUIGroup: SoundManager или AudioMixer не инициализирован.");
				return null;
			}
			AudioMixerGroup[] array = instance.audioMixer.FindMatchingGroups("Master/UI");
			if (array.Length != 0)
			{
				return array[0];
			}
			Debug.LogError("GetSFXGroup: Не найдена группа 'Master/SFX' в AudioMixer.");
			return null;
		}

		public static IEnumerator FadeOutSound(AudioSource source, float fadeDuration, float targetVolume = 0.5f)
		{
			if (source == null)
			{
				yield break;
			}
			float startVolume = source.volume;
			float elapsedTime = 0f;
			while (elapsedTime < fadeDuration)
			{
				if (source == null)
				{
					yield break;
				}
				elapsedTime += Time.deltaTime;
				source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
				yield return null;
			}
			if (source != null)
			{
				source.volume = targetVolume;
			}
		}

		public static void SetLoopingSoundVolume(AudioSource source, float volume)
		{
			if (source != null)
			{
				source.volume = volume;
			}
		}

		public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1f)
		{
			if (instance == null || instance.SO == null || instance.SO.sounds == null)
			{
				Debug.LogWarning($"PlaySound: SoundManager или его ScriptableObject отсутствует. Запрашиваемый звук: {sound}");
				return;
			}
			if (sound < SoundType.LampAmbince || (int)sound >= instance.SO.sounds.Length)
			{
				Debug.LogError($"PlaySound: Недопустимый индекс SoundType ({sound})");
				return;
			}
			SoundList soundList = instance.SO.sounds[(int)sound];
			if (soundList.sounds == null || soundList.sounds.Length == 0)
			{
				Debug.LogError($"PlaySound: Указанный SoundType ({sound}) не содержит звуковых клипов!");
				return;
			}
			AudioClip[] sounds = soundList.sounds;
			AudioClip clip = sounds[Random.Range(0, sounds.Length)];
			if (source != null)
			{
				if (source.gameObject == null)
				{
					Debug.LogWarning($"PlaySound: AudioSource был уничтожен или отсутствует для звука {sound}");
					return;
				}
				source.outputAudioMixerGroup = soundList.mixer;
				source.clip = clip;
				source.volume = volume * soundList.volume;
				source.Play();
			}
			else if (instance.audioSource == null)
			{
				Debug.LogWarning($"PlaySound: Instance SoundManager или его AudioSource отсутствует для звука {sound}");
			}
			else
			{
				instance.audioSource.outputAudioMixerGroup = soundList.mixer;
				instance.audioSource.PlayOneShot(clip, volume * soundList.volume);
			}
		}

		public static void PlaySoundWithDelay(SoundType sound, float delay, AudioSource source = null, float volume = 1f)
		{
			if (instance == null)
			{
				Debug.LogWarning("PlaySoundWithDelay: SoundManager не инициализирован!");
			}
			else
			{
				instance.StartCoroutine(instance.PlaySoundCoroutine(sound, delay, source, volume));
			}
		}

		private IEnumerator PlaySoundCoroutine(SoundType sound, float delay, AudioSource source, float volume)
		{
			yield return new WaitForSeconds(delay);
			PlaySound(sound, source, volume);
		}

		public static AudioSource PlayLoopingSound(SoundType sound, float volume = 1f)
		{
			SoundList soundList = instance.SO.sounds[(int)sound];
			AudioClip[] sounds = soundList.sounds;
			AudioClip clip = sounds[Random.Range(0, sounds.Length)];
			AudioSource obj = new GameObject("LoopingSound_" + sound).AddComponent<AudioSource>();
			obj.clip = clip;
			obj.volume = volume * soundList.volume;
			obj.outputAudioMixerGroup = soundList.mixer;
			obj.loop = true;
			obj.Play();
			return obj;
		}

		public static void StopLoopingSound(AudioSource source)
		{
			if (source != null)
			{
				source.Stop();
				Object.Destroy(source.gameObject);
			}
		}

		public static GameObject PlaySoundAtPosition(SoundType sound, Vector3 position, float volume = 1f, float minDistance = 200f, float maxDistance = 1000f)
		{
			SoundList soundList = instance.SO.sounds[(int)sound];
			AudioClip[] sounds = soundList.sounds;
			AudioClip audioClip = sounds[Random.Range(0, sounds.Length)];
			GameObject gameObject = new GameObject("Sound_" + sound);
			AudioSource obj = gameObject.AddComponent<AudioSource>();
			obj.clip = audioClip;
			obj.volume = volume * soundList.volume;
			obj.spatialBlend = 1f;
			obj.outputAudioMixerGroup = soundList.mixer;
			obj.dopplerLevel = 0f;
			obj.minDistance = minDistance;
			obj.maxDistance = maxDistance;
			obj.rolloffMode = AudioRolloffMode.Linear;
			gameObject.transform.position = position;
			obj.Play();
			Object.Destroy(gameObject, audioClip.length);
			return gameObject;
		}

		public static AudioSource PlaySoundAtObject(SoundType sound, GameObject targetObject, float volume = 1f, float minDistance = 100f, float maxDistance = 1000f)
		{
			if (targetObject == null)
			{
				Debug.LogWarning("PlaySoundAtObject: Переданный объект равен null!");
				return null;
			}
			Vector3 position = targetObject.transform.position;
			BoxCollider component = targetObject.GetComponent<BoxCollider>();
			if (component != null)
			{
				position = component.bounds.center;
			}
			return PlaySoundAtPosition(sound, position, volume, minDistance, maxDistance).GetComponent<AudioSource>();
		}

		public static void SetVolume(string parameter, float volumePercent, bool saveToPrefs = true)
		{
			if (instance == null || instance.audioMixer == null)
			{
				Debug.LogWarning("SoundManager или AudioMixer не инициализирован.");
				return;
			}
			float num = Mathf.Clamp(volumePercent / 100f, 0.0001f, 1f);
			float num2 = 20f * Mathf.Log10(num);
			Debug.Log("[SoundManager] Setting volume - Parameter: " + parameter + ", " + $"Input Percent: {volumePercent}, " + $"Linear: {num}, " + $"dB: {num2}");
			instance.audioMixer.SetFloat(parameter, num2);
			if (saveToPrefs)
			{
				PlayerPrefs.SetFloat(parameter, num);
				PlayerPrefs.Save();
				Debug.Log($"[SoundManager] Saved to PlayerPrefs - Key: {parameter}, Value: {num}");
			}
		}

		public static float GetVolume(string parameter)
		{
			if (instance == null)
			{
				Debug.LogWarning("SoundManager не инициализирован.");
				return 1f;
			}
			float num = PlayerPrefs.GetFloat(parameter, 1f);
			float num2 = num * 100f;
			Debug.Log("[SoundManager] Getting volume from PlayerPrefs - Parameter: " + parameter + ", " + $"Linear: {num}, " + $"Percent: {num2}%");
			return num;
		}

		public static AudioClip GetRandomClip(SoundType sound)
		{
			if (instance == null || instance.SO == null || instance.SO.sounds == null)
			{
				return null;
			}
			if (sound < SoundType.LampAmbince || (int)sound >= instance.SO.sounds.Length)
			{
				return null;
			}
			AudioClip[] sounds = instance.SO.sounds[(int)sound].sounds;
			if (sounds == null || sounds.Length == 0)
			{
				return null;
			}
			return sounds[Random.Range(0, sounds.Length)];
		}

		public static SoundList GetSoundList(SoundType sound)
		{
			if (instance == null || instance.SO == null || instance.SO.sounds == null)
			{
				Debug.LogWarning("GetSoundList: SoundManager не инициализирован.");
				return default;
			}
			if (sound < SoundType.LampAmbince || (int)sound >= instance.SO.sounds.Length)
			{
				Debug.LogError($"GetSoundList: Недопустимый индекс SoundType ({sound})");
				return default;
			}
			return instance.SO.sounds[(int)sound];
		}

		public static AudioClip GetClip(SoundType sound)
		{
			if (instance == null || instance.SO == null || instance.SO.sounds == null)
			{
				return null;
			}
			if (sound < SoundType.LampAmbince || (int)sound >= instance.SO.sounds.Length)
			{
				return null;
			}
			AudioClip[] sounds = instance.SO.sounds[(int)sound].sounds;
			if (sounds == null || sounds.Length == 0)
			{
				return null;
			}
			return sounds[0];
		}

		public static void FadeOutAndStopBGM(float duration)
		{
			if (!(instance == null))
			{
				AudioSource component = GameObject.Find("bgm").GetComponent<AudioSource>();
				instance.StartCoroutine(instance.FadeOutRoutine(component, duration));
			}
		}

		private IEnumerator FadeOutRoutine(AudioSource source, float duration)
		{
			if (source == null)
			{
				yield break;
			}
			float startVolume = source.volume;
			float time = 0f;
			while (time < duration)
			{
				if (source == null)
				{
					yield break;
				}
				time += Time.deltaTime;
				float t = Mathf.Clamp01(time / duration);
				source.volume = Mathf.Lerp(startVolume, 0f, t);
				yield return null;
			}
			if (source != null)
			{
				source.Stop();
				source.volume = startVolume;
			}
		}
	}
}
