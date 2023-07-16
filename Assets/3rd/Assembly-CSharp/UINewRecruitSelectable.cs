using Lamb.UI;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINewRecruitSelectable : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public int FormIndex;

	public int SkinVariationIndex;

	public int SkinColour;

	public SkeletonGraphic Spine;

	public MMSelectable Selectable;

	public Image Image;

	public RectTransform rectTransform;

	public GameObject Locked;

	public GameObject NewSkinIcon;

	public GameObject highlightedBorder;

	public GameObject selectedIcon;

	public Material blackWhiteMaterial;

	public bool Random;

	private string skin = "";

	public bool IsLocked { get; private set; }

	public void Show(string skin = "")
	{
		Locked.SetActive(false);
		Selectable.interactable = true;
		highlightedBorder.SetActive(false);
		if ((bool)Spine)
		{
			SkeletonGraphic spine = Spine;
			bool flag2 = (Selectable.interactable = true);
			spine.enabled = flag2;
		}
		Image.color = new Color(0f, 0f, 0f, 0.2f);
		this.skin = skin;
	}

	public void Hide()
	{
		Locked.SetActive(false);
		SkeletonGraphic spine = Spine;
		bool flag2 = (Selectable.interactable = false);
		spine.enabled = flag2;
		Image.color = new Color(1f, 1f, 1f, 0f);
	}

	public void Lock()
	{
		Locked.SetActive(true);
		highlightedBorder.SetActive(false);
		if ((bool)Spine)
		{
			Spine.material = blackWhiteMaterial;
			Spine.material.SetFloat("_GrayscaleLerpFade", 1f);
		}
		IsLocked = true;
	}

	public void OnSelect(BaseEventData eventData)
	{
		highlightedBorder.SetActive(true);
		if (!IsLocked)
		{
			Image.color = new Color(0f, 0f, 0f, 0f);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		highlightedBorder.SetActive(false);
		if (!IsLocked)
		{
			Image.color = new Color(0f, 0f, 0f, 0.2f);
		}
	}

	public void SetSelected()
	{
		selectedIcon.SetActive(true);
	}

	public void SetUnselected()
	{
		selectedIcon.SetActive(false);
	}
}
