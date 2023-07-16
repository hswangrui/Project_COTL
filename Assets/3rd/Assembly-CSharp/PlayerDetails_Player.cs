using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMTools;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDetails_Player : BaseMonoBehaviour
{
	public GameObject TarotCardPrefab;

	public GameObject BlankTarotCardPrefab;

	public UINavigator UINavigator;

	public Transform TarotCardParent;

	public CanvasGroup canvasGroup;

	private List<HUD_TrinketCard> _cards = new List<HUD_TrinketCard>();

	public List<WeaponCurseIcons> WeaponIcons = new List<WeaponCurseIcons>();

	public GameObject NoTarotCards;

	public Image Weapon;

	public TextMeshProUGUI WeaponTitle;

	public TextMeshProUGUI WeaponDescription;

	public TextMeshProUGUI WeaponDamage;

	public GameObject WeaponProgressBar;

	public Image WeaponProgress;

	public Image Curse;

	public TextMeshProUGUI CurseTitle;

	public TextMeshProUGUI CurseDescription;

	public TextMeshProUGUI CurseDamage;

	public GameObject CurseProgressBar;

	public Image CurseProgress;

	public Image Ability;

	public TextMeshProUGUI AbilityTitle;

	public TextMeshProUGUI AbilityDescription;

	public GameObject AbilityProgressBar;

	public Image AbilityProgress;

	public MMControlPrompt controlPrompt;

	public TextMeshProUGUI controlPromptTxt;

	private bool createdTrinket;

	public GameObject BlackTint;

	public GameObject canvas;

	public List<HUD_TrinketCard> Cards = new List<HUD_TrinketCard>();

	private bool TarotCardActive;

	public AnimationCurve animationCurve;

	public static string GetWeaponLevel(int _WeaponLevel)
	{
		switch (_WeaponLevel)
		{
		case 0:
			return "";
		case 1:
			return "I";
		case 2:
			return "II";
		case 3:
			return "III";
		case 4:
			return "IV";
		case 5:
			return "V";
		case 6:
			return "VI";
		case 7:
			return "VII";
		case 8:
			return "VIII";
		case 9:
			return "IX";
		case 10:
			return "X";
		case 11:
			return "XI";
		case 12:
			return "XII";
		case 13:
			return "XIII";
		case 14:
			return "XIV";
		case 15:
			return "XV";
		default:
			return "";
		}
	}

	public static string GetWeaponMod(TarotCards.Card card)
	{
		switch (card)
		{
		case TarotCards.Card.DiseasedHeart:
			return "<color=red>Cursed</color> ";
		case TarotCards.Card.Spider:
			return "<color=green>Poison</color> ";
		default:
			return "";
		}
	}

	public static string GetWeaponCondition(int _WeaponDurability)
	{
		switch (_WeaponDurability)
		{
		case 0:
			return "";
		case 1:
			return "Rusty ";
		case 2:
			return "Polished ";
		case 3:
			return "Blessed ";
		default:
			return "";
		}
	}

	private void GetWeapon()
	{
	}

	private void GetAbility()
	{
		AbilityTitle.text = "Replenesh Health";
		AbilityDescription.text = "Enlightenment fills your viens fueled by the death of non-beleivers";
		controlPrompt.Action = 9;
		controlPromptTxt.text = "Hold When Faith bar full";
	}

	private void ButtonDown()
	{
		Debug.Log("On Select Down");
		HUD_TrinketCard component = UINavigator.selectable.gameObject.GetComponent<HUD_TrinketCard>();
		if (component != null && !createdTrinket && component.CardType.CardType != TarotCards.Card.Count)
		{
			BlackTint.SetActive(true);
			createdTrinket = true;
			canvasGroup.interactable = false;
			UINavigator.enabled = false;
			UITrinketCards.PlayFromPause(component.CardType, TarotCardPopUpDone, canvas.gameObject);
		}
	}

	private void TarotCardPopUpDone()
	{
		BlackTint.SetActive(false);
		createdTrinket = false;
		canvasGroup.interactable = true;
		UINavigator.enabled = true;
	}

	private void OnDisable()
	{
		UINavigator uINavigator = UINavigator;
		uINavigator.OnSelectDown = (Action)Delegate.Remove(uINavigator.OnSelectDown, new Action(ButtonDown));
		UINavigator uINavigator2 = UINavigator;
		uINavigator2.OnChangeSelectionUnity = (UINavigator.ChangeSelectionUnity)Delegate.Remove(uINavigator2.OnChangeSelectionUnity, new UINavigator.ChangeSelectionUnity(OnChangeSelectionUnity));
	}

	private void OnChangeSelectionUnity(Selectable PrevSelectable, Selectable NewSelectable)
	{
		ShortcutExtensions.DOPunchScale(punch: new Vector3(1.1f, 1.1f, 1.1f), target: NewSelectable.transform, duration: 0.5f);
	}

	private void OnEnable()
	{
		Debug.Log("Player Details Enabled");
		if (SceneManager.GetActiveScene().name == "Base Biome 1")
		{
			DataManager.Instance.PlayerRunTrinkets.Clear();
		}
		BlackTint.SetActive(false);
		createdTrinket = false;
		UINavigator uINavigator = UINavigator;
		uINavigator.OnChangeSelectionUnity = (UINavigator.ChangeSelectionUnity)Delegate.Combine(uINavigator.OnChangeSelectionUnity, new UINavigator.ChangeSelectionUnity(OnChangeSelectionUnity));
		UINavigator uINavigator2 = UINavigator;
		uINavigator2.OnSelectDown = (Action)Delegate.Combine(uINavigator2.OnSelectDown, new Action(ButtonDown));
		if (Cards.Count == 0)
		{
			InitTrinketCards();
		}
		else
		{
			SetTrinketsCards();
		}
		GetWeapon();
		GetAbility();
	}

	public void InitTrinketCards()
	{
		int num = -1;
		while (++num < 9)
		{
			GameObject obj = UnityEngine.Object.Instantiate(TarotCardPrefab, TarotCardParent);
			obj.SetActive(true);
			RectTransform component = obj.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.5f, 0.5f);
			component.anchorMax = new Vector2(0.5f, 0.5f);
			component.pivot = new Vector2(0.5f, 0.5f);
			HUD_TrinketCard component2 = obj.GetComponent<HUD_TrinketCard>();
			Cards.Add(component2);
		}
		SetTrinketsCards();
		UINavigator.startingItem = Cards[0].GetComponent<Selectable>();
	}

	public void SetTrinketsCards()
	{
		TarotCardActive = false;
		int num = -1;
		if (DataManager.Instance.PlayerRunTrinkets.Count > 0)
		{
			while (++num < DataManager.Instance.PlayerRunTrinkets.Count)
			{
				if (num < 9)
				{
					Debug.Log(DataManager.Instance.PlayerRunTrinkets[num].CardType);
					TarotCards tarotCards = TarotCards.Create(DataManager.Instance.PlayerRunTrinkets[num].CardType, true);
					Cards[num].SetCard(new TarotCards.TarotCard(tarotCards.Type, 0));
					Cards[num].Card.enabled = true;
					Cards[num].GetComponent<Button>().enabled = true;
					UINavigator.Buttons[num].canUse = true;
					TarotCardActive = true;
				}
			}
		}
		if (DataManager.Instance.PlayerRunTrinkets.Count < 9)
		{
			while (++num < 9 - DataManager.Instance.PlayerRunTrinkets.Count)
			{
				Cards[num].Card.enabled = false;
				Cards[num].GetComponent<Button>().enabled = false;
				UINavigator.Buttons[num].canUse = false;
			}
		}
		if (!TarotCardActive)
		{
			NoTarotCards.SetActive(true);
		}
		else
		{
			NoTarotCards.SetActive(false);
		}
	}

	private IEnumerator CardAnimateIn(HUD_TrinketCard card)
	{
		float Progress = 0f;
		float Duration = 0.5f;
		card.transform.localPosition = new Vector3(card.transform.localPosition.x, -200f);
		card.Card.enabled = true;
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
}
