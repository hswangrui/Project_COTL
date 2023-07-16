using System;
using System.Collections;
using Lamb.UI.Assets;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class ConnectionListener : BaseMonoBehaviour
	{
		protected float kFillPerSecond = 2500f;

		public Action<ConnectionListener> OnStateChanged;

		protected bool _isDirty;

		protected UpgradeTreeNode.NodeState _highestNodeState;

		protected UpgradeTreeNode.NodeState _targetNodeState;

		[NonSerialized]
		public MMUILineRenderer.Branch Branch;

		public int BranchHash;

		public UpgradeTreeNode.TreeTier TreeTier;

		public int Depth;

		public UpgradeTreeConfiguration Configuration;

		public bool IsDirty
		{
			get
			{
				return _isDirty;
			}
		}

		public UpgradeTreeNode.NodeState HighestNodeState
		{
			get
			{
				return _highestNodeState;
			}
		}

		public UpgradeTreeNode.NodeState TargetNodeState
		{
			get
			{
				return _targetNodeState;
			}
		}

		public virtual void Configure(MMUILineRenderer.Branch rootBranch)
		{
			if (Branch == null)
			{
				Branch = rootBranch.FindBranchByHash(BranchHash);
			}
		}

		protected void UpdateState(float fill)
		{
			UpdateColor(_highestNodeState);
			Branch.Fill = fill;
		}

		private void UpdateColor(UpgradeTreeNode.NodeState nodeState)
		{
			switch (nodeState)
			{
			case UpgradeTreeNode.NodeState.Locked:
				Branch.Color = StaticColors.DarkGreyColor;
				break;
			case UpgradeTreeNode.NodeState.Unavailable:
				Branch.Color = StaticColors.GreyColor;
				break;
			case UpgradeTreeNode.NodeState.Available:
				Branch.Color = StaticColors.RedColor;
				break;
			case UpgradeTreeNode.NodeState.Unlocked:
				Branch.Color = StaticColors.GreenColor;
				break;
			}
		}

		public void PerformFillAnimation()
		{
			StartCoroutine(DoFillAnimation());
		}

		protected virtual IEnumerator DoFillAnimation()
		{
			float rate = kFillPerSecond / Branch.TotalLength;
			if (Branch.FillStyle == MMUILineRenderer.FillStyle.Standard)
			{
				while (Branch.Fill < 1f)
				{
					Branch.Fill += rate * Time.unscaledDeltaTime;
					yield return null;
				}
			}
			else if (Branch.FillStyle == MMUILineRenderer.FillStyle.Reverse)
			{
				while (Branch.Fill > 0f)
				{
					Branch.Fill -= rate * Time.unscaledDeltaTime;
					yield return null;
				}
				_highestNodeState = _targetNodeState;
				UpdateColor(_highestNodeState);
			}
		}
	}
}
