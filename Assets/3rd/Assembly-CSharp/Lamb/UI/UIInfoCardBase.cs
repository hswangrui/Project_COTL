using UnityEngine;

namespace Lamb.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class UIInfoCardBase<T> : MonoBehaviour
	{
		private const string kShownGenericAnimationState = "Shown";

		private const string kHiddenGenericAnimationState = "Hidden";

		private const string kShowTrigger = "Show";

		private const string kHideTrigger = "Hide";

		[SerializeField]
		private Animator _animator;

		public RectTransform RectTransform { get; private set; }

		public CanvasGroup CanvasGroup { get; private set; }

		public Animator Animator
		{
			get
			{
				return _animator;
			}
		}

		public virtual void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			CanvasGroup = GetComponent<CanvasGroup>();
		}

		public void Show(T config, bool instant = false)
		{
			Configure(config);
			Show(instant);
		}

		public void Show(bool instant = false)
		{
			ResetTriggers();
			DoShow(instant);
		}

		protected virtual void DoShow(bool instant)
		{
			if (instant)
			{
				_animator.Play("Shown");
			}
			else
			{
				_animator.SetTrigger("Show");
			}
		}

		public void Hide(bool instant = false)
		{
			ResetTriggers();
			DoHide(instant);
		}

		protected virtual void DoHide(bool instant)
		{
			if (instant)
			{
				_animator.Play("Hidden");
			}
			else
			{
				_animator.SetTrigger("Hide");
			}
		}

		private void ResetTriggers()
		{
			if (_animator != null)
			{
				_animator.ResetTrigger("Show");
				_animator.ResetTrigger("Hide");
			}
		}

		public abstract void Configure(T config);
	}
}
