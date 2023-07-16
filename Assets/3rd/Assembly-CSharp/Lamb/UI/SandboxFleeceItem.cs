using System;
using DG.Tweening;
using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class SandboxFleeceItem : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _shakeContainer;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private FleeceIconMapping _fleeceIconMapping;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private GameObject _lockedContainer;

		private int _fleeceIndex;

		private bool _unlocked;

		private Vector2 _origin;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public bool Unlocked
		{
			get
			{
				return _unlocked;
			}
		}

		public int FleeceIndex
		{
			get
			{
				return _fleeceIndex;
			}
		}

		public void Configure(int index)
		{
			_fleeceIndex = index;
			_origin = _shakeContainer.anchoredPosition;
			_unlocked = DataManager.Instance.UnlockedFleeces.Contains(_fleeceIndex);
			if (_fleeceIndex != 0 && !DungeonSandboxManager.HasFinishedAnyWithDefaultFleece())
			{
				_unlocked = false;
			}
			_button.Confirmable = _unlocked;
			_lockedContainer.SetActive(!_unlocked);
			_fleeceIconMapping.GetImage(_fleeceIndex, _icon);
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(Shake));
		}

		private void Shake()
		{
			_shakeContainer.DOKill();
			_shakeContainer.localScale = Vector2.one;
			_shakeContainer.anchoredPosition = _origin;
			_shakeContainer.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}
	}
}
