using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Lamb.UI
{
	public class BranchConnectionListener : ConnectionListener
	{
		[SerializeReference]
		public List<ConnectionListener> Connections = new List<ConnectionListener>();

		public override void Configure(MMUILineRenderer.Branch rootBranch)
		{
			base.Configure(rootBranch);
			UnityEngine.Debug.Log("Configure Branch Connection Listener".Colour(Color.yellow));
			foreach (ConnectionListener connection in Connections)
			{
				if (connection.HighestNodeState > _highestNodeState)
				{
					_highestNodeState = connection.HighestNodeState;
				}
			}
			_targetNodeState = _highestNodeState;
			UpdateState((Branch.FillStyle != MMUILineRenderer.FillStyle.Reverse) ? 1 : 0);
		}

		private void OnConnectionStateDidChange(ConnectionListener connectionListener)
		{
			if (Branch.FillStyle == MMUILineRenderer.FillStyle.Reverse)
			{
				if (connectionListener.TargetNodeState > _highestNodeState)
				{
					_targetNodeState = connectionListener.TargetNodeState;
					_isDirty = true;
				}
			}
			else if (connectionListener.HighestNodeState > _highestNodeState)
			{
				_highestNodeState = connectionListener.HighestNodeState;
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

		private void OnEnable()
		{
			foreach (ConnectionListener connection in Connections)
			{
				connection.OnStateChanged = (Action<ConnectionListener>)Delegate.Combine(connection.OnStateChanged, new Action<ConnectionListener>(OnConnectionStateDidChange));
			}
		}

		private void OnDisable()
		{
			foreach (ConnectionListener connection in Connections)
			{
				connection.OnStateChanged = (Action<ConnectionListener>)Delegate.Remove(connection.OnStateChanged, new Action<ConnectionListener>(OnConnectionStateDidChange));
			}
		}

		protected override IEnumerator DoFillAnimation()
		{
			yield return _003C_003En__0();
			foreach (ConnectionListener connection in Connections)
			{
				if (connection.IsDirty)
				{
					connection.PerformFillAnimation();
				}
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
