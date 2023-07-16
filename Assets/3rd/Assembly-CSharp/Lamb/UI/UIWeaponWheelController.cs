using System;
using System.Collections;
using UnityEngine;

namespace Lamb.UI
{
	public class UIWeaponWheelController : UIRadialMenuBase<WeaponWheelItem, EquipmentType>
	{
		[SerializeField]
		private RectTransform _container;

		private WeaponWheelItem _item;

		public static Action OnWheelHide;

		protected override bool SelectOnHighlight
		{
			get
			{
				return true;
			}
		}

		protected override void Start()
		{
			base.Start();
			_controlPrompts.HideAcceptButton();
			_controlPrompts.HideCancelButton();
			if (PlayerFarming.Instance != null)
			{
				Vector2 vector = Camera.main.WorldToScreenPoint(PlayerFarming.Instance.playerController.transform.position + new Vector3(0f, 2f, 0f));
				_container.position = vector;
			}
		}

		protected override void OnChoiceFinalized()
		{
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			Action onWheelHide = OnWheelHide;
			if (onWheelHide != null)
			{
				onWheelHide();
			}
		}

		protected override void MakeChoice(WeaponWheelItem item)
		{
			foreach (WeaponWheelItem wheelItem in _wheelItems)
			{
				wheelItem.SetSelected(item == wheelItem);
			}
			_item = item;
		}

		protected override IEnumerator DoHideAnimation()
		{
			yield return _animator.YieldForAnimation("Hide");
		}

		public static bool CanShowWheel()
		{
			if (PlayerFarming.Instance == null || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.InActive || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Attacking || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Dodging || PlayerFarming.Instance.GoToAndStopping || !LocationManager.LocationIsDungeon(PlayerFarming.Location) || DataManager.Instance.WeaponPool.Count <= 1)
			{
				return false;
			}
			return true;
		}
	}
}
