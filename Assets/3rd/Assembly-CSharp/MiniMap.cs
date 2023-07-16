using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMBiomeGeneration;
using UnityEngine;

public class MiniMap : BaseMonoBehaviour
{
	public GameObject CurrentRoom;

	public GameObject Background;

	public Sprite Room;

	public Sprite Room_Snake;

	public Sprite Room_Goat;

	public Sprite StartingRoom;

	public Sprite RoomPointOfInterest;

	public Sprite Door;

	public Sprite Base;

	public GameObject IconContainer;

	[HideInInspector]
	public RectTransform IconContainerRect;

	public float Spacing = 25f;

	private Vector2 IconSize;

	private float Scale = 0.2f;

	[HideInInspector]
	public List<MiniMapIcon> Icons = new List<MiniMapIcon>();

	public float PanSpeed = 10f;

	public GameObject RoomIconPrefab;

	public GameObject BaseIcon;

	public bool HideUnexplored = true;

	public bool MoveMap = true;

	[HideInInspector]
	public MiniMapIcon CurrentIcon;

	private static MiniMap Instance;

	private Vector3 NewPosition;

	public void VisitAll()
	{
		foreach (MiniMapIcon icon in Icons)
		{
			icon.room.Visited = true;
		}
		OnChangeRoom();
	}

	public void DiscoverAll()
	{
		foreach (MiniMapIcon icon in Icons)
		{
			icon.room.Discovered = true;
		}
		OnChangeRoom();
	}

	public virtual void StartMap()
	{
	}

	private void Start()
	{
		BiomeGenerator.OnBiomeGenerated += OnBiomeGenerated;
		BiomeGenerator.OnBiomeChangeRoom += OnChangeRoom;
		TrinketManager.OnTrinketAdded += OnTrinketAdded;
	}

	private void OnDestroy()
	{
		BiomeGenerator.OnBiomeGenerated -= OnBiomeGenerated;
		BiomeGenerator.OnBiomeChangeRoom -= OnChangeRoom;
		TrinketManager.OnTrinketAdded -= OnTrinketAdded;
	}

	private void OnChangeRoom()
	{
		CurrentRoom.SetActive(Icons.Count > 0);
		Background.SetActive(Icons.Count > 0);
		foreach (MiniMapIcon icon in Icons)
		{
			if (icon.room.Visited || !HideUnexplored)
			{
				icon.ShowIcon();
			}
			else if (icon.room.Discovered)
			{
				icon.ShowIcon(0.5f);
			}
			else
			{
				icon.SetSelfToQuestionMark();
			}
			if (icon.X == BiomeGenerator.Instance.CurrentX && icon.Y == BiomeGenerator.Instance.CurrentY)
			{
				CurrentIcon = icon;
			}
		}
		if (MoveMap && CurrentIcon != null)
		{
			StartCoroutine(MoveMiniMap(0.7f));
		}
	}

	private void OnEnable()
	{
		Instance = this;
		IconContainerRect = IconContainer.GetComponent<RectTransform>();
		StartCoroutine(Wait());
	}

	public IEnumerator Wait()
	{
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForEndOfFrame();
		}
		CurrentRoom.SetActive(false);
		Background.SetActive(false);
	}

	private void OnDisable()
	{
		if (WorldGen.Instance != null)
		{
			WorldGen.Instance.OnWorldGenerated -= OnWorldGenerated;
		}
		foreach (MiniMapIcon icon in Icons)
		{
			Object.Destroy(icon.gameObject);
		}
		Icons.Clear();
	}

	private void OnBiomeGenerated()
	{
		if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			return;
		}
		foreach (MiniMapIcon icon in Icons)
		{
			Object.Destroy(icon.gameObject);
		}
		Icons.Clear();
		if (BiomeGenerator.Instance.OverrideRandomWalk)
		{
			IconContainer.SetActive(false);
			return;
		}
		IconContainer.SetActive(true);
		MiniMapIcon miniMapIcon = null;
		foreach (BiomeRoom room in BiomeGenerator.Instance.Rooms)
		{
			miniMapIcon = Object.Instantiate(RoomIconPrefab, IconContainer.transform, true).GetComponent<MiniMapIcon>();
			NewPosition = new Vector3((float)room.x * (IconSize.x + Spacing), (float)room.y * (IconSize.y + Spacing));
			IconSize = miniMapIcon.Init(room, GetImage(room), Scale, NewPosition);
			Icons.Add(miniMapIcon);
		}
		OnChangeRoom();
		CheckTelescope();
	}

	private void OnTrinketAdded(TarotCards.Card trinketAdded)
	{
		CheckTelescope();
	}

	private void CheckTelescope()
	{
		if (TrinketManager.HasTrinket(TarotCards.Card.Telescope))
		{
			DiscoverAll();
		}
	}

	private void OnWorldGenerated()
	{
	}

	public static void CurrentRoomShowTeleporter()
	{
		if (Instance != null)
		{
			Instance.CurrentIcon.ShowTeleporter();
		}
	}

	private Sprite GetImage(BiomeRoom room)
	{
		if (room.IsCustom)
		{
			return Room_Snake;
		}
		return Room;
	}

	public IEnumerator MoveMiniMap(float Delay)
	{
		if (CurrentIcon == null)
		{
			yield break;
		}
		NewPosition = -CurrentIcon.rectTransform.localPosition;
		foreach (MiniMapIcon icon in Icons)
		{
			icon.transform.DOKill();
			if (icon != CurrentIcon)
			{
				icon.transform.DOScale(Vector3.one * Scale, 0.5f);
				icon.IconContainer.SetActive(true);
				icon.IconContainer.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			}
			else
			{
				icon.transform.DOScale(Vector3.one * Scale * 1.5f, 0.5f).SetEase(Ease.OutBack);
			}
			icon.outlineImage.enabled = icon == CurrentIcon;
		}
		yield return new WaitForSeconds(Delay * 0.5f);
		if (CurrentIcon != null && CurrentIcon.room != null && !CurrentIcon.room.IsRespawnRoom)
		{
			CurrentRoom.transform.DOKill();
			CurrentRoom.transform.DOScale(Vector3.one * 1.5f * 0.15f, 0.5f).SetEase(Ease.InOutBack);
		}
		yield return new WaitForSeconds(Delay * 0.5f);
		if (CurrentIcon != null && CurrentIcon.room != null && !CurrentIcon.room.IsRespawnRoom)
		{
			while (Vector3.Distance(NewPosition, IconContainerRect.localPosition) > 0.2f)
			{
				if (CurrentIcon.IconContainer.activeSelf && Vector3.Distance(NewPosition, IconContainerRect.localPosition) < 20f)
				{
					CurrentIcon.IconContainer.transform.DOScale(Vector3.zero, 0.5f);
				}
				IconContainerRect.localPosition = Vector3.Lerp(IconContainerRect.localPosition, NewPosition, PanSpeed * Time.deltaTime);
				yield return null;
			}
			CurrentRoom.transform.DOKill();
			CurrentRoom.transform.DOScale(Vector3.one * 1f * 0.15f, 0.5f).SetEase(Ease.OutBack);
		}
		IconContainerRect.localPosition = NewPosition;
	}
}
