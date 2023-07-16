using System.Collections.Generic;
using src.Extensions;
using src.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIFollowerSummaryMenuController : UIMenuBase
	{
		[Header("Content")]
		[SerializeField]
		private FollowerInformationBox _infoBox;

		[SerializeField]
		private GridLayoutGroup _characterTraitLayoutGroup;

		[SerializeField]
		private RectTransform _characterTraitContent;

		[SerializeField]
		private GameObject _cultTraitHeader;

		[SerializeField]
		private GridLayoutGroup _cultTraitLayoutGroup;

		[SerializeField]
		private RectTransform _cultTraitContent;

		[SerializeField]
		private RectTransform _followerThoughtContent;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private TextMeshProUGUI Necklace;

		[Header("Templates")]
		[SerializeField]
		private IndoctrinationTraitItem _traitItemTemplate;

		[SerializeField]
		private FollowerThoughtItem _followerThoughtItemTemplate;

		private List<IndoctrinationTraitItem> _traitItems = new List<IndoctrinationTraitItem>();

		private List<IndoctrinationTraitItem> _cultItems = new List<IndoctrinationTraitItem>();

		private List<FollowerThoughtItem> _thoughtItems = new List<FollowerThoughtItem>();

		private Follower _follower;

		public void Show(Follower follower, bool instant = false)
		{
			_follower = follower;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			_scrollRect.normalizedPosition = Vector2.one;
			_scrollRect.enabled = false;
			_infoBox.Configure(_follower.Brain._directInfoAccess);
			if (_traitItems.Count == 0)
			{
				foreach (FollowerTrait.TraitType trait in _follower.Brain.Info.Traits)
				{
					IndoctrinationTraitItem indoctrinationTraitItem = GameObjectExtensions.Instantiate(_traitItemTemplate, _characterTraitContent);
					indoctrinationTraitItem.Configure(trait);
					_traitItems.Add(indoctrinationTraitItem);
				}
				if (_traitItems.Count > _characterTraitLayoutGroup.constraintCount)
				{
					_characterTraitLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
				}
				else
				{
					_characterTraitLayoutGroup.childAlignment = TextAnchor.UpperLeft;
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
				if (_cultItems.Count > _cultTraitLayoutGroup.constraintCount)
				{
					_cultTraitLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
				}
				else
				{
					_cultTraitLayoutGroup.childAlignment = TextAnchor.UpperLeft;
				}
				_cultTraitHeader.SetActive(_cultItems.Count > 0);
			}
			if (_thoughtItems.Count == 0)
			{
				List<ThoughtData> list = new List<ThoughtData>(_follower.Brain.Stats.Thoughts);
				if (_follower.Brain.Info.CursedState == Thought.OldAge)
				{
					foreach (ThoughtData item in list)
					{
						if (item.ThoughtType == Thought.OldAge)
						{
							list.Remove(item);
							break;
						}
					}
					list.Add(new ThoughtData(Thought.OldAge));
				}
				else if (_follower.Brain.Info.CursedState == Thought.Dissenter)
				{
					list.Add(new ThoughtData(Thought.BiggestNeed_Dissenter));
				}
				else if (_follower.Brain.Info.CursedState == Thought.Ill)
				{
					list.Add(new ThoughtData(Thought.BiggestNeed_Sick));
				}
				else if (_follower.Brain.Info.CursedState == Thought.BecomeStarving)
				{
					list.Add(new ThoughtData(Thought.BiggestNeed_Hungry));
				}
				else if (_follower.Brain.Stats.Exhaustion > 0f)
				{
					list.Add(new ThoughtData(Thought.BiggestNeed_Exhausted));
				}
				else if (!_follower.Brain.HasHome)
				{
					list.Add(new ThoughtData(Thought.BiggestNeed_Homeless));
				}
				else
				{
					Structures_Bed structureByID = StructureManager.GetStructureByID<Structures_Bed>(_follower.Brain._directInfoAccess.DwellingID);
					if (structureByID != null && structureByID.IsCollapsed)
					{
						list.Add(new ThoughtData(Thought.BiggestNeed_BrokenBed));
					}
				}
				list.Reverse();
				foreach (ThoughtData item2 in list)
				{
					FollowerThoughtItem followerThoughtItem = GameObjectExtensions.Instantiate(_followerThoughtItemTemplate, _followerThoughtContent);
					followerThoughtItem.Configure(item2);
					_thoughtItems.Add(followerThoughtItem);
				}
			}
			Navigation navigation = _infoBox.Button.navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (_traitItems.Count > 0)
			{
				navigation.selectOnDown = _traitItems[0].Selectable;
			}
			else if (_cultItems.Count > 0)
			{
				navigation.selectOnDown = _cultItems[0].Selectable;
			}
			else if (_thoughtItems.Count > 0)
			{
				navigation.selectOnDown = _thoughtItems[0].Selectable;
			}
			_infoBox.Button.navigation = navigation;
			int constraintCount = _characterTraitLayoutGroup.constraintCount;
			int num = Mathf.FloorToInt((float)_traitItems.Count / (float)constraintCount);
			if (_traitItems.Count > 0)
			{
				for (int i = 0; i < _traitItems.Count; i++)
				{
					Navigation navigation2 = _traitItems[i].Selectable.navigation;
					navigation2.mode = Navigation.Mode.Explicit;
					int num2 = Mathf.FloorToInt((float)i / (float)constraintCount);
					int num3 = i - num2 * constraintCount;
					if (num2 == 0)
					{
						navigation2.selectOnUp = _infoBox.Button;
						if (i + constraintCount < _traitItems.Count)
						{
							navigation2.selectOnDown = _traitItems[i + constraintCount].Selectable;
						}
						else if (_cultItems.Count > 0)
						{
							if (num3 < _cultItems.Count)
							{
								navigation2.selectOnDown = _cultItems[num3].Selectable;
							}
							else
							{
								navigation2.selectOnDown = _cultItems.LastElement().Selectable;
							}
						}
						else if (_thoughtItems.Count > 0)
						{
							navigation2.selectOnDown = _thoughtItems[0].Selectable;
						}
					}
					else if (num2 == num)
					{
						navigation2.selectOnUp = _traitItems[i - constraintCount].Selectable;
						if (_cultItems.Count > 0)
						{
							if (num3 < _cultItems.Count)
							{
								navigation2.selectOnDown = _cultItems[num3].Selectable;
							}
							else
							{
								navigation2.selectOnDown = _cultItems.LastElement().Selectable;
							}
						}
						else if (_thoughtItems.Count > 0)
						{
							navigation2.selectOnDown = _thoughtItems[0].Selectable;
						}
					}
					else
					{
						navigation2.selectOnUp = _traitItems[i - constraintCount].Selectable;
						if (i + constraintCount < _traitItems.Count)
						{
							navigation2.selectOnDown = _traitItems[i + constraintCount].Selectable;
						}
						else
						{
							navigation2.selectOnDown = _traitItems.LastElement().Selectable;
						}
					}
					if (num3 > 0)
					{
						navigation2.selectOnLeft = _traitItems[i - 1].Selectable;
					}
					if (num3 < constraintCount && i + 1 < _traitItems.Count)
					{
						navigation2.selectOnRight = _traitItems[i + 1].Selectable;
					}
					_traitItems[i].Selectable.navigation = navigation2;
				}
			}
			int constraintCount2 = _characterTraitLayoutGroup.constraintCount;
			int num4 = Mathf.FloorToInt((float)_cultItems.Count / (float)constraintCount2);
			if (_cultItems.Count > 0)
			{
				for (int j = 0; j < _cultItems.Count; j++)
				{
					Navigation navigation3 = _cultItems[j].Selectable.navigation;
					navigation3.mode = Navigation.Mode.Explicit;
					int num5 = Mathf.FloorToInt((float)j / (float)constraintCount2);
					int num6 = j - num5 * constraintCount2;
					if (num5 == 0)
					{
						if (constraintCount * num + num6 < _traitItems.Count)
						{
							navigation3.selectOnUp = _traitItems[constraintCount * num + num6].Selectable;
						}
						else
						{
							navigation3.selectOnUp = _traitItems.LastElement().Selectable;
						}
						if (num6 + constraintCount2 < _cultItems.Count)
						{
							navigation3.selectOnDown = _cultItems[num6 + constraintCount2].Selectable;
						}
						else if (_thoughtItems.Count > 0)
						{
							navigation3.selectOnDown = _thoughtItems[0].Selectable;
						}
					}
					else if (num5 == num4)
					{
						navigation3.selectOnUp = _cultItems[j - constraintCount2].Selectable;
						if (_thoughtItems.Count > 0)
						{
							navigation3.selectOnDown = _thoughtItems[0].Selectable;
						}
					}
					else
					{
						navigation3.selectOnUp = _cultItems[j - constraintCount2].Selectable;
						if (j + constraintCount2 < _cultItems.Count)
						{
							navigation3.selectOnDown = _cultItems[j + constraintCount2].Selectable;
						}
						else
						{
							navigation3.selectOnDown = _cultItems.LastElement().Selectable;
						}
					}
					if (num6 > 0)
					{
						navigation3.selectOnLeft = _cultItems[j - 1].Selectable;
					}
					if (num6 < constraintCount2 && j + 1 < _cultItems.Count)
					{
						navigation3.selectOnRight = _cultItems[j + 1].Selectable;
					}
					_cultItems[j].Selectable.navigation = navigation3;
				}
			}
			if (_thoughtItems.Count > 0)
			{
				Navigation navigation4 = _thoughtItems[0].Selectable.navigation;
				navigation4.mode = Navigation.Mode.Explicit;
				if (_cultItems.Count > 0)
				{
					navigation4.selectOnUp = _cultItems[constraintCount2 * Mathf.Clamp(num4 - 1, 0, int.MaxValue)].Selectable;
				}
				else if (_traitItems.Count > 0)
				{
					navigation4.selectOnUp = _traitItems[constraintCount * num].Selectable;
				}
				if (_thoughtItems.Count > 1)
				{
					navigation4.selectOnDown = _thoughtItems[1].Selectable;
				}
				_thoughtItems[0].Selectable.navigation = navigation4;
			}
			if (_follower.Brain.Info.Necklace == InventoryItem.ITEM_TYPE.NONE)
			{
				Necklace.gameObject.SetActive(false);
			}
			else
			{
				Necklace.text = FontImageNames.GetIconByType(_follower.Brain.Info.Necklace) + " " + InventoryItem.LocalizedName(_follower.Brain.Info.Necklace) + ": " + InventoryItem.LocalizedDescription(_follower.Brain.Info.Necklace);
			}
			_scrollRect.enabled = true;
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
