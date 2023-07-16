using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIObjective : BaseMonoBehaviour
	{
		protected const string kTickBoxOnAnimation = "on";

		protected const string kTickBoxTurnOnAnimation = "turn-on";

		protected const string kTickBoxOffAnimation = "off";

		protected const string kTickBoxTurnOffFailedAnimation = "turn-on-failed";

		protected const string kTickBoxFailedAnimation = "on-failed";

		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private SkeletonGraphic _tickBox;

		[SerializeField]
		private Image[] _strikethroughs;

		[SerializeField]
		private GameObject _timeContainer;

		[SerializeField]
		private Image _timeWheel;

		private ObjectivesData _objectivesData;

		private Coroutine _deferredStrikethroughUpdate;

		private int NumLines
		{
			get
			{
				return _text.textInfo.lineCount;
			}
		}

		public ObjectivesData ObjectivesData
		{
			get
			{
				return _objectivesData;
			}
		}

		private void OnEnable()
		{
			LocalizationManager.OnLocalizeEvent += Localize;
			ObjectiveManager.OnObjectiveUpdated += OnObjectiveUpdated;
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnTextScaleChanged = (Action)Delegate.Combine(instance.OnTextScaleChanged, new Action(OnTextScaleChanged));
		}

		private void OnDisable()
		{
			LocalizationManager.OnLocalizeEvent -= Localize;
			ObjectiveManager.OnObjectiveUpdated -= OnObjectiveUpdated;
			AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
			instance.OnTextScaleChanged = (Action)Delegate.Remove(instance.OnTextScaleChanged, new Action(OnTextScaleChanged));
		}

		private void Localize()
		{
			_text.text = _objectivesData.Text;
			UpdateStrikethroughs();
		}

		private void OnTextScaleChanged()
		{
			UpdateStrikethroughs();
		}

		public void Configure(ObjectivesData objectivesData, bool ignoreStatus)
		{
			Image[] strikethroughs = _strikethroughs;
			for (int i = 0; i < strikethroughs.Length; i++)
			{
				strikethroughs[i].fillAmount = 0f;
			}
			_objectivesData = objectivesData;
			if (!ignoreStatus)
			{
				if (objectivesData.IsComplete)
				{
					_text.color = StaticColors.GreyColor;
					_tickBox.SetAnimation("on");
					strikethroughs = _strikethroughs;
					for (int i = 0; i < strikethroughs.Length; i++)
					{
						strikethroughs[i].fillAmount = 1f;
					}
				}
				else if (objectivesData.IsFailed)
				{
					_text.color = StaticColors.RedColor;
					_tickBox.SetAnimation("on-failed");
					strikethroughs = _strikethroughs;
					foreach (Image obj in strikethroughs)
					{
						obj.fillAmount = 1f;
						obj.color = StaticColors.RedColor;
					}
				}
				else
				{
					_text.color = StaticColors.OffWhiteColor;
					strikethroughs = _strikethroughs;
					for (int i = 0; i < strikethroughs.Length; i++)
					{
						strikethroughs[i].fillAmount = 0f;
					}
				}
			}
			_timeContainer.SetActive(_objectivesData.HasExpiry && !ObjectivesData.IsComplete && !objectivesData.IsFailed);
			Localize();
		}

		private void Update()
		{
			if (_timeContainer.activeSelf && _objectivesData != null)
			{
				_timeWheel.fillAmount = _objectivesData.ExpiryTimeNormalized;
			}
		}

		private void UpdateStrikethroughs()
		{
			if (_deferredStrikethroughUpdate != null)
			{
				StopCoroutine(_deferredStrikethroughUpdate);
			}
			_deferredStrikethroughUpdate = StartCoroutine(DeferredStrikethroughUpdate());
		}

		private IEnumerator DeferredStrikethroughUpdate()
		{
			yield return null;
			bool flag = _objectivesData.IsFailed || _objectivesData.IsComplete;
			float num = _text.rectTransform.rect.height / (float)(NumLines * 2);
			for (int i = 0; i < _strikethroughs.Length; i++)
			{
				_strikethroughs[i].gameObject.SetActive(i < NumLines);
				if (i < NumLines)
				{
					_strikethroughs[i].rectTransform.anchoredPosition = new Vector3(0f, _text.rectTransform.rect.height * 0.5f - num - num * 2f * (float)i);
					_strikethroughs[i].fillAmount = (flag ? 1 : 0);
				}
				else
				{
					_strikethroughs[i].fillAmount = 0f;
				}
			}
		}

		private void OnObjectiveUpdated(ObjectivesData objectivesData)
		{
			if (objectivesData == _objectivesData)
			{
				Localize();
			}
		}

		public void CompleteObjective(Action andThen = null)
		{
			StopAllCoroutines();
			StartCoroutine(DoCompleteObjective(andThen));
		}

		private IEnumerator DoCompleteObjective(Action andThen = null)
		{
			UIManager.PlayAudio("event:/ui/objective_complete");
			yield return _tickBox.YieldForAnimation("turn-on");
			yield return DoStrikethrough(StaticColors.OffWhiteColor);
			_text.color = StaticColors.GreyColor;
			if (andThen != null)
			{
				andThen();
			}
		}

		public void FailObjective(Action andThen = null)
		{
			StopAllCoroutines();
			StartCoroutine(DoFailedObjective(andThen));
		}

		private IEnumerator DoFailedObjective(Action andThen = null)
		{
			UIManager.PlayAudio("event:/ui/objective_failed");
			yield return _tickBox.YieldForAnimation("turn-on-failed");
			yield return DoStrikethrough(StaticColors.RedColor);
			_text.color = StaticColors.GreyColor;
			if (andThen != null)
			{
				andThen();
			}
		}

		private IEnumerator DoStrikethrough(Color color)
		{
			float t = 0.25f / (float)NumLines;
			for (int i = 0; i < NumLines; i++)
			{
				_strikethroughs[i].color = color;
				_strikethroughs[i].DOFillAmount(1f, t).SetEase((i != NumLines - 1) ? Ease.Linear : Ease.OutSine);
				yield return new WaitForSecondsRealtime(t);
			}
		}
	}
}
