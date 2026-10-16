using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManagerOld : MonoBehaviour
{
	public TextMeshProUGUI dialogueText;

	public TextMeshProUGUI nameText;

	public Image emotionImage;

	private Coroutine displayCoroutine;

	public void DisplayDialogue(string characterName, string dialogue, Sprite emotionSprite, TextEffect[] effects = null)
	{
		nameText.text = characterName;
		emotionImage.sprite = emotionSprite;
		if (displayCoroutine != null)
		{
			StopCoroutine(displayCoroutine);
		}
		displayCoroutine = StartCoroutine(TypeSentence(dialogue, effects));
	}

	private IEnumerator TypeSentence(string sentence, TextEffect[] effects)
	{
		dialogueText.text = "";
		char[] array = sentence.ToCharArray();
		foreach (char c in array)
		{
			dialogueText.text += c;
			if (effects != null)
			{
				foreach (TextEffect effect in effects)
				{
					ApplyEffect(effect);
				}
			}
			yield return null;
		}
	}

	private void ApplyEffect(TextEffect effect)
	{
		switch (effect.type)
		{
		case TextEffectType.Shake:
			StartCoroutine(ShakeText());
			break;
		case TextEffectType.ColorChange:
			dialogueText.color = effect.color;
			break;
		}
	}

	private IEnumerator ShakeText()
	{
		Vector3 originalPos = dialogueText.transform.localPosition;
		float elapsed = 0f;
		float duration = 0.5f;
		float magnitude = 5f;
		while (elapsed < duration)
		{
			float x = Random.Range(-1f, 1f) * magnitude;
			float y = Random.Range(-1f, 1f) * magnitude;
			dialogueText.transform.localPosition = new Vector3(x, y, originalPos.z);
			elapsed += Time.deltaTime;
			yield return null;
		}
		dialogueText.transform.localPosition = originalPos;
	}
}
