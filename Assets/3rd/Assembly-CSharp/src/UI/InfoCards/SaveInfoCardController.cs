using System;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class SaveInfoCardController : UIInfoCardController<SaveInfoCard, MetaData>
	{
		[SerializeField]
		private Image _fade;

		private void Show(SaveInfoCard info)
		{
			_fade.gameObject.SetActive(true);
			_fade.DOKill();
			_fade.DOFade(1f, 0.33f);
		}

		private void Hide()
		{
			_fade.gameObject.SetActive(true);
			_fade.DOKill();
			_fade.DOFade(0f, 0.33f).OnComplete(delegate
			{
				_fade.gameObject.SetActive(false);
			});
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			OnInfoCardShown = (Action<SaveInfoCard>)Delegate.Combine(OnInfoCardShown, new Action<SaveInfoCard>(Show));
			OnInfoCardsHidden = (Action)Delegate.Combine(OnInfoCardsHidden, new Action(Hide));
			_fade.gameObject.SetActive(false);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			OnInfoCardShown = (Action<SaveInfoCard>)Delegate.Remove(OnInfoCardShown, new Action<SaveInfoCard>(Show));
			OnInfoCardsHidden = (Action)Delegate.Remove(OnInfoCardsHidden, new Action(Hide));
		}

		protected override bool IsSelectionValid(Selectable selectable, out MetaData showParam)
		{
			showParam = default(MetaData);
			SaveSlotButtonBase component;
			if (selectable.TryGetComponent<SaveSlotButtonBase>(out component) && component.Occupied && component.MetaData.HasValue)
			{
				showParam = component.MetaData.Value;
				return true;
			}
			return false;
		}
	}
}
