using System;
using System.Collections;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class MMTab : MonoBehaviour
	{
		protected const string kInactiveLayer = "Inactive";

		protected const string kActiveLayer = "Active";

		public Action OnTabPressed;

		[SerializeField]
		protected MMButton _button;

		[SerializeField]
		protected UIMenuBase _menu;

		[SerializeField]
		protected RectTransform _rectTransform;

		[SerializeField]
		protected Animator _animator;

		private float _inactiveWeight = 1f;

		private float _activeWeight;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public UIMenuBase Menu
		{
			get
			{
				return _menu;
			}
		}

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public abstract void Configure();

		public void Awake()
		{
			UIMenuBase menu = _menu;
			menu.OnShow = (Action)Delegate.Combine(menu.OnShow, new Action(SetActive));
			UIMenuBase menu2 = _menu;
			menu2.OnHide = (Action)Delegate.Combine(menu2.OnHide, new Action(SetInactive));
			_button.onClick.AddListener(delegate
			{
				Action onTabPressed = OnTabPressed;
				if (onTabPressed != null)
				{
					onTabPressed();
				}
			});
			_animator.keepAnimatorControllerStateOnDisable = true;
		}

		protected virtual void SetActive()
		{
			StopAllCoroutines();
			StartCoroutine(Transition(1f, 0f));
		}

		protected virtual void SetInactive()
		{
			StopAllCoroutines();
			StartCoroutine(Transition(0f, 1f));
		}

		private IEnumerator Transition(float targetActiveWeight, float targetInactiveWeight)
		{
			int inactiveLayer = _animator.GetLayerIndex("Inactive");
			int activeLayer = _animator.GetLayerIndex("Active");
			float activeWeight = _activeWeight;
			float inactiveWeight = _inactiveWeight;
			float t = 0f;
			float time = 0.2f;
			while (t < time)
			{
				t += Time.unscaledDeltaTime;
				_activeWeight = Mathf.Lerp(activeWeight, targetActiveWeight, t / time);
				_inactiveWeight = Mathf.Lerp(inactiveWeight, targetInactiveWeight, t / time);
				_animator.SetLayerWeight(inactiveLayer, _inactiveWeight);
				_animator.SetLayerWeight(activeLayer, _activeWeight);
				yield return null;
			}
		}
	}
}
