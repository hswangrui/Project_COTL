using DG.Tweening;
using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class WeaponWheelItem : UIRadialWheelItem
	{
		[SerializeField]
		private TarotCards.Card _weaponType;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private WeaponCurseIconMapping _iconMapping;

		[SerializeField]
		private Image _lockIcon;

		[SerializeField]
		private Image _selectedIcon;

		private bool _isUnlocked;

		public TarotCards.Card WeaponType
		{
			get
			{
				return _weaponType;
			}
		}

		private void Start()
		{
			_icon.sprite = _iconMapping.GetSprite(_weaponType);
			_icon.color = new Color(0f, _isUnlocked ? 1 : 0, 1f, 1f);
			_lockIcon.gameObject.SetActive(!_isUnlocked);
		}

		public override string GetTitle()
		{
			return TarotCards.LocalisedName(_weaponType);
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
			return TarotCards.LocalisedDescription(_weaponType);
		}
	}
}
