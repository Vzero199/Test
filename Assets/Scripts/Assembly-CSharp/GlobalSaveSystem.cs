using System.Collections.Generic;
using UnityEngine;

public static class GlobalSaveSystem
{
	public static void SetProgress(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static bool GetProgress(string key)
	{
		return PlayerPrefs.GetInt(key, 0) == 1;
	}

	public static void ResetProgress()
	{
		PlayerPrefs.DeleteKey("FirstNightIsComplete");
		PlayerPrefs.DeleteKey("SecondNightIsComplete");
		PlayerPrefs.DeleteKey("ThirdNightIsComplete");
		PlayerPrefs.DeleteKey("FourthNightIsComplete");
		PlayerPrefs.DeleteKey("FifthNightIsComplete");
		PlayerPrefs.DeleteKey("FirstDayReached");
		PlayerPrefs.DeleteKey("SecondDayReached");
		PlayerPrefs.DeleteKey("ThirdDayReached");
		PlayerPrefs.DeleteKey("FourthDayReached");
		PlayerPrefs.DeleteKey("FifthDayReached");
		PlayerPrefs.DeleteKey("FirstNightReached");
		PlayerPrefs.DeleteKey("SecondNightReached");
		PlayerPrefs.DeleteKey("ThirdNightReached");
		PlayerPrefs.DeleteKey("FourthNightReached");
		PlayerPrefs.DeleteKey("FifthNightReached");
		PlayerPrefs.DeleteKey("TrueEndIsReceive");
		PlayerPrefs.DeleteKey("NintendoKarma");
		PlayerPrefs.DeleteKey("LelickKarma");
		PlayerPrefs.DeleteKey("SoraKarma");
		PlayerPrefs.DeleteKey("MomokoKarma");
		PlayerPrefs.DeleteKey("MiyuKarma");
		PlayerPrefs.DeleteKey("HarukoKarma");
		PlayerPrefs.DeleteKey("SirasakiKarma");
		PlayerPrefs.DeleteKey("HintFNotified");
		PlayerPrefs.DeleteKey("HintTabNotified");
		PlayerPrefs.DeleteKey("HintTabPickNotified");
		PlayerPrefs.DeleteKey("HintCableNotified");
		PlayerPrefs.DeleteKey("HintLightSwordNotified");
		PlayerPrefs.DeleteKey("HintTutorialMenuNotified");
		PlayerPrefs.DeleteKey("HintNHide");
		PlayerPrefs.DeleteKey("HintNSupNurseKey");
		PlayerPrefs.DeleteKey("HintNPoster");
		PlayerPrefs.DeleteKey("HintNBats");
		PlayerPrefs.DeleteKey("HintNDoll");
		PlayerPrefs.DeleteKey("HintNWhisper");
		PlayerPrefs.DeleteKey("HintNHideReady");
		PlayerPrefs.DeleteKey("HintNSupNurseKeyReady");
		PlayerPrefs.DeleteKey("HintNPosterReady");
		PlayerPrefs.DeleteKey("HintNBatsReady");
		PlayerPrefs.DeleteKey("HintNDollReady");
		PlayerPrefs.DeleteKey("HintNWhisperReady");
		PlayerPrefs.Save();
	}

	public static Dictionary<string, bool> GetAllProgress()
	{
		return new Dictionary<string, bool>
		{
			{
				"FirstNightIsComplete",
				GetProgress("FirstNightIsComplete")
			},
			{
				"SecondNightIsComplete",
				GetProgress("SecondNightIsComplete")
			},
			{
				"ThirdNightIsComplete",
				GetProgress("ThirdNightIsComplete")
			},
			{
				"FourthNightIsComplete",
				GetProgress("FourthNightIsComplete")
			},
			{
				"FifthNightIsComplete",
				GetProgress("FifthNightIsComplete")
			},
			{
				"FirstDayReached",
				GetProgress("FirstDayReached")
			},
			{
				"SecondDayReached",
				GetProgress("SecondDayReached")
			},
			{
				"ThirdDayReached",
				GetProgress("ThirdDayReached")
			},
			{
				"FourthDayReached",
				GetProgress("FourthDayReached")
			},
			{
				"FifthDayReached",
				GetProgress("FifthDayReached")
			},
			{
				"FirstNightReached",
				GetProgress("FirstNightReached")
			},
			{
				"SecondNightReached",
				GetProgress("SecondNightReached")
			},
			{
				"ThirdNightReached",
				GetProgress("ThirdNightReached")
			},
			{
				"FourthNightReached",
				GetProgress("FourthNightReached")
			},
			{
				"FifthNightReached",
				GetProgress("FifthNightReached")
			},
			{
				"TrueEndIsReceive",
				GetProgress("TrueEndIsReceive")
			},
			{
				"NintendoKarma",
				GetProgress("NintendoKarma")
			},
			{
				"LelickKarma",
				GetProgress("LelickKarma")
			},
			{
				"SoraKarma",
				GetProgress("SoraKarma")
			},
			{
				"MomokoKarma",
				GetProgress("MomokoKarma")
			},
			{
				"MiyuKarma",
				GetProgress("MiyuKarma")
			},
			{
				"HarukoKarma",
				GetProgress("HarukoKarma")
			},
			{
				"HarukoKarma",
				GetProgress("SirasakiKarma")
			},
			{
				"HintFNotified",
				GetProgress("HintFNotified")
			},
			{
				"HintTabNotified",
				GetProgress("HintTabNotified")
			},
			{
				"HintTabPickNotified",
				GetProgress("HintTabPickNotified")
			},
			{
				"HintCableNotified",
				GetProgress("HintCableNotified")
			},
			{
				"HintLightSwordNotified",
				GetProgress("HintLightSwordNotified")
			},
			{
				"HintTutorialMenuNotified",
				GetProgress("HintTutorialMenuNotified")
			},
			{
				"HintNHide",
				GetProgress("HintNHide")
			},
			{
				"HintNSupNurseKey",
				GetProgress("HintNSupNurseKey")
			},
			{
				"HintNPoster",
				GetProgress("HintNPoster")
			},
			{
				"HintNBats",
				GetProgress("HintNBats")
			},
			{
				"HintNDoll",
				GetProgress("HintNDoll")
			},
			{
				"HintNWhisper",
				GetProgress("HintNWhisper")
			},
			{
				"HintNHideReady",
				GetProgress("HintNHideReady")
			},
			{
				"HintNSupNurseKeyReady",
				GetProgress("HintNSupNurseKeyReady")
			},
			{
				"HintNPosterReady",
				GetProgress("HintNPosterReady")
			},
			{
				"HintNBatsReady",
				GetProgress("HintNBatsReady")
			},
			{
				"HintNDollReady",
				GetProgress("HintNDollReady")
			},
			{
				"HintNWhisperReady",
				GetProgress("HintNWhisperReady")
			}
		};
	}

	public static bool AreAllKarmasTrue()
	{
		if (GetProgress("NintendoKarma") && GetProgress("LelickKarma") && GetProgress("SoraKarma") && GetProgress("MomokoKarma") && GetProgress("MiyuKarma") && GetProgress("HarukoKarma"))
		{
			return GetProgress("SirasakiKarma");
		}
		return false;
	}

	public static void ResetKarmaProgress()
	{
		PlayerPrefs.DeleteKey("NintendoKarma");
		PlayerPrefs.DeleteKey("LelickKarma");
		PlayerPrefs.DeleteKey("SoraKarma");
		PlayerPrefs.DeleteKey("MomokoKarma");
		PlayerPrefs.DeleteKey("MiyuKarma");
		PlayerPrefs.DeleteKey("HarukoKarma");
		PlayerPrefs.DeleteKey("SirasakiKarma");
		PlayerPrefs.Save();
	}

	public static void SetAllProgressTrue()
	{
		SetProgress("FirstNightIsComplete", value: true);
		SetProgress("SecondNightIsComplete", value: true);
		SetProgress("ThirdNightIsComplete", value: true);
		SetProgress("FourthNightIsComplete", value: true);
		SetProgress("FifthNightIsComplete", value: true);
		SetProgress("NintendoKarma", value: true);
		SetProgress("TrueEndIsReceive", value: true);
	}
}
