using System;
using System.Collections;
using MMBiomeGeneration;
using MMTools;
using TMPro;
using UnityEngine;

public class ActivateMiniMap : BaseMonoBehaviour
{
	public static ActivateMiniMap Instance;

	public MiniMap miniMap;

	private RectTransform miniMapRT;

	private StateMachine playerState;

	public Vector2 SmallSize = new Vector2(250f, 150f);

	public Vector2 TargetSize = new Vector2(1500f, 800f);

	private Vector2 DefaultLocation;

	public GameObject TeleportPrompt;

	public TextMeshProUGUI Text;

	public static bool IsPlaying;

	public HUD_InventoryIcon[] InventoryIcons;

	public static bool DisableTeleporting;

	public RectTransform Marker;

	public float MoveCursorSpeedX = 75f;

	public float MoveCursorSpeedY = 50f;

	public float SnapToClosestAcceleration = 15f;

	private float SnapToClosestSpeed = 25f;

	public float PanSpeed = 15f;

	private MiniMapIcon Closest;

	private float Delay;

	private void OnEnable()
	{
		Instance = this;
		miniMapRT = miniMap.GetComponent<RectTransform>();
		DefaultLocation = miniMapRT.localPosition;
		TeleportPrompt.SetActive(false);
		if (DataManager.Instance.PlayerFleece == 11)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static void Show(StateMachine state)
	{
		if (Instance != null)
		{
			Instance.Init(state);
		}
	}

	public void Init(StateMachine state)
	{
		playerState = state;
		playerState.CURRENT_STATE = StateMachine.State.Map;
		StartCoroutine(MoveMap());
		IsPlaying = true;
	}

	private void ItemUpdated()
	{
		HUD_InventoryIcon[] inventoryIcons = InventoryIcons;
		for (int i = 0; i < inventoryIcons.Length; i++)
		{
			inventoryIcons[i].InitFromType();
		}
	}

	private IEnumerator MoveMap()
	{
		float Timer = 0f;
		Vector3 Velocity = Vector3.zero;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.3f))
			{
				break;
			}
			miniMapRT.localPosition = Vector3.SmoothDamp(miniMapRT.localPosition, Vector3.zero, ref Velocity, 0.1f);
			yield return null;
		}
		miniMapRT.localPosition = Vector3.zero;
		StartCoroutine(ScaleMap());
	}

	private IEnumerator ScaleMap()
	{
		float Timer = 0f;
		Vector3 Velocity = Vector3.zero;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.3f))
			{
				break;
			}
			miniMapRT.sizeDelta = Vector3.SmoothDamp(miniMapRT.sizeDelta, TargetSize, ref Velocity, 0.1f);
			yield return null;
		}
		StartCoroutine(Close());
	}

	private IEnumerator Close()
	{
		Coroutine navigateMap = StartCoroutine(NavigateMap());
		playerState.GetComponent<PlayerFarming>();
		while (!InputManager.UI.GetCancelButtonUp())
		{
			if (Closest != miniMap.CurrentIcon && !DisableTeleporting)
			{
				if (!TeleportPrompt.activeSelf)
				{
					TeleportPrompt.SetActive(true);
				}
			}
			else if (TeleportPrompt.activeSelf)
			{
				TeleportPrompt.SetActive(false);
			}
			if (InputManager.UI.GetAcceptButtonUp() && Closest != miniMap.CurrentIcon && !DisableTeleporting)
			{
				StopAllCoroutines();
				StartCoroutine(ReturnToStartingPositionAndSize());
				StartCoroutine(DoChangeRoom());
				Marker.position = Vector3.one * -999f;
			}
			yield return null;
		}
		playerState.CURRENT_STATE = StateMachine.State.Idle;
		StopCoroutine(navigateMap);
		StartCoroutine(ReturnToStartingPositionAndSize());
	}

	private IEnumerator DoChangeRoom()
	{
		playerState.CURRENT_STATE = StateMachine.State.Teleporting;
		yield return new WaitForSeconds(1f);
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.3f, "", ChangeRoom);
	}

	private void ChangeRoom()
	{
		playerState.CURRENT_STATE = StateMachine.State.Idle;
		BiomeGenerator.Instance.IsTeleporting = true;
		BiomeGenerator.ChangeRoom(Closest.X, Closest.Y);
	}

	private IEnumerator NavigateMap()
	{
		while (miniMap.CurrentIcon == null)
		{
			yield return null;
		}
		playerState.GetComponent<PlayerFarming>();
		Marker.position = miniMap.CurrentIcon.rectTransform.position;
		Vector3 zero = Vector3.zero;
		while (true)
		{
			float horizontalAxis = InputManager.Gameplay.GetHorizontalAxis();
			float verticalAxis = InputManager.Gameplay.GetVerticalAxis();
			float num = float.MaxValue;
			foreach (MiniMapIcon icon in miniMap.Icons)
			{
				float num2 = Vector2.Distance(Marker.position, icon.rectTransform.position);
				if (num2 < num && icon.gameObject.activeSelf && icon.room.Visited && !icon.room.IsCustom && icon.room.Completed)
				{
					num = num2;
					Closest = icon;
				}
			}
			if ((Delay -= Time.deltaTime) < 0f && (Mathf.Abs(horizontalAxis) > 0.3f || Mathf.Abs(verticalAxis) > 0.3f))
			{
				float f = Utils.GetAngle(Vector3.zero, new Vector3(horizontalAxis, verticalAxis)) * ((float)Math.PI / 180f);
				Vector3 localPosition = Vector3.zero + new Vector3(MoveCursorSpeedX * Mathf.Cos(f), MoveCursorSpeedY * Mathf.Sin(f));
				Marker.localPosition = localPosition;
				Delay = 0.1f;
			}
			else
			{
				Vector3 localPosition = Vector3.Lerp(Marker.position, Closest.rectTransform.position, SnapToClosestSpeed * Time.deltaTime);
				Marker.position = localPosition;
			}
			if (Closest != null)
			{
				miniMap.IconContainerRect.localPosition = Vector3.Lerp(miniMap.IconContainerRect.localPosition, -Closest.rectTransform.localPosition, PanSpeed * Time.deltaTime);
			}
			yield return null;
		}
	}

	private IEnumerator ReturnToStartingPositionAndSize()
	{
		float Timer = 0f;
		Vector3 VelocitySize = Vector3.zero;
		Vector3 VelocityScale = Vector3.zero;
		miniMap.StartCoroutine(miniMap.MoveMiniMap(0f));
		TeleportPrompt.SetActive(false);
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 0.4f))
			{
				break;
			}
			miniMapRT.sizeDelta = Vector3.SmoothDamp(miniMapRT.sizeDelta, SmallSize, ref VelocitySize, 0.1f);
			miniMapRT.localPosition = Vector3.SmoothDamp(miniMapRT.localPosition, DefaultLocation, ref VelocityScale, 0.1f);
			yield return null;
		}
		IsPlaying = false;
	}
}
