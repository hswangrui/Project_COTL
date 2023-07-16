using System;
using System.Collections;
using System.Collections.Generic;
using Lamb.UI.Assets;
using UnityEngine;

namespace Lamb.UI
{
	public class NodeConnectionLine : BaseMonoBehaviour
	{
		[SerializeField]
		private MMUILineRenderer _nodeLines;

		[SerializeField]
		[SerializeReference]
		private List<ConnectionListener> _connections = new List<ConnectionListener>();

		[SerializeField]
		private UpgradeTreeConfiguration _configuration;

		[SerializeField]
		private List<UpgradeTreeNode> _nodes = new List<UpgradeTreeNode>();

		private List<ConnectionListener> _dirtyListeners = new List<ConnectionListener>();

		public List<UpgradeTreeNode> Nodes
		{
			get
			{
				return _nodes;
			}
		}

		public bool IsDirty
		{
			get
			{
				return _dirtyListeners.Count > 0;
			}
		}

		private void Awake()
		{
			List<ConnectionListener> list = new List<ConnectionListener>(_connections);
			list.Reverse();
			foreach (ConnectionListener item in list)
			{
				item.Configure(_nodeLines.Root);
				item.OnStateChanged = (Action<ConnectionListener>)Delegate.Combine(item.OnStateChanged, new Action<ConnectionListener>(OnConnectionStateChanged));
			}
		}

		private void OnConnectionStateChanged(ConnectionListener connectionListener)
		{
			_dirtyListeners.Add(connectionListener);
		}

		public void PerformLineAnimation()
		{
			StartCoroutine(DoLineAnimation());
		}

		private IEnumerator DoLineAnimation()
		{
			int num = int.MaxValue;
			foreach (ConnectionListener dirtyListener in _dirtyListeners)
			{
				if (dirtyListener.Depth < num)
				{
					num = dirtyListener.Depth;
				}
			}
			foreach (ConnectionListener dirtyListener2 in _dirtyListeners)
			{
				if (dirtyListener2.Depth == num)
				{
					dirtyListener2.PerformFillAnimation();
				}
			}
			bool completed = false;
			while (!completed)
			{
				completed = true;
				yield return null;
				foreach (ConnectionListener dirtyListener3 in _dirtyListeners)
				{
					if (dirtyListener3.IsDirty)
					{
						completed = false;
						break;
					}
				}
			}
			yield return null;
			_dirtyListeners.Clear();
		}
	}
}
