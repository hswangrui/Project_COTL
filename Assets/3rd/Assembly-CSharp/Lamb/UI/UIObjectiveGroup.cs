using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIObjectiveGroup : BaseMonoBehaviour
	{
		private const float kUntrackedOnScreenDuration = 5f;

		private const float kShowHideDuration = 0.4f;

		public static Action<string> OnObjectiveGroupBeginHide;

		public static Action<string> OnObjectiveGroupHidden;

		public Action OnShown;

		public Action OnHidden;

		[Header("Objective Group")]
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _objectiveContent;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private TextMeshProUGUI _header;

		[SerializeField]
		private GameObject _trackingContainer;

		[Header("Templates")]
		[SerializeField]
		private UIObjective _objectiveTemplate;

		private Vector2 _onScreenPosition = new Vector2(220f, 0f);

		private Vector2 _offScreenPosition = new Vector2(800f, 0f);

		private string _uniqueGroupID;

		private string _groupID;

		private List<UIObjective> _objectiveItems = new List<UIObjective>();

		private List<ObjectivesData> _objectivesData = new List<ObjectivesData>();

		private List<Tweener> _tweeners = new List<Tweener>();

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public string UniqueGroupID
		{
			get
			{
				return _uniqueGroupID;
			}
		}

		public string GroupID
		{
			get
			{
				return _groupID;
			}
		}

		public bool AllCompleted
		{
			get
			{
				return ObjectiveManager.AllObjectivesComplete(_objectivesData);
			}
		}

		public bool Shown { get; private set; }

		public bool Showing { get; private set; }

		public void Configure(string uniqueGroupID, string groupID)
		{
			_uniqueGroupID = uniqueGroupID;
			_groupID = groupID;
			_canvasGroup.alpha = 0f;
			_onScreenPosition.y = (_offScreenPosition.y = _rectTransform.anchoredPosition.y);
			_rectTransform.anchoredPosition = _offScreenPosition;
			_trackingContainer.SetActive(false);
			Localize();
		}

		private void OnEnable()
		{
			LocalizationManager.OnLocalizeEvent += Localize;
		}

		private void OnDisable()
		{
			LocalizationManager.OnLocalizeEvent -= Localize;
		}

		private void Localize()
		{
			_header.text = LocalizationManager.GetTranslation(_groupID);
		}

		private void Update()
		{
			if (!ObjectiveManager.IsTracked(_uniqueGroupID) && InputManager.Gameplay.GetTrackQuestButtonDown())
			{
				AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive");
				MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
				ObjectiveManager.TrackGroup(_uniqueGroupID);
			}
		}

		public void Show(bool instant = false, Action andThen = null)
		{
			KillTweens();
			StopAllCoroutines();
			if (instant)
			{
				_rectTransform.anchoredPosition = _onScreenPosition;
				_canvasGroup.alpha = 1f;
				Action onShown = OnShown;
				if (onShown != null)
				{
					onShown();
				}
				Shown = true;
			}
			else
			{
				StartCoroutine(DoShow(andThen));
			}
		}

		private IEnumerator DoShow(Action andThen = null)
		{
			Showing = true;
			yield return null;
			_trackingContainer.SetActive(!ObjectiveManager.IsTracked(_uniqueGroupID) && !AllCompleted);
			_tweeners.Add(_rectTransform.DOAnchorPosX(_onScreenPosition.x, 0.4f).SetEase(Ease.OutBack));
			_tweeners.Add(_canvasGroup.DOFade(1f, 0.2f));
			yield return new WaitForSeconds(0.5f);
			if (andThen != null)
			{
				andThen();
			}
			Action onShown = OnShown;
			if (onShown != null)
			{
				onShown();
			}
			Shown = true;
			Showing = false;
			if (!ObjectiveManager.IsTracked(_uniqueGroupID))
			{
				yield return new WaitForSeconds(5f);
				if (!ObjectiveManager.IsTracked(_uniqueGroupID))
				{
					yield return DoHide();
				}
			}
		}

		public void Hide(bool instant = false, Action andThen = null)
		{
			StopAllCoroutines();
			if (Shown)
			{
				Action<string> onObjectiveGroupBeginHide = OnObjectiveGroupBeginHide;
				if (onObjectiveGroupBeginHide != null)
				{
					onObjectiveGroupBeginHide(_uniqueGroupID);
				}
			}
			Shown = false;
			if (instant)
			{
				_rectTransform.anchoredPosition = _offScreenPosition;
				_canvasGroup.alpha = 0f;
				Action onHidden = OnHidden;
				if (onHidden != null)
				{
					onHidden();
				}
				Action<string> onObjectiveGroupHidden = OnObjectiveGroupHidden;
				if (onObjectiveGroupHidden != null)
				{
					onObjectiveGroupHidden(_uniqueGroupID);
				}
			}
			else
			{
				StartCoroutine(DoHide());
			}
		}

		private IEnumerator DoHide(Action andThen = null)
		{
			KillTweens();
			_tweeners.Add(_rectTransform.DOAnchorPosX(_offScreenPosition.x, 0.4f).SetEase(Ease.InBack));
			_tweeners.Add(_canvasGroup.DOFade(0f, 0.2f).SetDelay(0.2f));
			yield return new WaitForSeconds(0.5f);
			if (andThen != null)
			{
				andThen();
			}
			Action onHidden = OnHidden;
			if (onHidden != null)
			{
				onHidden();
			}
			Action<string> onObjectiveGroupHidden = OnObjectiveGroupHidden;
			if (onObjectiveGroupHidden != null)
			{
				onObjectiveGroupHidden(_uniqueGroupID);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void HideSoft()
		{
			KillTweens();
			_tweeners.Add(_rectTransform.DOAnchorPosX(_offScreenPosition.x, 0.4f).SetEase(Ease.InBack));
			_tweeners.Add(_canvasGroup.DOFade(0f, 0.2f).SetDelay(0.2f));
		}

		public void UpdateTrackingState(bool trackingState)
		{
			_trackingContainer.SetActive(!trackingState);
			if (!Shown)
			{
				KillTweens();
				StopAllCoroutines();
				Show();
			}
		}

		public void AddObjective(ObjectivesData objectivesData, bool ignoreStatus = false)
		{
			if (!_objectivesData.Contains(objectivesData))
			{
				UIObjective uIObjective = GameObjectExtensions.Instantiate(_objectiveTemplate, _objectiveContent);
				uIObjective.Configure(objectivesData, ignoreStatus);
				_objectiveItems.Add(uIObjective);
				_objectivesData.Add(objectivesData);
				LayoutRebuilder.ForceRebuildLayoutImmediate(_objectiveContent);
			}
		}

		public void CompleteObjective(ObjectivesData objectivesData)
		{
			foreach (UIObjective objectiveItem in _objectiveItems)
			{
				if (objectivesData == objectiveItem.ObjectivesData)
				{
					objectiveItem.CompleteObjective(CheckCompletionStatus);
					break;
				}
			}
		}

		public void FailObjective(ObjectivesData objectivesData)
		{
			foreach (UIObjective objectiveItem in _objectiveItems)
			{
				if (objectivesData == objectiveItem.ObjectivesData)
				{
					objectiveItem.FailObjective(CheckCompletionStatus);
					break;
				}
			}
		}

		public bool HasObjective(ObjectivesData objectivesData)
		{
			if (_objectivesData.Contains(objectivesData))
			{
				return true;
			}
			foreach (ObjectivesData objectivesDatum in _objectivesData)
			{
				if (objectivesDatum.ID == objectivesData.ID)
				{
					return true;
				}
			}
			return false;
		}

		private void CheckCompletionStatus()
		{
			if (AllCompleted)
			{
				Hide();
			}
		}

		private void KillTweens()
		{
			foreach (Tweener tweener in _tweeners)
			{
				tweener.Kill();
			}
			_tweeners.Clear();
		}
	}
}
