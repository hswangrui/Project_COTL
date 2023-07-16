using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class TarotCustomTarget : MonoBehaviour
{
	private const string Path = "Prefabs/Resources/Tarot Custom Target";

	public SpriteRenderer SpriteRenderer;

	public TarotCards.Card CardOverride = TarotCards.Card.Count;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	public GameObject Menu;

	public TarotCards.Card CardType = TarotCards.Card.Count;

	private TarotCards.TarotCard DrawnCard;

	private Action Callback;

	private bool Activating;

	private StateMachine state
	{
		get
		{
			return PlayerFarming.Instance.state;
		}
	}

	public static TarotCustomTarget Create(Vector3 StartPosition, Vector3 EndPosition, float Duration, TarotCards.Card CardType, Action Callback)
	{
		TarotCustomTarget component = (UnityEngine.Object.Instantiate(parent: (!(RoomManager.Instance != null)) ? BiomeConstants.Instance.gameObject.transform : ((RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null) ? RoomManager.Instance.CurrentRoomPrefab.transform : GameObject.FindGameObjectWithTag("Unit Layer").transform), original: Resources.Load("Prefabs/Resources/Tarot Custom Target"), position: StartPosition + Vector3.back * 0.5f, rotation: Quaternion.identity) as GameObject).GetComponent<TarotCustomTarget>();
		component.Play(EndPosition, Duration, CardType, Callback);
		return component;
	}

	public void Play(Vector3 EndPosition, float Duration, TarotCards.Card CardType, Action Callback)
	{
		this.CardType = CardType;
		this.Callback = Callback;
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.5f);
		sequence.Append(base.transform.DOMove(EndPosition + Vector3.back * 0.5f, Duration).SetEase(Ease.InBack).OnComplete(delegate
		{
			SpriteRenderer.enabled = false;
			StartCoroutine(DoRoutineRoutine());
		}));
		sequence.Play();
	}

	private IEnumerator DoRoutineRoutine()
	{
		GameManager.GetInstance().CameraSetTargetZoom(4f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.facingAngle = -90f;
		c = CameraFollowTarget.Instance;
		c.DisablePlayerLook = true;
		pFarming = PlayerFarming.Instance;
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().CameraSetTargetZoom(6f);
		OpenMenu();
	}

	private void OpenMenu()
	{
		HUD_Manager.Instance.Hide(false, 0);
		GameManager.GetInstance().CameraSetOffset(new Vector3(-3f, 0f, 0f));
		UITarotCardsMenuController uITarotCardsMenuController = MonoSingleton<UIManager>.Instance.TarotCardsMenuTemplate.Instantiate();
		uITarotCardsMenuController.Show(CardType);
		uITarotCardsMenuController.OnHide = (Action)Delegate.Combine(uITarotCardsMenuController.OnHide, (Action)delegate
		{
			HUD_Manager.Instance.Show(0);
		});
		uITarotCardsMenuController.OnHidden = (Action)Delegate.Combine(uITarotCardsMenuController.OnHidden, (Action)delegate
		{
			GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
			BackToIdle();
		});
		DrawnCard = new TarotCards.TarotCard(CardType, 0);
	}

	private void BackToIdle()
	{
		StartCoroutine(BackToIdleRoutine());
	}

	private IEnumerator BackToIdleRoutine()
	{
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
		GameManager.GetInstance().CameraResetTargetZoom();
		c.DisablePlayerLook = false;
		GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		GameManager.GetInstance().AddPlayerToCamera();
		yield return null;
		pFarming.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
		pFarming.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(1.8333334f);
		StopAllCoroutines();
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine());
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator DelayEffectsRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		if (LocationManager.LocationIsDungeon(PlayerFarming.Location))
		{
			TrinketManager.AddTrinket(DrawnCard);
		}
		NotificationCentre.Instance.PlayGenericNotification("UI/TarotMenu/NewTarotCardUnlocked");
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
