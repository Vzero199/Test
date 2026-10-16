using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
	private static string savePath = Application.persistentDataPath + "/save.dat";

	private static string nightSavePath = Application.persistentDataPath + "/night.dat";

	private static string globalSavePath = Application.persistentDataPath + "/global.dat";

	private static string encryptionKey = "ChlomakiBestWaifu!";

	public static void SaveGame(GameSaveData data)
	{
		string text = JsonUtility.ToJson(data);
		File.WriteAllBytes(savePath, Encrypt(text));
		Debug.Log("Сохранено (день).");
	}

	public static GameSaveData LoadGame()
	{
		if (File.Exists(savePath))
		{
			return JsonUtility.FromJson<GameSaveData>(Decrypt(File.ReadAllBytes(savePath)));
		}
		return new GameSaveData();
	}

	public static void SaveNightScene(NightSaveData data)
	{
		string text = JsonUtility.ToJson(data);
		File.WriteAllBytes(nightSavePath, Encrypt(text));
		Debug.Log("Сохранено (ночь).");
	}

	public static NightSaveData LoadNightScene()
	{
		if (File.Exists(nightSavePath))
		{
			return JsonUtility.FromJson<NightSaveData>(Decrypt(File.ReadAllBytes(nightSavePath)));
		}
		return null;
	}

	public static void SaveAchievements(GlobalSaveData data)
	{
		string text = JsonUtility.ToJson(data);
		File.WriteAllBytes(globalSavePath, Encrypt(text));
		Debug.Log("Сохранено (достижения).");
	}

	public static GlobalSaveData LoadAchievements()
	{
		if (File.Exists(globalSavePath))
		{
			return JsonUtility.FromJson<GlobalSaveData>(Decrypt(File.ReadAllBytes(globalSavePath)));
		}
		return new GlobalSaveData();
	}

	public static void DeleteSave()
	{
		if (File.Exists(savePath))
		{
			File.Delete(savePath);
		}
		if (File.Exists(nightSavePath))
		{
			File.Delete(nightSavePath);
		}
		Debug.Log("Сохранение сброшено.");
	}

	private static byte[] Encrypt(string text)
	{
		using Aes aes = Aes.Create();
		SetEncryptionKey(aes);
		aes.IV = new byte[16];
		using ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		return cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
	}

	private static string Decrypt(byte[] cipherText)
	{
		using Aes aes = Aes.Create();
		SetEncryptionKey(aes);
		aes.IV = new byte[16];
		using ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
		byte[] bytes = cryptoTransform.TransformFinalBlock(cipherText, 0, cipherText.Length);
		return Encoding.UTF8.GetString(bytes);
	}

	private static byte[] GetKey(string key)
	{
		using SHA256 sHA = SHA256.Create();
		return sHA.ComputeHash(Encoding.UTF8.GetBytes(key));
	}

	private static void SetEncryptionKey(Aes aes)
	{
		aes.Key = GetKey(encryptionKey);
	}
}
