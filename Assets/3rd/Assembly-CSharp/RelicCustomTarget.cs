using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class RelicCustomTarget : MonoBehaviour
{
	private const string Path = "Prefabs/Resources/Relic Custom Target";

	public SpriteRenderer SpriteRenderer;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	public RelicType RelicType;

	private Action Callback;

	private bool Activating;

	private StateMachine state
	{
		get
		{
			return PlayerFarming.Instance.state;
		}
	}

	public static GameObject Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, RelicType relicType, Action Callback)
	{
		Transform parent = ((RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/Relic Custom Target"), StartPosition + Vector3.back * 0.5f, Quaternion.identity, parent) as GameObject;
		obj.GetComponent<RelicCustomTarget>().Play(EndPosition, Duration, relicType, Callback);
		return obj;
	}

	public void Play(Vector3 EndPosition, float Duration, RelicType relicType, Action Callback)
	{
		RelicType = relicType;
		this.Callback = Callback;
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		SpriteRenderer.sprite = EquipmentManager.GetRelicData(relicType).UISprite;
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
		AudioManager.Instance.PlayOneShot("event:/temple_key/fragment_pickup", base.gameObject);
		BiomeConstants.Instance.EmitPickUpVFX(base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(0.7f, 0.9f, 0.3f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		if (RelicType != 0)
		{
			DataManager.UnlockRelic(RelicType);
		}
		UIRelicMenuController uIRelicMenuController = MonoSingleton<UIManager>.Instance.RelicMenuTemplate.Instantiate();
		uIRelicMenuController.Show(RelicType);
		uIRelicMenuController.OnHidden = (Action)Delegate.Combine(uIRelicMenuController.OnHidden, new Action(BackToIdle));
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
