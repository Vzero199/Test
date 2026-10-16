// Temporary debugging aid: shows the current scene and the latest errors on screen.
// Put this file at Assets/PortDebugOverlay.cs. Delete it once the game works.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortDebugOverlay : MonoBehaviour
{
    static readonly List<string> lines = new List<string>();
    static string sceneInfo = "(no scene yet)";
    GUIStyle style;
    Texture2D bg;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var go = new GameObject("PortDebugOverlay");
        DontDestroyOnLoad(go);
        go.AddComponent<PortDebugOverlay>();
        Application.logMessageReceived += OnLog;
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            sceneInfo = "scene: " + scene.name + " (" + mode + ")";
            Add("LOADED " + scene.name);
        };
    }

    static void Add(string s)
    {
        lines.Add(s);
        if (lines.Count > 60) lines.RemoveAt(0);
    }

    static void OnLog(string condition, string stack, LogType type)
    {
        if (type == LogType.Log || type == LogType.Warning) return;
        string top = "";
        if (!string.IsNullOrEmpty(stack))
        {
            var parts = stack.Split('\n');
            for (int i = 0; i < parts.Length && i < 3; i++) top += "\n   " + parts[i];
        }
        Add(type + ": " + condition + top);
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = Mathf.Max(14, Screen.height / 50);
            style.wordWrap = true;
            style.normal.textColor = Color.white;
            bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
            bg.Apply();
        }
        GUI.depth = -1000;
        var area = new Rect(0, 0, Screen.width, Screen.height * 0.7f);
        GUI.DrawTexture(area, bg);
        string text = sceneInfo + "  time: " + Time.realtimeSinceStartup.ToString("F0") + "s\n";
        int start = Mathf.Max(0, lines.Count - 14);
        for (int i = start; i < lines.Count; i++) text += lines[i] + "\n";
        GUI.Label(area, text, style);
    }
}
