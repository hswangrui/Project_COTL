using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISimpleNewRecruit : BaseMonoBehaviour
{
	[SerializeField]
	private GameObject[] panels;

	[SerializeField]
	private Sprite[] panelIcons;

	[SerializeField]
	private TMP_InputField nameText;

	[SerializeField]
	private Button nameAcceptButton;

	[SerializeField]
	private Selectable nameBox;

	[SerializeField]
	private Selectable editNameButton;

	[SerializeField]
	private Selectable nameNextButton;

	[SerializeField]
	private UINewRecruitSelectable formPrefab;

	[SerializeField]
	private RectTransform formContainer;

	[SerializeField]
	private RectTransform formViewport;

	[SerializeField]
	private Selectable formRandom;

	[SerializeField]
	private GameObject lockIcon;

	[SerializeField]
	private Vector3 lockPositionOffset;

	[SerializeField]
	private Selectable formsNextButton;

	[SerializeField]
	private UINewRecruitSelectable variantPrefab;

	[SerializeField]
	private RectTransform variantContainer;

	[SerializeField]
	private RectTransform variantViewport;

	[SerializeField]
	private Selectable variantRandom;

	[SerializeField]
	private Selectable variantNextButton;

	[SerializeField]
	private UINewRecruitSelectable colorPrefab;

	[SerializeField]
	private RectTransform colorContainer;

	[SerializeField]
	private RectTransform colorViewport;

	[SerializeField]
	private Selectable colorRandom;

	[SerializeField]
	private UITrait traitPrefab;

	[SerializeField]
	private RectTransform followerTraitsContainer;

	[SerializeField]
	private RectTransform cultTraitsContainer;

	[SerializeField]
	private Button traitsAcceptButton;

	[SerializeField]
	private Selectable traitsNextButton;

	[SerializeField]
	private Image[] knobs;

	[SerializeField]
	private Image selectedIcon;

	private Material prevMat;

	public Material followerUIMat;

	private RendererMaterialSwap rendMatSwap;

	private List<UINewRecruitSelectable> forms = new List<UINewRecruitSelectable>();

	private List<UINewRecruitSelectable> variants = new List<UINewRecruitSelectable>();

	private List<UINewRecruitSelectable> colors = new List<UINewRecruitSelectable>();

	private Follower follower;

	private Camera camera;

	private Action callack;

	private UI_NavigatorSimple navigator;

	private Sprite baseSprite;

	private string previousName;

	private int selectionPanelIndex;

	private int formIndex;

	private int colorIndex;

	private int variantIndex;

	private bool formRandomSelected = true;

	private bool colorRandomSelected = true;

	public void Play(Follower Follower, Action Callback)
	{
		GameManager.InMenu = true;
		AudioManager.Instance.PlayOneShot("event:/followers/appearance_menu_appear", PlayerFarming.Instance.gameObject);
		navigator = GetComponent<UI_NavigatorSimple>();
		UI_NavigatorSimple uI_NavigatorSimple = navigator;
		uI_NavigatorSimple.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Combine(uI_NavigatorSimple.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(ChangeSelection));
		UI_NavigatorSimple uI_NavigatorSimple2 = navigator;
		uI_NavigatorSimple2.OnSelectDown = (Action)Delegate.Combine(uI_NavigatorSimple2.OnSelectDown, new Action(OptionSelected));
		callack = Callback;
		nameText.text = Follower.Brain.Info.Name;
		baseSprite = nameText.GetComponentInParent<Button>().image.sprite;
		for (int i = 0; i < WorshipperData.Instance.Characters.Count; i++)
		{
			if (WorshipperData.Instance.Characters[i].Skin[0].Skin == Follower.Brain.Info.SkinName)
			{
				formIndex = i;
				break;
			}
		}
		variantIndex = Follower.Brain.Info.SkinVariation;
		colorIndex = Follower.Brain.Info.SkinColour;
		follower = Follower;
		prevMat = follower.Spine.GetComponent<Renderer>().sharedMaterial;
		follower.Spine.CustomMaterialOverride.Add(prevMat, followerUIMat);
		followerUIMat.SetColor("_Color", Color.white);
		rendMatSwap = BiomeBaseManager.Instance.RecruitSpawnLocation.transform.parent.GetComponent<RendererMaterialSwap>();
		if (rendMatSwap != null)
		{
			rendMatSwap.SwapAll();
		}
		PopulateTraits();
		SetPanel(0);
		lockIcon.SetActive(false);
		camera = Camera.main;
		Follower.SetBodyAnimation("Indoctrinate/indoctrinate-start", false);
		Follower.AddBodyAnimation("Indoctrinate/indoctrinate-loop", true, 0f);
	}

	private void OnDestroy()
	{
		UI_NavigatorSimple uI_NavigatorSimple = navigator;
		uI_NavigatorSimple.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Remove(uI_NavigatorSimple.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(ChangeSelection));
		UI_NavigatorSimple uI_NavigatorSimple2 = navigator;
		uI_NavigatorSimple2.OnSelectDown = (Action)Delegate.Remove(uI_NavigatorSimple2.OnSelectDown, new Action(OptionSelected));
	}

	private void Update()
	{
		if (!nameText.isFocused)
		{
			if (InputManager.UI.GetPageNavigateRightDown())
			{
				ChangePanel(true);
			}
			else if (InputManager.UI.GetPageNavigateLeftDown())
			{
				ChangePanel(false);
			}
		}
		if (lockIcon.activeSelf)
		{
			lockIcon.transform.position = Camera.main.WorldToScreenPoint(follower.transform.position) + lockPositionOffset;
		}
	}

	private void RandomForm()
	{
		formIndex = WorshipperData.Instance.GetRandomAvailableSkin();
	}

	private void RandomColor()
	{
		WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[formIndex];
		colorIndex = UnityEngine.Random.Range(0, skinAndData.SlotAndColours.Count);
	}

	private void RandomVariant()
	{
		WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[formIndex];
		variantIndex = UnityEngine.Random.Range(0, skinAndData.Skin.Count);
	}

	private void ChangePanel(bool right)
	{
		selectionPanelIndex = Mathf.Clamp(selectionPanelIndex + (right ? 1 : (-1)), 0, 3);
		SetPanel(selectionPanelIndex);
		UpdateFollow(formIndex, colorIndex, variantIndex);
	}

	public void SetPanel(int index)
	{
		if ((bool)navigator.selectable && (bool)navigator.selectable.GetComponent<UINewRecruitSelectable>())
		{
			navigator.selectable.GetComponent<UINewRecruitSelectable>().OnDeselect(null);
		}
		else if ((bool)navigator.selectable && (bool)navigator.selectable.GetComponent<UITrait>())
		{
			navigator.selectable.GetComponent<UITrait>().OnDeselect(null);
		}
		selectionPanelIndex = index;
		for (int i = 0; i < panels.Length; i++)
		{
			if (i == selectionPanelIndex)
			{
				knobs[i].transform.DOScale(2f, 0.2f).SetUpdate(true);
				knobs[i].DOColor(Color.white, 0.2f).SetUpdate(true);
			}
			else
			{
				knobs[i].transform.DOScale(1f, 0.2f).SetUpdate(true);
				knobs[i].DOColor(Color.gray, 0.2f).SetUpdate(true);
			}
			panels[i].SetActive(i == selectionPanelIndex);
		}
		selectedIcon.sprite = panelIcons[selectionPanelIndex];
		selectedIcon.transform.DOMove(new Vector3(knobs[selectionPanelIndex].transform.position.x, selectedIcon.transform.position.y, selectedIcon.transform.position.z), 0.2f).SetUpdate(true);
		if (selectionPanelIndex == 0)
		{
			navigator.startingItem = editNameButton;
		}
		else if (selectionPanelIndex == 1)
		{
			PopulateForms();
			navigator.startingItem = GetCurrentCharacter();
			GetSnapToPositionToBringChildIntoView(formViewport, formContainer, navigator.startingItem.transform as RectTransform, 0f);
		}
		else if (selectionPanelIndex == 2)
		{
			PopulateColors();
			PopulateVariants();
			UpdateVariants();
			navigator.startingItem = GetCurrentSkinColor();
		}
		else if (selectionPanelIndex == 3)
		{
			navigator.startingItem = traitsAcceptButton;
		}
		navigator.setDefault();
	}

	private Selectable GetCurrentCharacter()
	{
		for (int i = 0; i < forms.Count; i++)
		{
			if (forms[i].FormIndex == formIndex)
			{
				return forms[i].Selectable;
			}
		}
		return null;
	}

	private Selectable GetCurrentSkinColor()
	{
		for (int i = 0; i < colors.Count; i++)
		{
			if (colors[i].SkinColour == colorIndex)
			{
				return colors[i].Selectable;
			}
		}
		return null;
	}

	public void RandomiseName()
	{
		nameText.text = FollowerInfo.GenerateName();
	}

	private void PopulateForms()
	{
		if (forms.Count > 0)
		{
			return;
		}
		for (int i = 0; i < WorshipperData.Instance.Characters.Count; i++)
		{
			UINewRecruitSelectable uINewRecruitSelectable = UnityEngine.Object.Instantiate(formPrefab, formContainer);
			uINewRecruitSelectable.gameObject.SetActive(true);
			uINewRecruitSelectable.FormIndex = i;
			forms.Add(uINewRecruitSelectable);
			SkeletonGraphic componentInChildren = forms[i].GetComponentInChildren<SkeletonGraphic>();
			WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[i];
			componentInChildren.Skeleton.SetSkin(skinAndData.Skin[Mathf.Min(0, skinAndData.Skin.Count - 1)].Skin);
			foreach (WorshipperData.SlotAndColor slotAndColour in skinAndData.SlotAndColours[Mathf.Min(0, skinAndData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = componentInChildren.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
			if (!DataManager.GetFollowerSkinUnlocked(skinAndData.Skin[0].Skin))
			{
				forms[i].Lock();
				continue;
			}
			forms[i].Show(skinAndData.Skin[Mathf.Min(0, skinAndData.Skin.Count - 1)].Skin);
			forms[i].transform.SetSiblingIndex(1);
		}
	}

	private void PopulateColors()
	{
		for (int num = colors.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(colors[num].gameObject);
		}
		colors.Clear();
		WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[formIndex];
		for (int i = 0; i < skinAndData.SlotAndColours.Count; i++)
		{
			UINewRecruitSelectable uINewRecruitSelectable = UnityEngine.Object.Instantiate(colorPrefab, colorContainer);
			uINewRecruitSelectable.gameObject.SetActive(true);
			uINewRecruitSelectable.FormIndex = formIndex;
			uINewRecruitSelectable.SkinVariationIndex = variantIndex;
			uINewRecruitSelectable.SkinColour = i;
			colors.Add(uINewRecruitSelectable);
			SkeletonGraphic componentInChildren = colors[i].GetComponentInChildren<SkeletonGraphic>();
			componentInChildren.Skeleton.SetSkin(skinAndData.Skin[Mathf.Min(0, skinAndData.Skin.Count - 1)].Skin);
			foreach (WorshipperData.SlotAndColor slotAndColour in skinAndData.SlotAndColours[Mathf.Min(i, skinAndData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = componentInChildren.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
			if (!DataManager.GetFollowerSkinUnlocked(skinAndData.Skin[0].Skin))
			{
				colors[i].Lock();
			}
			else
			{
				colors[i].Show();
			}
		}
	}

	private void UpdateVariants()
	{
		WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[formIndex];
		for (int i = 0; i < variants.Count; i++)
		{
			variants[i].SkinColour = colorIndex;
			SkeletonGraphic componentInChildren = variants[i].GetComponentInChildren<SkeletonGraphic>();
			componentInChildren.Skeleton.SetSkin(skinAndData.Skin[Mathf.Min(variants[i].SkinVariationIndex, skinAndData.Skin.Count - 1)].Skin);
			foreach (WorshipperData.SlotAndColor slotAndColour in skinAndData.SlotAndColours[Mathf.Min(colorIndex, skinAndData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = componentInChildren.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
		}
		variantRandom.GetComponent<UINewRecruitSelectable>().FormIndex = formIndex;
	}

	private void PopulateVariants()
	{
		for (int num = variants.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(variants[num].gameObject);
		}
		variants.Clear();
		WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[formIndex];
		for (int i = 0; i < skinAndData.Skin.Count; i++)
		{
			UINewRecruitSelectable uINewRecruitSelectable = UnityEngine.Object.Instantiate(variantPrefab, variantContainer);
			uINewRecruitSelectable.gameObject.SetActive(true);
			uINewRecruitSelectable.FormIndex = formIndex;
			uINewRecruitSelectable.SkinVariationIndex = i;
			uINewRecruitSelectable.SkinColour = colorIndex;
			variants.Add(uINewRecruitSelectable);
			SkeletonGraphic componentInChildren = variants[i].GetComponentInChildren<SkeletonGraphic>();
			componentInChildren.Skeleton.SetSkin(skinAndData.Skin[Mathf.Min(i, skinAndData.Skin.Count - 1)].Skin);
			WorshipperData.SlotsAndColours slotsAndColours = skinAndData.SlotAndColours[colorIndex];
			foreach (WorshipperData.SlotAndColor slotAndColour in skinAndData.SlotAndColours[Mathf.Min(variants[i].SkinVariationIndex, skinAndData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = componentInChildren.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotsAndColours.AllColor);
				}
			}
			if (!DataManager.GetFollowerSkinUnlocked(skinAndData.Skin[0].Skin))
			{
				variants[i].Lock();
			}
			else
			{
				variants[i].Show();
			}
		}
	}

	private void OptionSelected()
	{
		if (!navigator.selectable || !navigator.selectable.GetComponent<UINewRecruitSelectable>())
		{
			return;
		}
		UINewRecruitSelectable component = navigator.selectable.GetComponent<UINewRecruitSelectable>();
		if (!component.Locked.activeSelf)
		{
			if (component.FormIndex != formIndex)
			{
				colorIndex = 0;
				variantIndex = 0;
			}
			if (component.Random)
			{
				if (selectionPanelIndex == 1)
				{
					RandomForm();
				}
				else if (selectionPanelIndex == 2)
				{
					if (component.transform.parent == colorContainer)
					{
						RandomColor();
					}
					else if (component.transform.parent == variantContainer)
					{
						RandomVariant();
					}
				}
			}
			else if (component.transform.parent == formContainer)
			{
				formIndex = component.FormIndex;
				formRandomSelected = false;
				StartCoroutine(SetSelectable(formsNextButton));
			}
			else if (component.transform.parent == colorContainer)
			{
				colorIndex = component.SkinColour;
				colorRandomSelected = false;
			}
			else if (component.transform.parent == variantContainer)
			{
				variantIndex = component.SkinVariationIndex;
				StartCoroutine(SetSelectable(variantNextButton));
			}
		}
		UpdateFollow(formIndex, colorIndex, variantIndex);
	}

	private IEnumerator SetSelectable(Selectable selectable)
	{
		yield return new WaitForEndOfFrame();
		navigator.startingItem = selectable;
		navigator.setDefault();
	}

	private void ChangeSelection(Selectable NewSelectable, Selectable PrevSelectable)
	{
		UINewRecruitSelectable component = NewSelectable.GetComponent<UINewRecruitSelectable>();
		if (component != null)
		{
			if (component.transform.parent == formContainer)
			{
				UpdateFollow(component.FormIndex, 0, 0, component.IsLocked);
			}
			else if (component.transform.parent == colorContainer)
			{
				UpdateFollow(formIndex, component.SkinColour, variantIndex);
			}
			else if (component.transform.parent == variantContainer)
			{
				UpdateFollow(formIndex, colorIndex, component.SkinVariationIndex);
			}
		}
		if (selectionPanelIndex == 1 && PrevSelectable != null && NewSelectable.transform.position.y != PrevSelectable.transform.position.y)
		{
			bool flag = NewSelectable.transform.position.y < PrevSelectable.transform.position.y;
			if (flag && (bool)NewSelectable.FindSelectableOnDown())
			{
				GetSnapToPositionToBringChildIntoView(formViewport, formContainer, NewSelectable.FindSelectableOnDown().transform as RectTransform);
			}
			else if (!flag && (bool)NewSelectable.FindSelectableOnUp())
			{
				GetSnapToPositionToBringChildIntoView(formViewport, formContainer, NewSelectable.FindSelectableOnUp().transform as RectTransform);
			}
		}
		if (NewSelectable == nameBox)
		{
			EditingName();
		}
	}

	private void UpdateFollow(int formIndex, int colorIndex, int variantIndex, bool locked = false)
	{
		follower.Brain.Info.SkinCharacter = formIndex;
		follower.Brain.Info.SkinVariation = variantIndex;
		follower.Brain.Info.SkinColour = colorIndex;
		follower.Brain.Info.SkinName = WorshipperData.Instance.Characters[formIndex].Skin[variantIndex].Skin;
		follower.Brain.Info.Name = nameText.text;
		follower.SetOutfit(FollowerOutfitType.Rags, false);
		follower.Spine.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", locked ? Color.black : Color.white);
		if (!locked && (selectionPanelIndex == 2 || selectionPanelIndex == 1))
		{
			Animator component = BiomeBaseManager.Instance.RecruitSpawnLocation.transform.parent.GetComponent<Animator>();
			if (component != null)
			{
				component.SetTrigger("UpdateFollower");
			}
		}
		lockIcon.transform.position = Camera.main.WorldToScreenPoint(follower.transform.position) + lockPositionOffset;
		lockIcon.SetActive(locked);
		UpdateSelectedItems();
	}

	public void GetSnapToPositionToBringChildIntoView(RectTransform viewport, RectTransform content, RectTransform child, float moveSpeed = 0.2f)
	{
		Canvas.ForceUpdateCanvases();
		Vector2 vector = viewport.localPosition;
		Vector2 vector2 = child.localPosition;
		Vector2 vector3 = new Vector2(0f - (vector.x + vector2.x), 0f - (vector.y + vector2.y));
		vector3.x = content.localPosition.x;
		if (!RectTransformUtility.RectangleContainsScreenPoint(viewport, child.transform.position))
		{
			if (moveSpeed == 0f)
			{
				content.localPosition = vector3;
			}
			else
			{
				content.DOLocalMove(vector3, moveSpeed).SetUpdate(true);
			}
		}
	}

	private void UpdateSelectedItems()
	{
		for (int i = 0; i < forms.Count; i++)
		{
			if (forms[i].FormIndex == formIndex)
			{
				forms[i].SetSelected();
			}
			else
			{
				forms[i].SetUnselected();
			}
		}
		for (int j = 0; j < colors.Count; j++)
		{
			if (colors[j].SkinColour == colorIndex)
			{
				colors[j].SetSelected();
			}
			else
			{
				colors[j].SetUnselected();
			}
		}
		for (int k = 0; k < variants.Count; k++)
		{
			if (variants[k].SkinVariationIndex == variantIndex)
			{
				variants[k].SetSelected();
			}
			else
			{
				variants[k].SetUnselected();
			}
		}
	}

	public void AcceptName()
	{
		ChangePanel(true);
	}

	public void EditingName()
	{
		previousName = nameText.text;
		nameText.Select();
		Button componentInParent = nameText.GetComponentInParent<Button>();
		componentInParent.image.sprite = componentInParent.spriteState.selectedSprite;
		nameText.onSubmit.AddListener(OnSubmit);
		nameText.onEndEdit.AddListener(OnEndEdit);
	}

	private void OnSubmit(string r)
	{
		navigator.startingItem = editNameButton.GetComponentInParent<Button>();
		navigator.setDefault();
		if (previousName != r)
		{
			follower.SetBodyAnimation("Indoctrinate/indoctrinate-react", false);
			follower.AddBodyAnimation("pray", true, 0f);
		}
		nameText.onEndEdit.RemoveListener(OnSubmit);
	}

	private void OnEndEdit(string r)
	{
		nameText.GetComponentInParent<Button>().image.sprite = baseSprite;
		if (previousName != r)
		{
			follower.SetBodyAnimation("Indoctrinate/indoctrinate-react", false);
			follower.AddBodyAnimation("pray", true, 0f);
		}
		nameText.onEndEdit.RemoveListener(OnEndEdit);
	}

	public void Done()
	{
		GameManager.InMenu = false;
		if (rendMatSwap != null)
		{
			rendMatSwap.SwapAll();
		}
		follower.Spine.CustomMaterialOverride.Remove(prevMat);
		Action action = callack;
		if (action != null)
		{
			action();
		}
		UnityEngine.Object.Destroy(base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/followers/appearance_accept", PlayerFarming.Instance.gameObject);
	}

	private void PopulateTraits()
	{
		foreach (FollowerTrait.TraitType trait in follower.Brain.Info.Traits)
		{
			InstantiateTrait(trait, followerTraitsContainer);
		}
		foreach (FollowerTrait.TraitType cultTrait in DataManager.Instance.CultTraits)
		{
			InstantiateTrait(cultTrait, cultTraitsContainer);
		}
		traitPrefab.gameObject.SetActive(false);
	}

	private UITrait InstantiateTrait(FollowerTrait.TraitType trait, RectTransform container)
	{
		UITrait uITrait = UnityEngine.Object.Instantiate(traitPrefab, container);
		uITrait.Play(trait);
		uITrait.gameObject.SetActive(true);
		return uITrait;
	}
}
