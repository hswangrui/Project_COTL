using System;
using System.Collections.Generic;
using src.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class RewardPath : MonoBehaviour
	{
		public Action<PlayerFleeceManager.FleeceType> OnSelected;

		public Action<ScenarioData> OnConfirmed;

		[Header("Fleeces")]
		[SerializeField]
		private PlayerFleeceManager.FleeceType _fleece;

		[SerializeField]
		private SandboxFleeceItem _fleeceItem;

		[Header("Rewards")]
		[SerializeField]
		private MMUILineRenderer _topLine;

		[SerializeField]
		private MMUILineRenderer _bottomLine;

		[SerializeField]
		private RectTransform _itemContainer;

		[SerializeField]
		private RewardItem _rewardItemTemplate;

		private List<ScenarioData> _scenarioData;

		private List<RewardItem> _rewardItems = new List<RewardItem>();

		public PlayerFleeceManager.FleeceType Fleece
		{
			get
			{
				return _fleece;
			}
		}

		public SandboxFleeceItem FleeceItem
		{
			get
			{
				return _fleeceItem;
			}
		}

		public void Configure(List<ScenarioData> scenarioData)
		{
			DungeonSandboxManager.ProgressionSnapshot progressionForScenario = DungeonSandboxManager.GetProgressionForScenario(scenarioData[0].ScenarioType, _fleece);
			int completedRuns = Mathf.Min(progressionForScenario.CompletedRuns, 5);
			_fleeceItem.Configure((int)_fleece);
			MMButton button = _fleeceItem.Button;
			button.OnSelected = (Action)Delegate.Combine(button.OnSelected, (Action)delegate
			{
				Action<PlayerFleeceManager.FleeceType> onSelected = OnSelected;
				if (onSelected != null)
				{
					onSelected(_fleece);
				}
				if (_fleeceItem.Unlocked)
				{
					_rewardItems[completedRuns].Highlight();
				}
			});
			MMButton button2 = _fleeceItem.Button;
			button2.OnDeselected = (Action)Delegate.Combine(button2.OnDeselected, (Action)delegate
			{
				if (_fleeceItem.Unlocked)
				{
					_rewardItems[completedRuns].UnHighlight();
				}
			});
			_fleeceItem.Button.onClick.AddListener(delegate
			{
				Action<ScenarioData> onConfirmed = OnConfirmed;
				if (onConfirmed != null)
				{
					onConfirmed(_scenarioData[completedRuns]);
				}
			});
			_scenarioData = scenarioData;
			foreach (ScenarioData scenarioDatum in _scenarioData)
			{
				if (_rewardItems.Count == 6)
				{
					break;
				}
				RewardItem rewardItem = GameObjectExtensions.Instantiate(_rewardItemTemplate, _itemContainer);
				rewardItem.Configure(scenarioDatum, progressionForScenario, base.transform.rotation.z > 0f);
				_rewardItems.Add(rewardItem);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(_itemContainer);
			MMUILineRenderer.Branch root = _topLine.Root;
			root.Points.Clear();
			if (completedRuns == 0)
			{
				root.Points.Add(new MMUILineRenderer.BranchPoint(Vector3.zero));
			}
			else
			{
				root.Points.Add(new MMUILineRenderer.BranchPoint(_rewardItems[completedRuns - 1].transform.localPosition));
			}
			root.Points.Add(new MMUILineRenderer.BranchPoint(_rewardItems[completedRuns].transform.localPosition));
			_topLine.UpdateValues();
			_rewardItems[completedRuns].ConfigureSecondaryLine(root);
			MMUILineRenderer.Branch branch = _bottomLine.Root;
			branch.Points.Clear();
			branch.Points.Add(new MMUILineRenderer.BranchPoint(Vector2.zero));
			foreach (RewardItem rewardItem2 in _rewardItems)
			{
				branch.Points.Add(new MMUILineRenderer.BranchPoint(rewardItem2.transform.localPosition));
				rewardItem2.ConfigureLine(branch);
				branch = branch.Points.LastElement().AddNewBranch();
			}
			_bottomLine.UpdateValues();
		}

		public void SetIncognitoMode()
		{
			_fleeceItem.Button.interactable = false;
		}

		public void RemoveIncognitoMode()
		{
			_fleeceItem.Button.interactable = true;
		}
	}
}
