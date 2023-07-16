using System.Collections.Generic;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class TraitsMenu : UISubmenuBase
	{
		[SerializeField]
		private MMScrollRect _scrollRect;

		[Header("Character Traits")]
		[SerializeField]
		private RectTransform _characterTraitContent;

		[Header("Cult Traits")]
		[SerializeField]
		private RectTransform _cultTraitContent;

		[Header("Template")]
		[SerializeField]
		private IndoctrinationTraitItem _traitItemTemplate;

		private List<IndoctrinationTraitItem> _traitItems = new List<IndoctrinationTraitItem>();

		private List<IndoctrinationTraitItem> _cultItems = new List<IndoctrinationTraitItem>();

		private Follower _follower;

		public void Configure(Follower follower)
		{
			_follower = follower;
		}

		protected override void OnShowStarted()
		{
			_scrollRect.normalizedPosition = Vector2.one;
			_scrollRect.enabled = false;
			if (_traitItems.Count == 0)
			{
				foreach (FollowerTrait.TraitType trait in _follower.Brain.Info.Traits)
				{
					IndoctrinationTraitItem indoctrinationTraitItem = GameObjectExtensions.Instantiate(_traitItemTemplate, _characterTraitContent);
					indoctrinationTraitItem.Configure(trait);
					_traitItems.Add(indoctrinationTraitItem);
				}
			}
			if (_cultItems.Count == 0)
			{
				foreach (FollowerTrait.TraitType cultTrait in DataManager.Instance.CultTraits)
				{
					IndoctrinationTraitItem indoctrinationTraitItem2 = GameObjectExtensions.Instantiate(_traitItemTemplate, _cultTraitContent);
					indoctrinationTraitItem2.Configure(cultTrait);
					_cultItems.Add(indoctrinationTraitItem2);
				}
			}
			OverrideDefaultOnce(_traitItems[0].Selectable);
			ActivateNavigation();
			_scrollRect.enabled = true;
		}
	}
}
