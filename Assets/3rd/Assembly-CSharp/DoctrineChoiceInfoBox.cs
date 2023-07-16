using DG.Tweening;
using I2.Loc;
using MMTools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoctrineChoiceInfoBox : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public RectTransform rectTransform;

	public RectTransform containerRectTransform;

	public Image Icon;

	public TextMeshProUGUI ResponseText;

	public TextMeshProUGUI UnlockName;

	public TextMeshProUGUI UnlockDescription;

	public TextMeshProUGUI UnlockType;

	public TextMeshProUGUI UnlockTypeIcon;

	public TextMeshProUGUI UnlockTypeDescription;

	public GameObject HoldToUnlock;

	public Image RadialProgress;

	public Selectable Selectable;

	public GameObject DoctrineUnlocked;

	public Image SelectedSymbol;

	public Sprite SelectedSymbolSprite;

	public Sprite UnSelectedSymbolSprite;

	public Image UnselectedOverlay;

	public Image RedOutline;

	public Image WhiteFlash;

	public void Init(DoctrineResponse d)
	{
		RedOutline.rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		ResponseText.text = "\"" + LocalizationManager.Sources[0].GetTranslation(string.Concat("DoctrineUpgradeSystem/", d.SermonCategory, d.RewardLevel, "_Response", d.isFirstChoice ? "A" : "B")) + "\"";
		DoctrineUpgradeSystem.DoctrineType sermonReward = DoctrineUpgradeSystem.GetSermonReward(d.SermonCategory, d.RewardLevel, d.isFirstChoice);
		Icon.sprite = DoctrineUpgradeSystem.GetIcon(sermonReward);
		UnlockName.text = DoctrineUpgradeSystem.GetLocalizedName(sermonReward);
		UnlockDescription.text = DoctrineUpgradeSystem.GetLocalizedDescription(sermonReward);
		UnlockType.text = DoctrineUpgradeSystem.GetDoctrineUnlockString(sermonReward);
		UnlockTypeIcon.text = DoctrineUpgradeSystem.GetDoctrineUnlockIcon(sermonReward);
		Selectable.enabled = true;
	}

	private void OnEnable()
	{
		HoldToUnlock.SetActive(false);
		RadialProgress.fillAmount = 0f;
		DoctrineUnlocked.SetActive(false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Debug.Log("ON SELECT! " + base.gameObject.name);
		rectTransform.DOKill();
		rectTransform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.1f).SetEase(Ease.InOutSine);
		RedOutline.rectTransform.DOKill();
		RedOutline.rectTransform.DOScale(1f, 0.5f).SetEase(Ease.OutQuad);
		UnselectedOverlay.DOKill();
		UnselectedOverlay.color = new Vector4(1f, 1f, 1f, 1f);
		UnselectedOverlay.DOFade(0f, 0.5f).SetEase(Ease.OutCirc);
		SelectedSymbol.sprite = SelectedSymbolSprite;
		SelectedSymbol.color = StaticColors.RedColor;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Debug.Log("ON DESELECT! " + base.gameObject.name);
		ResetTweens();
		UnselectedOverlay.DOKill();
		UnselectedOverlay.color = new Vector4(1f, 1f, 1f, 0f);
		UnselectedOverlay.DOFade(1f, 0.5f).SetEase(Ease.OutCirc);
		SelectedSymbol.sprite = UnSelectedSymbolSprite;
		SelectedSymbol.color = StaticColors.OffWhiteColor;
	}

	public void ResetTweens()
	{
		RedOutline.rectTransform.DOKill();
		RedOutline.rectTransform.DOScale(0.8f, 0.5f).SetEase(Ease.OutQuad);
		rectTransform.DOKill();
		rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.InOutSine);
	}
}
