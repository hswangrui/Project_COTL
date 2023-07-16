using System.Collections;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class QuestItemObjective : MonoBehaviour
	{
		private const string kTickboxOnAnimation = "on";

		private const string kTickBoxOffAnimation = "off";

		private const string kTickBoxOnFailedAnimation = "on-failed";

		[SerializeField]
		private SkeletonGraphic _tickBox;

		[SerializeField]
		private TextMeshProUGUI _description;

		[SerializeField]
		private Image[] _strikethroughs;

		[Header("Time")]
		[SerializeField]
		private GameObject _timeContainer;

		[SerializeField]
		private Image _radialProgress;

		private float _fill;

		private int NumLines
		{
			get
			{
				return _description.textInfo.lineCount;
			}
		}

		public void Configure(ObjectivesData objectivesData)
		{
			_description.text = objectivesData.Text;
			if (objectivesData.IsComplete)
			{
				_fill = 1f;
				_description.color = StaticColors.GreyColor;
				_tickBox.SetAnimation("on");
			}
			else
			{
				_description.color = StaticColors.OffWhiteColor;
				_fill = 0f;
				_tickBox.SetAnimation("off");
			}
			_timeContainer.SetActive(objectivesData.HasExpiry);
			if (objectivesData.HasExpiry)
			{
				_radialProgress.fillAmount = objectivesData.ExpiryTimeNormalized;
			}
			StartCoroutine(DeferredStrikethroughUpdate());
		}

		public void Configure(ObjectivesDataFinalized objectivesData, bool failed = false)
		{
			_description.text = objectivesData.GetText();
			_fill = 1f;
			if (failed)
			{
				_tickBox.SetAnimation("on-failed");
			}
			else
			{
				_tickBox.SetAnimation("on");
			}
			_timeContainer.SetActive(false);
			StartCoroutine(DeferredStrikethroughUpdate());
		}

		private IEnumerator DeferredStrikethroughUpdate()
		{
			yield return null;
			float num = _description.rectTransform.rect.height / (float)(NumLines * 2);
			for (int i = 0; i < _strikethroughs.Length; i++)
			{
				_strikethroughs[i].gameObject.SetActive(i < NumLines);
				_strikethroughs[i].fillAmount = _fill;
				if (i < NumLines)
				{
					_strikethroughs[i].rectTransform.anchoredPosition = new Vector3(0f, _description.rectTransform.rect.height * 0.5f - num - num * 2f * (float)i);
				}
			}
		}
	}
}
