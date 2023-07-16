using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITrait : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject selectedBorder;

	[SerializeField]
	private GameObject removeIcon;

	[SerializeField]
	private GameObject descriptionContainer;

	[SerializeField]
	private TMP_Text traitTitle;

	[SerializeField]
	private TMP_Text traitDescription;

	public FollowerTrait.TraitType Trait { get; private set; }

	public void Play(FollowerTrait.TraitType trait)
	{
		Trait = trait;
		icon.sprite = FollowerTrait.GetIcon(trait);
		removeIcon.SetActive(false);
		traitTitle.text = FollowerTrait.GetLocalizedTitle(trait);
		traitDescription.text = FollowerTrait.GetLocalizedDescription(trait);
	}

	public void OnSelect(BaseEventData eventData)
	{
		descriptionContainer.SetActive(true);
		selectedBorder.SetActive(true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		descriptionContainer.SetActive(false);
		selectedBorder.SetActive(false);
	}
}
