using System;
using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class SaveInfoCard : UIInfoCardBase<MetaData>
	{
		[Header("Headers")]
		[SerializeField]
		private Image _difficultyIcon;

		[SerializeField]
		private TextMeshProUGUI _cultNameHeader;

		[SerializeField]
		private TextMeshProUGUI _quoteHeader;

		[Header("Stats")]
		[SerializeField]
		private GameObject _permadeathIcon;

		[SerializeField]
		private TextMeshProUGUI _playtime;

		[SerializeField]
		private TextMeshProUGUI _dayText;

		[SerializeField]
		private TextMeshProUGUI _followerCount;

		[SerializeField]
		private TextMeshProUGUI _structureCount;

		[SerializeField]
		private TextMeshProUGUI _deathCount;

		[SerializeField]
		private TextMeshProUGUI _percentageCompleted;

		[Header("Images")]
		[SerializeField]
		private Sprite _easySprite;

		[SerializeField]
		private Sprite _mediumSprite;

		[SerializeField]
		private Sprite _hardSprite;

		[SerializeField]
		private Sprite _extraHardSprite;

		[Header("Bishops")]
		[SerializeField]
		private GameObject _cross1;

		[SerializeField]
		private CanvasGroup _recruited1;

		[SerializeField]
		private GameObject _mystery1;

		[SerializeField]
		private GameObject _cross2;

		[SerializeField]
		private CanvasGroup _recruited2;

		[SerializeField]
		private GameObject _mystery2;

		[SerializeField]
		private GameObject _cross3;

		[SerializeField]
		private CanvasGroup _recruited3;

		[SerializeField]
		private GameObject _mystery3;

		[SerializeField]
		private GameObject _cross4;

		[SerializeField]
		private CanvasGroup _recruited4;

		[SerializeField]
		private GameObject _mystery4;

		[SerializeField]
		private GameObject _cross5;

		[SerializeField]
		private GameObject _deathCatKilled;

		[SerializeField]
		private GameObject _deathCatRecruited;

		[SerializeField]
		private GameObject _newGamePlusContainer;

		public override void Configure(MetaData metaData)
		{
			switch (metaData.Difficulty)
			{
			case 0:
				_difficultyIcon.sprite = _easySprite;
				break;
			case 1:
				_difficultyIcon.sprite = _mediumSprite;
				break;
			case 2:
				_difficultyIcon.sprite = _hardSprite;
				break;
			case 3:
				_difficultyIcon.sprite = _extraHardSprite;
				break;
			}
			if (string.IsNullOrEmpty(metaData.CultName))
			{
				_cultNameHeader.text = ScriptLocalization.NAMES_Place.Cult;
			}
			else
			{
				_cultNameHeader.text = metaData.CultName;
			}
			_percentageCompleted.text = "";
			TimeSpan timeSpan = new TimeSpan(0, 0, 0, (int)metaData.PlayTime);
			if (timeSpan.TotalHours < 1.0)
			{
				_playtime.text = string.Format("{0}m", timeSpan.Minutes);
			}
			else if (timeSpan.TotalDays < 1.0)
			{
				_playtime.text = string.Format("{0}h {1}m", timeSpan.Hours, timeSpan.Minutes);
			}
			else
			{
				_playtime.text = string.Format("{0}d {1}h {2}m", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
			}
			_dayText.text = string.Format(ScriptLocalization.UI.DayNumber, metaData.Day);
			_followerCount.text = string.Format("{0} {1}", "<sprite name=\"icon_Followers\">", metaData.FollowerCount);
			_structureCount.text = string.Format("{0} {1}", "<sprite name=\"icon_House\">", metaData.StructureCount);
			_deathCount.text = string.Format("{0} {1}", "<sprite name=\"icon_Dead\">", metaData.DeathCount);
			_cross1.SetActive(metaData.Dungeon1Completed);
			_cross2.SetActive(metaData.Dungeon2Completed);
			_cross3.SetActive(metaData.Dungeon3Completed);
			_cross4.SetActive(metaData.Dungeon4Completed);
			_cross5.SetActive(metaData.GameBeaten);
			if (metaData.Version != null)
			{
				string[] array = metaData.Version.Split('.');
				int result;
				if (array.Length >= 2 && int.TryParse(array[1], out result) && result >= 2)
				{
					bool num = metaData.Dungeon1NGPCompleted || metaData.Dungeon2NGPCompleted || metaData.Dungeon3NGPCompleted || metaData.Dungeon4NGPCompleted;
					_newGamePlusContainer.SetActive(metaData.GameBeaten);
					if (num)
					{
						_mystery1.SetActive(false);
						_mystery2.SetActive(false);
						_mystery3.SetActive(false);
						_mystery4.SetActive(false);
						_recruited1.alpha = (metaData.Dungeon1NGPCompleted ? 1f : 0.25f);
						_recruited2.alpha = (metaData.Dungeon2NGPCompleted ? 1f : 0.25f);
						_recruited3.alpha = (metaData.Dungeon3NGPCompleted ? 1f : 0.25f);
						_recruited4.alpha = (metaData.Dungeon4NGPCompleted ? 1f : 0.25f);
					}
					else
					{
						_recruited1.alpha = 0f;
						_recruited2.alpha = 0f;
						_recruited3.alpha = 0f;
						_recruited4.alpha = 0f;
					}
					_deathCatKilled.SetActive(metaData.GameBeaten && !metaData.DeathCatRecruited);
					_deathCatRecruited.SetActive(metaData.GameBeaten && metaData.DeathCatRecruited);
				}
			}
			else
			{
				_newGamePlusContainer.SetActive(false);
			}
			_permadeathIcon.SetActive(metaData.Permadeath);
		}
	}
}
