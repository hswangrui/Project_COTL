using System.Collections;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

namespace Lamb.UI
{
	public class DoctrineTabNavigatorBase : MMTabNavigatorBase<DoctrineBookmark>
	{
		[SerializeField]
		private SkeletonGraphic _book;

		[SerializeField]
		private CanvasGroup _canvasgroup;

		[SerializeField]
		private GameObject _leftTab;

		[SerializeField]
		private GameObject _rightTab;

		private EventInstance LoopedSound;

		public CanvasGroup CanvasGroup
		{
			get
			{
				return _canvasgroup;
			}
		}

		protected override void OnMenuShow()
		{
		}

		protected override void OnMenuHide()
		{
			AudioManager.Instance.StopLoop(LoopedSound);
		}

		protected override void PerformTransitionTo(DoctrineBookmark from, DoctrineBookmark to)
		{
			StopAllCoroutines();
			StartCoroutine(DoTransitionTo(from, to));
		}

		private IEnumerator DoTransitionTo(MMTab from, MMTab to)
		{
			from.Menu.Hide(true);
			_canvasgroup.alpha = 0f;
			if (!LoopedSound.isValid())
			{
				LoopedSound = UIManager.CreateLoop("event:/player/new_item_pages_loop");
			}
			yield return _book.YieldForAnimation("flicking");
			AudioManager.Instance.StopLoop(LoopedSound);
			to.Menu.Show(true);
			_canvasgroup.alpha = 1f;
			UIManager.PlayAudio("event:/ui/open_menu");
			_leftTab.SetActive(CanNavigateLeft());
			_rightTab.SetActive(CanNavigateRight());
			yield return _book.YieldForAnimation("page_settle");
			_book.AnimationState.SetAnimation(0, "openpage", false);
		}
	}
}
