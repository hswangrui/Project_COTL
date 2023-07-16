using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public class UITarotDisplay : BaseMonoBehaviour
{
	[SerializeField]
	private TMP_Text title;

	[SerializeField]
	private TMP_Text descriptionText;

	[SerializeField]
	private TMP_Text loreText;

	public CanvasGroup canvasGroup;

	[Space]
	[SerializeField]
	private Vector3 offset;

	private RectTransform rectTransform;

	private GameObject lockPosition;

	[SerializeField]
	private Camera camera;

	private TarotCards.Card tarotCard;

	public void Play(TarotCards.Card card, GameObject lockPos)
	{
		tarotCard = card;
		LocalizeText();
		camera = Camera.main;
		lockPosition = lockPos;
		rectTransform = base.transform as RectTransform;
		Vector3 endValue = offset;
		offset += Vector3.up * 165f;
		DOTween.To(() => offset, delegate(Vector3 x)
		{
			offset = x;
		}, endValue, 0.3f).SetEase(Ease.OutBack);
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 0.5f);
	}

	private void LateUpdate()
	{
		if (!(lockPosition == null))
		{
			Vector2 vector = camera.WorldToScreenPoint(lockPosition.transform.position) + offset;
			rectTransform.position = vector;
		}
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += LocalizeText;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= LocalizeText;
	}

	private void LocalizeText()
	{
		title.text = TarotCards.LocalisedName(tarotCard);
		descriptionText.text = TarotCards.LocalisedDescription(tarotCard);
		loreText.text = TarotCards.LocalisedLore(tarotCard);
	}
}
