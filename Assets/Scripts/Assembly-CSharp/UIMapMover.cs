using System.Collections.Generic;
using UnityEngine;

public class UIMapMover : MonoBehaviour
{
	public RectTransform uiImage;

	private const float YPos3Floor = 160f;

	private const float YPos2Floor = 31f;

	private const float YPos1Floor = -97f;

	private Dictionary<string, Vector2> segmentPositions = new Dictionary<string, Vector2>();

	private void Start()
	{
		segmentPositions["Palate301"] = new Vector2(-256f, 160f);
		segmentPositions["Palate302"] = new Vector2(-2f, 160f);
		segmentPositions["Palate303"] = new Vector2(306f, 160f);
		segmentPositions["WCMale3F"] = new Vector2(-170f, 160f);
		segmentPositions["Elev3F"] = new Vector2(-83f, 160f);
		segmentPositions["LockerRoom3F"] = new Vector2(98f, 160f);
		segmentPositions["Vends3F"] = new Vector2(213f, 160f);
		segmentPositions["UtilityRoom3F"] = new Vector2(393f, 160f);
		segmentPositions["Stairs3F"] = new Vector2(466f, 160f);
		segmentPositions["Palate201"] = new Vector2(-256f, 31f);
		segmentPositions["Palate202"] = new Vector2(-2f, 31f);
		segmentPositions["Palate203"] = new Vector2(306f, 31f);
		segmentPositions["WCMale2F"] = new Vector2(-170f, 31f);
		segmentPositions["Elev2F"] = new Vector2(-83f, 31f);
		segmentPositions["Vends2F"] = new Vector2(157f, 31f);
		segmentPositions["UtilityRoom2F"] = new Vector2(393f, 31f);
		segmentPositions["Stairs2F"] = new Vector2(466f, 31f);
		segmentPositions["WCMale1F"] = new Vector2(-2f, -97f);
		segmentPositions["Elev1F"] = new Vector2(-83f, -97f);
		segmentPositions["LockerRoom1F"] = new Vector2(213f, -97f);
		segmentPositions["Vends1F"] = new Vector2(98f, -97f);
		segmentPositions["UtilityRoom1F"] = new Vector2(306f, -97f);
		segmentPositions["Stairs1F"] = new Vector2(466f, -97f);
		segmentPositions["Exit"] = new Vector2(-170f, -97f);
		segmentPositions["Basement"] = new Vector2(393f, -97f);
	}

	public void MoveToSegment(string segmentName)
	{
		if (segmentPositions.TryGetValue(segmentName, out var value))
		{
			uiImage.anchoredPosition = value;
		}
		else
		{
			Debug.LogWarning("Сегмент с именем " + segmentName + " не найден!");
		}
	}
}
