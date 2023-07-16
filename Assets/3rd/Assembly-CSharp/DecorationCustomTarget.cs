using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using UnityEngine;

public class DecorationCustomTarget : MonoBehaviour
{
	private const string Path = "Prefabs/Resources/Decoration Custom Target";

	public SpriteRenderer SpriteRenderer;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	public StructureBrain.TYPES DecorationType;

	private Action Callback;

	private bool Activating;

	private StateMachine state
	{
		get
		{
			return PlayerFarming.Instance.state;
		}
	}

	public static void Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, StructureBrain.TYPES Decoration, Action Callback)
	{
		Transform parent = ((RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		Create(StartPosition, EndPosition, Duration, Decoration, parent, Callback);
	}

	public static void Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, StructureBrain.TYPES Decoration, Transform parent, Action Callback)
	{
		(UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/Decoration Custom Target"), StartPosition + Vector3.back * 0.5f, Quaternion.identity, parent) as GameObject).GetComponent<DecorationCustomTarget>().Play(EndPosition, Duration, Decoration, Callback);
	}

	public void Play(Vector3 EndPosition, float Duration, StructureBrain.TYPES Decoration, Action Callback)
	{
		DecorationType = Decoration;
		this.Callback = Callback;
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.5f);
		sequence.Append(base.transform.DOMove(EndPosition + Vector3.back * 0.5f, Duration).SetEase(Ease.InBack).OnComplete(delegate
		{
			SpriteRenderer.enabled = false;
			OpenMenu();
		}));
		sequence.Play();
	}

	private void OpenMenu()
	{
		AudioManager.Instance.PlayOneShot("event:/player/new_item_pickup", base.gameObject);
		BiomeConstants.Instance.EmitPickUpVFX(base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(0.7f, 0.9f, 0.3f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		if (DecorationType != 0)
		{
			StructuresData.CompleteResearch(DecorationType);
			StructuresData.SetRevealed(DecorationType);
		}
		UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
		uINewItemOverlayController.pickedBuilding = DecorationType;
		uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.Decoration, base.transform.position, false);
		uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, new Action(BackToIdle));
	}

	private void BackToIdle()
	{
		StartCoroutine(BackToIdleRoutine());
	}

	private IEnumerator BackToIdleRoutine()
	{
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return null;
		StopAllCoroutines();
		GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine());
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator DelayEffectsRoutine()
	{
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
