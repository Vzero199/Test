using System;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class BasicInkExample : MonoBehaviour
{
	[SerializeField]
	private TextAsset inkJSONAsset;

	public Story story;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Text textPrefab;

	[SerializeField]
	private Button buttonPrefab;

	public static event Action<Story> OnCreateStory;

	private void Awake()
	{
		RemoveChildren();
		StartStory();
	}

	private void StartStory()
	{
		story = new Story(inkJSONAsset.text);
		if (OnCreateStory != null)
		{
			OnCreateStory(story);
		}
		RefreshView();
	}

	private void RefreshView()
	{
		RemoveChildren();
		while (story.canContinue)
		{
			string text = story.Continue();
			text = text.Trim();
			CreateContentView(text);
		}
		if (story.currentChoices.Count > 0)
		{
			for (int i = 0; i < story.currentChoices.Count; i++)
			{
				Choice choice = story.currentChoices[i];
				CreateChoiceView(choice.text.Trim()).onClick.AddListener(() =>
				{
					OnClickChoiceButton(choice);
				});
			}
		}
		else
		{
			CreateChoiceView("End of story.\nRestart?").onClick.AddListener(() =>
			{
				StartStory();
			});
		}
	}

	private void OnClickChoiceButton(Choice choice)
	{
		story.ChooseChoiceIndex(choice.index);
		RefreshView();
	}

	private void CreateContentView(string text)
	{
		Text text2 = UnityEngine.Object.Instantiate(textPrefab);
		text2.text = text;
		text2.transform.SetParent(canvas.transform, worldPositionStays: false);
	}

	private Button CreateChoiceView(string text)
	{
		Button button = UnityEngine.Object.Instantiate(buttonPrefab);
		button.transform.SetParent(canvas.transform, worldPositionStays: false);
		button.GetComponentInChildren<Text>().text = text;
		button.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = false;
		return button;
	}

	private void RemoveChildren()
	{
		for (int num = canvas.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(canvas.transform.GetChild(num).gameObject);
		}
	}
}
