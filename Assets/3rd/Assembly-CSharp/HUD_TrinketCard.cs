using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HUD_TrinketCard : BaseMonoBehaviour
{
	public SkeletonGraphic Card;

	public Image FillImage;

	public GrowAndFade RechargeIcon;

	public TarotCards.TarotCard CardType { get; private set; } = new TarotCards.TarotCard(TarotCards.Card.Count, 0);


	private void Start()
	{
		if (Card != null)
		{
			Card.material.SetFloat("_GrayscaleLerpFade", 0f);
		}
	}

	private void Update()
	{
		if (FillImage != null)
		{
			FillImage.fillAmount = TrinketManager.GetRemainingCooldownPercent(CardType.CardType);
		}
	}

	public void SetCard(TarotCards.TarotCard card)
	{
		CardType = card;
		if (Card != null)
		{
			if (Card.Skeleton != null)
			{
				Card.Skeleton.SetSkin(TarotCards.Skin(card.CardType));
			}
			TrinketManager.OnTrinketCooldownStart += OnCooldownStart;
			TrinketManager.OnTrinketCooldownEnd += OnCooldownEnd;
		}
	}

	private void OnDisable()
	{
		if (Card != null)
		{
			TrinketManager.OnTrinketCooldownStart -= OnCooldownStart;
			TrinketManager.OnTrinketCooldownEnd -= OnCooldownEnd;
		}
	}

	private void OnDestroy()
	{
		if (Card != null)
		{
			TrinketManager.OnTrinketCooldownStart -= OnCooldownStart;
			TrinketManager.OnTrinketCooldownEnd -= OnCooldownEnd;
		}
	}

	private void OnCooldownStart(TarotCards.Card trinket)
	{
		if (trinket == CardType.CardType)
		{
			StartCoroutine(CooldownStartRoutine());
		}
	}

	private IEnumerator CooldownStartRoutine()
	{
		float fade = 0f;
		while (fade <= 1f)
		{
			fade += Time.deltaTime * 2f;
			Card.material.SetFloat("_GrayscaleLerpFade", fade);
			yield return null;
		}
	}

	private void OnCooldownEnd(TarotCards.Card trinket)
	{
		RechargeIcon.Play();
		Card.material.SetFloat("_GrayscaleLerpFade", 0f);
	}
}
