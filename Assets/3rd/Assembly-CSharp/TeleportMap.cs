using System;
using UnityEngine;

public class TeleportMap : MiniMap
{
	public RectTransform TargetRoom;

	public RectTransform TargetRoomPointer;

	private MiniMapIcon TargetIcon;

	public RectTransform CurrentRoomIcon;

	private float TargetAngle;

	public RectTransform Line;

	public Canvas canvas;

	private static TeleportMap Instance;

	private Action Callback;

	private Action CallbackCancel;

	private void Awake()
	{
		Instance = this;
		base.gameObject.SetActive(false);
	}

	public override void StartMap()
	{
		TargetAngle = 0f;
	}

	private void CenterMap()
	{
		IconContainerRect = IconContainer.GetComponent<RectTransform>();
		foreach (MiniMapIcon icon in Icons)
		{
			if (icon.X == WorldGen.WIDTH / 2 && icon.Y == WorldGen.HEIGHT / 2)
			{
				IconContainerRect.localPosition = -icon.rectTransform.localPosition;
			}
			if (icon.X == RoomManager.CurrentX && icon.Y == RoomManager.CurrentY)
			{
				CurrentIcon = icon;
			}
		}
		TargetRoom.position = -CurrentIcon.rectTransform.position;
		Line.position = CurrentIcon.rectTransform.position;
		Line.gameObject.SetActive(false);
		CurrentRoomIcon.position = CurrentIcon.rectTransform.position;
	}

	public static void Show(Action Callback, Action CallbackCancel)
	{
		Time.timeScale = 0f;
		Instance.gameObject.SetActive(true);
		Instance.CenterMap();
		Instance.Callback = Callback;
		Instance.CallbackCancel = CallbackCancel;
	}

	public static void Hide()
	{
		Time.timeScale = 1f;
		Instance.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > 0.2f || Mathf.Abs(InputManager.UI.GetVerticalAxis()) > 0.2f)
		{
			TargetAngle = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.UI.GetHorizontalAxis(), InputManager.UI.GetHorizontalAxis()));
			TargetRoomPointer.position = CurrentIcon.rectTransform.position + new Vector3(100f * Mathf.Cos(TargetAngle * ((float)Math.PI / 180f)), 100f * Mathf.Sin(TargetAngle * ((float)Math.PI / 180f)));
			GetClosestIcon();
			if (TargetIcon != null)
			{
				TargetRoom.position = TargetIcon.rectTransform.position;
				Line.gameObject.SetActive(true);
				Line.eulerAngles = new Vector3(0f, 0f, Utils.GetAngle(CurrentIcon.rectTransform.position, TargetRoom.position));
				Line.sizeDelta = new Vector2(Vector3.Distance(CurrentIcon.rectTransform.position, TargetRoom.position) / canvas.scaleFactor, Line.sizeDelta.y);
			}
		}
		if (TargetIcon != null && InputManager.UI.GetAcceptButtonDown())
		{
			RoomManager.CurrentX = TargetIcon.X;
			RoomManager.CurrentY = TargetIcon.Y;
			if (Callback != null)
			{
				Callback();
			}
			Hide();
		}
		if (InputManager.UI.GetCancelButtonUp())
		{
			if (CallbackCancel != null)
			{
				CallbackCancel();
			}
			Hide();
		}
	}

	private void GetClosestIcon()
	{
		foreach (MiniMapIcon icon in Icons)
		{
			float num = Mathf.Abs(Utils.GetAngle(CurrentIcon.rectTransform.position, icon.rectTransform.position) - TargetAngle);
			if (icon.gameObject.activeSelf && icon != CurrentIcon)
			{
				TargetIcon = icon;
			}
		}
	}
}
