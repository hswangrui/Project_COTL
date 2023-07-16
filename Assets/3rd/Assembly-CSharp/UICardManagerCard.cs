using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class UICardManagerCard : BaseMonoBehaviour
{
	public GameObject ControlPrompt;

	public GameObject RadialProgressBarGameObject;

	public Image RadialProgressBar;

	public Image RadialProgressBarWhite;

	public SkeletonGraphic Spine;

	public RectTransform Parent;

	public bool Unlocking;

	public Vector2 Shake = Vector2.zero;

	public Vector2 ShakeSpeed = Vector2.zero;

	public Vector2 ShakeDamping = Vector2.zero;

	private float SmoothStepRadial;

	private float SmoothStepStart;

	private float SmoothStepTarget;

	public float UnlockProgressWait;

	public GameObject newCardIcon;

	public TarotCards Card;

	private void Start()
	{
		Image radialProgressBar = RadialProgressBar;
		float fillAmount = (RadialProgressBarWhite.fillAmount = 0f);
		radialProgressBar.fillAmount = fillAmount;
		UnlockProgressWait = Card.UnlockProgress;
		Deselected();
	}

	public void DoShake(float X, float Y)
	{
		ShakeSpeed += new Vector2(X, Y);
	}

	private void Update()
	{
		ShakeSpeed.x += (0f - Shake.x) * 0.2f;
		Shake.x += (ShakeSpeed.x *= 0.8f);
		ShakeSpeed.y += (0f - Shake.y) * 0.2f;
		Shake.y += (ShakeSpeed.y *= 0.8f);
		Parent.localPosition = Shake;
		RadialProgressBar.fillAmount = Mathf.SmoothStep(SmoothStepStart, SmoothStepTarget, SmoothStepRadial += 2f * Time.deltaTime);
	}

	public void UnlockCard()
	{
		Card.Unlocked = true;
		RadialProgressBarGameObject.SetActive(!Card.Unlocked);
		Spine.AnimationState.SetAnimation(0, "menu-reveal", false);
		Spine.AnimationState.Complete += CompletedAnimation;
		TarotCards.UnlockTrinket(Card.Type);
		newCardIcon.SetActive(true);
	}

	private void CompletedAnimation(TrackEntry trackEntry)
	{
		Unlocking = false;
		Spine.AnimationState.Complete -= CompletedAnimation;
		Spine.AnimationState.SetAnimation(0, "menu-static", true);
		ControlPrompt.SetActive(false);
	}

	private void OnDisable()
	{
		Spine.AnimationState.Complete -= CompletedAnimation;
	}

	public void Deselected()
	{
		ControlPrompt.SetActive(false);
	}

	public void Selected()
	{
		ControlPrompt.SetActive(!Card.Unlocked);
	}

	public void SetCard(TarotCards Card)
	{
		this.Card = Card;
		if (TarotCards.IsUnlocked(Card.Type))
		{
			Card.Unlocked = false;
		}
		else
		{
			Card.Unlocked = true;
		}
		Spine.AnimationState.SetAnimation(0, Card.Unlocked ? "menu-static" : "menu-static-back", true);
		RadialProgressBarGameObject.SetActive(false);
		ControlPrompt.SetActive(false);
	}

	public void SetSkin(string Skin)
	{
		Spine.Skeleton.SetSkin(Skin);
	}
}
