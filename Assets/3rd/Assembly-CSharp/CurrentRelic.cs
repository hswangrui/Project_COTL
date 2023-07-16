using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CurrentRelic : MonoBehaviour
{
	private RelicType relic;

	[SerializeField]
	private CanvasGroup unchargedRelicCG;

	[SerializeField]
	private Image unchargedIcon;

	[SerializeField]
	private BarController barController;

	[SerializeField]
	private CanvasGroup chargedRelicCG;

	[SerializeField]
	private GameObject buttonPrompt;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image iconOutline;

	[SerializeField]
	private Image fragileIcon;

	[SerializeField]
	private Image Background;

	[SerializeField]
	private CanvasGroup SubIconCanvasGroup;

	[SerializeField]
	private Image SubIcon;

	[SerializeField]
	private Image SubIconOutline;

	[SerializeField]
	private List<RelicCantUse> _cantUseRelics = new List<RelicCantUse>();

	[SerializeField]
	private Image requiresItemIcon;

	private Vector3 requiresItemIconStartPos;

	private Vector3 iconStartingPos;

	private bool playedChargedSFX;

	private RelicType cacheRelic;

	public void OnEnable()
	{
		SubIconCanvasGroup.gameObject.SetActive(false);
		if (DataManager.Instance.CurrentRelic == RelicType.None || !GameManager.IsDungeon(PlayerFarming.Location))
		{
			base.transform.localScale = Vector3.zero;
		}
		PlayerRelic.OnRelicEquipped += SetRelic;
		PlayerRelic.OnRelicChargeModified += OnRelicChargeModified;
		PlayerRelic.OnRelicConsumed += OnRelicConsumed;
		PlayerRelic.OnRelicCantUse += OnRelicCantUse;
		PlayerRelic.OnSubRelicChanged += SetSubIcon;
		requiresItemIcon.gameObject.SetActive(false);
		requiresItemIconStartPos = requiresItemIcon.transform.localPosition;
		iconStartingPos = icon.transform.localPosition;
		chargedRelicCG.alpha = 0f;
		unchargedRelicCG.alpha = 0f;
		Background.enabled = false;
	}

	private void OnRelicCantUse(RelicData relic)
	{
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", PlayerFarming.Instance.gameObject);
		icon.transform.DOKill();
		iconOutline.transform.DOKill();
		icon.DOKill();
		icon.transform.localPosition = iconStartingPos;
		iconOutline.transform.localPosition = iconStartingPos;
		icon.color = Color.red;
		icon.DOColor(Color.white, 0.5f);
		icon.transform.DOShakePosition(1f, new Vector3(10f, 0f, 0f));
		iconOutline.transform.DOShakePosition(1f, new Vector3(10f, 0f, 0f));
		bool flag = false;
		foreach (RelicCantUse cantUseRelic in _cantUseRelics)
		{
			if (cantUseRelic.relic == this.relic)
			{
				requiresItemIcon.sprite = cantUseRelic.sprite;
				flag = true;
			}
		}
		if (flag)
		{
			requiresItemIcon.gameObject.SetActive(true);
			requiresItemIcon.transform.DOKill();
			requiresItemIcon.transform.localPosition = requiresItemIconStartPos;
			requiresItemIcon.transform.localScale = Vector3.one;
			requiresItemIcon.DOKill();
			requiresItemIcon.color = Color.white;
			requiresItemIcon.transform.localScale = Vector3.one * 2f;
			requiresItemIcon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			requiresItemIcon.transform.DOPunchPosition(new Vector3(10f, 0f, 0f), 1f);
			requiresItemIcon.transform.DOLocalMove(new Vector3(0f, 50f, 0f), 1.5f).SetDelay(1f).SetEase(Ease.InQuad);
			requiresItemIcon.DOFade(0f, 1f).SetDelay(1.5f).OnComplete(delegate
			{
				requiresItemIcon.gameObject.SetActive(false);
			});
		}
	}

	private void OnDisable()
	{
		PlayerRelic.OnRelicCantUse -= OnRelicCantUse;
		PlayerRelic.OnRelicConsumed -= OnRelicConsumed;
		PlayerRelic.OnSubRelicChanged -= SetSubIcon;
		PlayerRelic.OnRelicEquipped -= SetRelic;
		PlayerRelic.OnRelicChargeModified -= OnRelicChargeModified;
	}

	private void OnRelicConsumed(RelicData relic)
	{
		chargedRelicCG.alpha = 1f;
		buttonPrompt.SetActive(false);
		iconOutline.DOKill();
		iconOutline.DOFade(0f, 0.5f);
		Material material = new Material(icon.material);
		icon.material = material;
		material.DOKill();
		material.DOFloat(1f, "_FillColorLerpFade", 0.2f);
		material.DOFloat(0f, "_FillColorLerpFade", 0.2f).SetDelay(1f);
		material.DOFloat(0f, "_DissolveAmount", 1f).SetDelay(0.5f);
		material.DOFloat(1f, "_DissolveAmount", 0.1f).SetDelay(1.5f);
		chargedRelicCG.DOKill();
		chargedRelicCG.DOFade(0f, 1f).SetDelay(1f);
	}

	private void OnRelicChargeModified(RelicData relic)
	{
		barController.gameObject.SetActive(true);
		barController.SetBarSize(PlayerFarming.Instance.playerRelic.ChargedAmount / PlayerFarming.Instance.playerRelic.RequiredChargeAmount, true);
		chargedRelicCG.DOKill();
		unchargedRelicCG.DOKill();
		if (PlayerFarming.Instance.playerRelic.ChargedAmount >= PlayerFarming.Instance.playerRelic.RequiredChargeAmount)
		{
			if (!playedChargedSFX)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/relic_charged");
				MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
				playedChargedSFX = true;
			}
			buttonPrompt.SetActive(true);
			iconOutline.color = Color.white;
			if (chargedRelicCG.alpha != 1f)
			{
				chargedRelicCG.DOFade(1f, 0.5f);
			}
			if (unchargedRelicCG.alpha != 0f)
			{
				unchargedRelicCG.DOFade(0f, 0.5f);
			}
		}
		else
		{
			playedChargedSFX = false;
			if (chargedRelicCG.alpha != 0f)
			{
				chargedRelicCG.DOFade(0f, 0.5f);
			}
			if (unchargedRelicCG.alpha != 1f)
			{
				unchargedRelicCG.DOFade(1f, 0.5f);
			}
		}
		if (PlayerFarming.Instance.playerRelic.ChargedAmount <= 0f)
		{
			barController.ShrinkBarToEmpty(0f);
		}
	}

	private void SetRelic(RelicData relic)
	{
		SetRelic((relic != null) ? relic.RelicType : RelicType.None, true);
	}

	private void SetSubIcon(RelicData relic)
	{
		SetSubIcon((relic != null) ? relic.RelicType : RelicType.None);
	}

	private void SetSubIcon(RelicType relic)
	{
		Debug.Log("SetSubIcon".Colour(Color.green) + "  " + relic);
		SubIconCanvasGroup.gameObject.SetActive(relic == RelicType.UseRandomRelic || relic == RelicType.UseRandomRelic_Blessed || relic == RelicType.UseRandomRelic_Dammed);
		if (SubIconCanvasGroup.gameObject.activeSelf)
		{
			SubIconCanvasGroup.DOKill();
			SubIconCanvasGroup.transform.localScale = Vector3.one * 1.5f;
			SubIconCanvasGroup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			SubIconCanvasGroup.alpha = 0f;
			SubIconCanvasGroup.DOFade(1f, 0.5f);
			Debug.Log(EquipmentManager.NextRandomRelic.ToString().Colour(Color.yellow));
			SubIcon.sprite = EquipmentManager.GetRelicIcon(EquipmentManager.NextRandomRelic);
			SubIconOutline.sprite = EquipmentManager.GetRelicIconOutline(EquipmentManager.NextRandomRelic);
		}
	}

	private void SetRelic(RelicType relic, bool punch)
	{
		Debug.Log("SetRelic".Colour(Color.yellow));
		if (cacheRelic == RelicType.None)
		{
			HUD_Manager.Instance._relicTransitions.hideBar();
			HUD_Manager.Instance._relicTransitions.MoveBackInFunction();
		}
		if (relic == RelicType.None)
		{
			return;
		}
		this.relic = relic;
		cacheRelic = relic;
		fragileIcon.enabled = relic != RelicType.None;
		iconOutline.enabled = relic != RelicType.None;
		unchargedIcon.enabled = relic != RelicType.None;
		icon.enabled = relic != RelicType.None;
		fragileIcon.enabled = EquipmentManager.GetRelicSingleUse(relic);
		chargedRelicCG.alpha = 0f;
		unchargedRelicCG.alpha = 0f;
		if (relic == RelicType.None)
		{
			base.transform.localScale = Vector3.zero;
			return;
		}
		chargedRelicCG.DOKill();
		if (PlayerFarming.Instance.playerRelic.ChargedAmount >= PlayerFarming.Instance.playerRelic.RequiredChargeAmount)
		{
			chargedRelicCG.DOFade(1f, 1f);
		}
		else
		{
			unchargedRelicCG.DOFade(1f, 1f);
		}
		icon.sprite = EquipmentManager.GetRelicIcon(relic);
		iconOutline.sprite = EquipmentManager.GetRelicIconOutline(relic);
		unchargedIcon.sprite = icon.sprite;
		base.transform.localScale = Vector3.one;
		icon.transform.localScale = Vector3.one;
		if (punch)
		{
			icon.transform.localScale *= 2f;
			icon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
			Background.transform.DOKill();
			Background.enabled = true;
			Background.transform.localScale = Vector3.one * 0.9f;
			Background.transform.DOScale(Vector3.one * 2f, 0.5f);
			Color c = Background.color;
			Background.DOFade(0f, 0.5f).OnComplete(delegate
			{
				Background.color = c;
				Background.enabled = false;
			});
		}
		OnRelicChargeModified(EquipmentManager.GetRelicData(relic));
	}
}
