using System;
using DG.Tweening;
using Map;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class AdventureMapNode : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, IPoolListener
	{
		private const float kHoverScaleFactor = 1.066f;

		public Action<AdventureMapNode> OnNodeSelected;

		public Action<AdventureMapNode> OnNodeDeselected;

		public Action<AdventureMapNode> OnNodeChosen;

		[Header("General Components")]
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private RectTransform _shakeContainer;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[Header("Main")]
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _selected;

		[SerializeField]
		private Image _whiteFill;

		[SerializeField]
		private Image _lockedFill;

		[SerializeField]
		private Image _imageOutline;

		[SerializeField]
		private Image _selectionIcon;

		[SerializeField]
		private GameObject _notification;

		[SerializeField]
		private CanvasGroup _startingIconCanvasGroup;

		[Header("Materials")]
		[SerializeField]
		private Material _normalOutline;

		[SerializeField]
		private Material _unselectedOutline;

		[SerializeField]
		private Material _selectedOutline;

		[SerializeField]
		private Material _completedOutline;

		[SerializeField]
		private Material _specialOutline;

		[Header("Flair")]
		[SerializeField]
		private Image _flairImage;

		[Header("Modifier")]
		[SerializeField]
		private Image _modifierIcon;

		[SerializeField]
		private CanvasGroup _modifierCanvasGroup;

		[Header("Sprites")]
		[SerializeField]
		private Sprite _questionMarkSprite;

		[SerializeField]
		private Sprite _potentialSelectionSprite;

		[SerializeField]
		private Sprite _selectedSelectionSprite;

		public Node _mapNode;

		private NodeStates _state;

		private float _initialScale;

		public NodeType NodeType;

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

		public Node MapNode
		{
			get
			{
				return _mapNode;
			}
		}

		public NodeBlueprint NodeBlueprint
		{
			get
			{
				return _mapNode.blueprint;
			}
		}

		public DungeonModifier Modifier
		{
			get
			{
				return _mapNode.Modifier;
			}
		}

		public NodeStates State
		{
			get
			{
				return _state;
			}
		}

		public CanvasGroup CanvasGroup
		{
			get
			{
				return _canvasGroup;
			}
		}

		private void Awake()
		{
			_button.onClick.AddListener(OnNodeClicked);
			MMButton button = _button;
			button.OnConfirmDenied = (Action)Delegate.Combine(button.OnConfirmDenied, new Action(Shake));
		}

		public void Configure(Node mapNode)
		{
			bool flag = DataManager.Instance.PlayerFleece == 11;
			_mapNode = mapNode;
			_modifierCanvasGroup.alpha = 0f;
			_selectionIcon.enabled = false;
			_startingIconCanvasGroup.alpha = 0f;
			_selected.enabled = false;
			NodeType = mapNode.nodeType;
			_icon.sprite = NodeBlueprint.GetSprite(_mapNode.DungeonLocation);
			if (_mapNode.nodeType == NodeType.Boss)
			{
				base.transform.localScale *= 1.5f;
			}
			_initialScale = _icon.transform.localScale.x;
			SetState(NodeStates.Locked);
			if (NodeBlueprint.flair != null && !flag)
			{
				_flairImage.enabled = true;
				_flairImage.sprite = NodeBlueprint.flair;
			}
			else
			{
				_flairImage.enabled = false;
			}
			if (NodeBlueprint.CanHaveModifier && !flag && (bool)Modifier && !_mapNode.Hidden)
			{
				_modifierCanvasGroup.alpha = 1f;
				_modifierIcon.sprite = Modifier.modifierIcon;
				_modifierCanvasGroup.gameObject.transform.localPosition = new Vector3(0f, 100f, 0f);
				_modifierCanvasGroup.gameObject.transform.DOLocalMove(Vector3.zero, 0.5f).SetUpdate(true).SetEase(Ease.InQuart);
			}
			if (NodeBlueprint.ForceDisplayModifier && !_mapNode.Hidden && !flag)
			{
				_modifierCanvasGroup.alpha = 1f;
				_modifierIcon.sprite = NodeBlueprint.ForceDisplayModifierIcon;
				_modifierCanvasGroup.gameObject.transform.localPosition = new Vector3(0f, 100f, 0f);
				_modifierCanvasGroup.gameObject.transform.DOLocalMove(Vector3.zero, 0.5f).SetUpdate(true).SetEase(Ease.InQuart);
			}
			if (_mapNode.nodeType == NodeType.None)
			{
				base.gameObject.SetActive(false);
			}
			if (_mapNode.Hidden || flag)
			{
				_icon.sprite = _questionMarkSprite;
			}
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (NodeBlueprint.nodeType == NodeType.Special_FindRelic)
				{
					if (objective.Type == Objectives.TYPES.FIND_RELIC && ((Objective_FindRelic)objective).TargetLocation == BiomeGenerator.Instance.DungeonLocation)
					{
						ShowNotification(true);
					}
				}
				else if ((NodeBlueprint.nodeType == NodeType.Follower_Easy || NodeBlueprint.nodeType == NodeType.Follower_Medium || NodeBlueprint.nodeType == NodeType.Follower_Hard) && objective.Type == Objectives.TYPES.FIND_FOLLOWER && ((Objectives_FindFollower)objective).TargetLocation == BiomeGenerator.Instance.DungeonLocation)
				{
					ShowNotification(true);
				}
			}
			if (NodeBlueprint.nodeType == NodeType.MarketPlaceCat)
			{
				bool hasReturnedBaal = DataManager.Instance.HasReturnedBaal;
				bool hasReturnedBaal2 = DataManager.Instance.HasReturnedBaal;
				bool flag2 = DataManager.Instance.Followers_Demons_Types.Contains(6);
				bool flag3 = DataManager.Instance.Followers_Demons_Types.Contains(7);
				if (((!hasReturnedBaal && flag2) || (!hasReturnedBaal2 && flag3)) && !DungeonSandboxManager.Active)
				{
					ShowNotification(true);
				}
			}
			_button.SetInteractionState(true);
		}

		public void SetStartingNode()
		{
			Debug.Log("Set Starting Node Sprite");
			_startingIconCanvasGroup.alpha = 1f;
		}

		public void SetState(NodeStates state, bool changeAppearance = true)
		{
			ScaleOutline(0.01f, Vector3.one, new Vector3(0.925f, 0.925f));
			_lockedFill.enabled = false;
			switch (state)
			{
			case NodeStates.Locked:
				if (changeAppearance)
				{
					if (NodeBlueprint.flair != null)
					{
						_imageOutline.material = _specialOutline;
					}
					else
					{
						_imageOutline.material = _normalOutline;
					}
					_imageOutline.color = new Color(1f, 1f, 1f, 0.75f);
					_icon.DOKill();
					_icon.color = UIAdventureMapOverlayController.LockedColourLight;
					_lockedFill.enabled = true;
				}
				break;
			case NodeStates.Visited:
				_imageOutline.material = _completedOutline;
				_imageOutline.color = Color.white;
				_icon.DOKill();
				_icon.color = UIAdventureMapOverlayController.VisitedColour;
				_icon.sprite = NodeBlueprint.GetSprite(MapNode.DungeonLocation);
				break;
			case NodeStates.Attainable:
				_imageOutline.material = _unselectedOutline;
				_imageOutline.color = Color.white;
				_selectionIcon.enabled = true;
				_icon.color = UIAdventureMapOverlayController.LockedColourLight;
				_icon.DOKill();
				_icon.DOColor(Color.white, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
				break;
			}
			_state = state;
			_button.Confirmable = _state == NodeStates.Attainable;
			if (state == NodeStates.Visited)
			{
				ShowNotification(false);
			}
		}

		public void ShowSwirlAnimation()
		{
			_whiteFill.color = Color.white;
			_whiteFill.DOFade(0f, 1f).SetUpdate(true);
			base.gameObject.transform.DOScale(Vector3.one * 1.05f, 1f).SetUpdate(true).SetEase(Ease.InOutBack);
		}

		public void ShowNotification(bool show)
		{
			_notification.SetActive(show);
		}

		private void OnNodeClicked()
		{
			Action<AdventureMapNode> onNodeChosen = OnNodeChosen;
			if (onNodeChosen != null)
			{
				onNodeChosen(this);
			}
			OnSelect(null);
		}

		public void OnSelect(BaseEventData eventData)
		{
			base.transform.DOScale(1.1f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
			_imageOutline.material = _selectedOutline;
			ScaleOutline(0.125f, new Vector3(0.925f, 0.925f), Vector3.one);
			if (_state == NodeStates.Attainable)
			{
				_selectionIcon.enabled = true;
				_selectionIcon.sprite = _selectedSelectionSprite;
				SelectionIconFade(1.5f);
			}
			_selected.enabled = true;
			_icon.transform.DOKill();
			_icon.transform.DOScale(_initialScale * 1.066f, 0.3f).SetUpdate(true);
			Action<AdventureMapNode> onNodeSelected = OnNodeSelected;
			if (onNodeSelected != null)
			{
				onNodeSelected(this);
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			base.transform.DOScale(0.9f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
			ScaleOutline(0.01f, Vector3.one, new Vector3(0.925f, 0.925f));
			if (_state == NodeStates.Locked)
			{
				if (NodeBlueprint.flair != null)
				{
					_imageOutline.material = _specialOutline;
				}
				else
				{
					_imageOutline.material = _normalOutline;
				}
			}
			else if (_state == NodeStates.Visited)
			{
				_imageOutline.material = _completedOutline;
			}
			else
			{
				_imageOutline.material = _unselectedOutline;
			}
			if (_state == NodeStates.Attainable)
			{
				_selectionIcon.enabled = true;
				_selectionIcon.sprite = _potentialSelectionSprite;
				SelectionIconFade(1f);
			}
			_selected.enabled = false;
			_icon.transform.DOKill();
			_icon.transform.DOScale(_initialScale, 0.3f).SetUpdate(true).SetEase(Ease.OutQuart);
			Action<AdventureMapNode> onNodeDeselected = OnNodeDeselected;
			if (onNodeDeselected != null)
			{
				onNodeDeselected(this);
			}
		}

		private void SelectionIconFade(float scale)
		{
			Transform target = _selectionIcon.transform;
			target.DOKill();
			target.DOScale(new Vector3(scale, scale), 0.3f).SetUpdate(true).SetEase(Ease.OutQuart);
			_selectionIcon.DOKill();
			_selectionIcon.color = new Color(1f, 1f, 1f, 0f);
			_selectionIcon.DOFade(1f, 0.3f).SetUpdate(true).SetEase(Ease.OutQuart);
		}

		public void Shake()
		{
			_shakeContainer.DOKill();
			_shakeContainer.anchoredPosition = Vector2.zero;
			_shakeContainer.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		public void Punch()
		{
			_whiteFill.color = Color.white;
			_whiteFill.DOFade(0f, 1f).SetUpdate(true);
			_rectTransform.DOPunchScale(Vector3.one * 1.25f, 0.5f, 1).SetUpdate(true);
		}

		public void ScaleIn()
		{
			_whiteFill.color = Color.white;
			_whiteFill.DOFade(0f, 1f).SetUpdate(true);
			_rectTransform.localScale = Vector3.one * 2f;
			_rectTransform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack).SetUpdate(true);
		}

		private void ScaleOutline(float duration, Vector3 from, Vector3 to)
		{
			Transform obj = _imageOutline.gameObject.transform;
			obj.DOKill();
			obj.localScale = from;
			obj.DOScale(to, duration).SetUpdate(true).SetEase(Ease.InQuart);
		}

		public void OnRecycled()
		{
			Navigation navigation = _button.navigation;
			navigation.selectOnLeft = null;
			navigation.selectOnRight = null;
			navigation.mode = Navigation.Mode.Automatic;
			_button.navigation = navigation;
			OnNodeSelected = null;
			OnNodeDeselected = null;
			OnNodeChosen = null;
		}
	}
}
