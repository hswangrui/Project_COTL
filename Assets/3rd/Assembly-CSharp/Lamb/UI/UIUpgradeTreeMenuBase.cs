using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Lamb.UI.Assets;
using src.Extensions;
using src.UINavigator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIUpgradeTreeMenuBase<T> : UIMenuBase where T : UIUpgradeUnlockOverlayControllerBase
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass42_0
		{
			public List<UpgradeTreeNode> changedNodes;

			internal void _003CDoUnlockAnimation_003Eg__StateChanged_007C0(UpgradeTreeNode changedNode)
			{
				if (!changedNodes.Contains(changedNode))
				{
					changedNodes.Add(changedNode);
				}
			}
		}

		public Action<UpgradeSystem.Type> OnUpgradeUnlocked;

		[Header("Upgrade Tree Menu")]
		[SerializeField]
		private UpgradeTreeConfiguration _configuration;

		[SerializeField]
		private AnimationCurve _focusCurve;

		[SerializeField]
		private RectTransform _treeContainer;

		[SerializeField]
		private RectTransform _nodeContainer;

		[SerializeField]
		private RectTransform _connectionContainer;

		[SerializeField]
		private RectTransform _tierLockContainer;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _rootViewport;

		[SerializeField]
		private RectTransform _nodeNameRectTransform;

		[SerializeField]
		private TextMeshProUGUI _nodeNameText;

		[Header("Cursor")]
		[SerializeField]
		protected UpgradeMenuCursor _cursor;

		[Header("Node Tree")]
		[SerializeField]
		private Material _materialNode;

		[SerializeField]
		private UpgradeTreeNode _rootNode;

		[SerializeField]
		protected List<UpgradeTreeNode> _treeNodes = new List<UpgradeTreeNode>();

		[SerializeField]
		private List<NodeConnectionLine> _nodeConnections = new List<NodeConnectionLine>();

		[SerializeField]
		private List<TierLockIcon> _tierLocks = new List<TierLockIcon>();

		[Header("Upgrade")]
		[SerializeField]
		private T _unlockOverlayTemplate;

		[Header("Points")]
		[SerializeField]
		private GameObject _pointsContainer;

		[SerializeField]
		protected TextMeshProUGUI _pointsText;

		[Header("Effects")]
		[SerializeField]
		private UpgradeTreeComputeShaderController _computeShaderController;

		[SerializeField]
		private GoopFade _goopFade;

		private Coroutine _focusCoroutine;

		public GameObject disableBackPrompt;

		private bool _didCancel;

		public bool _didUpgraded;

		private Camera currentMain;

		private float previousClipPlane;

		public override void Awake()
		{
			base.Awake();
			_rootNode.Configure(TreeTier());
			foreach (TierLockIcon tierLock in _tierLocks)
			{
				tierLock.Configure(TreeTier());
			}
			_canvasGroup.alpha = 0f;
		}

		protected override void OnShowStarted()
		{
			AudioManager.Instance.PauseActiveLoops();
			if (BiomeConstants.Instance != null)
			{
				BiomeConstants.Instance.GoopFadeOut(1f, 0f, false);
			}
			foreach (UpgradeTreeNode treeNode in _treeNodes)
			{
				treeNode.OnUpgradeNodeSelected = (Action<UpgradeTreeNode>)Delegate.Combine(treeNode.OnUpgradeNodeSelected, new Action<UpgradeTreeNode>(OnNodeSelected));
			}
			Selectable defaultSelectable = GetDefaultSelectable();
			if (defaultSelectable != null)
			{
				OverrideDefault(defaultSelectable);
				_treeContainer.anchoredPosition = -defaultSelectable.GetComponent<RectTransform>().anchoredPosition;
				_cursor.RectTransform.anchoredPosition = defaultSelectable.GetComponent<RectTransform>().anchoredPosition;
				_cursor.RectTransform.localScale = defaultSelectable.transform.localScale;
			}
			UpdatePointsText();
		}

		protected abstract Selectable GetDefaultSelectable();

		protected override void OnShowCompleted()
		{
			ActivateNavigation();
		}

		private void OnEnable()
		{
			UpgradeMenuCursor cursor = _cursor;
			cursor.OnAtRest = (Action)Delegate.Combine(cursor.OnAtRest, new Action(OnCursorAtRest));
			UpgradeMenuCursor cursor2 = _cursor;
			cursor2.OnCursorMoved = (Action<Vector2>)Delegate.Combine(cursor2.OnCursorMoved, new Action<Vector2>(OnCursorMoved));
			MonoSingleton<UINavigatorNew>.Instance.LockNavigation = true;
		}

		private void OnDisable()
		{
			UpgradeMenuCursor cursor = _cursor;
			cursor.OnAtRest = (Action)Delegate.Remove(cursor.OnAtRest, new Action(OnCursorAtRest));
			UpgradeMenuCursor cursor2 = _cursor;
			cursor2.OnCursorMoved = (Action<Vector2>)Delegate.Remove(cursor2.OnCursorMoved, new Action<Vector2>(OnCursorMoved));
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.LockNavigation = false;
			}
		}

		private void OnCursorMoved(Vector2 movement)
		{
			Vector2 anchoredPosition = _scrollRect.content.anchoredPosition;
			anchoredPosition += movement;
			_scrollRect.content.anchoredPosition = anchoredPosition;
		}

		private void OnCursorAtRest()
		{
			if (MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable != null)
			{
				OnSelection(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			}
		}

		private void OnSelection(Selectable selectable)
		{
			UpgradeTreeNode component;
			if (!InputManager.General.MouseInputActive && selectable.TryGetComponent<UpgradeTreeNode>(out component))
			{
				if (_focusCoroutine != null)
				{
					StopCoroutine(_focusCoroutine);
				}
				_focusCoroutine = StartCoroutine(DoFocusPosition(-component.RectTransform.anchoredPosition, 0.25f));
			}
		}

		private IEnumerator DoFocusPosition(Vector2 focalPoint, float time, float zoom = 1f)
		{
			float num = (_treeContainer.rect.width - _rootViewport.rect.width) * 0.5f;
			float num2 = (_treeContainer.rect.height - _rootViewport.rect.height) * 0.5f;
			focalPoint.x = Mathf.Clamp(focalPoint.x, 0f - num, num);
			focalPoint.y = Mathf.Clamp(focalPoint.y, 0f - num2, num2);
			Vector2 currentPosition = _treeContainer.anchoredPosition;
			Vector2 currentZoom = _rootViewport.localScale;
			Vector2 targetZoom = Vector2.one * zoom;
			float t = 0f;
			while (t <= time)
			{
				t += Time.unscaledDeltaTime;
				_rootViewport.localScale = Vector2.Lerp(currentZoom, targetZoom, _focusCurve.Evaluate(t / time));
				_treeContainer.anchoredPosition = Vector2.Lerp(currentPosition, focalPoint, _focusCurve.Evaluate(t / time));
				yield return null;
			}
		}

		private void OnNodeSelected(UpgradeTreeNode node)
		{
			if (CheatConsole.QuickUnlock)
			{
				PerformUnlock(node);
				return;
			}
			_cursor.LockPosition = true;
			bool didUnlock = false;
			T upgradeOverlayInstance = _unlockOverlayTemplate.Instantiate();
			PushInstance(upgradeOverlayInstance);
			upgradeOverlayInstance.Show(node);
			T val = upgradeOverlayInstance;
			val.OnUnlocked = (Action)Delegate.Combine(val.OnUnlocked, (Action)delegate
			{
				didUnlock = true;
			});
			T val2 = upgradeOverlayInstance;
			val2.OnCancel = (Action)Delegate.Combine(val2.OnCancel, (Action)delegate
			{
			});
			T val3 = upgradeOverlayInstance;
			val3.OnShow = (Action)Delegate.Combine(val3.OnShow, (Action)delegate
			{
				if (disableBackPrompt != null)
				{
					disableBackPrompt.gameObject.SetActive(true);
				}
			});
			T val4 = upgradeOverlayInstance;
			val4.OnHidden = (Action)Delegate.Combine(val4.OnHidden, (Action)delegate
			{
				if (disableBackPrompt != null)
				{
					disableBackPrompt.gameObject.SetActive(false);
				}
				if (didUnlock)
				{
					PerformUnlock(node);
				}
				else
				{
					_cursor.LockPosition = false;
					SetActiveStateForMenu(true);
				}
				upgradeOverlayInstance = null;
			});
		}

		protected abstract int UpgradePoints();

		protected abstract void DoUnlock(UpgradeSystem.Type upgrade);

		protected abstract UpgradeTreeNode.TreeTier TreeTier();

		protected abstract void UpdateTier(UpgradeTreeNode.TreeTier tier);

		protected abstract string GetPointsText();

		protected virtual void UpdatePointsText()
		{
			if (_pointsText != null)
			{
				_pointsText.text = GetPointsText();
			}
		}

		private void PerformUnlock(UpgradeTreeNode node)
		{
			Debug.Log(string.Format("Unlock {0}!", node.Upgrade).Colour(Color.red));
			DoUnlock(node.Upgrade);
			UpdatePointsText();
			StartCoroutine(DoUnlockAnimation(node));
		}

		private IEnumerator DoUnlockAnimation(UpgradeTreeNode targetNode)
		{
			_003C_003Ec__DisplayClass42_0 _003C_003Ec__DisplayClass42_ = new _003C_003Ec__DisplayClass42_0();
			_003C_003Ec__DisplayClass42_.changedNodes = new List<UpgradeTreeNode>();
			float zoom2 = 1f;
			bool unlockedNewTier = false;
			bool unlockedCentralNode = false;
			OverrideDefault(targetNode.Button);
			SetActiveStateForMenu(false);
			_cursor.enabled = false;
			_cursor.CanvasGroup.DOFade(0f, 0.1f).SetUpdate(true);
			_scrollRect.enabled = false;
			yield return new WaitForSecondsRealtime(0.1f);
			targetNode.Configure(TreeTier(), true);
			foreach (UpgradeTreeNode nodeConnection in targetNode.NodeConnections)
			{
				if (nodeConnection.NodeTier <= TreeTier())
				{
					_003C_003Ec__DisplayClass42_.changedNodes.Add(nodeConnection);
				}
			}
			Vector2 focalPoint = targetNode.RectTransform.anchoredPosition;
			if (targetNode.PrerequisiteNodes != null && targetNode.PrerequisiteNodes.Length != 0)
			{
				UpgradeTreeNode item = targetNode.PrerequisiteNodes[0];
				DetermineFocalPointAndZoom(new List<UpgradeTreeNode> { item, targetNode }, out focalPoint, out zoom2);
			}
			yield return DoFocusPosition(-focalPoint, 0.25f, zoom2);
			List<NodeConnectionLine> list = new List<NodeConnectionLine>();
			foreach (NodeConnectionLine nodeConnection2 in _nodeConnections)
			{
				if (nodeConnection2.Nodes.Contains(targetNode))
				{
					list.Add(nodeConnection2);
					nodeConnection2.PerformLineAnimation();
				}
			}
			if (list.Count > 0)
			{
				yield return YieldForConnections(list);
			}
			else
			{
				yield return targetNode.DoUpdateStateAnimation();
			}
			yield return new WaitForSecondsRealtime(0.1f);
			zoom2 = 1f;
			foreach (UpgradeTreeNode treeNode in _treeNodes)
			{
				treeNode.OnStateDidChange = (Action<UpgradeTreeNode>)Delegate.Combine(treeNode.OnStateDidChange, new Action<UpgradeTreeNode>(_003C_003Ec__DisplayClass42_._003CDoUnlockAnimation_003Eg__StateChanged_007C0));
			}
			UpgradeTreeNode.TreeTier treeTier = TreeTier() + 1;
			UpgradeTreeConfiguration.TreeTierConfig configForTier = _configuration.GetConfigForTier(treeTier);
			if (configForTier != null)
			{
				int num = _configuration.NumRequiredNodesForTier(treeTier);
				int num2 = _configuration.NumUnlockedUpgrades();
				Debug.Log(string.Format("{0} - {1}/{2}", treeTier - 1, num2, num).Colour(Color.yellow));
				if (num2 >= num)
				{
					if (!configForTier.RequiresCentralTier || targetNode.Upgrade == configForTier.CentralNode)
					{
						unlockedNewTier = true;
						UpdateTier(treeTier);
						_rootNode.Configure(TreeTier(), true);
					}
					else
					{
						foreach (UpgradeTreeNode treeNode2 in _treeNodes)
						{
							if (treeNode2.Upgrade == configForTier.CentralNode && treeNode2.State == UpgradeTreeNode.NodeState.Locked)
							{
								treeNode2.Configure(TreeTier(), true);
								break;
							}
						}
					}
				}
			}
			foreach (UpgradeTreeNode treeNode3 in _treeNodes)
			{
				treeNode3.OnStateDidChange = (Action<UpgradeTreeNode>)Delegate.Remove(treeNode3.OnStateDidChange, new Action<UpgradeTreeNode>(_003C_003Ec__DisplayClass42_._003CDoUnlockAnimation_003Eg__StateChanged_007C0));
			}
			TierLockIcon targetTierLock = null;
			if (unlockedNewTier)
			{
				foreach (TierLockIcon tierLock in _tierLocks)
				{
					if (tierLock.Tier == TreeTier())
					{
						targetTierLock = tierLock;
						break;
					}
				}
			}
			else
			{
				UpgradeTreeNode.TreeTier[] obj = Enum.GetValues(typeof(UpgradeTreeNode.TreeTier)) as UpgradeTreeNode.TreeTier[];
				int num3 = _configuration.NumUnlockedUpgrades();
				UpgradeTreeNode.TreeTier[] array = obj;
				foreach (UpgradeTreeNode.TreeTier treeTier2 in array)
				{
					int num4 = _configuration.NumRequiredNodesForTier(treeTier2);
					if (num3 != num4)
					{
						continue;
					}
					unlockedCentralNode = true;
					foreach (TierLockIcon tierLock2 in _tierLocks)
					{
						if (tierLock2.Tier == treeTier2)
						{
							targetTierLock = tierLock2;
							break;
						}
					}
					break;
				}
			}
			if (unlockedCentralNode)
			{
				yield return DoFocusPosition(-targetTierLock.RectTransform.anchoredPosition, 0.25f, zoom2);
				yield return targetTierLock.DestroyTierLock();
				yield return new WaitForSecondsRealtime(0.1f);
				focalPoint = ((targetTierLock.Tier - TreeTier() != 1 || _003C_003Ec__DisplayClass42_.changedNodes.Count <= 0) ? targetNode.RectTransform.anchoredPosition : _003C_003Ec__DisplayClass42_.changedNodes[0].RectTransform.anchoredPosition);
			}
			else if (unlockedNewTier)
			{
				List<UpgradeTreeNode> list2 = new List<UpgradeTreeNode>();
				foreach (UpgradeTreeNode treeNode4 in _treeNodes)
				{
					if (treeNode4.NodeTier == TreeTier())
					{
						list2.Add(treeNode4);
					}
				}
				DetermineFocalPointAndZoom(list2, out focalPoint, out zoom2);
			}
			else
			{
				List<UpgradeTreeNode> list3 = new List<UpgradeTreeNode>(_003C_003Ec__DisplayClass42_.changedNodes);
				list3.Add(targetNode);
				DetermineFocalPointAndZoom(list3, out focalPoint, out zoom2);
			}
			yield return DoFocusPosition(-focalPoint, 0.25f, zoom2);
			if (unlockedNewTier)
			{
				yield return targetTierLock.RevealTier();
			}
			foreach (NodeConnectionLine nodeConnection3 in _nodeConnections)
			{
				if (nodeConnection3.IsDirty)
				{
					nodeConnection3.PerformLineAnimation();
				}
			}
			yield return YieldForConnections(_nodeConnections);
			yield return new WaitForSecondsRealtime(0.1f);
			if (zoom2 < 1f)
			{
				yield return DoFocusPosition(-targetNode.RectTransform.anchoredPosition, 0.25f);
			}
			OnUnlockAnimationCompleted();
			Action<UpgradeSystem.Type> onUpgradeUnlocked = OnUpgradeUnlocked;
			if (onUpgradeUnlocked != null)
			{
				onUpgradeUnlocked(targetNode.Upgrade);
			}
		}

		private void DetermineFocalPointAndZoom(List<UpgradeTreeNode> nodes, out Vector2 focalPoint, out float zoom)
		{
			zoom = 1f;
			Bounds bounds = new Bounds(nodes[0].RectTransform.anchoredPosition, Vector3.zero);
			foreach (UpgradeTreeNode node in nodes)
			{
				bounds.Encapsulate(node.RectTransform.anchoredPosition);
			}
			focalPoint = bounds.center;
			float num = 250f;
			float num2 = float.MaxValue;
			if (bounds.size.x > 1920f)
			{
				num2 = 1920f / (bounds.size.x + num);
			}
			float num3 = 400f;
			float num4 = float.MaxValue;
			if (bounds.size.y > 1080f)
			{
				num4 = 1080f / (bounds.size.y + num3);
			}
			zoom = Mathf.Clamp((num2 < num4) ? num2 : num4, 0f, 1f);
		}

		private IEnumerator YieldForConnections(List<NodeConnectionLine> connectionLines)
		{
			bool completed = false;
			while (!completed)
			{
				completed = true;
				yield return null;
				foreach (NodeConnectionLine connectionLine in connectionLines)
				{
					if (connectionLine.IsDirty)
					{
						completed = false;
						break;
					}
				}
			}
		}

		protected virtual void OnUnlockAnimationCompleted()
		{
			SetActiveStateForMenu(true);
			_scrollRect.enabled = true;
			_cursor.RectTransform.anchoredPosition = -_treeContainer.anchoredPosition;
			_cursor.CanvasGroup.DOFade(1f, 0.1f).SetUpdate(true).OnComplete(delegate
			{
				_cursor.RectTransform.localScale = ((MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable != null) ? MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable.transform.localScale : Vector3.one);
			});
			_cursor.enabled = true;
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable && !base.IsShowing)
			{
				if (!_didUpgraded)
				{
					_didCancel = true;
				}
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			AudioManager.Instance.ResumeActiveLoops();
			if (BiomeConstants.Instance != null)
			{
				BiomeConstants.Instance.GoopFadeOut(1f, 0f, false);
			}
			_cursor.enabled = false;
			UIManager.PlayAudio("event:/upgrade_statue/upgrade_statue_close");
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
