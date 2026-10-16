using System;
using UnityEngine;

public static class GameSettings
{
	private const string LanguageKey = "CurrentLanguage";

	private static string currentLanguage;

	private static bool isLoaded;

	public static string CurrentLanguage
	{
		get
		{
			if (!isLoaded)
			{
				LoadSettings();
			}
			return currentLanguage;
		}
		set
		{
			if (!isLoaded)
			{
				LoadSettings();
			}
			if (currentLanguage != value)
			{
				currentLanguage = value;
				PlayerPrefs.SetString("CurrentLanguage", currentLanguage);
				PlayerPrefs.Save();
				OnLanguageChanged?.Invoke();
			}
		}
	}

	public static event Action OnLanguageChanged;

	public static void LoadSettings()
	{
		if (!isLoaded)
		{
			if (PlayerPrefs.HasKey("CurrentLanguage"))
			{
				currentLanguage = PlayerPrefs.GetString("CurrentLanguage");
			}
			else
			{
				currentLanguage = "JP";
				PlayerPrefs.SetString("CurrentLanguage", currentLanguage);
				PlayerPrefs.Save();
			}
			isLoaded = true;
		}
	}
}
