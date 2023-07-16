using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using src.UINavigator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITwitchFollowerSelectOverlayController : UIMenuBase
{
	//public enum State
	//{
	//	Pre,
	//	Active,
	//	Waiting,
	//	Resulted,
	//	Loading,
	//	Error
	//}

	//[SerializeField]
	//private UINavigatorFollowElement buttonHighlightController;

	//[SerializeField]
	//private GameObject _loadingContainer;

	//[SerializeField]
	//private GameObject _errorContainer;

	//[SerializeField]
	//private MMButton _acceptErrorButton;

	//[SerializeField]
	//private GameObject preRaffleContainer;

	//[SerializeField]
	//private MMButton startRaffleButton;

	//[SerializeField]
	//private GameObject activeRaffleContainer;

	//[SerializeField]
	//private MMButton _cancelRaffleButton;

	//[SerializeField]
	//private MMButton endRaffleButton;

	//[SerializeField]
	//private TMP_Text currentParticipants;

	//[SerializeField]
	//private GameObject waitingRaffleContainer;

	//[SerializeField]
	//private MMButton _cancelButton;

	//[SerializeField]
	//private TMP_Text resultName;

	//[SerializeField]
	//private TMP_Text totalParticipants;

	//[SerializeField]
	//private Image progressBar;

	//[SerializeField]
	//private SkeletonGraphic _waitingFollower;

	//[SerializeField]
	//private GameObject resultedRaffleContainer;

	//[SerializeField]
	//private MMButton continueButton;

	//[SerializeField]
	//private TMP_Text winnerChannelName;

	//[SerializeField]
	//private SkeletonGraphic winnerFollower;

	//[SerializeField]
	//private ParticleSystem _confettiLeft;

	//[SerializeField]
	//private ParticleSystem _confettiRight;

	//private UIFollowerIndoctrinationMenuController _indoctrinationMenu;

	//private State _currentState;

	//private bool resulted;

	//public override void Awake()
	//{
	//	base.Awake();
	//	_confettiLeft.Stop();
	//	_confettiRight.Stop();
	//	_confettiLeft.Clear();
	//	_confettiRight.Clear();
	//}

	//private void OnEnable()
	//{
	//	TwitchFollowers.RaffleUpdated += RaffleUpdated;
	//	TwitchFollowers.FollowerCreated += FollowerCreated;
	//	TwitchFollowers.FollowerCreationProgress += FollowerCreationProgress;
	//}

	//private void OnDisable()
	//{
	//	TwitchFollowers.RaffleUpdated -= RaffleUpdated;
	//	TwitchFollowers.FollowerCreated -= FollowerCreated;
	//	TwitchFollowers.FollowerCreationProgress -= FollowerCreationProgress;
	//}

	//public void Show(UIFollowerIndoctrinationMenuController indoctrinationMenu, bool immediate = false)
	//{
	//	currentParticipants.text = LocalizationManager.GetTranslation("UI/Twitch/Raffle/Participants") + " 0";
	//	totalParticipants.text = LocalizationManager.GetTranslation("UI/Twitch/Raffle/TotalParticipants") + " 0";
	//	_indoctrinationMenu = indoctrinationMenu;
	//	Show(immediate);
	//}

	//private void SetState(State state)
	//{
	//	buttonHighlightController.gameObject.SetActive(state != State.Loading);
	//	_loadingContainer.SetActive(state == State.Loading);
	//	_errorContainer.SetActive(state == State.Error);
	//	preRaffleContainer.SetActive(state == State.Pre);
	//	activeRaffleContainer.SetActive(state == State.Active);
	//	waitingRaffleContainer.SetActive(state == State.Waiting);
	//	resultedRaffleContainer.SetActive(state == State.Resulted);
	//	switch (state)
	//	{
	//	case State.Pre:
	//		OverrideDefault(startRaffleButton);
	//		break;
	//	case State.Active:
	//		OverrideDefault(_cancelRaffleButton);
	//		endRaffleButton.interactable = false;
	//		break;
	//	case State.Waiting:
	//		_confettiLeft.Play();
	//		_confettiRight.Play();
	//		OverrideDefault(_cancelButton);
	//		break;
	//	case State.Resulted:
	//		_confettiLeft.Play();
	//		_confettiRight.Play();
	//		OverrideDefault(continueButton);
	//		break;
	//	case State.Error:
	//		OverrideDefault(_acceptErrorButton);
	//		break;
	//	}
	//	if (state != State.Loading)
	//	{
	//		ActivateNavigation();
	//	}
	//}

	//protected override void OnShowStarted()
	//{
	//	base.OnShowStarted();
	//	SetState(State.Loading);
	//	TwitchFollowers.GetFollowersAll(delegate(TwitchFollowers.ViewerFollowerData[] data)
	//	{
	//		bool flag = false;
	//		foreach (TwitchFollowers.ViewerFollowerData viewerFollowerData in data)
	//		{
	//			if (viewerFollowerData != null)
	//			{
	//				string item = viewerFollowerData.viewer_display_name + viewerFollowerData.created_at;
	//				if (viewerFollowerData.status == "CREATED" && !DataManager.Instance.TwitchFollowerViewerIDs.Contains(item) && !DataManager.Instance.TwitchFollowerIDs.Contains(viewerFollowerData.id))
	//				{
	//					FollowerCreated(viewerFollowerData);
	//					flag = true;
	//					break;
	//				}
	//			}
	//		}
	//		if (!flag && !resulted)
	//		{
	//			SetState(State.Pre);
	//		}
	//	});
	//	startRaffleButton.onClick.AddListener(OnStartRaffleButtonClicked);
	//	endRaffleButton.onClick.AddListener(OnEndRaffleButtonClicked);
	//	_cancelButton.onClick.AddListener(OnContinueButtonClicked);
	//	continueButton.onClick.AddListener(OnContinueButtonClicked);
	//	_acceptErrorButton.onClick.AddListener(OnAcceptErrorButtonClicked);
	//	_cancelRaffleButton.onClick.AddListener(OnContinueButtonClicked);
	//}

	//private void OnStartRaffleButtonClicked()
	//{
	//	startRaffleButton.Interactable = false;
	//	TwitchFollowers.StartRaffle(delegate(TwitchRequest.ResponseType response, TwitchFollowers.RaffleData data)
	//	{
	//		if (response == TwitchRequest.ResponseType.Failure)
	//		{
	//			TwitchFollowers.Abort();
	//			SetState(State.Error);
	//		}
	//	});
	//	SetState(State.Active);
	//}

	//private void OnEndRaffleButtonClicked()
	//{
	//	endRaffleButton.interactable = false;
	//	TwitchFollowers.EndRaffle(delegate(TwitchRequest.ResponseType response, TwitchFollowers.RaffleData data)
	//	{
	//		if (response == TwitchRequest.ResponseType.Failure)
	//		{
	//			TwitchFollowers.Abort();
	//			SetState(State.Error);
	//		}
	//	});
	//	SetState(State.Loading);
	//}

	//private void RaffleUpdated(TwitchRequest.ResponseType response, TwitchFollowers.RaffleData data)
	//{
	//	if (data.participants == 0 && MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable == endRaffleButton)
	//	{
	//		MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_cancelRaffleButton);
	//	}
	//	endRaffleButton.interactable = data.participants > 0;
	//	if (!string.IsNullOrEmpty(data.created_follower.viewer_display_name))
	//	{
	//		SetState(State.Waiting);
	//		SetWinner(data.created_follower.viewer_display_name);
	//	}
	//	currentParticipants.text = LocalizationManager.GetTranslation("UI/Twitch/Raffle/Participants") + " " + data.participants;
	//	totalParticipants.text = LocalizationManager.GetTranslation("UI/Twitch/Raffle/TotalParticipants") + " " + data.participants;
	//}

	//private void SetWinner(string winnerName)
	//{
	//	resultName.text = string.Format(LocalizationManager.GetTranslation("UI/Twitch/Raffle/Winner"), winnerName);
	//	winnerChannelName.text = string.Format(LocalizationManager.GetTranslation("UI/Twitch/Raffle/WinnerCreated"), winnerName);
	//	resulted = true;
	//}

	//private void OnContinueButtonClicked()
	//{
	//	Hide();
	//	TwitchFollowers.WaitingForCreationCancelled();
	//}

	//private void FollowerCreated(TwitchFollowers.ViewerFollowerData data)
	//{
	//	TwitchFollowers.WaitingForCreationCancelled();
	//	SetState(State.Resulted);
	//	if (data.customisations.skin_name == "HorseKing")
	//	{
	//		data.customisations.skin_name = "Horse1";
	//	}
	//	string text = data.customisations.skin_name;
	//	int skinVariation = 0;
	//	if (char.IsDigit(text[text.Length - 1]) && text[text.Length - 2] != ' ')
	//	{
	//		skinVariation = int.Parse(text[text.Length - 1].ToString()) - 1;
	//		text = text.Remove(text.Length - 1);
	//	}
	//	int colorOptionIndex = data.customisations.color.colorOptionIndex;
	//	FollowerInfoSnapshot followerInfoSnapshot = new FollowerInfoSnapshot
	//	{
	//		Name = data.viewer_display_name,
	//		SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(text),
	//		SkinVariation = skinVariation,
	//		SkinColour = colorOptionIndex,
	//		SkinName = text
	//	};
	//	winnerFollower.ConfigureFollowerSkin(followerInfoSnapshot);
	//	_indoctrinationMenu.CreatedTwitchFollower(followerInfoSnapshot, data.viewer_display_name + data.created_at, data.id, data.viewer_id);
	//	SetWinner(data.viewer_display_name);
	//}

	//private void FollowerCreationProgress(TwitchFollowers.ViewerFollowerData data)
	//{
	//	float endValue = 0f;
	//	if (data.customisation_step == "INTRO")
	//	{
	//		endValue = 0f;
	//	}
	//	else if (data.customisation_step == "SKIN_SELECTION")
	//	{
	//		endValue = 0.25f;
	//	}
	//	else if (data.customisation_step == "COLOR_SELECTION")
	//	{
	//		endValue = 0.5f;
	//	}
	//	else if (data.customisation_step == "VARIATION_SELECTION")
	//	{
	//		endValue = 0.75f;
	//	}
	//	progressBar.DOKill();
	//	progressBar.DOFillAmount(endValue, 0.25f).SetUpdate(true).SetEase(Ease.OutBack);
	//	if (data.customisations != null && !string.IsNullOrEmpty(data.customisations.skin_name))
	//	{
	//		if (data.customisations.skin_name == "HorseKing")
	//		{
	//			data.customisations.skin_name = "Horse1";
	//		}
	//		string text = data.customisations.skin_name;
	//		int skinVariation = 0;
	//		if (char.IsDigit(text[text.Length - 1]) && text[text.Length - 2] != ' ')
	//		{
	//			skinVariation = int.Parse(text[text.Length - 1].ToString()) - 1;
	//			text = text.Remove(text.Length - 1);
	//		}
	//		int skinColour = ((data.customisations.color != null) ? data.customisations.color.colorOptionIndex : 0);
	//		FollowerInfoSnapshot followerInfoSnapshot = new FollowerInfoSnapshot
	//		{
	//			Name = data.viewer_display_name,
	//			SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(text),
	//			SkinVariation = skinVariation,
	//			SkinColour = skinColour,
	//			SkinName = text
	//		};
	//		_waitingFollower.ConfigureFollowerSkin(followerInfoSnapshot);
	//	}
	//}

	//private void OnAcceptErrorButtonClicked()
	//{
	//	Hide();
	//}

	//protected override void OnHideCompleted()
	//{
	//	Object.Destroy(base.gameObject);
	//}

	//public override void OnCancelButtonInput()
	//{
	//	base.OnCancelButtonInput();
	//	if (_canvasGroup.interactable)
	//	{
	//		Hide();
	//		TwitchFollowers.WaitingForCreationCancelled();
	//	}
	//}
}
