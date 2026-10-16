using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAudioInfo", menuName = "ScriptableObjects/DialogueAudioInfoSO", order = 1)]
public class DialogueAudioInfoSO : ScriptableObject
{
	public string id;

	public AudioClip[] dialogueTypingSoundClips;

	[Range(1f, 5f)]
	public int frequencyLevel = 2;

	[Range(-3f, 3f)]
	public float minPitch = 0.5f;

	[Range(-3f, 3f)]
	public float maxPitch = 3f;

	public bool stopAudioSource;
}
