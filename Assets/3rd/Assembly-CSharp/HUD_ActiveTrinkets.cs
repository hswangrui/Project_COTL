using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_ActiveTrinkets : BaseMonoBehaviour
{
	public static HUD_ActiveTrinkets Instance;

	public HUD_TrinketCard CardPrefab;

	private List<HUD_TrinketCard> _cards = new List<HUD_TrinketCard>();

	public bool movePosition = true;

	public AnimationCurve animationCurve;

	private void OnEnable()
	{
		Instance = this;
		OnTrinketsChanged();
	}

	private void Start()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(OnTrinketsChanged));
		TrinketManager.OnTrinketAdded += OnTrinketAdded;
		TrinketManager.OnTrinketRemoved += OnTrinketRemoved;
		TrinketManager.OnTrinketsCleared += OnTrinketsChanged;
	}

	private void OnDestroy()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnTrinketsChanged));
		TrinketManager.OnTrinketAdded -= OnTrinketAdded;
		TrinketManager.OnTrinketRemoved -= OnTrinketRemoved;
		TrinketManager.OnTrinketsCleared -= OnTrinketsChanged;
	}

	private void OnTrinketAdded(TarotCards.Card card)
	{
		OnTrinketsChanged();
	}

	private void OnTrinketRemoved(TarotCards.Card trinket)
	{
		HUD_TrinketCard hUD_TrinketCard = null;
		foreach (HUD_TrinketCard card in _cards)
		{
			if (card.CardType.CardType == trinket)
			{
				hUD_TrinketCard = card;
				break;
			}
		}
		if (hUD_TrinketCard != null)
		{
			_cards.Remove(hUD_TrinketCard);
			OnTrinketsChanged();
			StartCoroutine(CardAnimateDestroy(hUD_TrinketCard));
		}
	}

	private void OnTrinketsChanged()
	{
		int i;
		for (i = 0; i < DataManager.Instance.PlayerRunTrinkets.Count; i++)
		{
			TarotCards.TarotCard card = DataManager.Instance.PlayerRunTrinkets[i];
			HUD_TrinketCard hUD_TrinketCard;
			if (i >= _cards.Count)
			{
				hUD_TrinketCard = UnityEngine.Object.Instantiate(CardPrefab, base.transform);
				StartCoroutine(CardAnimateIn(hUD_TrinketCard));
				_cards.Add(hUD_TrinketCard);
			}
			else
			{
				hUD_TrinketCard = _cards[i];
			}
			hUD_TrinketCard.gameObject.SetActive(true);
			hUD_TrinketCard.SetCard(card);
		}
		for (; i < _cards.Count; i++)
		{
			_cards[i].gameObject.SetActive(false);
		}
	}

	private IEnumerator CardAnimateIn(HUD_TrinketCard card)
	{
		float Progress = 0f;
		float Duration = 0.5f;
		if (movePosition)
		{
			card.transform.localPosition = new Vector3(card.transform.localPosition.x, -200f);
		}
		if (card.Card != null)
		{
			card.Card.enabled = true;
		}
		if (!movePosition)
		{
			yield break;
		}
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				float y = Mathf.LerpUnclamped(-200f, 0f, animationCurve.Evaluate(Progress / Duration));
				card.transform.localPosition = new Vector3(card.transform.localPosition.x, y);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator CardAnimateDestroy(HUD_TrinketCard card)
	{
		float Progress2 = 0f;
		float Duration = 0.5f;
		if (movePosition)
		{
			card.transform.localPosition = new Vector3(card.transform.localPosition.x, 0f);
		}
		if (movePosition)
		{
			while (true)
			{
				float num;
				Progress2 = (num = Progress2 + Time.deltaTime);
				if (!(num < Duration))
				{
					break;
				}
				float y = Mathf.LerpUnclamped(0f, 200f, animationCurve.Evaluate(Progress2 / Duration));
				card.transform.localPosition = new Vector3(card.transform.localPosition.x, y);
				yield return null;
			}
		}
		Progress2 = 0f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.LerpUnclamped(0f, 1f, animationCurve.Evaluate(1f - Progress2 / Duration));
			card.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		UnityEngine.Object.Destroy(card.gameObject);
	}
}
