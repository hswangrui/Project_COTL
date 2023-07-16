using System;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class IndoctrinationCharacterItem<T> : MonoBehaviour
	{
		private const string kNormalLayerID = "Normal";

		private const string kSelectedLayerID = "Selected";

		private const string kLockedLayerID = "Locked";

		public Action<T> OnItemSelected;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		protected SkeletonGraphic _spine;

		[SerializeField]
		private RectTransform _container;

		protected WorshipperData.SkinAndData _skinAndData;

		private bool _locked;

		private bool _selected;

		private Vector2 _containerOrigin;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public bool Locked
		{
			get
			{
				return _locked;
			}
		}

		public WorshipperData.DropLocation DropLocation
		{
			get
			{
				return _skinAndData.DropLocation;
			}
		}

		public string Skin
		{
			get
			{
				return _skinAndData.Skin[0].Skin;
			}
		}

		public void Awake()
		{
			_containerOrigin = _container.anchoredPosition;
			_button.onClick.AddListener(OnButtonClicked);
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(Shake));
			UpdateState();
		}

		public virtual void Configure(WorshipperData.SkinAndData skinAndData)
		{
			_skinAndData = skinAndData;
			_spine.ConfigureFollowerSkin(skinAndData);
		}

		private void OnButtonClicked()
		{
			OnButtonClickedImpl();
		}

		public void Shake()
		{
			_container.DOKill();
			_container.anchoredPosition = _containerOrigin;
			_container.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		protected abstract void OnButtonClickedImpl();

		public void SetAsDefault()
		{
			_selected = false;
			_locked = !DataManager.GetFollowerSkinUnlocked(_skinAndData.Skin[0].Skin) && !_skinAndData.Invariant;
			_button.Confirmable = !_locked;
			if (_locked)
			{
				base.transform.SetAsLastSibling();
			}
			UpdateState();
		}

		public void SetAsSelected()
		{
			_selected = true;
			_locked = false;
			UpdateState();
		}

		private void UpdateState()
		{
			_animator.SetLayerWeight(_animator.GetLayerIndex("Normal"), (!_locked && !_selected) ? 1 : 0);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Selected"), (!_locked && _selected) ? 1 : 0);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), (_locked && !_selected) ? 1 : 0);
		}
	}
}
