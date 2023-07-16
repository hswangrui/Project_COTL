using System;
using Lamb.UI;
using src.Extensions;
using src.UI.Overlays.TwitchFollowerVotingOverlay;
using TMPro;
using UnityEngine;

public class TwitchInformationBox : FollowerSelectItem
{
	[SerializeField]
	private TMP_Text votingText;

	private ITwitchVotingProvider _votingProvider;

	public void Configure(ITwitchVotingProvider votingProvider)
	{
		_votingProvider = votingProvider;
		ConfigureImpl();
	}

	protected override void ConfigureImpl()
	{
		base.Button.onClick.AddListener(delegate
		{
			UIMenuBase uIMenuBase = UIMenuBase.ActiveMenus.LastElement();
			//UITwitchFollowerVotingOverlayController uITwitchFollowerVotingOverlayController = MonoSingleton<UIManager>.Instance.TwitchFollowerVotingOverlayController.Instantiate();
			//uITwitchFollowerVotingOverlayController.Show(_votingProvider.ProvideInfo(), _votingProvider.VotingType, false);
		//	uIMenuBase.PushInstance(uITwitchFollowerVotingOverlayController);
			//uITwitchFollowerVotingOverlayController.OnFollowerChosen = (Action<FollowerBrain>)Delegate.Combine(uITwitchFollowerVotingOverlayController.OnFollowerChosen, (Action<FollowerBrain>)delegate(FollowerBrain result)
			//{
			//	_votingProvider.FinalizeVote(result._directInfoAccess);
			//});
		});
	}
}
