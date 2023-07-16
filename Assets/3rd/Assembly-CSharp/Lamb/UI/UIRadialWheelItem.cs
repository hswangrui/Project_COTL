using UnityEngine;

namespace Lamb.UI
{
	public abstract class UIRadialWheelItem : MonoBehaviour
	{
		protected const string kSelectedAnimatorState = "NormalToPressed";

		protected const string kDeselectedAnimatorState = "PressedToNormal";

		protected const string kInactiveAnimatorState = "Inactive";

		protected const string kActiveAnimatorState = "Normal";

		protected const string kSelectedInactiveAnimatorState = "InactiveToPressed";

		protected const string kDeselectedInactiveAnimatorState = "PressedToInactive";

		[SerializeField]
		protected MMButton _button;

		[SerializeField]
		protected Animator _animator;

		[SerializeField]
		protected CanvasGroup _canvasGroup;

		protected bool _selected;

		protected bool _inactive;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public Vector2 Vector
		{
			get
			{
				return base.transform.localPosition.normalized;
			}
		}

		public virtual void DoSelected()
		{
			if (!_selected)
			{
				UIManager.PlayAudio("event:/ui/change_selection");
				if (_inactive)
				{
					_animator.Play("InactiveToPressed");
				}
				else
				{
					_animator.Play("NormalToPressed");
				}
				_selected = true;
			}
		}

		public virtual void DoDeselected()
		{
			if (_selected)
			{
				if (_inactive)
				{
					_animator.Play("PressedToInactive");
				}
				else
				{
					_animator.Play("PressedToNormal");
				}
				_selected = false;
			}
		}

		public virtual void DoInactive()
		{
			_animator.Play("Inactive");
			_inactive = true;
		}

		public virtual void DoActive()
		{
			_animator.Rebind();
			_animator.Play("Normal");
			_inactive = false;
		}

		public abstract string GetTitle();

		public abstract string GetDescription();

		public abstract bool IsValidOption();

		public abstract bool Visible();
	}
}
