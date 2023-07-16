using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.Tarot;
using MMTools;
using src.Extensions;
using src.UI.Menus;
using Unify;
using UnityEngine;

public class Interaction_Knucklebones : Interaction
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass17_0
	{
		public Interaction_Knucklebones _003C_003E4__this;

		public bool betConfirmed;

		public bool betCancelled;

		internal void _003CMakeBet_003Eb__2(int bet)
		{
			_003C_003E4__this._betAmount = bet * 5;
			_003CMakeBet_003Eg__ConfirmBet_007C0();
		}

		internal void _003CMakeBet_003Eb__3()
		{
			if (!betConfirmed)
			{
				_003CMakeBet_003Eg__CancelBet_007C1();
			}
		}

		internal void _003CMakeBet_003Eg__ConfirmBet_007C0()
		{
			betConfirmed = true;
		}

		internal void _003CMakeBet_003Eg__CancelBet_007C1()
		{
			betCancelled = true;
		}
	}

	//[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass18_0
	{
		public Interaction_Knucklebones _003C_003E4__this;

		public bool opponentSelected;

		public bool opponentCancelled;

		internal void _003CSelectOpponent_003Eb__2(int selection)
		{
			Debug.Log(string.Format("Knucklebones Selection {0}", selection).Colour(Color.red));
			_003C_003E4__this.KnuckleboneOpponent = _003C_003E4__this._availableOpponents[selection];
			GameManager.GetInstance().OnConversationNext(_003C_003E4__this.KnuckleboneOpponent.Spine.gameObject, 10f);
			AudioManager.Instance.PlayOneShot(_003C_003E4__this.KnuckleboneOpponent.Config.SoundToPlay);
			AudioManager.Instance.PlayOneShot("event:/ui/arrow_change_selection", _003C_003E4__this.gameObject);
			_003C_003E4__this.KnuckleboneOpponent.Spine.AnimationState.SetAnimation(0, "selected", false);
			_003C_003E4__this.KnuckleboneOpponent.Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		}

		internal void _003CSelectOpponent_003Eb__3()
		{
			if (!opponentSelected)
			{
				_003CSelectOpponent_003Eg__CancelSelection_007C1();
			}
		}

		internal void _003CSelectOpponent_003Eg__ConfirmSelection_007C0()
		{
			opponentSelected = true;
		}

		internal void _003CSelectOpponent_003Eg__CancelSelection_007C1()
		{
			opponentCancelled = true;
		}
	}

	public List<KnucklebonesOpponent> KnucklebonePlayers = new List<KnucklebonesOpponent>();

	public KnucklebonesOpponent KnuckleboneOpponent;

	public GameObject Exit;

	public GameObject Blocker;

	public GoopFade goopTransition;

	private UIKnuckleBonesController _knuckleBonesInstance;

	private List<KnucklebonesOpponent> _availableOpponents = new List<KnucklebonesOpponent>();

	private int _betAmount;

	private Interaction_TarotCardUnlock t;

	public GameObject CareToBetConversation;

	public Interaction_KeyPiece KeyPiecePrefab;

	private void Start()
	{
		Interactable = GetAvailableOpponents().Count > 0;
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		if (!Interactable)
		{
			base.Label = "";
		}
		else
		{
			base.Label = ScriptLocalization.Interactions.PlayKnucklebones;
		}
	}

	public override void GetLabel()
	{
		if (!Interactable)
		{
			base.Label = "";
		}
		else
		{
			base.Label = ScriptLocalization.Interactions.PlayKnucklebones;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		_availableOpponents.Clear();
		_availableOpponents = GetAvailableOpponents();
	}

	private List<KnucklebonesOpponent> GetAvailableOpponents()
	{
		List<KnucklebonesOpponent> list = new List<KnucklebonesOpponent>();
		List<KnucklebonesOpponent> list2 = KnucklebonePlayers.ToList();
		if (DataManager.Instance.RatauKilled)
		{
			list2.Remove(list2.LastElement());
		}
		foreach (KnucklebonesOpponent item in list2)
		{
			if (item.Config.VariableToShow != DataManager.Variables.Knucklebones_Opponent_Ratau_Won)
			{
				Debug.Log("Variable Cast: " + item.Config.VariableToShow);
				if (!DataManager.Instance.GetVariable(item.Config.VariableToShow))
				{
					Debug.Log("NPC locked" + item.Spine.gameObject.name);
					item.Spine.gameObject.SetActive(false);
				}
				else
				{
					Debug.Log("NPC unlocked" + item.Spine.gameObject.name);
					item.Spine.gameObject.SetActive(true);
					list.Add(item);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		return list;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (_availableOpponents.Count > 0)
		{
			StartCoroutine(SelectOpponent());
		}
	}

	private void GameQuit()
	{
		Debug.Log("quitGame");
		AudioManager.Instance.SetMusicRoomID(1, SoundParams.Ratau);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
	}

	private void CompleteGame(UIKnuckleBonesController.KnucklebonesResult result)
	{
		Debug.Log("COMPLETE GAME!");
		GameQuit();
		switch (result)
		{
		case UIKnuckleBonesController.KnucklebonesResult.Win:
		{
			bool flag = false;
			if (!DataManager.Instance.GetVariable(KnuckleboneOpponent.Config.VariableToChangeOnWin))
			{
				Debug.Log("Show First Win Convo");
				base.enabled = false;
				KnuckleboneOpponent.FirstWinConvo.Callback.AddListener(delegate
				{
					base.enabled = true;
					StartCoroutine(GiveKeyPieceRoutine());
				});
				KnuckleboneOpponent.FirstWinConvo.gameObject.SetActive(true);
				Debug.Log("FIRST TIME DE");
				flag = true;
			}
			else
			{
				Debug.Log("Show Win Convo");
				if (_betAmount > 0)
				{
					StartCoroutine(GiveGold(true));
				}
			}
			DataManager.Instance.SetVariable(KnuckleboneOpponent.Config.VariableToChangeOnWin, true);
			if (flag)
			{
				ObjectiveManager.CompleteDefeatKnucklebones(KnuckleboneOpponent.Config.OpponentName);
			}
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("WIN_KNUCKLEBONES"));
			break;
		}
		case UIKnuckleBonesController.KnucklebonesResult.Loss:
			Debug.Log("Show Lose Convo");
			if (_betAmount > 0)
			{
				StartCoroutine(GiveGold(false));
			}
			KnuckleboneOpponent.LoseConvo.gameObject.SetActive(true);
			break;
		default:
			KnuckleboneOpponent.DrawConvo.gameObject.SetActive(true);
			break;
		}
		if (DataManager.Instance.GetVariable(DataManager.Variables.Knucklebones_Opponent_0_Won) && DataManager.Instance.GetVariable(DataManager.Variables.Knucklebones_Opponent_1_Won) && DataManager.Instance.GetVariable(DataManager.Variables.Knucklebones_Opponent_2_Won))
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("WIN_KNUCKLEBONES_ALL"));
		}
	}

	private IEnumerator MakeBet()
	{
		_003C_003Ec__DisplayClass17_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass17_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		CS_0024_003C_003E8__locals0.betConfirmed = false;
		CS_0024_003C_003E8__locals0.betCancelled = false;
		_betAmount = 0;
		UIKnucklebonesBettingSelectionController uIKnucklebonesBettingSelectionController = MonoSingleton<UIManager>.Instance.KnucklebonesBettingSelectionTemplate.Instantiate();
		uIKnucklebonesBettingSelectionController.Show(KnuckleboneOpponent.Config);
		uIKnucklebonesBettingSelectionController.OnConfirmBet = (Action<int>)Delegate.Combine(uIKnucklebonesBettingSelectionController.OnConfirmBet, (Action<int>)delegate(int bet)
		{
			CS_0024_003C_003E8__locals0._003C_003E4__this._betAmount = bet * 5;
			CS_0024_003C_003E8__locals0._003CMakeBet_003Eg__ConfirmBet_007C0();
		});
		uIKnucklebonesBettingSelectionController.OnHide = (Action)Delegate.Combine(uIKnucklebonesBettingSelectionController.OnHide, (Action)delegate
		{
			if (!CS_0024_003C_003E8__locals0.betConfirmed)
			{
				CS_0024_003C_003E8__locals0._003CMakeBet_003Eg__CancelBet_007C1();
			}
		});
		while (!CS_0024_003C_003E8__locals0.betConfirmed && !CS_0024_003C_003E8__locals0.betCancelled)
		{
			yield return null;
		}
		if (CS_0024_003C_003E8__locals0.betConfirmed)
		{
			yield return ContinueToKnucklebones();
		}
		else if (CS_0024_003C_003E8__locals0.betCancelled)
		{
			GameQuit();
		}
	}

	private IEnumerator SelectOpponent()
	{
		_003C_003Ec__DisplayClass18_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass18_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		CS_0024_003C_003E8__locals0.opponentSelected = false;
		CS_0024_003C_003E8__locals0.opponentCancelled = false;
		GameManager.GetInstance().OnConversationNew();
		List<KnucklebonesPlayerConfiguration> list = new List<KnucklebonesPlayerConfiguration>();
		foreach (KnucklebonesOpponent availableOpponent in _availableOpponents)
		{
			list.Add(availableOpponent.Config);
		}
		UIKnucklebonesOpponentSelectionController uIKnucklebonesOpponentSelectionController = MonoSingleton<UIManager>.Instance.KnucklebonesOpponentSelectionTemplate.Instantiate();
		uIKnucklebonesOpponentSelectionController.Show(list.ToArray());
		uIKnucklebonesOpponentSelectionController.OnConfirmOpponent = (Action)Delegate.Combine(uIKnucklebonesOpponentSelectionController.OnConfirmOpponent, new Action(CS_0024_003C_003E8__locals0._003CSelectOpponent_003Eg__ConfirmSelection_007C0));
		uIKnucklebonesOpponentSelectionController.OnOpponentSelectionChanged = (Action<int>)Delegate.Combine(uIKnucklebonesOpponentSelectionController.OnOpponentSelectionChanged, (Action<int>)delegate(int selection)
		{
			Debug.Log(string.Format("Knucklebones Selection {0}", selection).Colour(Color.red));
			CS_0024_003C_003E8__locals0._003C_003E4__this.KnuckleboneOpponent = CS_0024_003C_003E8__locals0._003C_003E4__this._availableOpponents[selection];
			GameManager.GetInstance().OnConversationNext(CS_0024_003C_003E8__locals0._003C_003E4__this.KnuckleboneOpponent.Spine.gameObject, 10f);
			AudioManager.Instance.PlayOneShot(CS_0024_003C_003E8__locals0._003C_003E4__this.KnuckleboneOpponent.Config.SoundToPlay);
			AudioManager.Instance.PlayOneShot("event:/ui/arrow_change_selection", CS_0024_003C_003E8__locals0._003C_003E4__this.gameObject);
			CS_0024_003C_003E8__locals0._003C_003E4__this.KnuckleboneOpponent.Spine.AnimationState.SetAnimation(0, "selected", false);
			CS_0024_003C_003E8__locals0._003C_003E4__this.KnuckleboneOpponent.Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		});
		uIKnucklebonesOpponentSelectionController.OnHide = (Action)Delegate.Combine(uIKnucklebonesOpponentSelectionController.OnHide, (Action)delegate
		{
			if (!CS_0024_003C_003E8__locals0.opponentSelected)
			{
				CS_0024_003C_003E8__locals0._003CSelectOpponent_003Eg__CancelSelection_007C1();
			}
		});
		while (!CS_0024_003C_003E8__locals0.opponentSelected && !CS_0024_003C_003E8__locals0.opponentCancelled)
		{
			yield return null;
		}
		if (CS_0024_003C_003E8__locals0.opponentCancelled)
		{
			GameQuit();
		}
		else if (DataManager.Instance.GetVariable(KnuckleboneOpponent.Config.VariableToChangeOnWin))
		{
			AudioManager.Instance.PlayOneShot("event:/ui/confirm_selection", base.gameObject);
			StartCoroutine(MakeBet());
		}
		else
		{
			yield return ContinueToKnucklebones();
		}
	}

	private IEnumerator ContinueToKnucklebones()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		goopTransition.FadeIn(1f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		yield return new WaitForSeconds(1f);
		_knuckleBonesInstance = MonoSingleton<UIManager>.Instance.KnucklebonesTemplate.Instantiate();
		SimulationManager.Pause();
		UIKnuckleBonesController knuckleBonesInstance = _knuckleBonesInstance;
		knuckleBonesInstance.OnHidden = (Action)Delegate.Combine(knuckleBonesInstance.OnHidden, new Action(SimulationManager.UnPause));
		_knuckleBonesInstance.Show(KnuckleboneOpponent, _betAmount);
		UIKnuckleBonesController knuckleBonesInstance2 = _knuckleBonesInstance;
		knuckleBonesInstance2.OnHidden = (Action)Delegate.Combine(knuckleBonesInstance2.OnHidden, (Action)delegate
		{
			goopTransition.FadeOut(1f);
			_knuckleBonesInstance = null;
		});
		UIKnuckleBonesController knuckleBonesInstance3 = _knuckleBonesInstance;
		knuckleBonesInstance3.OnGameCompleted = (Action<UIKnuckleBonesController.KnucklebonesResult>)Delegate.Combine(knuckleBonesInstance3.OnGameCompleted, new Action<UIKnuckleBonesController.KnucklebonesResult>(CompleteGame));
		UIKnuckleBonesController knuckleBonesInstance4 = _knuckleBonesInstance;
		knuckleBonesInstance4.OnGameQuit = (Action)Delegate.Combine(knuckleBonesInstance4.OnGameQuit, new Action(GameQuit));
		yield return null;
	}

	private IEnumerator GiveGold(bool Won)
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		PlayerFarming.Instance.state.facingAngle = 0f;
		PlayerFarming.Instance.state.LookAngle = 0f;
		if (Won)
		{
			for (int j = 0; j < _betAmount; j++)
			{
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
				ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, KnuckleboneOpponent.Spine.gameObject.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
				Inventory.AddItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1);
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.05f));
			}
		}
		else
		{
			for (int j = 0; j < _betAmount; j++)
			{
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
				ResourceCustomTarget.Create(KnuckleboneOpponent.Spine.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
				Inventory.ChangeItemQuantity(20, -1);
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.05f));
			}
		}
		yield return new WaitForSeconds(1f);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.25f, 0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.25f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForEndOfFrame();
		if (Won)
		{
			if (!KnuckleboneOpponent.WinConvo.Spoken)
			{
				base.enabled = false;
				KnuckleboneOpponent.WinConvo.Callback.AddListener(delegate
				{
					base.enabled = true;
				});
				KnuckleboneOpponent.WinConvo.gameObject.SetActive(true);
			}
		}
		else if (!KnuckleboneOpponent.LoseConvo.Spoken)
		{
			base.enabled = false;
			KnuckleboneOpponent.LoseConvo.Callback.AddListener(delegate
			{
				base.enabled = true;
			});
			KnuckleboneOpponent.LoseConvo.gameObject.SetActive(true);
		}
	}

	private IEnumerator BlockDoor()
	{
		while (t != null)
		{
			Exit.SetActive(false);
			Blocker.SetActive(true);
			yield return null;
		}
		Exit.SetActive(true);
		Blocker.SetActive(false);
	}

	private void TestReward(int Player = 3)
	{
		KnuckleboneOpponent = KnucklebonePlayers[Player];
		StartCoroutine(GiveKeyPieceRoutine());
	}

	public void GiveReward()
	{
	}

	private IEnumerator GiveKeyPieceRoutine()
	{
		yield return null;
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		Debug.Log("Card Reward = " + KnuckleboneOpponent.TarotCardReward);
		TarotCustomTarget tarotCustomTarget = TarotCustomTarget.Create(KnuckleboneOpponent.Spine.transform.position + Vector3.back * 0.5f, PlayerFarming.Instance.transform.position + Vector3.back * 0.5f, 1f, KnuckleboneOpponent.TarotCardReward, delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			if (KnuckleboneOpponent.Tag == KnucklebonesOpponent.OppnentTags.Ratau)
			{
				base.enabled = false;
				CareToBetConversation.SetActive(true);
			}
		});
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(tarotCustomTarget.gameObject, 6f);
	}
}
