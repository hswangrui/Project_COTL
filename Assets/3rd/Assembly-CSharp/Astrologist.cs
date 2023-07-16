using System.Collections.Generic;
using UnityEngine;

public class Astrologist : BaseMonoBehaviour
{
	public List<SpriteRenderer> MoonBoards;

	public List<Sprite> MoonBoardsImage;

	public List<GameObject> MoonBoardPositions;

	public List<bool> MoonBoardsUpdated;

	private Sprite BlankImage;

	public List<Sprite> Images;

	private void Start()
	{
		SetImages();
		foreach (SpriteRenderer moonBoard in MoonBoards)
		{
			SpriteRenderer spriteRenderer = moonBoard;
			MoonBoardsUpdated.Add(false);
		}
	}

	private void SetImages()
	{
		int num = -1;
		while (++num < MoonBoards.Count)
		{
			MoonBoardsImage.Add(Images[DataManager.Instance.DayList[num].MoonPhase]);
			MoonBoards[num].sprite = BlankImage;
		}
	}
}
