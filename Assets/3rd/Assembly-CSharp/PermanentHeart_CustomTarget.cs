using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PermanentHeart_CustomTarget : MonoBehaviour
{
	private const string Path = "Prefabs/Resources/Permanent Half Heart Custom Target";

	[SerializeField]
	private SpriteRenderer SpriteRenderer;

	[SerializeField]
	private Interaction_PermanentHeart InteractionPermanentHeart;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	private Action Callback;

	private bool Activating;

	private StateMachine state
	{
		get
		{
			return PlayerFarming.Instance.state;
		}
	}

	public static void Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, Action Callback)
	{
		Transform parent = ((RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		(UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/Permanent Half Heart Custom Target"), StartPosition + Vector3.back * 0.5f, Quaternion.identity, parent) as GameObject).GetComponent<PermanentHeart_CustomTarget>().Play(EndPosition, Duration, Callback);
	}

	public void Play(Vector3 EndPosition, float Duration, Action Callback)
	{
		InteractionPermanentHeart.enabled = false;
		InteractionPermanentHeart.Particles.gameObject.SetActive(false);
		this.Callback = Callback;
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.5f);
		sequence.Append(base.transform.DOMove(EndPosition + Vector3.back * 0.5f, Duration).SetEase(Ease.InBack).OnComplete(delegate
		{
			InteractionPermanentHeart.enabled = true;
			InteractionPermanentHeart.OnInteract(PlayerFarming.Instance.state);
		}));
		sequence.Play();
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
