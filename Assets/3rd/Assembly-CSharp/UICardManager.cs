using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UICardManager : BaseMonoBehaviour
{
	public class ButtonsUICardManager : Buttons
	{
		[HideInInspector]
		public UICardManagerCard uICardManagerCard;

		[HideInInspector]
		public TarotCards Card;

		[HideInInspector]
		public SermonsAndRituals.SermonRitualType SermonRitualType;

		public ButtonsUICardManager(GameObject Button, int Index, buttons buttonTypes, bool selected, TarotCards Card, UICardManagerCard uICardManagerCard)
		{
			base.Button = Button;
			base.Index = Index;
			base.buttonTypes = buttonTypes;
			base.selected = selected;
			this.Card = Card;
			this.uICardManagerCard = uICardManagerCard;
		}
	}

	public GameObject CardPrefab;

	public GameObject BlankCardPrefab;

	public TextMeshProUGUI SoulsCount;

	public GameObject SoulPrefab;

	public RectTransform SoulSpawnPoint;

	public UIWeaponCardFeature DisplayFeature;

	public Action CallBack;

	public UINavigator WeaponsUINavigator;

	public RectTransform WeaponRT;

	public UINavigator CursesUINavigator;

	public RectTransform CursesRT;

	public UINavigator TrinketsUINavigator;

	public RectTransform TrinketRT;

	private void Start()
	{
		SoulsCount.text = "<sprite name=\"icon_spirits\"> " + Inventory.Souls;
		StartCoroutine(Begin());
		SpawnTrinketsCards();
		DisplayFeature.gameObject.SetActive(false);
		GameManager.GetInstance().CameraSetOffset(new Vector3(-3f, 0f, 0f));
	}

	private IEnumerator Begin()
	{
		WeaponsUINavigator.ControlsEnabled = false;
		CursesUINavigator.ControlsEnabled = false;
		TrinketsUINavigator.ControlsEnabled = false;
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num > 0.5f))
			{
				break;
			}
			yield return null;
		}
		WeaponsUINavigator.ControlsEnabled = true;
		CursesUINavigator.ControlsEnabled = true;
		TrinketsUINavigator.ControlsEnabled = true;
	}

	public void OnClose()
	{
		Action callBack = CallBack;
		if (callBack != null)
		{
			callBack();
		}
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDisable()
	{
		UINavigator weaponsUINavigator = WeaponsUINavigator;
		weaponsUINavigator.OnChangeSelection = (UINavigator.ChangeSelection)Delegate.Remove(weaponsUINavigator.OnChangeSelection, new UINavigator.ChangeSelection(OnChangeWeaponSelection));
		UINavigator weaponsUINavigator2 = WeaponsUINavigator;
		weaponsUINavigator2.OnButtonDown = (UINavigator.ButtonDown)Delegate.Remove(weaponsUINavigator2.OnButtonDown, new UINavigator.ButtonDown(OnWeaponButtonDown));
		UINavigator weaponsUINavigator3 = WeaponsUINavigator;
		weaponsUINavigator3.OnClose = (UINavigator.Close)Delegate.Remove(weaponsUINavigator3.OnClose, new UINavigator.Close(OnClose));
		UINavigator cursesUINavigator = CursesUINavigator;
		cursesUINavigator.OnChangeSelection = (UINavigator.ChangeSelection)Delegate.Remove(cursesUINavigator.OnChangeSelection, new UINavigator.ChangeSelection(OnChangeCursesSelection));
		UINavigator cursesUINavigator2 = CursesUINavigator;
		cursesUINavigator2.OnButtonDown = (UINavigator.ButtonDown)Delegate.Remove(cursesUINavigator2.OnButtonDown, new UINavigator.ButtonDown(OnCursesButtonDown));
		UINavigator cursesUINavigator3 = CursesUINavigator;
		cursesUINavigator3.OnClose = (UINavigator.Close)Delegate.Remove(cursesUINavigator3.OnClose, new UINavigator.Close(OnClose));
		UINavigator trinketsUINavigator = TrinketsUINavigator;
		trinketsUINavigator.OnChangeSelection = (UINavigator.ChangeSelection)Delegate.Remove(trinketsUINavigator.OnChangeSelection, new UINavigator.ChangeSelection(OnChangeTrinketsSelection));
		UINavigator trinketsUINavigator2 = TrinketsUINavigator;
		trinketsUINavigator2.OnButtonDown = (UINavigator.ButtonDown)Delegate.Remove(trinketsUINavigator2.OnButtonDown, new UINavigator.ButtonDown(OnTrinketsButtonDown));
		UINavigator trinketsUINavigator3 = TrinketsUINavigator;
		trinketsUINavigator3.OnClose = (UINavigator.Close)Delegate.Remove(trinketsUINavigator3.OnClose, new UINavigator.Close(OnClose));
	}

	public void CreateSoul(Buttons CurrentButton, UIWeaponCardSoul.CardComplete Callback)
	{
		(CurrentButton as ButtonsUICardManager).uICardManagerCard.UnlockProgressWait += 0.1f;
		UIWeaponCardSoul component = UnityEngine.Object.Instantiate(SoulPrefab, Vector3.zero, Quaternion.identity, base.transform).GetComponent<UIWeaponCardSoul>();
		component.Play(TargetPosition: CurrentButton.Button.GetComponent<RectTransform>().position, CurrentButtons: CurrentButton, StartPosition: SoulSpawnPoint.position);
		Inventory.Souls--;
		SoulsCount.text = "<sprite name=\"icon_spirits\"> " + Inventory.Souls;
		component.OnCardComplete = (UIWeaponCardSoul.CardComplete)Delegate.Combine(component.OnCardComplete, Callback);
	}

	private void OnCardCompleteWeapon(Buttons CurrentButton)
	{
		(CurrentButton as ButtonsUICardManager).Card.UnlockProgress += 0.1f;
		UpdateCard(CurrentButton as ButtonsUICardManager);
		if ((CurrentButton as ButtonsUICardManager).Card.UnlockProgress >= 1f)
		{
			StartCoroutine(UnlockCard(CurrentButton as ButtonsUICardManager, WeaponsUINavigator));
		}
	}

	private void OnCardCompleteCurses(Buttons CurrentButton)
	{
		(CurrentButton as ButtonsUICardManager).Card.UnlockProgress += 0.1f;
		UpdateCard(CurrentButton as ButtonsUICardManager);
		if ((CurrentButton as ButtonsUICardManager).Card.UnlockProgress >= 1f)
		{
			StartCoroutine(UnlockCard(CurrentButton as ButtonsUICardManager, CursesUINavigator));
		}
	}

	private void OnCardCompleteTrinkets(Buttons CurrentButton)
	{
		(CurrentButton as ButtonsUICardManager).Card.UnlockProgress += 0.1f;
		UpdateCard(CurrentButton as ButtonsUICardManager);
		if ((CurrentButton as ButtonsUICardManager).Card.UnlockProgress >= 1f)
		{
			StartCoroutine(UnlockCard(CurrentButton as ButtonsUICardManager, TrinketsUINavigator));
		}
	}

	public void SpawnWeaponCards()
	{
		int num = -1;
		while (++num <= 3)
		{
			UnityEngine.Object.Instantiate(BlankCardPrefab, WeaponRT);
		}
		UINavigator weaponsUINavigator = WeaponsUINavigator;
		weaponsUINavigator.OnChangeSelection = (UINavigator.ChangeSelection)Delegate.Combine(weaponsUINavigator.OnChangeSelection, new UINavigator.ChangeSelection(OnChangeWeaponSelection));
		UINavigator weaponsUINavigator2 = WeaponsUINavigator;
		weaponsUINavigator2.OnDeselect = (UINavigator.Deselect)Delegate.Combine(weaponsUINavigator2.OnDeselect, new UINavigator.Deselect(OnDeselect));
		UINavigator weaponsUINavigator3 = WeaponsUINavigator;
		weaponsUINavigator3.OnButtonDown = (UINavigator.ButtonDown)Delegate.Combine(weaponsUINavigator3.OnButtonDown, new UINavigator.ButtonDown(OnWeaponButtonDown));
		UINavigator weaponsUINavigator4 = WeaponsUINavigator;
		weaponsUINavigator4.OnClose = (UINavigator.Close)Delegate.Combine(weaponsUINavigator4.OnClose, new UINavigator.Close(OnClose));
		WeaponsUINavigator.setDefault();
	}

	private void OnDeselect(Buttons Button)
	{
		(Button as ButtonsUICardManager).uICardManagerCard.Deselected();
	}

	private void OnChangeWeaponSelection(Buttons Button)
	{
		(Button as ButtonsUICardManager).uICardManagerCard.Selected();
		DisplayFeature.Selected(Button, "Weapons/" + (Button as ButtonsUICardManager).Card.Type);
	}

	private void OnWeaponButtonDown(Buttons CurrentButton)
	{
		if ((CurrentButton as ButtonsUICardManager).uICardManagerCard.UnlockProgressWait < 1f && Inventory.Souls > 0)
		{
			CreateSoul(CurrentButton, OnCardCompleteWeapon);
		}
	}

	private void UpdateCard(ButtonsUICardManager CurrentButton)
	{
		if (!CurrentButton.Card.Unlocked)
		{
			float num = UnityEngine.Random.Range(-10, -5);
			float num2 = 130 + UnityEngine.Random.Range(-45, 45);
			CurrentButton.uICardManagerCard.DoShake(num + Mathf.Cos(num2 * ((float)Math.PI / 180f)), num + Mathf.Sin(num2 * ((float)Math.PI / 180f)));
			CurrentButton.uICardManagerCard.SetCard(CurrentButton.Card);
		}
	}

	private IEnumerator UnlockCard(ButtonsUICardManager CurrentButton, UINavigator uINavigator)
	{
		uINavigator.ControlsEnabled = false;
		while (CurrentButton.uICardManagerCard.Unlocking)
		{
			yield return null;
		}
		if (uINavigator == WeaponsUINavigator)
		{
			DisplayFeature.Selected(CurrentButton, "Weapons/" + CurrentButton.Card.Type);
		}
		else if (uINavigator == CursesUINavigator)
		{
			DisplayFeature.Selected(CurrentButton, "Curses/" + CurrentButton.Card.Type);
		}
		else if (uINavigator == TrinketsUINavigator)
		{
			DisplayFeature.Selected(CurrentButton, TarotCards.Skin(CurrentButton.Card.Type));
		}
		yield return new WaitForSeconds(0.5f);
		uINavigator.ControlsEnabled = true;
	}

	public void SpawnCursesCards()
	{
		int num = -1;
		while (++num <= 3)
		{
			UnityEngine.Object.Instantiate(BlankCardPrefab, CursesRT);
		}
		UINavigator cursesUINavigator = CursesUINavigator;
		cursesUINavigator.OnChangeSelection = (UINavigator.ChangeSelection)Delegate.Combine(cursesUINavigator.OnChangeSelection, new UINavigator.ChangeSelection(OnChangeCursesSelection));
		UINavigator cursesUINavigator2 = CursesUINavigator;
		cursesUINavigator2.OnButtonDown = (UINavigator.ButtonDown)Delegate.Combine(cursesUINavigator2.OnButtonDown, new UINavigator.ButtonDown(OnCursesButtonDown));
		UINavigator cursesUINavigator3 = CursesUINavigator;
		cursesUINavigator3.OnDeselect = (UINavigator.Deselect)Delegate.Combine(cursesUINavigator3.OnDeselect, new UINavigator.Deselect(OnDeselect));
		UINavigator cursesUINavigator4 = CursesUINavigator;
		cursesUINavigator4.OnClose = (UINavigator.Close)Delegate.Combine(cursesUINavigator4.OnClose, new UINavigator.Close(OnClose));
		CursesUINavigator.setDefault();
	}

	private void OnChangeCursesSelection(Buttons Button)
	{
		(Button as ButtonsUICardManager).uICardManagerCard.Selected();
		DisplayFeature.Selected(Button, "Curses/" + (Button as ButtonsUICardManager).Card.Type);
	}

	private void OnCursesButtonDown(Buttons CurrentButton)
	{
		if ((CurrentButton as ButtonsUICardManager).uICardManagerCard.UnlockProgressWait < 1f)
		{
			CreateSoul(CurrentButton, OnCardCompleteCurses);
		}
	}

	public void SpawnTrinketsCards()
	{
		int num = -1;
		while (++num < DataManager.Instance.PlayerFoundTrinkets.Count)
		{
			TarotCards tarotCards = TarotCards.Create(DataManager.Instance.PlayerFoundTrinkets[num], true);
			GameObject obj = UnityEngine.Object.Instantiate(CardPrefab, TrinketRT);
			UICardManagerCard component = obj.GetComponent<UICardManagerCard>();
			component.SetSkin(TarotCards.Skin(tarotCards.Type));
			component.SetCard(tarotCards);
			Buttons item = new ButtonsUICardManager(obj, num, buttons.HorizontalSelector, num == 0, tarotCards, component);
			TrinketsUINavigator.Buttons.Add(item);
		}
		while (++num <= DataManager.AllTrinkets.Count)
		{
			UnityEngine.Object.Instantiate(BlankCardPrefab, TrinketRT);
		}
		while (++num <= 3)
		{
			UnityEngine.Object.Instantiate(BlankCardPrefab, TrinketRT);
		}
		UINavigator trinketsUINavigator = TrinketsUINavigator;
		trinketsUINavigator.OnChangeSelection = (UINavigator.ChangeSelection)Delegate.Combine(trinketsUINavigator.OnChangeSelection, new UINavigator.ChangeSelection(OnChangeTrinketsSelection));
		UINavigator trinketsUINavigator2 = TrinketsUINavigator;
		trinketsUINavigator2.OnButtonDown = (UINavigator.ButtonDown)Delegate.Combine(trinketsUINavigator2.OnButtonDown, new UINavigator.ButtonDown(OnTrinketsButtonDown));
		UINavigator trinketsUINavigator3 = TrinketsUINavigator;
		trinketsUINavigator3.OnDeselect = (UINavigator.Deselect)Delegate.Combine(trinketsUINavigator3.OnDeselect, new UINavigator.Deselect(OnDeselect));
		UINavigator trinketsUINavigator4 = TrinketsUINavigator;
		trinketsUINavigator4.OnClose = (UINavigator.Close)Delegate.Combine(trinketsUINavigator4.OnClose, new UINavigator.Close(OnClose));
		TrinketsUINavigator.setDefault();
	}

	private void OnChangeTrinketsSelection(Buttons Button)
	{
		(Button as ButtonsUICardManager).uICardManagerCard.Selected();
		DisplayFeature.Selected(Button, TarotCards.Skin((Button as ButtonsUICardManager).Card.Type));
	}

	private void OnTrinketsButtonDown(Buttons CurrentButton)
	{
		if ((CurrentButton as ButtonsUICardManager).uICardManagerCard.UnlockProgressWait < 1f)
		{
			CreateSoul(CurrentButton, OnCardCompleteTrinkets);
		}
	}
}
