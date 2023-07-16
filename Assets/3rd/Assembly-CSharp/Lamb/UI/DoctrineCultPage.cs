using System;
using I2.Loc;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class DoctrineCultPage : UISubmenuBase
	{
		[SerializeField]
		private TextMeshProUGUI _cultName;

		[SerializeField]
		private MMButton _renameCultButton;

		[SerializeField]
		private MMButton _viewFollowersButton;

		[SerializeField]
		private TMP_Text _quoteText;

		[SerializeField]
		private LayoutElement _quoteLayoutElement;

		[Header("Statistics")]
		[SerializeField]
		private TMP_Text _totalFollowersCount;

		[SerializeField]
		private TMP_Text _murderCount;

		[SerializeField]
		private TMP_Text _starvationCount;

		[SerializeField]
		private TMP_Text _sacrificesCount;

		[SerializeField]
		private TMP_Text _naturalDeaths;

		[SerializeField]
		private TMP_Text _crusadesCount;

		[SerializeField]
		private TMP_Text _deathsCount;

		[SerializeField]
		private TMP_Text _killsCount;

		public override void Awake()
		{
			base.Awake();
			_renameCultButton.gameObject.SetActive(DataManager.Instance.OnboardedCultName);
			_cultName.text = DataManager.Instance.CultName;
			_renameCultButton.onClick.AddListener(OnRenameCultClicked);
			_viewFollowersButton.onClick.AddListener(OnViewFollowersClicked);
			_totalFollowersCount.text = (DataManager.Instance.Followers.Count + DataManager.Instance.Followers_Dead.Count).ToString();
			_murderCount.text = DataManager.Instance.STATS_Murders.ToString();
			_starvationCount.text = DataManager.Instance.STATS_FollowersStarvedToDeath.ToString();
			_sacrificesCount.text = DataManager.Instance.STATS_Sacrifices.ToString();
			_naturalDeaths.text = DataManager.Instance.STATS_NaturalDeaths.ToString();
			_crusadesCount.text = DataManager.Instance.dungeonRun.ToString();
			_deathsCount.text = DataManager.Instance.playerDeaths.ToString();
			_killsCount.text = DataManager.Instance.KillsInGame.ToString();
			if (SettingsManager.Settings.Accessibility.DyslexicFont)
			{
				_quoteLayoutElement.preferredWidth = 550f;
			}
			else
			{
				_quoteLayoutElement.preferredWidth = 350f;
			}
			if (DataManager.Instance.BeatenPostGame)
			{
				_quoteText.text = ScriptLocalization.QUOTE.QuoteBoss5;
			}
			else
			{
				_quoteText.text = ScriptLocalization.QUOTE.IntroQuote;
			}
		}

		private void OnRenameCultClicked()
		{
			UICultNameMenuController uICultNameMenuController = MonoSingleton<UIManager>.Instance.CultNameMenuTemplate.Instantiate();
			uICultNameMenuController.Show(DataManager.Instance.CultName, true, false);
			uICultNameMenuController.OnNameConfirmed = (Action<string>)Delegate.Combine(uICultNameMenuController.OnNameConfirmed, (Action<string>)delegate(string result)
			{
				_cultName.text = result;
				DataManager.Instance.CultName = result;
			});
			PushInstance(uICultNameMenuController);
		}

		private void OnViewFollowersClicked()
		{
			UIFollowerSelectMenuController uIFollowerSelectMenuController = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
			uIFollowerSelectMenuController.AllowsVoting = false;
			uIFollowerSelectMenuController.Show(DataManager.Instance.Followers, null, false, UpgradeSystem.Type.Count, false, true, false);
			PushInstance(uIFollowerSelectMenuController);
		}
	}
}
