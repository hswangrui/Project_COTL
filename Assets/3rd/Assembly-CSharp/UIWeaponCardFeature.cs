using System.Collections;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWeaponCardFeature : BaseMonoBehaviour
{
	private Coroutine cFadeIn;

	private CanvasGroup canvasGroup;

	private RectTransform rectTransform;

	public TextMeshProUGUI Name;

	public TextMeshProUGUI Lore;

	public TextMeshProUGUI Description;

	public SkeletonGraphic Spine;

	[SerializeField]
	private Image BG;

	[SerializeField]
	private CanvasGroup rightCanvasGroup;

	private Vector3 StartingPos;

	public void Selected(Buttons Button, string Skin)
	{
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.DOKill();
		rectTransform.DOKill();
		UICardManager.ButtonsUICardManager buttonsUICardManager = Button as UICardManager.ButtonsUICardManager;
		if (!buttonsUICardManager.Card.Unlocked)
		{
			if (canvasGroup.alpha != 0f)
			{
				canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuart);
			}
			return;
		}
		canvasGroup.alpha = 0f;
		canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuart);
		rectTransform.DOShakePosition(0.5f, new Vector3(10f, 0f, 0f));
		Spine.Skeleton.SetSkin(Skin);
		Name.text = TarotCards.LocalisedName(buttonsUICardManager.Card.Type);
		Lore.text = TarotCards.LocalisedLore(buttonsUICardManager.Card.Type);
		Description.text = TarotCards.LocalisedDescription(buttonsUICardManager.Card.Type, 0);
	}

	public void Reveal()
	{
		StartCoroutine(RevealRoutine());
	}

	private IEnumerator RevealRoutine()
	{
		Spine.gameObject.transform.DOPunchScale(new Vector3(1f, 1f), 0.5f);
		Spine.AnimationState.SetAnimation(0, "menu-static-back", false);
		StartingPos = Spine.transform.position;
		BG.color = new Color(0f, 0f, 0f, 0f);
		rightCanvasGroup.alpha = 0f;
		Spine.transform.DOMove(base.gameObject.transform.position, 0f);
		yield return new WaitForSecondsRealtime(1f);
		Spine.AnimationState.AddAnimation(0, "menu-reveal", false, 0f);
		Spine.AnimationState.AddAnimation(0, "menu-static", true, 0f);
		yield return new WaitForSecondsRealtime(1f);
		Spine.transform.DOMove(StartingPos, 0.5f).SetEase(Ease.OutQuart);
		BG.DOFade(1f, 0.75f);
		rightCanvasGroup.DOFade(1f, 0.75f);
	}

	public void Selected(UICardManagerCard Card, string Skin)
	{
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.DOKill();
		rectTransform.DOKill();
		if (!Card.Card.Unlocked)
		{
			Debug.Log("Card is not unlocked");
			if (canvasGroup.alpha != 0f)
			{
				canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuart);
			}
		}
		else
		{
			canvasGroup.alpha = 0f;
			canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuart);
			rectTransform.DOShakePosition(0.5f, new Vector3(10f, 0f, 0f));
			Spine.Skeleton.SetSkin(Skin);
			Name.text = TarotCards.LocalisedName(Card.Card.Type);
			Lore.text = TarotCards.LocalisedLore(Card.Card.Type);
			Description.text = TarotCards.LocalisedDescription(Card.Card.Type, 0);
		}
	}
}
