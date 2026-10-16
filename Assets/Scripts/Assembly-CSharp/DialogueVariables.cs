using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

public class DialogueVariables
{
	private Story globalVariablesStory;

	private const string saveVariablesKey = "INK_VARIABLES";

	public Dictionary<string, Ink.Runtime.Object> variables { get; private set; }

	public DialogueVariables(TextAsset loadGlobalsJSON)
	{
		globalVariablesStory = new Story(loadGlobalsJSON.text);
		variables = new Dictionary<string, Ink.Runtime.Object>();
		foreach (string item in globalVariablesStory.variablesState)
		{
			Ink.Runtime.Object variableWithName = globalVariablesStory.variablesState.GetVariableWithName(item);
			variables.Add(item, variableWithName);
			Debug.Log("Initialized global dialogue variable: " + item + " = " + variableWithName);
		}
	}

	public void SaveVariables()
	{
		if (globalVariablesStory != null)
		{
			VariablesToStory(globalVariablesStory);
			PlayerPrefs.SetString("INK_VARIABLES", globalVariablesStory.state.ToJson());
		}
	}

	public void StartListening(Story story)
	{
		VariablesToStory(story);
		story.variablesState.variableChangedEvent += VariableChanged;
	}

	public void StopListening(Story story)
	{
		story.variablesState.variableChangedEvent -= VariableChanged;
	}

	private void VariableChanged(string name, Ink.Runtime.Object value)
	{
		if (variables.ContainsKey(name))
		{
			variables.Remove(name);
			variables.Add(name, value);
		}
	}

	private void VariablesToStory(Story story)
	{
		foreach (KeyValuePair<string, Ink.Runtime.Object> variable in variables)
		{
			story.variablesState.SetGlobal(variable.Key, variable.Value);
		}
	}
}
