using System.Collections.Generic;
using Lamb.UI.PauseDetails;
using src.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lamb.UI
{
	public class CrownMenu : UISubmenuBase
	{
		[Header("Crown Menu")]
		[SerializeField]
		private MMScrollRect _scrollRect;

		[Header("Tarot")]
		[SerializeField]
		private RectTransform _tarotCardContentContainer;

		[FormerlySerializedAs("_tarotCardItemTemplate")]
		[SerializeField]
		private TarotCardItem_Run _tarotCardItemRunTemplate;

		[SerializeField]
		private GameObject _noTarotText;

		private List<WeaponItem> _weaponItems = new List<WeaponItem>();

		private List<CurseItem> _curseItems = new List<CurseItem>();

		private List<TarotCardItem_Run> _tarotCardItems = new List<TarotCardItem_Run>();

		protected override void OnShowStarted()
		{
			_scrollRect.enabled = false;
			if (_tarotCardItems.Count == 0 && DataManager.Instance.PlayerRunTrinkets.Count > 0)
			{
				foreach (TarotCards.TarotCard playerRunTrinket in DataManager.Instance.PlayerRunTrinkets)
				{
					TarotCardItem_Run tarotCardItem_Run = GameObjectExtensions.Instantiate(_tarotCardItemRunTemplate, _tarotCardContentContainer);
					tarotCardItem_Run.Configure(playerRunTrinket.CardType);
					_tarotCardItems.Add(tarotCardItem_Run);
				}
				_noTarotText.SetActive(false);
			}
			_scrollRect.enabled = true;
			_scrollRect.normalizedPosition = Vector2.one;
		}
	}
}
