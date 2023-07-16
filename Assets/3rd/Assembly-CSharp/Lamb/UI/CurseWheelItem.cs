using DG.Tweening;
using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class CurseWheelItem : UIRadialWheelItem
	{
		[SerializeField]
		private EquipmentType _curseType;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private WeaponCurseIconMapping _iconMapping;

		[SerializeField]
		private Image _lockIcon;

		[SerializeField]
		private Image _selectedIcon;

		private bool _isUnlocked;

		public EquipmentType CurseType
		{
			get
			{
				return EquipmentType.Tentacles;
			}
		}

		private void Start()
		{
			_icon.color = new Color(0f, _isUnlocked ? 1 : 0, 1f, 1f);
			_lockIcon.gameObject.SetActive(!_isUnlocked);
			SetSelected(DataManager.Instance.CurrentCurse == _curseType);
		}

		public override string GetTitle()
		{
			return "";
		}

		public override bool IsValidOption()
		{
			return _isUnlocked;
		}

		public override bool Visible()
		{
			return true;
		}

		public override void DoSelected()
		{
			base.DoSelected();
			if (_isUnlocked)
			{
				_icon.DOKill();
				_icon.DOColor(Color.white, 0.25f).SetUpdate(true);
			}
		}

		public override void DoDeselected()
		{
			base.DoDeselected();
			if (_isUnlocked)
			{
				_icon.DOKill();
				_icon.DOColor(new Color(0f, 1f, 1f, 1f), 0.25f).SetUpdate(true);
			}
		}

		public void SetSelected(bool selected)
		{
			_selectedIcon.gameObject.SetActive(selected);
		}

		public override string GetDescription()
		{
			return "";
		}
	}
}
