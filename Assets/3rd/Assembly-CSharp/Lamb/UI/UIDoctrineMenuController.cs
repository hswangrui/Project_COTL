using System.Collections;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lamb.UI
{
	public class UIDoctrineMenuController : UIMenuBase
	{
		[FormerlySerializedAs("_tabNavigator")]
		[Header("Doctrine Menu")]
		[SerializeField]
		private DoctrineTabNavigatorBase tabNavigatorBase;

		[SerializeField]
		private SkeletonGraphic _book;

		[SerializeField]
		private GameObject _leftTab;

		[SerializeField]
		private GameObject _rightTab;

		private EventInstance LoopedSound;

		public override void Awake()
		{
			base.Awake();
			tabNavigatorBase.CanvasGroup.alpha = 0f;
			_book.startingAnimation = "open";
			_leftTab.gameObject.SetActive(false);
			_rightTab.gameObject.SetActive(false);
		}

		protected override IEnumerator DoShowAnimation()
		{
			_canvasGroup.interactable = false;
			yield return _book.YieldForAnimation("open");
			_animator.Play("Show");
			if (!LoopedSound.isValid())
			{
				LoopedSound = UIManager.CreateLoop("event:/player/new_item_pages_loop");
			}
			yield return _book.YieldForAnimation("flicking");
			AudioManager.Instance.StopLoop(LoopedSound);
			tabNavigatorBase.CanvasGroup.alpha = 1f;
			tabNavigatorBase.ShowDefault();
			_leftTab.gameObject.SetActive(tabNavigatorBase.CanNavigateLeft());
			_rightTab.gameObject.SetActive(tabNavigatorBase.CanNavigateRight());
			UIManager.PlayAudio("event:/ui/open_menu");
			yield return _book.YieldForAnimation("page_settle");
			_book.AnimationState.SetAnimation(0, "openpage", false);
			_canvasGroup.interactable = true;
		}

		protected override IEnumerator DoHideAnimation()
		{
			UIManager.PlayAudio("event:/player/new_item_book_close");
			_animator.Play("Hide");
			tabNavigatorBase.gameObject.SetActive(false);
			tabNavigatorBase.CurrentMenu.Hide(true);
			_leftTab.gameObject.SetActive(false);
			_rightTab.gameObject.SetActive(false);
			yield return _book.YieldForAnimation("close");
		}

		protected override void OnHideCompleted()
		{
			AudioManager.Instance.StopLoop(LoopedSound);
			Object.Destroy(base.gameObject);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}
	}
}
