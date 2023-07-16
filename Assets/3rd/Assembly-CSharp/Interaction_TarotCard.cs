using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_TarotCard : Interaction
{
	private string sSummonTrinket;

	private string sNoAvailableTrinkets;

	public bool Activated;

	public SpriteRenderer sprite;

	private PlayerFarming pFarming;

	private CameraFollowTarget c;

	private int devotionCost;

	public Transform SpawnPosition;

	private TarotCards.TarotCard DrawnCard;

	public TarotCards.Card CardOverride { get; set; } = TarotCards.Card.Count;


	public bool ForceAllow { get; set; }

	private void Start()
	{
		UpdateLocalisation();
		if ((PlayerFleeceManager.FleecePreventTarotCards() && !ForceAllow) || TarotCards.DrawRandomCard() == null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sSummonTrinket = ScriptLocalization.Interactions.PickUp;
	}

	public override void GetLabel()
	{
		Interactable = true;
		base.Label = (Activated ? "" : sSummonTrinket);
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			base.OnInteract(state);
			Activated = true;
			state.CURRENT_STATE = StateMachine.State.InActive;
			DoRoutine();
		}
	}

	private IEnumerator CentrePlayer()
	{
		float Progress = 0f;
		Vector3 StartPosition = state.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * 2f);
			if (num < 0.5f)
			{
				state.transform.position = Vector3.Lerp(StartPosition, base.transform.position + Vector3.down, Mathf.SmoothStep(0f, 1f, Progress));
				yield return null;
				continue;
			}
			break;
		}
	}

	private void DoRoutine()
	{
		StartCoroutine(DoRoutineRoutine());
	}

	private IEnumerator DoRoutineRoutine()
	{
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		base.transform.DOScale(0f, 0.5f).SetEase(Ease.InOutBack).OnComplete(delegate
		{
			for (int j = 0; j < base.transform.childCount; j++)
			{
				base.transform.GetChild(j).gameObject.SetActive(false);
			}
		})
			.SetUpdate(true);
		GameManager.GetInstance().CameraSetTargetZoom(4f);
		LetterBox.Show(false);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.facingAngle = -90f;
		c = CameraFollowTarget.Instance;
		c.DisablePlayerLook = true;
		pFarming = state.GetComponent<PlayerFarming>();
		pFarming.SpineUseDeltaTime(false);
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
		pFarming.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
		pFarming.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
		if (CardOverride == TarotCards.Card.Count)
		{
			DrawnCard = TarotCards.DrawRandomCard();
		}
		else
		{
			DataManager.UnlockTrinket(CardOverride);
			NotificationCentre.Instance.PlayGenericNotification("UI/TarotMenu/NewTarotCardUnlocked");
			DrawnCard.CardType = CardOverride;
		}
		float t = 0f;
		while (t < 1f)
		{
			t += Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Lerp(1f, 0f, t / 1f);
			yield return null;
		}
		GameManager.GetInstance().CameraSetTargetZoom(6f);
		UITrinketCards.Play(DrawnCard, BackToIdle, 0f);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	private void BackToIdle()
	{
		StartCoroutine(BackToIdleRoutine());
	}

	private IEnumerator BackToIdleRoutine()
	{
		Time.timeScale = 1f;
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
		GameManager.GetInstance().CameraResetTargetZoom();
		CameraFollowTarget.Instance.DisablePlayerLook = false;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "cards/cards-stop-seperate", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine());
	}

	private IEnumerator DelayEffectsRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		TrinketManager.AddTrinket(DrawnCard);
		if (base.gameObject != null)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
