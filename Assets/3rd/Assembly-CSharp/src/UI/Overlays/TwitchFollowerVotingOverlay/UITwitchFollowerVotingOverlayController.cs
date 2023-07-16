using System;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace src.UI.Overlays.TwitchFollowerVotingOverlay
{
	//public class UITwitchFollowerVotingOverlayController : UIMenuBase
	//{
	//	private enum State
	//	{
	//		Loading,
	//		Voting,
	//		Result,
	//		Error
	//	}

	//	public Action<FollowerBrain> OnFollowerChosen;

	//	[SerializeField]
	//	private UINavigatorFollowElement buttonHighlightController;

	//	[SerializeField]
	//	private GameObject _loadingContainer;

	//	[SerializeField]
	//	private GameObject _activeVotingContainer;

	//	[SerializeField]
	//	private MMButton _cancelVoting;

	//	[SerializeField]
	//	private MMButton _endVoting;

	//	[SerializeField]
	//	private TMP_Text _totalVotesText;

	//	[SerializeField]
	//	private GameObject _resultContainer;

	//	[SerializeField]
	//	private MMButton _continueButton;

	//	[SerializeField]
	//	private SkeletonGraphic _chosenFollowerGraphic;

	//	[SerializeField]
	//	private ParticleSystem _confettiLeft;

	//	[SerializeField]
	//	private ParticleSystem _confettiRight;

	//	[SerializeField]
	//	private TMP_Text _votedForText;

	//	[SerializeField]
	//	private GameObject _errorContianer;

	//	[SerializeField]
	//	private MMButton _acceptErrorButton;

	//	private FollowerBrain _chosenFollower;

	//	public override void Awake()
	//	{
	//		base.Awake();
	//		_confettiLeft.Stop();
	//		_confettiRight.Stop();
	//		_confettiLeft.Clear();
	//		_confettiRight.Clear();
	//	}

	//	public void Show(List<FollowerInfo> followerInfos, TwitchVoting.VotingType votingType, bool instant)
	//	{
	//		Show(instant);
	//		SetState(State.Loading);
	//		TwitchVoting.StartVoting(votingType, followerInfos, delegate(bool result)
	//		{
	//			if (result)
	//			{
	//				SetState(State.Voting);
	//			}
	//			else
	//			{
	//				SetState(State.Error);
	//			}
	//		});
	//		TwitchVoting.OnVotingUpdated += delegate(int totalVotes)
	//		{
	//			_totalVotesText.text = string.Format(LocalizationManager.GetTranslation("UI/Twitch/Voting/Votes"), totalVotes.ToString());
	//		};
	//	}

	//	private void SetState(State state)
	//	{
	//		buttonHighlightController.gameObject.SetActive(state != State.Loading);
	//		_loadingContainer.SetActive(state == State.Loading);
	//		_activeVotingContainer.SetActive(state == State.Voting);
	//		_resultContainer.SetActive(state == State.Result);
	//		_totalVotesText.text = string.Format(LocalizationManager.GetTranslation("UI/Twitch/Voting/Votes"), 0);
	//		switch (state)
	//		{
	//		case State.Voting:
	//			OverrideDefault(_endVoting);
	//			break;
	//		case State.Result:
	//			_confettiLeft.Play();
	//			_confettiRight.Play();
	//			OverrideDefault(_continueButton);
	//			break;
	//		case State.Error:
	//			OverrideDefault(_acceptErrorButton);
	//			break;
	//		}
	//		if (state != 0)
	//		{
	//			ActivateNavigation();
	//		}
	//	}

	//	protected override void OnShowStarted()
	//	{
	//		base.OnShowStarted();
	//		_cancelVoting.onClick.AddListener(OnCancelVotingButtonClicked);
	//		_endVoting.onClick.AddListener(OnEndVotingButtonClicked);
	//		_continueButton.onClick.AddListener(OnContinueButtonClicked);
	//		_acceptErrorButton.onClick.AddListener(OnAcceptErrorButtonClicked);
	//	}

	//	private void OnCancelVotingButtonClicked()
	//	{
	//		TwitchVoting.Abort();
	//		Hide();
	//	}

	//	private void OnEndVotingButtonClicked()
	//	{
	//		SetState(State.Loading);
	//		_votedForText.text = string.Format(LocalizationManager.GetTranslation("UI/Twitch/Voting/Result"), "");
	//		TwitchVoting.EndVoting(delegate(FollowerBrain result)
	//		{
	//			if (result != null)
	//			{
	//				SetState(State.Result);
	//				_chosenFollower = result;
	//				_chosenFollowerGraphic.ConfigureFollower(_chosenFollower._directInfoAccess);
	//				_votedForText.text = string.Format(LocalizationManager.GetTranslation("UI/Twitch/Voting/Result"), "<color=#FFD201>" + _chosenFollower.Info.Name);
	//			}
	//			else
	//			{
	//				SetState(State.Error);
	//			}
	//		});
	//	}

	//	private void OnContinueButtonClicked()
	//	{
	//		Hide();
	//	}

	//	private void OnAcceptErrorButtonClicked()
	//	{
	//		Hide();
	//	}

	//	protected override void OnHideStarted()
	//	{
	//		if (_chosenFollower != null)
	//		{
	//			Action<FollowerBrain> onFollowerChosen = OnFollowerChosen;
	//			if (onFollowerChosen != null)
	//			{
	//				onFollowerChosen(_chosenFollower);
	//			}
	//		}
	//	}

	//	protected override void OnHideCompleted()
	//	{
	//		UnityEngine.Object.Destroy(base.gameObject);
	//	}
	//}
}
