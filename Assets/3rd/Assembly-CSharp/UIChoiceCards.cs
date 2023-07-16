using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChoiceCards : BaseMonoBehaviour
{
	public GameObject ChoiceCardPrefab;

	public RectTransform Container;

	public UI_NavigatorSimple UINavigator;

	public List<UIChoiceCard> ChoiceCards;

	public Action MakePayment;

	public Action<ChoiceReward> CompleteCallback;

	public Action CancelCallback;

	public void Play(List<ChoiceReward> Rewards, Action MakePayment, Action<ChoiceReward> CompleteCallback)
	{
		Time.timeScale = 0f;
		ChoiceCards = new List<UIChoiceCard>();
		int num = -1;
		while (++num < Rewards.Count)
		{
			GameObject obj = UnityEngine.Object.Instantiate(ChoiceCardPrefab, Container);
			obj.SetActive(true);
			UIChoiceCard component = obj.GetComponent<UIChoiceCard>();
			component.Play(Rewards[num], (float)num * 0.2f);
			ChoiceCards.Add(component);
		}
		UINavigator.startingItem = ChoiceCards[0].gameObject.GetComponent<Selectable>();
		UINavigator.setDefault();
		UI_NavigatorSimple uINavigator = UINavigator;
		uINavigator.OnCancelDown = (Action)Delegate.Combine(uINavigator.OnCancelDown, new Action(OnClose));
		UI_NavigatorSimple uINavigator2 = UINavigator;
		uINavigator2.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Combine(uINavigator2.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelectionUnity));
		UI_NavigatorSimple uINavigator3 = UINavigator;
		uINavigator3.OnSelectDown = (Action)Delegate.Combine(uINavigator3.OnSelectDown, new Action(OnSelect));
		this.MakePayment = MakePayment;
		this.CompleteCallback = CompleteCallback;
	}

	private void OnChangeSelectionUnity(Selectable PrevSelectable, Selectable NewSelectable)
	{
		PrevSelectable.gameObject.GetComponent<UIChoiceCard>().OnDehighlighted();
		NewSelectable.gameObject.GetComponent<UIChoiceCard>().OnHighlighted();
	}

	private void OnClose()
	{
		if (CancelCallback != null)
		{
			Action cancelCallback = CancelCallback;
			if (cancelCallback != null)
			{
				cancelCallback();
			}
			Close();
		}
	}

	private void Close()
	{
		UI_NavigatorSimple uINavigator = UINavigator;
		uINavigator.OnCancelDown = (Action)Delegate.Remove(uINavigator.OnCancelDown, new Action(OnClose));
		Time.timeScale = 1f;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnSelect()
	{
		int num = -1;
		while (++num < ChoiceCards.Count)
		{
			if (!(UINavigator.selectable.gameObject == ChoiceCards[num].gameObject))
			{
				continue;
			}
			if (ChoiceCards[num].Reward.Locked)
			{
				ChoiceCards[num].Shake();
				return;
			}
			if (!ChoiceCards[num].CanAfford())
			{
				ChoiceCards[num].Shake();
				return;
			}
			if (ChoiceCards[num].Play(CancelCallback))
			{
				Action<ChoiceReward> completeCallback = CompleteCallback;
				if (completeCallback != null)
				{
					completeCallback(ChoiceCards[num].Reward);
				}
			}
		}
		Action makePayment = MakePayment;
		if (makePayment != null)
		{
			makePayment();
		}
		UINavigator.canvasGroup.interactable = false;
		Close();
	}

	private void OnDisable()
	{
		UI_NavigatorSimple uINavigator = UINavigator;
		uINavigator.OnCancelDown = (Action)Delegate.Combine(uINavigator.OnCancelDown, new Action(OnClose));
		UI_NavigatorSimple uINavigator2 = UINavigator;
		uINavigator2.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Remove(uINavigator2.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelectionUnity));
		UI_NavigatorSimple uINavigator3 = UINavigator;
		uINavigator3.OnSelectDown = (Action)Delegate.Remove(uINavigator3.OnSelectDown, new Action(OnSelect));
	}
}
