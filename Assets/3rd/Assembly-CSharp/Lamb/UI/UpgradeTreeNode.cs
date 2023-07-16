using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeTreeNode : MonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		public enum NodeState
		{
			Locked,
			Unavailable,
			Available,
			Unlocked
		}

		public enum TreeTier
		{
			Tier1,
			Tier2,
			Tier3,
			Tier4,
			Tier5,
			Tier6
		}

		private const string kLockedStateLayer = "Locked";

		private const string kUnavailableStateLayer = "Unavailable";

		private const string kAvailableStateLayer = "Available";

		private const string kUnlockedStateLayer = "Unlocked";

		public Action<UpgradeTreeNode> OnUpgradeNodeSelected;

		public Action<UpgradeTreeNode> OnStateDidChange;

		[Header("Components")]
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private TextMeshProUGUI _title;

		[SerializeField]
		private Localize _localize;

		[SerializeField]
		private Animator _animator;

		[Header("Upgrade Category")]
		[SerializeField]
		private TextMeshProUGUI _categoryText;

		[SerializeField]
		private UpgradeCategoryIconMapping _categoryIconMapping;

		[Header("Upgrade")]
		[SerializeField]
		private UpgradeTreeConfiguration _treeConfig;

		[SerializeField]
		private UpgradeSystem.Type _upgrade;

		[SerializeField]
		private StructureBrain.TYPES _requiresBuiltStructure;

		[SerializeField]
		private Image _upgradeIcon;

		[SerializeField]
		private UpgradeTypeMapping _upgradeMapping;

		[Header("Tier")]
		[SerializeField]
		private TreeTier _nodeTier;

		[SerializeField]
		private UpgradeTreeNode[] _prerequisiteNodes;

		[SerializeField]
		private List<UpgradeTreeNode> _nodeConnections = new List<UpgradeTreeNode>();

		[Header("Tree Appearance Modifiers")]
		[SerializeField]
		private bool _nonCenteredStem;

		private NodeState _state;

		private float _lockedWeight;

		private float _unavailableWeight;

		private float _availableWeight;

		private float _unlockedWeight;

		private bool _configured;

		private UpgradeTreeConfiguration.TreeTierConfig _tierConfig;

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public UpgradeSystem.Type Upgrade
		{
			get
			{
				return _upgrade;
			}
		}

		public StructureBrain.TYPES RequiresBuiltStructure
		{
			get
			{
				return _requiresBuiltStructure;
			}
		}

		public NodeState State
		{
			get
			{
				return _state;
			}
		}

		public float LockedWeight
		{
			get
			{
				return _lockedWeight;
			}
		}

		public float UnavailableWeight
		{
			get
			{
				return _unavailableWeight;
			}
		}

		public float AvailableWeight
		{
			get
			{
				return _availableWeight;
			}
		}

		public float UnlockedWeight
		{
			get
			{
				return _unlockedWeight;
			}
		}

		public TreeTier NodeTier
		{
			get
			{
				return _nodeTier;
			}
		}

		public UpgradeTreeConfiguration.TreeTierConfig TierConfig
		{
			get
			{
				return _tierConfig;
			}
		}

		public UpgradeTreeConfiguration TreeConfig
		{
			get
			{
				return _treeConfig;
			}
		}

		public UpgradeTreeNode[] PrerequisiteNodes
		{
			get
			{
				return _prerequisiteNodes;
			}
		}

		public bool NonCenteredStem
		{
			get
			{
				return _nonCenteredStem;
			}
		}

		public List<UpgradeTreeNode> NodeConnections
		{
			get
			{
				return _nodeConnections;
			}
		}

		private void Start()
		{
			_button.onClick.AddListener(OnButtonClicked);
		}

		public void Configure(TreeTier currentTreeTier, bool forceReconfigure = false)
		{
			NodeState state = _state;
			if (forceReconfigure)
			{
				_configured = false;
			}
			if (_tierConfig == null)
			{
				_tierConfig = _treeConfig.GetConfigForTier(_nodeTier);
			}
			if (!_configured)
			{
				if (_requiresBuiltStructure != 0 && !DataManager.Instance.HistoryOfStructures.Contains(RequiresBuiltStructure))
				{
					_state = NodeState.Locked;
					_configured = true;
				}
				else if (UpgradeSystem.GetUnlocked(_upgrade))
				{
					_state = NodeState.Unlocked;
					_configured = true;
				}
				else if (_tierConfig.RequiresCentralTier && _tierConfig.CentralNode == Upgrade && _treeConfig.NumUnlockedUpgrades() >= _treeConfig.NumRequiredNodesForTier(_nodeTier) && UnlockedPrerequisites())
				{
					_state = NodeState.Available;
					_configured = true;
				}
				else if (currentTreeTier < _nodeTier)
				{
					_state = NodeState.Locked;
					_configured = true;
				}
				else
				{
					_state = NodeState.Unavailable;
					if (_prerequisiteNodes.Length != 0)
					{
						UpgradeTreeNode[] prerequisiteNodes = _prerequisiteNodes;
						foreach (UpgradeTreeNode upgradeTreeNode in prerequisiteNodes)
						{
							if (!upgradeTreeNode._configured)
							{
								upgradeTreeNode.Configure(currentTreeTier);
							}
							if (upgradeTreeNode.State == NodeState.Unlocked)
							{
								_state = NodeState.Available;
								break;
							}
						}
					}
					else
					{
						_state = NodeState.Available;
					}
					_configured = true;
				}
				if (!forceReconfigure)
				{
					if (_state == NodeState.Locked)
					{
						_lockedWeight = 1f;
					}
					else if (_state == NodeState.Unavailable)
					{
						_unavailableWeight = 1f;
					}
					else if (_state == NodeState.Available)
					{
						_availableWeight = 1f;
					}
					else if (_state == NodeState.Unlocked)
					{
						_unlockedWeight = 1f;
					}
				}
			}
			foreach (UpgradeTreeNode nodeConnection in _nodeConnections)
			{
				if (!nodeConnection._configured || forceReconfigure)
				{
					nodeConnection.Configure(currentTreeTier, forceReconfigure);
				}
			}
			if (!forceReconfigure)
			{
				UpdateAnimationLayerStates();
			}
			if (_state != state)
			{
				Action<UpgradeTreeNode> onStateDidChange = OnStateDidChange;
				if (onStateDidChange != null)
				{
					onStateDidChange(this);
				}
			}
		}

		private bool UnlockedPrerequisites()
		{
			UpgradeTreeNode[] prerequisiteNodes = _prerequisiteNodes;
			for (int i = 0; i < prerequisiteNodes.Length; i++)
			{
				if (prerequisiteNodes[i].State == NodeState.Unlocked)
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerator DoUpdateStateAnimation()
		{
			Vector2 scale = _rectTransform.localScale;
			UpdateAnimationLayerStates();
			if (State == NodeState.Unlocked)
			{
				UIManager.PlayAudio("event:/unlock_building/unlock");
			}
			else if (State == NodeState.Available)
			{
				UIManager.PlayAudio("event:/unlock_building/selection_flash");
			}
			_rectTransform.DOScale(scale * 1.5f, 0.25f).SetEase(Ease.OutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			StartCoroutine(UpdateStateWeights(0.75f));
			base.transform.DOScale(scale, 0.25f).SetEase(Ease.InSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
		}

		private IEnumerator UpdateStateWeights(float duration)
		{
			float t = 0f;
			while (t < duration)
			{
				t += Time.unscaledDeltaTime;
				if (_lockedWeight < 1f && _state == NodeState.Locked)
				{
					_lockedWeight = t / duration;
				}
				else if (_lockedWeight > 0f)
				{
					_lockedWeight = 1f - t / duration;
				}
				if (_unavailableWeight < 1f && _state == NodeState.Unavailable)
				{
					_unavailableWeight = t / duration;
				}
				else if (_unavailableWeight > 0f)
				{
					_unavailableWeight = 1f - t / duration;
				}
				if (_availableWeight < 1f && _state == NodeState.Available)
				{
					_availableWeight = t / duration;
				}
				else if (_availableWeight > 0f)
				{
					_availableWeight = 1f - t / duration;
				}
				if (_unlockedWeight < 1f && _state == NodeState.Unlocked)
				{
					_unlockedWeight = t / duration;
				}
				else if (_unlockedWeight > 0f)
				{
					_unlockedWeight = 1f - t / duration;
				}
				yield return null;
			}
		}

		private void OnButtonClicked()
		{
			Action<UpgradeTreeNode> onUpgradeNodeSelected = OnUpgradeNodeSelected;
			if (onUpgradeNodeSelected != null)
			{
				onUpgradeNodeSelected(this);
			}
		}

		public void UpdateAnimationLayerStates()
		{
			SetAnimationLayerState(_state);
		}

		private void SetAnimationLayerState(NodeState nodeState)
		{
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), (nodeState == NodeState.Locked) ? 1 : 0);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Unavailable"), (nodeState == NodeState.Unavailable) ? 1 : 0);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Available"), (nodeState == NodeState.Available) ? 1 : 0);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Unlocked"), (nodeState == NodeState.Unlocked) ? 1 : 0);
		}

		private void OnValidate()
		{
			if (_upgradeMapping != null)
			{
				UpgradeTypeMapping.SpriteItem item = _upgradeMapping.GetItem(_upgrade);
				if (item != null)
				{
					if (_categoryText != null && _categoryIconMapping != null)
					{
						_categoryText.text = UpgradeCategoryIconMapping.GetIcon(item.Category);
						_categoryText.color = _categoryIconMapping.GetColor(item.Category);
					}
					if (_upgradeIcon != null)
					{
						_upgradeIcon.sprite = item.Sprite;
					}
				}
			}
			if (_title != null)
			{
				_title.text = UpgradeSystem.GetLocalizedName(_upgrade);
				_localize.Term = string.Format("UpgradeSystem/{0}/Name", _upgrade);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			UIManager.PlayAudio("event:/upgrade_statue/upgrade_statue_scroll");
		}
	}
}
