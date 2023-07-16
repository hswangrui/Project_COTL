using System;
using System.Collections.Generic;
using src.UINavigator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class DoctrineSubmenu : UISubmenuBase
	{
		[Serializable]
		private class DoctrineChoicePair
		{
			[SerializeField]
			private DoctrineChoice _choiceA;

			[SerializeField]
			private DoctrineChoice _choiceB;

			public DoctrineChoice ActiveChoice
			{
				get
				{
					if (!_choiceA.UnlockedWithCrystal && _choiceA.Type != 0)
					{
						return _choiceA;
					}
					if (!_choiceB.UnlockedWithCrystal && _choiceB.Type != 0)
					{
						return _choiceB;
					}
					return null;
				}
			}

			public DoctrineChoice CrystalChoice
			{
				get
				{
					if (_choiceA.UnlockedWithCrystal && _choiceA.Type != 0)
					{
						return _choiceA;
					}
					if (_choiceB.UnlockedWithCrystal && _choiceB.Type != 0)
					{
						return _choiceB;
					}
					return null;
				}
			}

			public DoctrineChoice OtherChoice(DoctrineChoice target)
			{
				if (target == _choiceA)
				{
					return _choiceB;
				}
				return _choiceA;
			}

			public void Configure(List<DoctrineUpgradeSystem.DoctrineType> unlockedDoctrines, SermonCategory sermonCategory, int level)
			{
				DoctrineUpgradeSystem.DoctrineType sermonReward = DoctrineUpgradeSystem.GetSermonReward(sermonCategory, level, true);
				DoctrineUpgradeSystem.DoctrineType sermonReward2 = DoctrineUpgradeSystem.GetSermonReward(sermonCategory, level, false);
				int num = unlockedDoctrines.IndexOf(sermonReward);
				int num2 = unlockedDoctrines.IndexOf(sermonReward2);
				_choiceA.Configure(unlockedDoctrines.Contains(sermonReward) ? sermonReward : DoctrineUpgradeSystem.DoctrineType.None, num >= 4);
				_choiceB.Configure(unlockedDoctrines.Contains(sermonReward2) ? sermonReward2 : DoctrineUpgradeSystem.DoctrineType.None, num2 >= 4);
			}
		}

		[Header("Sermon Info")]
		[SerializeField]
		private SermonCategory _sermonCategory;

		[SerializeField]
		private TextMeshProUGUI _sermonCategoryTitle;

		[SerializeField]
		private TextMeshProUGUI _sermonCategoryLore;

		[Header("Pages")]
		[SerializeField]
		private DoctrineDetailsPage _detailsPage;

		[SerializeField]
		private DoctrineDetailsPage _detailsPage2;

		[SerializeField]
		private DoctrineForbiddenPage _forbiddenPage;

		[SerializeField]
		private DoctrineLockedPage _lockedPage;

		[Header("Choices")]
		[SerializeField]
		private GridLayoutGroup _gridLayoutgroup;

		[SerializeField]
		private MMUILineRenderer _lineRenderer;

		[SerializeField]
		private MMUILineRenderer _crystalLineRenderer;

		[SerializeField]
		private DoctrineChoicePair[] _doctrineChoicePairs;

		private UIMenuBase _currentPage;

		protected override void OnShowStarted()
		{
			if (_lineRenderer.Points.Count != 0 && _crystalLineRenderer.Points.Count != 0)
			{
				return;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(_gridLayoutgroup.transform as RectTransform);
			List<Vector2> list = new List<Vector2>();
			DoctrineChoicePair[] doctrineChoicePairs = _doctrineChoicePairs;
			foreach (DoctrineChoicePair doctrineChoicePair in doctrineChoicePairs)
			{
				if (doctrineChoicePair.ActiveChoice != null)
				{
					list.Add(doctrineChoicePair.ActiveChoice.transform.localPosition);
				}
			}
			if (list.Count > 1)
			{
				List<MMUILineRenderer.BranchPoint> list2 = new List<MMUILineRenderer.BranchPoint>();
				foreach (Vector2 item in list)
				{
					list2.Add(new MMUILineRenderer.BranchPoint(item));
				}
				_lineRenderer.Points = list2;
				_lineRenderer.Color = StaticColors.RedColor;
			}
			if (!DataManager.Instance.OnboardedCrystalDoctrine)
			{
				return;
			}
			MMUILineRenderer.Branch branch = _crystalLineRenderer.Root;
			for (int j = 0; j < _doctrineChoicePairs.Length; j++)
			{
				DoctrineChoicePair doctrineChoicePair2 = _doctrineChoicePairs[j];
				if (!(doctrineChoicePair2.ActiveChoice != null))
				{
					continue;
				}
				if (_crystalLineRenderer.Points.Count == 0)
				{
					branch.Points.Add(new MMUILineRenderer.BranchPoint(doctrineChoicePair2.OtherChoice(doctrineChoicePair2.ActiveChoice).transform.localPosition));
					continue;
				}
				branch = branch.Points[0].AddNewBranch();
				branch.Color = StaticColors.BlueColor;
				branch.Points.Add(new MMUILineRenderer.BranchPoint(doctrineChoicePair2.OtherChoice(doctrineChoicePair2.ActiveChoice).transform.localPosition));
				if (j > 0 && (_doctrineChoicePairs[j - 1].CrystalChoice == null || doctrineChoicePair2.CrystalChoice == null))
				{
					branch.Fill = 0f;
				}
			}
			_crystalLineRenderer.Color = StaticColors.BlueColor;
			_crystalLineRenderer.UpdateValues();
		}

		private void OnEnable()
		{
			_sermonCategoryTitle.text = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(_sermonCategory) + " - " + DoctrineUpgradeSystem.GetLevelBySermon(_sermonCategory).ToNumeral();
			_sermonCategoryLore.text = DoctrineUpgradeSystem.GetSermonCategoryLocalizedDescription(_sermonCategory);
			List<DoctrineUpgradeSystem.DoctrineType> unlockedDoctrinesForCategory = DoctrineUpgradeSystem.GetUnlockedDoctrinesForCategory(_sermonCategory);
			for (int i = 0; i < _doctrineChoicePairs.Length; i++)
			{
				_doctrineChoicePairs[i].Configure(unlockedDoctrinesForCategory, _sermonCategory, i + 1);
			}
			if (_doctrineChoicePairs[0].ActiveChoice != null)
			{
				OverrideDefaultOnce(_doctrineChoicePairs[0].ActiveChoice.GetComponent<MMButton>());
			}
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
		}

		private void OnDisable()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
			}
		}

		private void OnSelectionChanged(Selectable current, Selectable previous)
		{
			OnSelection(current);
		}

		private void OnSelection(Selectable current)
		{
			DoctrineChoice component;
			if (current.TryGetComponent<DoctrineChoice>(out component))
			{
				if (component.CurrentState == DoctrineChoice.State.Locked)
				{
					TransitionTo(_lockedPage);
				}
				else if (component.CurrentState == DoctrineChoice.State.Unchosen)
				{
					TransitionTo(_forbiddenPage);
				}
				else if (_currentPage == _detailsPage2)
				{
					TransitionTo(_detailsPage);
					_detailsPage.UpdateDetails(component.Type);
				}
				else
				{
					TransitionTo(_detailsPage2);
					_detailsPage2.UpdateDetails(component.Type);
				}
			}
		}

		private void TransitionTo(UIMenuBase newPage)
		{
			if (_currentPage != newPage)
			{
				PerformTransitionTo(_currentPage, newPage);
				_currentPage = newPage;
			}
		}

		protected virtual void PerformTransitionTo(UIMenuBase from, UIMenuBase to)
		{
			if (from != null)
			{
				from.Hide();
			}
			to.Show();
		}
	}
}
