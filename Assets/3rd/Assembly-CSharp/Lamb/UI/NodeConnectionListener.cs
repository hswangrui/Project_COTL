using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Lamb.UI
{
	public class NodeConnectionListener : ConnectionListener
	{
		public UpgradeTreeNode Node;

		public override void Configure(MMUILineRenderer.Branch rootBranch)
		{
			base.Configure(rootBranch);
			UnityEngine.Debug.Log("Configure Node Connection Listener".Colour(Color.cyan));
			_highestNodeState = Node.State;
			_targetNodeState = _highestNodeState;
			UpdateState((Branch.FillStyle != MMUILineRenderer.FillStyle.Reverse) ? 1 : 0);
		}

		private void OnEnable()
		{
			UpgradeTreeNode node = Node;
			node.OnStateDidChange = (Action<UpgradeTreeNode>)Delegate.Combine(node.OnStateDidChange, new Action<UpgradeTreeNode>(OnNodeStateDidChange));
		}

		private void OnDisable()
		{
			UpgradeTreeNode node = Node;
			node.OnStateDidChange = (Action<UpgradeTreeNode>)Delegate.Remove(node.OnStateDidChange, new Action<UpgradeTreeNode>(OnNodeStateDidChange));
		}

		private void OnNodeStateDidChange(UpgradeTreeNode node)
		{
			if (Branch.FillStyle == MMUILineRenderer.FillStyle.Reverse)
			{
				if (Node.State > _highestNodeState)
				{
					_targetNodeState = Node.State;
					_isDirty = true;
				}
			}
			else if (Node.State > _highestNodeState)
			{
				_highestNodeState = Node.State;
				_isDirty = true;
			}
			if (_isDirty)
			{
				Action<ConnectionListener> onStateChanged = OnStateChanged;
				if (onStateChanged != null)
				{
					onStateChanged(this);
				}
				UpdateState((Branch.FillStyle == MMUILineRenderer.FillStyle.Reverse) ? 1 : 0);
			}
		}

		protected override IEnumerator DoFillAnimation()
		{
			yield return _003C_003En__0();
			if (Branch.FillStyle != MMUILineRenderer.FillStyle.Reverse)
			{
				yield return Node.DoUpdateStateAnimation();
			}
			_isDirty = false;
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoFillAnimation();
		}
	}
}
