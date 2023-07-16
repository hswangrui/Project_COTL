using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.Menus.DoctrineChoicesMenu;
using MMTools;
using src.Extensions;
using UnityEngine;

public class RitualUnlockCustomTarget : MonoBehaviour
{
	private DoctrineUpgradeSystem.DoctrineType unlockType;

	private const string Path = "Prefabs/Resources/RitualUnlock Custom Target";

	public SpriteRenderer SpriteRenderer;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	private TarotCards.TarotCard DrawnCard;

	private Action Callback;

	private int level;

	private UpgradeSystem.Type type;

	private StateMachine state
	{
		get
		{
			return PlayerFarming.Instance.state;
		}
	}

	public static void Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, DoctrineUpgradeSystem.DoctrineType _unlockType, Action Callback)
	{
		Transform parent = ((RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform);
		(UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/RitualUnlock Custom Target"), StartPosition + Vector3.back * 0.5f, Quaternion.identity, parent) as GameObject).GetComponent<RitualUnlockCustomTarget>().Play(EndPosition, Duration, _unlockType, Callback);
	}

	public void Play(Vector3 EndPosition, float Duration, DoctrineUpgradeSystem.DoctrineType _unlockType, Action Callback)
	{
		unlockType = _unlockType;
		switch (unlockType)
		{
		case DoctrineUpgradeSystem.DoctrineType.Special_Sacrifice:
			level = 1;
			type = UpgradeSystem.Type.Ritual_Sacrifice;
			break;
		case DoctrineUpgradeSystem.DoctrineType.Special_Brainwashed:
			level = 2;
			type = UpgradeSystem.Type.Ritual_Brainwashing;
			break;
		case DoctrineUpgradeSystem.DoctrineType.Special_Consume:
			level = 3;
			type = UpgradeSystem.Type.Ritual_ConsumeFollower;
			break;
		}
		this.Callback = Callback;
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.5f);
		sequence.Append(base.transform.DOMove(EndPosition + Vector3.back * 0.5f, Duration).SetEase(Ease.InBack).OnComplete(delegate
		{
			SpriteRenderer.enabled = false;
			StartCoroutine(GiveRitual());
		}));
		sequence.Play();
	}

	private IEnumerator GiveRitual()
	{
		MMConversation.CURRENT_CONVERSATION = new ConversationObject(null, null, null, new List<DoctrineResponse>
		{
			new DoctrineResponse(SermonCategory.Special, level, true, null)
		});
		UIDoctrineChoicesMenuController doctrineChoicesInstance = MonoSingleton<UIManager>.Instance.DoctrineChoicesMenuTemplate.Instantiate();
		doctrineChoicesInstance.Show();
		while (doctrineChoicesInstance.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		UpgradeSystem.UnlockAbility(type);
		BackToIdle();
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
