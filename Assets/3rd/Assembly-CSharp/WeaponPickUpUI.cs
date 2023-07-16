using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

public class WeaponPickUpUI : BaseMonoBehaviour
{
	[SerializeField]
	private TMP_Text title;

	[SerializeField]
	private TMP_Text damageText;

	[SerializeField]
	private TMP_Text speedText;

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private TMP_Text descriptionText;

	[SerializeField]
	private TMP_Text loreText;

	[SerializeField]
	private InfoCardOutlineRenderer _outlineRenderer;

	public CanvasGroup canvasGroup;

	[Space]
	[SerializeField]
	private Vector3 offset;

	private RectTransform rectTransform;

	private GameObject lockPosition;

	[SerializeField]
	private GameObject SpeedAndDamageContainer;

	private Camera camera;

	private Canvas canvas;

	private int _weaponLevel;

	private float _damage;

	private float _speed;

	private EquipmentType _weaponType;

	private List<WeaponAttachmentData> _attachments;

	public void Play(EquipmentType weaponType, int weaponLevel, Sprite weaponImage, float damage, float speed, List<WeaponAttachmentData> attachments, GameObject lockPos, bool HideDamageAndSpeed, Interaction_WeaponSelectionPodium.Types type)
	{
		_weaponType = weaponType;
		_weaponLevel = weaponLevel;
		_damage = damage;
		_speed = speed;
		if (attachments != null)
		{
			_attachments = new List<WeaponAttachmentData>(attachments);
		}
		LocalizeText();
		camera = CameraManager.instance.CameraRef;
		lockPosition = lockPos;
		rectTransform = base.transform as RectTransform;
		canvas = GlobalCanvasReference.CanvasInstance;
		if (type == Interaction_WeaponSelectionPodium.Types.Curse)
		{
			_outlineRenderer.BadgeVariant = 7;
		}
		else
		{
			_outlineRenderer.BadgeVariant = 6;
		}
		Vector3 TargetPosition = new Vector3(0f, 130f);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.01f);
		sequence.AppendCallback(delegate
		{
			rectTransform.localPosition = TargetPosition + Vector3.up * 50f;
		});
		sequence.Append(rectTransform.DOLocalMove(TargetPosition, 0.3f).SetEase(Ease.OutBack));
		sequence.Play();
		canvasGroup = GetComponent<CanvasGroup>();
		if (base.gameObject != null && canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			DOTween.To(() => canvasGroup.alpha, delegate(float x)
			{
				canvasGroup.alpha = x;
			}, 1f, 0.5f);
			SpeedAndDamageContainer.SetActive(!HideDamageAndSpeed);
		}
	}

	private void SetWeaponDataText(float damage, float speed, int weaponLevel)
	{
		damage = Mathf.Round(damage * 100f) / 100f;
		float averageWeaponDamage = PlayerFarming.Instance.playerWeapon.GetAverageWeaponDamage(DataManager.Instance.CurrentWeapon, DataManager.Instance.CurrentWeaponLevel);
		float weaponSpeed = PlayerFarming.Instance.playerWeapon.GetWeaponSpeed(DataManager.Instance.CurrentWeapon);
		string damage2 = ScriptLocalization.UI_WeaponSelect.Damage;
		string text = "";
		string arg = "<color=#F5EDD5>";
		if (averageWeaponDamage > damage)
		{
			text = "<sprite name=\"icon_FaithDown\">";
			arg = "<color=#FF1C1C>";
		}
		if (averageWeaponDamage < damage)
		{
			text = "<sprite name=\"icon_FaithUp\">";
			arg = "<color=#2DFF1C>";
		}
		string speed2 = ScriptLocalization.UI_WeaponSelect.Speed;
		string text2 = "";
		string arg2 = "<color=#F5EDD5>";
		if (weaponSpeed > speed)
		{
			text2 = "<sprite name=\"icon_FaithDown\">";
			arg2 = "<color=#FF1C1C>";
		}
		if (weaponSpeed < speed)
		{
			text2 = "<sprite name=\"icon_FaithUp\">";
			arg2 = "<color=#2DFF1C>";
		}
		damageText.text = string.Format(damage2 + ": " + text + "{0}{1}</color>", arg, damage);
		speedText.text = string.Format(speed2 + ": " + text2 + "{0}{1}</color>", arg2, speed);
	}

	public void Shake(float progress, float normAmount)
	{
		rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, normAmount);
		container.localPosition = Random.insideUnitCircle * progress * 2f;
	}

	private void SetAttachmentsDescription(List<WeaponAttachmentData> attachments)
	{
		string text = "";
		foreach (WeaponAttachmentData attachment in attachments)
		{
			text += "- ";
			text += LocalizationManager.GetTranslation(attachment.DescriptionKey);
			text += "\n";
		}
		descriptionText.text = text;
	}

	private void LateUpdate()
	{
		bool flag = lockPosition == null;
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += LocalizeText;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= LocalizeText;
	}

	private void LocalizeText()
	{
		title.text = EquipmentManager.GetEquipmentData(_weaponType).GetLocalisedTitle() + " " + _weaponLevel.ToNumeral();
		descriptionText.text = EquipmentManager.GetEquipmentData(_weaponType).GetLocalisedDescription();
		loreText.text = EquipmentManager.GetEquipmentData(_weaponType).GetLocalisedLore();
		if (EquipmentManager.GetWeaponData(_weaponType) != null)
		{
			SetWeaponDataText(_damage, _speed, _weaponLevel);
		}
	}
}
