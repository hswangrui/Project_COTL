using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Upgrade Tree Configuration", menuName = "Massive Monster/Upgrade Tree Configuration", order = 1)]
	public class UpgradeTreeConfiguration : ScriptableObject
	{
		[Serializable]
		public class TreeTierConfig
		{
			[SerializeField]
			private UpgradeTreeNode.TreeTier _tier;

			[SerializeField]
			private bool _requiresCentralTier = true;

			[SerializeField]
			private UpgradeSystem.Type _centralNode;

			[SerializeField]
			private int _numRequiredToUnlock;

			public UpgradeTreeNode.TreeTier Tier
			{
				get
				{
					return _tier;
				}
			}

			public UpgradeSystem.Type CentralNode
			{
				get
				{
					return _centralNode;
				}
			}

			public bool RequiresCentralTier
			{
				get
				{
					return _requiresCentralTier;
				}
			}

			public int NumRequiredToUnlock
			{
				get
				{
					return _numRequiredToUnlock;
				}
			}
		}

		[Header("Tiers")]
		[SerializeField]
		private List<TreeTierConfig> _tierConfigurations = new List<TreeTierConfig>();

		[Header("Connections")]
		[SerializeField]
		private float _tierBridgeLength;

		[SerializeField]
		private float _branchOffset;

		[SerializeField]
		private Texture _lockedTexture;

		[SerializeField]
		private Texture _unavailableTexture;

		[SerializeField]
		private Texture _availableTexture;

		[SerializeField]
		private Texture _unlockedTexture;

		[SerializeField]
		private Color _lockedConnectionColor;

		[SerializeField]
		private Color _unavailableConnectionColor;

		[SerializeField]
		private Color _availableConnectionColor;

		[SerializeField]
		private Color _unlockedConnectionColor;

		[SerializeField]
		private List<UpgradeSystem.Type> _allUpgrades = new List<UpgradeSystem.Type>();

		public float TierBridgeLength
		{
			get
			{
				return _tierBridgeLength;
			}
		}

		public float BranchOffset
		{
			get
			{
				return _branchOffset;
			}
		}

		public Texture LockedTexture
		{
			get
			{
				return _lockedTexture;
			}
		}

		public Texture UnavailableTexture
		{
			get
			{
				return _unavailableTexture;
			}
		}

		public Texture AvailableTexture
		{
			get
			{
				return _availableTexture;
			}
		}

		public Texture UnlockedTexture
		{
			get
			{
				return _unlockedTexture;
			}
		}

		public Color LockedConnectionColor
		{
			get
			{
				return _lockedConnectionColor;
			}
		}

		public Color UnavailableConnectionColor
		{
			get
			{
				return _unavailableConnectionColor;
			}
		}

		public Color AvailableConnectionColor
		{
			get
			{
				return _availableConnectionColor;
			}
		}

		public Color UnlockedConnectionColor
		{
			get
			{
				return _unlockedConnectionColor;
			}
		}

		public List<UpgradeSystem.Type> AllUpgrades
		{
			get
			{
				return _allUpgrades;
			}
		}

		public TreeTierConfig GetConfigForTier(UpgradeTreeNode.TreeTier tier)
		{
			foreach (TreeTierConfig tierConfiguration in _tierConfigurations)
			{
				if (tierConfiguration.Tier == tier)
				{
					return tierConfiguration;
				}
			}
			return null;
		}

		public int NumRequiredNodesForTier(UpgradeTreeNode.TreeTier tier)
		{
			int num = 0;
			foreach (TreeTierConfig tierConfiguration in _tierConfigurations)
			{
				if (tierConfiguration.Tier <= tier)
				{
					num += tierConfiguration.NumRequiredToUnlock;
					continue;
				}
				break;
			}
			return num;
		}

		public float NormalizedProgressToNextTier(UpgradeTreeNode.TreeTier tier)
		{
			int num = NumRequiredNodesForTier(tier);
			int num2 = NumUnlockedUpgrades();
			return 1f - Mathf.Clamp((float)(num - num2) / (float)GetConfigForTier(tier).NumRequiredToUnlock, 0f, 1f);
		}

		public float TotalNormalizedProgress()
		{
			return 1 - _allUpgrades.Count / NumUnlockedUpgrades();
		}

		public int NumUnlockedUpgrades()
		{
			int num = 0;
			foreach (UpgradeSystem.Type allUpgrade in _allUpgrades)
			{
				if (UpgradeSystem.GetUnlocked(allUpgrade))
				{
					num++;
				}
			}
			return num;
		}

		public UpgradeTreeNode.TreeTier HighestTier()
		{
			UpgradeTreeNode.TreeTier treeTier = UpgradeTreeNode.TreeTier.Tier1;
			foreach (TreeTierConfig tierConfiguration in _tierConfigurations)
			{
				if (tierConfiguration.Tier > treeTier)
				{
					treeTier = tierConfiguration.Tier;
				}
			}
			return treeTier;
		}

		public bool HasUnlockAvailable()
		{
			int num = 0;
			foreach (UpgradeSystem.Type allUpgrade in _allUpgrades)
			{
				if (UpgradeSystem.GetUnlocked(allUpgrade))
				{
					num++;
				}
			}
			return num < _allUpgrades.Count;
		}
	}
}
