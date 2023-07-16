using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DoctrineBookController : MonoBehaviour
{
	public SkeletonGraphic DoctrineBookSpine;

	public CanvasGroup LeftPageCanvasGroup;

	public CanvasGroup RightPageCanvasGroup;

	public CanvasGroup TabLeft;

	public CanvasGroup TabRight;

	public SermonCategory SelectedCategory;

	public Image SermonCategoryIcon;

	public TextMeshProUGUI SermonCategoryTitle;

	public TextMeshProUGUI SermonCategoryLore;

	public GameObject ProgressBar;

	public Image ProgressBarProgress;

	public List<GameObject> UnlockSelectionsChoices = new List<GameObject>();

	public List<GameObject> UnlockSelections = new List<GameObject>();

	public List<GameObject> CategoryTabsSelected = new List<GameObject>();

	public List<GameObject> CategoryTabsUnselected = new List<GameObject>();

	public Image UnlockIcon;

	public TextMeshProUGUI UnlockTitle;

	public TextMeshProUGUI UnlockType;

	public TextMeshProUGUI UnlockTypeIcon;

	public TextMeshProUGUI UnlockDescription;

	public CanvasGroup CurrentSelection;

	private void LocaliseCategory()
	{
		SermonCategoryTitle.text = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(SelectedCategory) + " [" + (DoctrineUpgradeSystem.GetLevelBySermon(SelectedCategory) + 1) + "]";
		SermonCategoryLore.text = DoctrineUpgradeSystem.GetSermonCategoryLocalizedDescription(SelectedCategory);
	}

	private void GetProgress()
	{
		switch (SelectedCategory)
		{
		case SermonCategory.Afterlife:
			if (DataManager.Instance.HasBuiltTemple2)
			{
				ProgressBar.SetActive(true);
			}
			else
			{
				ProgressBar.SetActive(false);
			}
			break;
		case SermonCategory.LawAndOrder:
		case SermonCategory.Possession:
			if (DataManager.Instance.HasBuiltTemple3)
			{
				ProgressBar.SetActive(true);
			}
			else
			{
				ProgressBar.SetActive(false);
			}
			break;
		}
		ProgressBarProgress.fillAmount = DoctrineUpgradeSystem.GetXPBySermonNormalised(SelectedCategory);
	}

	private void OnChangeSelection(Selectable NewSelectable, Selectable PrevSelectable)
	{
		if (NewSelectable == null)
		{
			DOTween.Kill(CurrentSelection.alpha);
			DOTween.To(() => CurrentSelection.alpha, delegate(float x)
			{
				CurrentSelection.alpha = x;
			}, 0f, 0.3f);
			return;
		}
		UIDoctrineIcon component = NewSelectable.GetComponent<UIDoctrineIcon>();
		if (component.Locked)
		{
			DOTween.Kill(CurrentSelection.alpha);
			DOTween.To(() => CurrentSelection.alpha, delegate(float x)
			{
				CurrentSelection.alpha = x;
			}, 0f, 0.3f);
		}
		else
		{
			DOTween.Kill(CurrentSelection.alpha);
			DOTween.To(() => CurrentSelection.alpha, delegate(float x)
			{
				CurrentSelection.alpha = x;
			}, 1f, 0.3f);
			DoctrineUpgradeSystem.DoctrineType type = component.Type;
			UnlockTitle.text = DoctrineUpgradeSystem.GetLocalizedName(type);
			UnlockDescription.text = DoctrineUpgradeSystem.GetLocalizedDescription(type);
			UnlockIcon.sprite = DoctrineUpgradeSystem.GetIcon(type);
			UnlockType.text = DoctrineUpgradeSystem.GetDoctrineUnlockString(type);
		}
		AudioManager.Instance.PlayOneShot("event:/upgrade_statue/upgrade_statue_scroll", base.gameObject);
	}
}
