using System;
using System.Collections;
using UnityEngine;

namespace Lamb.UI
{
	public class UICurseWheelController : UIRadialMenuBase<CurseWheelItem, EquipmentType>
	{
		[SerializeField]
		private RectTransform _container;

		private CurseWheelItem _item;

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

		protected override void MakeChoice(CurseWheelItem item)
		{
			foreach (CurseWheelItem wheelItem in _wheelItems)
			{
				wheelItem.SetSelected(item == wheelItem);
			}
			_item = item;
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			Action onWheelHide = OnWheelHide;
			if (onWheelHide != null)
			{
				onWheelHide();
			}
			if (_item != null)
			{
				Action<EquipmentType> onItemChosen = OnItemChosen;
				if (onItemChosen != null)
				{
					onItemChosen(_item.CurseType);
				}
			}
		}

		protected override IEnumerator DoHideAnimation()
		{
			yield return _animator.YieldForAnimation("Hide");
		}

		public static bool CanShowWheel()
		{
			if (PlayerFarming.Instance == null || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.InActive || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Dodging || PlayerFarming.Instance.GoToAndStopping || !LocationManager.LocationIsDungeon(PlayerFarming.Location))
			{
				return false;
			}
			if (!DataManager.Instance.EnabledSpells)
			{
				return false;
			}
			return true;
		}
	}
}
