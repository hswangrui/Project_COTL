using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI;
using Spine.Unity;
using UnityEngine;

public class FollowerSkinCustomTarget : MonoBehaviour
{
	private const string Path = "Prefabs/Resources/FollowerSkin Custom Target";

	public SpriteRenderer SpriteRenderer;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	public GameObject Menu;

	private TarotCards.TarotCard DrawnCard;

	private Action Callback;

	public bool FollowerSkinForceSelection;

	public SkeletonDataAsset Spine;

	private List<string> FollowerSkinsAvailable = new List<string>();

	public string PickedSkin;

	private bool Activating;

	private StateMachine state
	{
		get
		{
			return PlayerFarming.Instance.state;
		}
	}

	public static void Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, string Skin, Action Callback)
	{
		Transform parent = ((RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		Create(StartPosition, EndPosition, parent, Duration, Skin, Callback);
	}

	public static FollowerSkinCustomTarget Create(Vector3 startPosition, Vector3 endPosition, Transform parent, float duration, string skin, Action callback)
	{
		FollowerSkinCustomTarget component = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/FollowerSkin Custom Target"), startPosition + Vector3.back * 0.5f, Quaternion.identity, parent) as GameObject).GetComponent<FollowerSkinCustomTarget>();
		component.Play(endPosition, duration, skin, callback);
		return component;
	}

	public void Play(Vector3 EndPosition, float Duration, string Skin, Action Callback)
	{
		PickedSkin = Skin;
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
		Debug.Log("Show Menu: Skin = " + PickedSkin);
		UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
		uINewItemOverlayController.Show(UINewItemOverlayController.TypeOfCard.FollowerSkin, PlayerFarming.Instance.gameObject.transform.position, PickedSkin);
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
