using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Map;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIAdventureMapOverlayController : UIMenuBase
	{
		public const float kNodePositionScale = 300f;

		[Header("General")]
		[SerializeField]
		private GoopFade _goopFade;

		[SerializeField]
		private CanvasGroup _containerCanvasGroup;

		[SerializeField]
		private MMScrollRect _scrollView;

		[SerializeField]
		private RectTransform _scrollContent;

		[SerializeField]
		private GameObject _controlPrompts;

		[Header("Nodes")]
		[SerializeField]
		private RectTransform _nodeContent;

		[Header("Connections")]
		[SerializeField]
		private RectTransform _connectionContent;

		[SerializeField]
		private Texture _connectionTexture;

		[SerializeField]
		private Material _scrollingMaterial;

		[SerializeField]
		private Material _idleDottedMaterial;

		[Header("Crown Spine")]
		[SerializeField]
		private RectTransform _crownSpineRectTransform;

		[SerializeField]
		private RectTransform _eyeSpineRectTransform;

		private global::Map.Map _map;

		private bool _disableInput;

		private float _mapHeight;

		private float _contentHeight;

		private List<AdventureMapNode> _adventureMapNodes = new List<AdventureMapNode>();

		private List<NodeConnection> _nodeConnections = new List<NodeConnection>();

		private AdventureMapNode _currentNode;

		private AdventureMapNode _selectedNode;

		private Tweener _scrollTween;

		private bool _didCancel;

		private bool _doCameraPositionMoveOnShow = true;

		private bool _FadeInNodesAndConnections;

		private Coroutine _scrollCoroutine;

		private bool cancellable;

		public Action OnNodeEntered;

		private bool set;

		public global::Map.Map Map
		{
			get
			{
				return _map;
			}
		}

		private bool _shouldScroll
		{
			get
			{
				return _contentHeight > _scrollView.viewport.rect.height;
			}
		}

		public static Color VisitedColour
		{
			get
			{
				return StaticColors.GreenColor;
			}
		}

		public static Color LockedColour
		{
			get
			{
				return StaticColors.DarkGreyColor;
			}
		}

		public static Color LockedColourLight
		{
			get
			{
				return StaticColors.LightGreyColor;
			}
		}

		public static Color TryVisitColour
		{
			get
			{
				return StaticColors.RedColor;
			}
		}

		public static Color AvailableColour
		{
			get
			{
				return StaticColors.OffWhiteColor;
			}
		}

		public bool Cancellable { get; set; } = true;


		public override void Awake()
		{
			base.Awake();
			_containerCanvasGroup.alpha = 0f;
		}

		public void Show(global::Map.Map map, bool disableInput = false, bool instant = false)
		{
			_map = map;
			_disableInput = disableInput;
			_controlPrompts.SetActive(false);
			_crownSpineRectTransform.localScale = Vector3.zero;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			AudioManager.Instance.PauseActiveLoops();
			UIManager.PlayAudio("event:/enter_leave_buildings/enter_building");
			foreach (Node node2 in _map.nodes)
			{
				if (node2.incoming.Count != 0 || node2.outgoing.Count != 0)
				{
					_adventureMapNodes.Add(MakeMapNode(node2));
				}
			}
			Bounds bounds = new Bounds(_adventureMapNodes[0].RectTransform.localPosition, Vector3.zero);
			for (int i = 1; i < _adventureMapNodes.Count; i++)
			{
				bounds.Encapsulate(_adventureMapNodes[i].RectTransform.localPosition);
			}
			foreach (AdventureMapNode adventureMapNode3 in _adventureMapNodes)
			{
				adventureMapNode3.RectTransform.localPosition -= bounds.center;
			}
			_mapHeight = bounds.size.y;
			_contentHeight = _mapHeight;
			if (_contentHeight > _scrollView.viewport.rect.height / 3f * 2f)
			{
				_contentHeight += _scrollView.viewport.rect.height;
			}
			if (_doCameraPositionMoveOnShow)
			{
				_scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(_contentHeight, _scrollView.viewport.rect.height));
			}
			if (_map.path.Count == 0)
			{
				_map.path.Add(_map.GetFirstNode().point);
			}
			foreach (Point item in _map.path)
			{
				AdventureMapNode adventureMapNode = NodeFromPoint(item);
				if (adventureMapNode != null)
				{
					adventureMapNode.SetState(NodeStates.Visited);
				}
			}
			Point point = _map.path.LastElement();
			Node node = _map.GetNode(point);
			_currentNode = NodeFromPoint(point);
			_currentNode.SetStartingNode();
			if (_doCameraPositionMoveOnShow)
			{
				_scrollView.content.anchoredPosition = new Vector2(0f, GetScrollPosition(_currentNode.RectTransform));
			}
			if (!_disableInput)
			{
				foreach (Point item2 in node.outgoing)
				{
					AdventureMapNode adventureMapNode2 = NodeFromPoint(item2);
					if (adventureMapNode2 != null)
					{
						adventureMapNode2.SetState(NodeStates.Attainable);
					}
				}
			}
			List<AdventureMapNode> list = new List<AdventureMapNode>();
			foreach (Point item3 in node.outgoing)
			{
				list.Add(NodeFromPoint(item3));
			}
			list.Sort((AdventureMapNode n1, AdventureMapNode n2) => n1.MapNode.point.x.CompareTo(n2.MapNode.point.x));
			for (int j = 0; j < list.Count; j++)
			{
				MMButton button = list[j].Button;
				Selectable selectOnUp = button.FindSelectableOnUp();
				Selectable selectOnDown = button.FindSelectableOnDown();
				Navigation navigation = button.Selectable.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnUp = selectOnUp;
				navigation.selectOnDown = selectOnDown;
				if (j < list.Count - 1)
				{
					navigation.selectOnRight = list[j + 1].Button;
				}
				if (j > 0)
				{
					navigation.selectOnLeft = list[j - 1].Button;
				}
				list[j].Button.Selectable.navigation = navigation;
			}
			foreach (AdventureMapNode adventureMapNode4 in _adventureMapNodes)
			{
				adventureMapNode4.Button.SetInteractionState(false);
			}
			Node nextNode = _map.GetNextNode(point);
			_selectedNode = NodeFromPoint(nextNode.point);
			OverrideDefault(_selectedNode.Button);
			foreach (AdventureMapNode adventureMapNode5 in _adventureMapNodes)
			{
				if (adventureMapNode5.MapNode.nodeType == NodeType.Boss)
				{
					adventureMapNode5.transform.localPosition = new Vector3(0f, adventureMapNode5.transform.localPosition.y, adventureMapNode5.transform.localPosition.z);
				}
			}
			_crownSpineRectTransform.position = _currentNode.transform.position;
			foreach (AdventureMapNode adventureMapNode6 in _adventureMapNodes)
			{
				foreach (Point item4 in adventureMapNode6.MapNode.outgoing)
				{
					MakeLineConnection(adventureMapNode6, NodeFromPoint(item4));
				}
			}
		}

		protected override IEnumerator DoShowAnimation()
		{
			AudioManager.Instance.PlayOneShot("event:/ui/open_menu");
			_goopFade.FadeIn(0.5f, 0f, false);
			yield return new WaitForSecondsRealtime(0.25f);
			_containerCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
			if (!_disableInput)
			{
				float position = (GetScrollPosition(_selectedNode.RectTransform) + GetScrollPosition(_currentNode.RectTransform)) / 2f;
				yield return ScrollTo(position, 0.5f);
			}
			_crownSpineRectTransform.DOScale(Vector3.one * 0.75f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.3f);
			if (!_disableInput)
			{
				SetActiveStateForMenu(true);
				ActivateNavigation();
				if (Cancellable)
				{
					_controlPrompts.SetActive(true);
				}
				cancellable = true;
			}
			if (BiomeConstants.Instance != null)
			{
				BiomeConstants.Instance.ChromaticAbberationTween(0.1f, 0.6f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable && cancellable && Cancellable)
			{
				_didCancel = true;
				Hide();
				AudioManager.Instance.ResumeActiveLoops();
				UIManager.PlayAudio("event:/enter_leave_buildings/leave_building");
				UIManager.PlayAudio("event:/ui/close_menu");
			}
		}

		protected override IEnumerator DoHideAnimation()
		{
			_goopFade.FadeOut(0.5f, 0f, false);
			_containerCanvasGroup.DOFade(0f, 0.25f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}

		protected override void OnHideCompleted()
		{
			foreach (AdventureMapNode adventureMapNode in _adventureMapNodes)
			{
				adventureMapNode.Recycle();
			}
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

		private float GetScrollPosition(RectTransform rectTransform)
		{
			return _mapHeight - (rectTransform.anchoredPosition.y + _mapHeight * 0.5f);
		}

		private IEnumerator ScrollTo(RectTransform rectTransform, float duration, Ease ease = Ease.OutSine, bool renableScrollViewAfter = true)
		{
			float scrollPosition = GetScrollPosition(rectTransform);
			yield return ScrollTo(scrollPosition, duration, ease, renableScrollViewAfter);
		}

		private IEnumerator ScrollTo(float position, float duration, Ease ease = Ease.OutSine, bool renableScrollViewAfter = true)
		{
			if (_shouldScroll)
			{
				_scrollView.enabled = false;
				ScrollToTween(position, duration, ease);
				yield return new WaitForSecondsRealtime(duration);
				if (renableScrollViewAfter)
				{
					_scrollView.enabled = true;
				}
			}
			_scrollCoroutine = null;
		}

		private void ScrollToTween(float position, float duration, Ease ease)
		{
			if (_shouldScroll)
			{
				_scrollView.DOKill();
				_scrollTween = _scrollView.content.DOAnchorPosY(position, duration).SetEase(ease).SetUpdate(true);
			}
		}

		public IEnumerator ConvertAllNodesToCombatNodes()
		{
			cancellable = false;
			ScrollToTween(1f, 3.5f, Ease.InOutSine);
			List<Node> nodes = MapGenerator.GetAllFutureNodes(MapGenerator.GetNodeLayer(_currentNode.MapNode));
			float increment = 8f / (float)nodes.Count;
			yield return new WaitForSecondsRealtime(0.1f);
			List<Node> nodesList = (from n in MapGenerator.Nodes.SelectMany((List<Node> n) => n)
				where n.incoming.Count > 0 || n.outgoing.Count > 0
				select n).ToList();
			for (int i = 0; i < nodesList.Count; i++)
			{
				if (nodes.Contains(nodesList[i]))
				{
					AdventureMapNode adventureMapNode = NodeFromPoint(nodesList[i].point);
					if (adventureMapNode != null && nodesList[i].nodeType != NodeType.DungeonFloor && nodesList[i].nodeType != NodeType.MiniBossFloor && nodesList[i].nodeType != NodeType.FirstFloor)
					{
						AudioManager.Instance.PlayOneShot("event:/ui/level_node_end_screen_ui_appear");
						nodesList[i].nodeType = NodeType.DungeonFloor;
						nodesList[i].Hidden = false;
						nodesList[i].blueprint = MapManager.Instance.DungeonConfig.SecondFloorBluePrint;
						nodesList[i].Modifier = null;
						adventureMapNode.Configure(nodesList[i]);
						adventureMapNode.Button.SetInteractionState(false);
						adventureMapNode.Punch();
						yield return new WaitForSecondsRealtime(increment);
					}
				}
			}
			while (_scrollTween != null && _scrollTween.active)
			{
				yield return null;
			}
			yield return new WaitForSecondsRealtime(0.1f);
			_map.nodes = nodesList;
		}

		public IEnumerator ConvertMiniBossNodeToBossNode()
		{
			cancellable = false;
			List<Node> list = (from n in _map.nodes
				select (n) into n
				where n.incoming.Count > 0 || n.outgoing.Count > 0
				select n).ToList();
			MapConfig dungeonConfig = MapManager.Instance.DungeonConfig;
			Node bossMapNode = _map.GetBossNode();
			AdventureMapNode bossNode = NodeFromPoint(bossMapNode.point);
			bossMapNode.nodeType = NodeType.MiniBossFloor;
			bossMapNode.Hidden = false;
			bossMapNode.blueprint = dungeonConfig.MiniBossFloorBluePrint;
			bossNode.Configure(bossMapNode);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].nodeType == NodeType.MiniBossFloor)
				{
					list[i].nodeType = NodeType.MiniBossFloor;
					list[i].Hidden = false;
					list[i].blueprint = dungeonConfig.LeaderFloorBluePrint;
					break;
				}
			}
			yield return ScrollTo(bossNode.RectTransform, 1.5f, Ease.InOutCubic);
			yield return new WaitForSecondsRealtime(0.25f);
			bossMapNode.blueprint = dungeonConfig.LeaderFloorBluePrint;
			bossNode.Configure(bossMapNode);
			bossNode.Button.SetInteractionState(false);
			bossNode.ScaleIn();
			AudioManager.Instance.PlayOneShot("event:/ui/level_node_beat_level");
			yield return new WaitForSecondsRealtime(1.25f);
		}

		public IEnumerator RandomiseNextNodes()
		{
			cancellable = false;
			ScrollToTween(1f, 3.5f, Ease.InOutSine);
			List<Node> nodes = MapGenerator.GetAllFutureNodes(MapGenerator.GetNodeLayer(_currentNode.MapNode));
			float increment = 8f / (float)nodes.Count;
			yield return new WaitForSecondsRealtime(0.1f);
			MapConfig dungeonConfig = MapManager.Instance.DungeonConfig;
			List<Node> nodesList = (from n in MapGenerator.Nodes.SelectMany((List<Node> n) => n)
				where n.incoming.Count > 0 || n.outgoing.Count > 0
				select n).ToList();
			int i;
			for (i = 0; i < nodesList.Count; i++)
			{
				if (!nodes.Contains(nodesList[i]))
				{
					continue;
				}
				AdventureMapNode adventureMapNode = NodeFromPoint(nodesList[i].point);
				if (adventureMapNode != null && nodesList[i].nodeType != NodeType.DungeonFloor && nodesList[i].nodeType != NodeType.MiniBossFloor && nodesList[i].nodeType != NodeType.FirstFloor)
				{
					nodesList[i].nodeType = MapGenerator.GetRandomNode(dungeonConfig);
					nodesList[i].Hidden = false;
					string blueprintName = dungeonConfig.nodeBlueprints.Where((NodeBlueprint b) => b.nodeType == nodesList[i].nodeType).ToList().Random()
						.name;
					NodeBlueprint blueprint = dungeonConfig.nodeBlueprints.FirstOrDefault((NodeBlueprint n) => n.name == blueprintName);
					nodesList[i].blueprint = blueprint;
					adventureMapNode.Configure(nodesList[i]);
					adventureMapNode.Button.SetInteractionState(false);
					adventureMapNode.Punch();
					yield return new WaitForSecondsRealtime(increment);
				}
			}
			_map.nodes = nodesList;
			while (_scrollTween != null && _scrollTween.active)
			{
				yield return null;
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}

		private void ResetMap()
		{
			foreach (AdventureMapNode mapNode in _adventureMapNodes)
			{
				mapNode.CanvasGroup.DOFade(0f, 1f).SetUpdate(true).OnComplete(delegate
				{
					mapNode.CanvasGroup.alpha = 1f;
					mapNode.Recycle();
				});
			}
			_adventureMapNodes.Clear();
			foreach (NodeConnection connection in _nodeConnections)
			{
				connection.LineRenderer.material.DOFade(0f, 1f).SetUpdate(true).OnComplete(delegate
				{
					connection.LineRenderer.material.DOFade(1f, 0f).SetUpdate(true);
					UnityEngine.Object.Destroy(connection.gameObject);
				});
			}
			_nodeConnections.Clear();
			_map.path.Clear();
			_map.nodes.Clear();
		}

		public IEnumerator RegenerateMapRoutine()
		{
			NodeFromPoint(_map.GetFirstNode().point);
			_doCameraPositionMoveOnShow = false;
			_FadeInNodesAndConnections = true;
			yield return ScrollTo(_mapHeight / 2f, 1.5f, Ease.InOutCubic);
			yield return new WaitForSecondsRealtime(0.25f);
			_scrollView.DOKill();
			ResetMap();
			yield return new WaitForSecondsRealtime(1f);
			_map = MapManager.Instance.GenerateNewMap();
			OnShowStarted();
			AudioManager.Instance.PlayOneShot("event:/ui/level_node_beat_level");
			yield return new WaitForSecondsRealtime(1f);
			AdventureMapNode firstNode = NodeFromPoint(_map.GetFirstNode().point);
			yield return ScrollTo(firstNode.RectTransform, 1f, Ease.InOutCubic, false);
			yield return new WaitForSecondsRealtime(0.25f);
			OnNodeChosen(firstNode);
			_doCameraPositionMoveOnShow = true;
			_FadeInNodesAndConnections = false;
		}

		public IEnumerator TeleportNode(Node node)
		{
			AdventureMapNode target = NodeFromPoint(node.point);
			Tweener t = _crownSpineRectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(t.Duration());
			yield return new WaitForSecondsRealtime(0.1f);
			_crownSpineRectTransform.anchoredPosition = target.RectTransform.anchoredPosition;
			yield return ScrollTo(target.RectTransform, 1.5f, Ease.InOutCubic);
			yield return new WaitForSecondsRealtime(0.1f);
			target.SetState(NodeStates.Attainable);
			target.OnSelect(null);
			target.Punch();
			yield return new WaitForSecondsRealtime(0.25f);
			Tween t2 = _crownSpineRectTransform.DOScale(Vector3.one * 0.75f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(t2.Duration());
			yield return new WaitForSecondsRealtime(0.1f);
			MapManager.Instance.AddNodeToPath(node);
			MapManager.Instance.EnterNode(node);
		}

		public IEnumerator NextSandboxLayer()
		{
			yield return null;
		}

		public AdventureMapNode NodeFromPoint(Point p)
		{
			return _adventureMapNodes.FirstOrDefault((AdventureMapNode n) => n.MapNode.point.Equals(p));
		}

		private void OnNodeSelected(AdventureMapNode node)
		{
			Shader.SetGlobalVector("SelectedObject", node.transform.position);
			Vector2 vector = Vector2.zero;
			if (node != _currentNode)
			{
				vector = (node.transform.position - _eyeSpineRectTransform.position).normalized * 25f;
			}
			_eyeSpineRectTransform.DOLocalMove(vector, 0.75f).SetUpdate(true).SetEase(Ease.OutQuart);
			foreach (Point point in node.MapNode.incoming)
			{
				NodeConnection nodeConnection = _nodeConnections.FirstOrDefault((NodeConnection conn) => conn.To.MapNode == node.MapNode && conn.From.MapNode.point.Equals(point) && conn.From.MapNode == _map.GetCurrentNode());
				if (nodeConnection != null)
				{
					nodeConnection.LineRenderer.Color = TryVisitColour;
					nodeConnection.LineRenderer.material = _scrollingMaterial;
				}
			}
			if (!InputManager.General.MouseInputActive)
			{
				if (_scrollCoroutine != null)
				{
					StopCoroutine(_scrollCoroutine);
				}
				_scrollCoroutine = StartCoroutine(ScrollTo(node.RectTransform, 0.5f));
			}
		}

		private void OnNodeDeselected(AdventureMapNode node)
		{
			foreach (Point point in node.MapNode.incoming)
			{
				NodeConnection nodeConnection = _nodeConnections.FirstOrDefault((NodeConnection conn) => conn.To.MapNode == node.MapNode && conn.From.MapNode.point.Equals(point) && conn.From.MapNode == _map.GetCurrentNode());
				if (nodeConnection != null)
				{
					nodeConnection.LineRenderer.Color = AvailableColour;
					nodeConnection.LineRenderer.material = _idleDottedMaterial;
				}
			}
		}

		public void OnNodeChosen(AdventureMapNode node)
		{
			AdventureMapNode adventureMapNode = node;
			adventureMapNode.OnNodeDeselected = (Action<AdventureMapNode>)Delegate.Remove(adventureMapNode.OnNodeDeselected, new Action<AdventureMapNode>(OnNodeDeselected));
			_canvasGroup.interactable = false;
			Action onNodeEntered = OnNodeEntered;
			if (onNodeEntered != null)
			{
				onNodeEntered();
			}
			SetActiveStateForMenu(false);
			foreach (AdventureMapNode adventureMapNode2 in _adventureMapNodes)
			{
				adventureMapNode2.Button.Interactable = false;
			}
			NodeConnection traversalConnection = MakeLineConnection(NodeFromPoint(_map.path.LastElement()), node);
			traversalConnection.LineRenderer.Color = TryVisitColour;
			traversalConnection.LineRenderer.material = null;
			traversalConnection.LineRenderer.Fill = 0f;
			if (!_currentNode.gameObject.activeSelf && _currentNode.MapNode.outgoing.Count <= 1)
			{
				foreach (Point point in node.MapNode.incoming)
				{
					NodeConnection nodeConnection = _nodeConnections.FirstOrDefault((NodeConnection conn) => conn.To.MapNode == node.MapNode && conn.From.MapNode.point.Equals(point) && conn.From.MapNode == _map.GetCurrentNode());
					if (nodeConnection != null)
					{
						nodeConnection.gameObject.SetActive(false);
					}
				}
				traversalConnection.gameObject.SetActive(false);
			}
			DOTween.To(() => traversalConnection.LineRenderer.Fill, delegate(float x)
			{
				traversalConnection.LineRenderer.Fill = x;
			}, 1f, 1f).SetEase(Ease.OutExpo).SetUpdate(true);
			_crownSpineRectTransform.transform.DOLocalMove(node.RectTransform.localPosition, 1f).SetUpdate(true).SetEase(Ease.OutExpo);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(0.4f).SetUpdate(true);
			sequence.Append(_crownSpineRectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true));
			sequence.Play().SetUpdate(true);
			sequence.onComplete = (TweenCallback)Delegate.Combine(sequence.onComplete, (TweenCallback)delegate
			{
				MapManager.Instance.EnterNode(node.MapNode);
			});
			AudioManager.Instance.PlayOneShot("event:/ui/map_location_pan");
			AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short");
			MMVibrate.Haptic(MMVibrate.HapticTypes.Selection);
			MapManager.Instance.AddNodeToPath(node.MapNode);
			node.ShowSwirlAnimation();
		}

		private AdventureMapNode MakeMapNode(Node mapNode)
		{
			AdventureMapNode adventureMapNode = MonoSingleton<UIManager>.Instance.AdventureMapNodeTemplate.Spawn(_nodeContent);
			adventureMapNode.Configure(mapNode);
			adventureMapNode.SetState(NodeStates.Locked);
			Vector2 vector = new Vector2(mapNode.point.x, mapNode.point.y) * 300f + UnityEngine.Random.insideUnitCircle * 50f;
			adventureMapNode.RectTransform.localPosition = vector;
			adventureMapNode.RectTransform.localScale = Vector3.one;
			adventureMapNode.OnNodeSelected = (Action<AdventureMapNode>)Delegate.Combine(adventureMapNode.OnNodeSelected, new Action<AdventureMapNode>(OnNodeSelected));
			adventureMapNode.OnNodeDeselected = (Action<AdventureMapNode>)Delegate.Combine(adventureMapNode.OnNodeDeselected, new Action<AdventureMapNode>(OnNodeDeselected));
			adventureMapNode.OnNodeChosen = (Action<AdventureMapNode>)Delegate.Combine(adventureMapNode.OnNodeChosen, new Action<AdventureMapNode>(OnNodeChosen));
			if (_FadeInNodesAndConnections)
			{
				adventureMapNode.CanvasGroup.alpha = 0f;
				adventureMapNode.CanvasGroup.DOFade(1f, 1f).SetUpdate(true);
			}
			else
			{
				adventureMapNode.CanvasGroup.alpha = 1f;
			}
			return adventureMapNode;
		}

		private NodeConnection MakeLineConnection(AdventureMapNode from, AdventureMapNode to)
		{
			if (from == null || to == null)
			{
				return null;
			}
			GameObject obj = new GameObject();
			RectTransform rectTransform = obj.AddComponent<RectTransform>();
			rectTransform.SetParent(_connectionContent);
			rectTransform.localPosition = Vector2.zero;
			rectTransform.localScale = Vector3.one;
			MMUILineRenderer mMUILineRenderer = obj.AddComponent<MMUILineRenderer>();
			mMUILineRenderer.Texture = _connectionTexture;
			mMUILineRenderer.Points = new List<MMUILineRenderer.BranchPoint>
			{
				new MMUILineRenderer.BranchPoint(from.RectTransform.localPosition),
				new MMUILineRenderer.BranchPoint(to.RectTransform.localPosition)
			};
			mMUILineRenderer.Width = 7.5f;
			NodeConnection nodeConnection = obj.AddComponent<NodeConnection>();
			nodeConnection.Configure(from, to, mMUILineRenderer, null, _idleDottedMaterial);
			_nodeConnections.Add(nodeConnection);
			if (_FadeInNodesAndConnections)
			{
				if (!set)
				{
					nodeConnection.LineRenderer.material.DOFade(0f, 0f).SetUpdate(true);
					set = true;
				}
				nodeConnection.LineRenderer.material.DOFade(1f, 1f).SetUpdate(true);
			}
			else
			{
				nodeConnection.LineRenderer.material.DOFade(1f, 0f).SetUpdate(true);
			}
			return nodeConnection;
		}
	}
}
