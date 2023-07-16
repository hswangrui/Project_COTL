using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrownAbilitiesManager : BaseMonoBehaviour
{
	public Animator animator;

	public Action CancelCallback;

	public UINavigator uiNav;

	public Selectable selectable;

	public Selectable oldSelectable;

	public Animator cardAnimator;

	public Image selectedIconFeatureIcon;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI DescriptionText;

	public TextMeshProUGUI CostText;

	public GameObject lockIcon;

	public List<CrownAbilitiesIcon> objectsInScene;

	public StructuresData.ResearchState state;

	private CrownAbilitiesIcon icon;

	public TextMeshProUGUI Souls;

	private bool closing;

	private int currentSouls;

	public void updateListing()
	{
		icon = uiNav.selectable.gameObject.GetComponent<CrownAbilitiesIcon>();
		if (cardAnimator != null)
		{
			cardAnimator.Play("Panel In");
		}
		if (icon.Icon != null)
		{
			selectedIconFeatureIcon.sprite = icon.Icon.sprite;
		}
		if (icon.dependancy != null && !CrownAbilities.CrownAbilityUnlocked(icon.dependancy.type))
		{
			lockIcon.SetActive(true);
		}
		TitleText.text = CrownAbilities.LocalisedName(icon.type);
		DescriptionText.text = CrownAbilities.LocalisedDescription(icon.type);
		if (!CrownAbilities.CrownAbilityUnlocked(icon.type))
		{
			CostText.text = "";
			int abilityPoints = DataManager.Instance.AbilityPoints;
			int crownAbilitiesCost = CrownAbilities.GetCrownAbilitiesCost(icon.type);
			CostText.text += ((crownAbilitiesCost > abilityPoints) ? "<color=#ff0000>" : "<color=#00ff00>");
			CostText.text += FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.SOUL);
			TextMeshProUGUI costText = CostText;
			costText.text = costText.text + " x" + crownAbilitiesCost;
			TextMeshProUGUI costText2 = CostText;
			costText2.text = costText2.text + "   (" + abilityPoints + ")";
			CostText.text += "\n";
			CostText.text += "</color>";
			if (icon.dependancy != null && !CrownAbilities.CrownAbilityUnlocked(icon.dependancy.type))
			{
				CrownAbilitiesIcon component = icon.dependancy.GetComponent<CrownAbilitiesIcon>();
				CostText.text += "<color=#ff0000>";
				CostText.text += "Requires ";
				CostText.text += CrownAbilities.LocalisedName(component.type);
				CostText.text += "\n";
				CostText.text += "</color>";
			}
		}
		else
		{
			lockIcon.SetActive(false);
			CostText.text = "Already Bought";
		}
	}

	private void OnEnable()
	{
		Time.timeScale = 0f;
	}

	public void Play(GameObject CameraTarget, Action CancelCallback)
	{
		this.CancelCallback = CancelCallback;
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddToCamera(CameraTarget);
		GameManager.GetInstance().CameraSetOffset(new Vector3(-5f, 0f, 0f));
		GameManager.GetInstance().CameraSetTargetZoom(15f);
		closing = false;
		updateListing();
		updateSouls();
	}

	private IEnumerator Closing(bool SetActive = false)
	{
		closing = true;
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().CameraResetTargetZoom();
		Time.timeScale = 1f;
		yield return new WaitForEndOfFrame();
		closing = SetActive;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void updateSouls()
	{
		Souls.text = "AbilityTokens " + DataManager.Instance.AbilityPoints;
	}

	private void Update()
	{
		updateSouls();
		selectable = uiNav.selectable;
		if (selectable != oldSelectable)
		{
			updateListing();
			oldSelectable = selectable;
		}
		if (closing)
		{
			return;
		}
		if (InputManager.UI.GetCancelButtonDown())
		{
			StartCoroutine(Closing());
			GameManager.GetInstance().RemoveAllFromCamera();
			GameManager.GetInstance().AddPlayerToCamera();
			Action cancelCallback = CancelCallback;
			if (cancelCallback != null)
			{
				cancelCallback();
			}
		}
		else
		{
			if (!InputManager.UI.GetAcceptButtonDown())
			{
				return;
			}
			bool flag = false;
			flag = icon.gameObject.GetComponent<CrownAbilitiesIcon>().CanAfford;
			if (CheatConsole.BuildingsFree)
			{
				flag = true;
			}
			if (flag)
			{
				StartCoroutine(Closing());
				CrownAbilities.UnlockAbility(icon.type);
				DataManager.Instance.AbilityPoints -= CrownAbilities.GetCrownAbilitiesCost(icon.type);
				for (int i = 0; i < objectsInScene.Count; i++)
				{
					if (objectsInScene[i] != null)
					{
						objectsInScene[i].Init();
					}
				}
				GameManager.GetInstance().RemoveAllFromCamera();
				GameManager.GetInstance().AddPlayerToCamera();
			}
			else
			{
				icon.Shake();
			}
		}
	}
}
