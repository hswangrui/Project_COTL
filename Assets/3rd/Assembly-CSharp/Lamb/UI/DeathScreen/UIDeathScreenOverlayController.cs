using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using I2.Loc;
using Map;
using MMTools;
using src.Extensions;
using src.UINavigator;
using TMPro;
using Unify;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.DeathScreen
{
	public class UIDeathScreenOverlayController : UIMenuBase
	{
		public enum Results
		{
			Killed,
			Completed,
			Escaped,
			None,
			BeatenMiniBoss,
			BeatenBoss,
			GameOver,
			BeatenBossNoDamage
		}

		public static Results Result;

		public static UIDeathScreenOverlayController Instance;

		[Header("Selectables")]
		[SerializeField]
		private MMButton _continueButton;

		[SerializeField]
		private MMButton _restartButton;

		[SerializeField]
		private GameObject _buttonHighlight;

		[Header("Text")]
		[SerializeField]
		private TextMeshProUGUI _title;

		[SerializeField]
		private TextMeshProUGUI _subTitle;

		[SerializeField]
		private TextMeshProUGUI _timeText;

		[SerializeField]
		private TextMeshProUGUI _killsText;

		[Header("Containers")]
		[SerializeField]
		private RectTransform _root;

		[SerializeField]
		private RectTransform _titleContainer;

		[SerializeField]
		private RectTransform _levelNodeContainer;

		[SerializeField]
		private RectTransform _itemsContainer;

		[Header("Prefabs")]
		[SerializeField]
		private GameObject _levelNodePrefab;

		[SerializeField]
		private DeathScreenInventoryItem _itemTemplate;

		[Header("Canvas Groups")]
		[SerializeField]
		private CanvasGroup _runtimeCanvasGroup;

		[SerializeField]
		private RectTransform _runtimeRectTransform;

		[SerializeField]
		private CanvasGroup _killsCanvasGroup;

		[SerializeField]
		private RectTransform _killsRectTransform;

		[SerializeField]
		private CanvasGroup _continueCanvasGroup;

		[SerializeField]
		private CanvasGroup _restartCanvasGroup;

		[SerializeField]
		private CanvasGroup _backgroundGroup;

		[SerializeField]
		private CanvasGroup _particleGroup;

		[SerializeField]
		private CanvasGroup _buttonbackgroundGroup;

		[SerializeField]
		private CanvasGroup _controllerPrompts;

		[SerializeField]
		private CanvasGroup _titleGroup;

		[SerializeField]
		private CanvasGroup _itemCanvasGroup;

		[SerializeField]
		private Image _dividerIcon;

		[Header("Effects")]
		[SerializeField]
		private UIParticle _uiParticle;

		[SerializeField]
		private Material _diedMaterial;

		[SerializeField]
		private Material _completedMaterial;

		[Header("Sandbox")]
		[SerializeField]
		private RectTransform _sandBoxContainer;

		[SerializeField]
		private RectTransform _godTear;

		[SerializeField]
		private DeathScreenInventoryItem _godTearTotal;

		[SerializeField]
		private RectTransform _instantBar;

		[SerializeField]
		private RectTransform _xpBar;

		[SerializeField]
		private CanvasGroup _xpCanvasGroup;

		[SerializeField]
		private TextMeshProUGUI _xpText;

		[Header("Other")]
		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		private List<InventoryItem.ITEM_TYPE> _blacklistedItems = new List<InventoryItem.ITEM_TYPE>
		{
			InventoryItem.ITEM_TYPE.SEEDS,
			InventoryItem.ITEM_TYPE.INGREDIENTS,
			InventoryItem.ITEM_TYPE.MEALS,
			InventoryItem.ITEM_TYPE.GIFT_MEDIUM,
			InventoryItem.ITEM_TYPE.GIFT_SMALL,
			InventoryItem.ITEM_TYPE.MONSTER_HEART
		};

		private static List<InventoryItem.ITEM_TYPE> _excludeLootFromBonus = new List<InventoryItem.ITEM_TYPE>
		{
			InventoryItem.ITEM_TYPE.Necklace_1,
			InventoryItem.ITEM_TYPE.Necklace_2,
			InventoryItem.ITEM_TYPE.Necklace_3,
			InventoryItem.ITEM_TYPE.Necklace_4,
			InventoryItem.ITEM_TYPE.Necklace_5,
			InventoryItem.ITEM_TYPE.Necklace_Dark,
			InventoryItem.ITEM_TYPE.Necklace_Demonic,
			InventoryItem.ITEM_TYPE.Necklace_Loyalty,
			InventoryItem.ITEM_TYPE.Necklace_Light,
			InventoryItem.ITEM_TYPE.Necklace_Missionary,
			InventoryItem.ITEM_TYPE.Necklace_Gold_Skull,
			InventoryItem.ITEM_TYPE.GIFT_MEDIUM,
			InventoryItem.ITEM_TYPE.GIFT_SMALL,
			InventoryItem.ITEM_TYPE.MONSTER_HEART,
			InventoryItem.ITEM_TYPE.BEHOLDER_EYE,
			InventoryItem.ITEM_TYPE.SHELL,
			InventoryItem.ITEM_TYPE.GOD_TEAR
		};

		private Dictionary<InventoryItem.ITEM_TYPE, int> _lootDelta;

		private Results _result;

		private int _levels;

		private List<DeathScreenInventoryItem> inventoryItems = new List<DeathScreenInventoryItem>();

		public static bool LastRunWasSandBox = false;

		private int ResetSandBoxCount;

		private Vector3 ChallengeCoinsStartPosition;

		private Vector3 FullGodTearPosition;

		private Vector3 TotalGodTearPosition;

		private DeathScreenInventoryItem ChallengeCoins;

		private DeathScreenInventoryItem FullGodTear;

		private int FullGodTearCount;

		private bool beatBoss;

		private bool HasGodTears;

		private bool DisplayinPenalty;

		public static List<InventoryItem.ITEM_TYPE> ExcludeLootFromBonus
		{
			get
			{
				return _excludeLootFromBonus;
			}
		}

		public void Show(Results result, bool instant = false)
		{
			Show(result, (!(MapManager.Instance != null)) ? 1 : ((!MapManager.Instance.DungeonConfig) ? 1 : MapManager.Instance.DungeonConfig.layers.Count), instant);
		}

		public void Show(Results result, int levels, bool instant = false)
		{
			Instance = this;
			if (result == Results.Killed && DataManager.Instance.PermadeDeathActive)
			{
				result = Results.GameOver;
			}
			_result = result;
			_levels = levels;
			MMButton continueButton = _continueButton;
			bool interactable = (_continueButton.enabled = false);
			continueButton.interactable = interactable;
			_buttonHighlight.SetActive(false);
			_xpCanvasGroup.alpha = 0f;
			_titleGroup.alpha = 0f;
			_subTitle.alpha = 0f;
			_particleGroup.alpha = 0f;
			_backgroundGroup.alpha = 0f;
			_runtimeCanvasGroup.alpha = 0f;
			_killsCanvasGroup.alpha = 0f;
			_continueCanvasGroup.alpha = 0f;
			_restartCanvasGroup.alpha = 0f;
			_buttonbackgroundGroup.alpha = 0f;
			_controllerPrompts.alpha = 0f;
			_itemCanvasGroup.alpha = 0f;
			_dividerIcon.enabled = false;
			Time.timeScale = 0f;
			Result = result;
			if ((bool)_controlPrompts)
			{
				_controlPrompts.HideAcceptButton();
			}
			Show(instant);
		}

		private void InitSandboxXPBar()
		{
			float x = (float)DataManager.Instance.CurrentChallengeModeXP / (float)DataManager.Instance.CurrentChallengeModeTargetXP;
			_instantBar.localScale = new Vector3(x, 1f);
			_xpBar.localScale = new Vector3(x, 1f);
			_xpText.text = DataManager.Instance.CurrentChallengeModeXP + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT) + "/" + DataManager.Instance.CurrentChallengeModeTargetXP;
		}

		private void ResetSandboxXPBar()
		{
			RectTransform xpBar = _xpBar;
			Vector3 localScale = (_instantBar.localScale = new Vector3(0f, 1f));
			xpBar.localScale = localScale;
			ResetSandBoxCount++;
			Color color = _xpText.color;
			Color color2 = _xpText.color;
			color.a = 0f;
			_xpText.color = color;
			_xpText.DOColor(color2, 0.5f).SetUpdate(true);
			_xpText.text = "0" + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT) + "/" + DataManager.Instance.CurrentChallengeModeTargetXP;
		}

		private void MoveChallengeGold()
		{
			StartCoroutine(MoveChallengeGoldRoutine());
		}

		private IEnumerator MoveChallengeGoldRoutine()
		{
			yield return new WaitForSecondsRealtime(3f);
			while (ChallengeCoins == null && Inventory.itemsDungeon.Count > 0)
			{
				yield return null;
			}
			yield return new WaitForSecondsRealtime(1f);
			if ((bool)ChallengeCoins)
			{
				ChallengeCoinsStartPosition = ChallengeCoins.transform.position;
				FullGodTearPosition = FullGodTear.transform.position;
				ChallengeCoins.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.25f);
				UIManager.PlayAudio("event:/player/float_follower");
				yield return new WaitForSecondsRealtime(0.5f);
				ChallengeCoins.transform.DOMove(_sandBoxContainer.transform.position, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				ChallengeCoins.transform.DOScale(1f, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
				UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
				MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
				ChallengeCoins.gameObject.SetActive(false);
				ChallengeCoins.transform.position = ChallengeCoinsStartPosition;
				ChallengeCoins.transform.localScale = Vector3.one;
				TotalGodTearPosition = _godTearTotal.transform.position;
				_godTearTotal.gameObject.SetActive(false);
			}
			UpdateSandboxXPBar();
		}

		private void UpdateSandboxXPBar(float Delay = 0f)
		{
			StartCoroutine(UpdateSandboxXpBarRoutine(Delay));
		}

		private IEnumerator UpdateSandboxXpBarRoutine(float Delay)
		{
			int itemQuantity = Inventory.GetItemQuantity(128);
			Inventory.SetItemQuantity(128, 0);
			DataManager.Instance.CurrentChallengeModeXP += itemQuantity;
			float TargetScale = Mathf.Min((float)DataManager.Instance.CurrentChallengeModeXP / (float)DataManager.Instance.CurrentChallengeModeTargetXP, 1f);
			_instantBar.localScale = new Vector3(TargetScale, 1f);
			yield return new WaitForSecondsRealtime(Delay);
			float duration = 0.5f;
			StartCoroutine(TweenText(duration));
			_xpBar.DOScale(new Vector3(Mathf.Min(TargetScale, 1f), 1f), duration).SetUpdate(true).OnComplete(delegate
			{
				StartCoroutine(CheckXP());
			});
		}

		private IEnumerator TweenText(float Duration)
		{
			float Time = 0f;
			float StartingValue = 0f;
			while (true)
			{
				float num;
				Time = (num = Time + UnityEngine.Time.unscaledDeltaTime);
				if (!(num < Duration))
				{
					break;
				}
				float f = Mathf.Lerp(StartingValue, Mathf.Min(DataManager.Instance.CurrentChallengeModeXP, DataManager.Instance.CurrentChallengeModeTargetXP), Time / Duration);
				_xpText.text = Mathf.RoundToInt(f) + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT) + "/" + DataManager.Instance.CurrentChallengeModeTargetXP;
				if (UnityEngine.Random.value < 0.2f)
				{
					UIManager.PlayAudio("event:/followers/pop_in");
				}
				yield return null;
			}
			_xpText.text = Mathf.Min(DataManager.Instance.CurrentChallengeModeXP, DataManager.Instance.CurrentChallengeModeTargetXP) + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.GOD_TEAR_FRAGMENT) + "/" + DataManager.Instance.CurrentChallengeModeTargetXP;
		}

		private IEnumerator CheckXP()
		{
			if (DataManager.Instance.CurrentChallengeModeXP >= DataManager.Instance.CurrentChallengeModeTargetXP)
			{
				_godTear.transform.localScale = Vector3.one * 1.2f;
				_godTear.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
				UIManager.PlayAudio("event:/player/float_follower");
				yield return new WaitForSecondsRealtime(0.5f);
				Vector3 GodTearStartPosition = _godTear.position;
				_godTear.DOMove(FullGodTear.transform.position, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
				FullGodTear.gameObject.SetActive(true);
				FullGodTear.transform.localScale = Vector3.one * 1.5f;
				FullGodTear.transform.DOScale(1f, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				_godTear.gameObject.SetActive(false);
				FullGodTear.Configure(InventoryItem.ITEM_TYPE.GOD_TEAR);
				Inventory.AddItem(InventoryItem.ITEM_TYPE.GOD_TEAR, 1);
				FullGodTearCount++;
				FullGodTear.AmountText.text = string.Concat(FullGodTearCount);
				UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
				MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
				yield return new WaitForSecondsRealtime(0.25f);
				_godTear.transform.position = GodTearStartPosition;
				_godTear.transform.localScale = Vector3.zero;
				_godTear.gameObject.SetActive(true);
				_godTear.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
				DataManager.Instance.CurrentChallengeModeXP -= DataManager.Instance.CurrentChallengeModeTargetXP;
				DataManager.Instance.CurrentChallengeModeLevel++;
				bool flag = DataManager.Instance.CurrentChallengeModeXP > 0;
				for (int num = inventoryItems.Count - 1; num >= 0; num--)
				{
					if (inventoryItems[num].Type == InventoryItem.ITEM_TYPE.GOD_TEAR)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					yield return new WaitForSecondsRealtime(0.5f);
					ResetSandboxXPBar();
					UpdateSandboxXPBar(0.5f);
					yield break;
				}
				ShowContinueButton();
				MMButton continueButton = _continueButton;
				MMButton continueButton2 = _continueButton;
				bool interactable = true;
				continueButton2.enabled = true;
				continueButton.interactable = interactable;
			}
			else
			{
				_godTearTotal.gameObject.SetActive(true);
				_godTearTotal.transform.position = TotalGodTearPosition + new Vector3(0f, -200f, 0f);
				_godTearTotal.transform.DOMove(TotalGodTearPosition, 0.75f).SetEase(Ease.InElastic).SetUpdate(true);
				_godTearTotal.transform.localScale = Vector3.zero;
				_godTearTotal.transform.DOScale(1.7f, 0.75f).SetEase(Ease.InBack).SetUpdate(true);
				_godTearTotal.Configure(InventoryItem.ITEM_TYPE.GOD_TEAR);
				_godTearTotal.AmountText.text = string.Concat(_godTearTotal.Item.quantity - FullGodTearCount);
				UIManager.PlayAudio("event:/ui/level_node_die");
				yield return new WaitForSecondsRealtime(1.5f);
				UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
				FullGodTear.transform.DOMove(TotalGodTearPosition, 0.75f).SetEase(Ease.InBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.75f);
				UIManager.PlayAudio("event:/ui/objective_complete");
				_godTearTotal.transform.localScale = Vector3.one * 3f;
				_godTearTotal.transform.DOScale(1.7f, 0.25f).SetEase(Ease.InBack).SetUpdate(true);
				_godTearTotal.AmountText.text = string.Concat(_godTearTotal.Item.quantity);
				FullGodTear.gameObject.SetActive(false);
				ShowContinueButton();
				MMButton continueButton3 = _continueButton;
				MMButton continueButton4 = _continueButton;
				bool interactable = true;
				continueButton4.enabled = true;
				continueButton3.interactable = interactable;
			}
		}

		protected override void OnShowStarted()
		{
		//	TwitchHelpHinder.EndHHEvent(null);
			SimulationManager.UnPause();
			DataManager.ResetRunData();
			_continueButton.onClick.AddListener(delegate
			{
				Hide();
			});
			switch (_result)
			{
			case Results.Completed:
				_title.text = ScriptLocalization.UI_DeathScreen_Completed.Title;
				_subTitle.text = ScriptLocalization.UI_DeathScreen_Completed.Subtitle;
				beatBoss = true;
				_uiParticle.material = _completedMaterial;
				break;
			case Results.Killed:
				_title.text = ScriptLocalization.UI_DeathScreen_Killed.Title;
				_subTitle.text = ScriptLocalization.UI_DeathScreen_Killed.Subtitle;
				_uiParticle.material = _diedMaterial;
				break;
			case Results.Escaped:
				_title.text = ScriptLocalization.UI_DeathScreen_Escaped.Title;
				_subTitle.text = ScriptLocalization.UI_DeathScreen_Escaped.Subtitle;
				_uiParticle.material = _diedMaterial;
				break;
			case Results.GameOver:
				_title.text = ScriptLocalization.UI_DeathScreen_GameOver.Title;
				_subTitle.text = ScriptLocalization.UI_DeathScreen_GameOver.Subtitle;
				_uiParticle.material = _diedMaterial;
				break;
			}
			if (Result == Results.Killed)
			{
				DataManager.Instance.LastRunResults = Results.Killed;
				DataManager.Instance.DiedLastRun = true;
			}
			else
			{
				DataManager.Instance.DiedLastRun = false;
			}
			if (_result != Results.Escaped)
			{
				if (!DungeonSandboxManager.Active)
				{
					for (int i = 0; i < DataManager.Instance.Followers_Demons_IDs.Count; i++)
					{
						FollowerInfo infoByID = FollowerInfo.GetInfoByID(DataManager.Instance.Followers_Demons_IDs[i]);
						if (infoByID != null)
						{
							FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
							if (orCreateBrain != null)
							{
								orCreateBrain.AddThought((_result == Results.Completed) ? Thought.DemonSuccessfulRun : Thought.DemonFailedRun);
							}
						}
					}
				}
			}
			else if (_result == Results.Completed)
			{
				DataManager.Instance.playerDeathsInARow = 0;
				DataManager.Instance.playerDeathsInARowFightingLeader = 0;
			}
			if (!DungeonSandboxManager.Active)
			{
				DataManager.Instance.Followers_Demons_IDs.Clear();
				DataManager.Instance.Followers_Demons_Types.Clear();
			}
			if (_result == Results.GameOver)
			{
				SimulationManager.Pause();
				FollowerManager.Reset();
				StructureManager.Reset();
				DeviceLightingManager.Reset();
				SaveAndLoad.DeleteSaveSlot(SaveAndLoad.SAVE_SLOT);
			//	TwitchManager.Abort();
				UIDynamicNotificationCenter.Reset();
			}
		}

		protected override IEnumerator DoHide()
		{
			if (PlayerFarming.Instance != null)
			{
				PlayerFarming.Instance.playerController.enabled = false;
			}
			_continueButton.interactable = false;
			Inventory.ClearDungeonItems();
			Debug.Log("typE: " + DeathCatRoomManager.GetConversationType());
			if (CheatConsole.IN_DEMO && PlayerFarming.Location == FollowerLocation.Dungeon1_1)
			{
				MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "DemoOver", 1f, "", delegate
				{
					Hide(true);
				});
			}
			else if (_result == Results.GameOver)
			{
				SimulationManager.Pause();
				DeviceLightingManager.Reset();
				FollowerManager.Reset();
				StructureManager.Reset();
				SaveAndLoad.DeleteSaveSlot(SaveAndLoad.SAVE_SLOT);
				//TwitchManager.Abort();
				UIDynamicNotificationCenter.Reset();
				MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", delegate
				{
					Hide(true);
				});
			}
			else if (!DataManager.Instance.ForeshadowedMysticShop && !DataManager.Instance.OnboardedMysticShop && DataManager.Instance.GetDungeonLayer(FollowerLocation.Dungeon1_4) > 2)
			{
				if (RespawnRoomManager.Instance != null)
				{
					RespawnRoomManager.Instance.gameObject.SetActive(false);
				}
				MysticShopKeeperManager.Play();
				MysticShopKeeperManager.Instance.transform.parent = null;
				Sequence sequence = DOTween.Sequence();
				sequence.AppendInterval(0.5f).SetUpdate(true);
				sequence.Play().SetUpdate(true);
			}
			else if (DeathCatRoomManager.GetConversationType() != 0 && DeathCatController.Instance == null)
			{
				if (RespawnRoomManager.Instance != null)
				{
					RespawnRoomManager.Instance.gameObject.SetActive(false);
				}
				DeathCatRoomManager.Play();
				Sequence sequence2 = DOTween.Sequence();
				sequence2.AppendInterval(0.5f).SetUpdate(true);
				sequence2.Play().SetUpdate(true);
			}
			else if (GameManager.Layer2 && (DataManager.Instance.LastRunResults == Results.BeatenBoss || DataManager.Instance.LastRunResults == Results.BeatenBossNoDamage))
			{
				DataManager.Instance.LastRunResults = Results.None;
				QuoteScreenController.Init(new List<QuoteScreenController.QuoteTypes> { GetQuoteType() }, delegate
				{
					GameManager.ToShip();
				}, delegate
				{
					GameManager.ToShip();
				});
				MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "QuoteScreen", 5f, "", delegate
				{
					Time.timeScale = 1f;
				});
			}
			else
			{
				DataManager.Instance.LastRunResults = Results.None;
				GameManager.ToShip();
			}
			yield return new WaitForSecondsRealtime(1f);
			Action onHide = OnHide;
			if (onHide != null)
			{
				onHide();
			}
			SetActiveStateForMenu(false);
			base.gameObject.SetActive(false);
			Action onHidden = OnHidden;
			if (onHidden != null)
			{
				onHidden();
			}
			OnHideCompleted();
		}

		protected override void OnShowCompleted()
		{
			if (!LastRunWasSandBox || !HasGodTears)
			{
				MMButton continueButton = _continueButton;
				bool interactable = (_continueButton.enabled = true);
				continueButton.interactable = interactable;
			}
			_buttonHighlight.SetActive(true);
			ActivateNavigation();
			_controlPrompts.ShowAcceptButton();
		}

		protected override IEnumerator DoShowAnimation()
		{
			Debug.Log("Do show animation");
			if (PlayerFarming.Instance != null)
			{
				HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
				component.HP = component.totalHP;
			}
			_levelNodeContainer.DestroyAllChildren();
			_itemsContainer.DestroyAllChildren();
			DataManager.Instance.FollowersRecruitedInNodes.Add(DataManager.Instance.FollowersRecruitedThisNode);
			DataManager.Instance.FollowersRecruitedThisNode = 0;
			yield return null;
			switch (_result)
			{
			case Results.Completed:
				UIManager.PlayAudio("event:/ui/heretics_defeated");
				break;
			case Results.Killed:
			case Results.GameOver:
				UIManager.PlayAudio("event:/ui/martyred");
				break;
			case Results.Escaped:
				UIManager.PlayAudio("event:/ui/martyred");
				break;
			}
			if (Inventory.itemsDungeon.Count == 0)
			{
				_itemsContainer.gameObject.SetActive(false);
			}
			Vector2 titleOrigin = _title.rectTransform.localPosition;
			LastRunWasSandBox = DungeonSandboxManager.Active;
			HasGodTears = Inventory.GetDungeonItemByType(128) != null;
			_sandBoxContainer.gameObject.SetActive(LastRunWasSandBox && HasGodTears);
			if (LastRunWasSandBox)
			{
				if (DungeonSandboxManager.Active && _result != 0)
				{
					DungeonSandboxManager.ProgressionSnapshot progressionForScenario = DungeonSandboxManager.GetProgressionForScenario(DungeonSandboxManager.CurrentScenario.ScenarioType, (PlayerFleeceManager.FleeceType)DataManager.Instance.PlayerFleece);
					progressionForScenario.CompletedRuns++;
					if (progressionForScenario.CompletedRuns >= DungeonSandboxManager.GetDataForScenarioType(DungeonSandboxManager.CurrentScenario.ScenarioType).Count)
					{
						AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("COMPLETE_CHALLENGE_ROW"));
						Debug.Log("ACHIEVEMENT GOT : COMPLETE_CHALLENGE_ROW");
					}
				}
				InitSandboxXPBar();
				if (HasGodTears)
				{
					MoveChallengeGold();
				}
			}
			_title.rectTransform.SetParent(_root);
			_title.rectTransform.localPosition = Vector3.zero;
			_title.rectTransform.localScale = Vector3.one * 4f;
			_title.rectTransform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			_titleGroup.DOFade(1f, 0.5f).SetUpdate(true);
			DOTween.To(() => _backgroundGroup.alpha, delegate(float x)
			{
				_backgroundGroup.alpha = x;
			}, 1f, 2f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(1f);
			_particleGroup.DOFade(1f, 2f).SetUpdate(true).SetEase(Ease.InQuart);
			if (HUD_Manager.Instance != null)
			{
				HUD_Manager.Instance.ShowBW(0.5f, 1f, 0f);
			}
			_title.rectTransform.SetParent(_titleContainer, true);
			_title.rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			_title.rectTransform.DOLocalMove(titleOrigin, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
			_subTitle.DOFade(1f, 0.5f).SetEase(Ease.OutSine).SetUpdate(true);
			_titleGroup.DOFade(1f, 0.5f).SetUpdate(true);
			_title.rectTransform.DOShakeScale(0.5f, 0.2f).SetUpdate(true);
			inventoryItems = new List<DeathScreenInventoryItem>();
			if (Inventory.itemsDungeon.Count > 0)
			{
				foreach (InventoryItem item in Inventory.itemsDungeon)
				{
					if (_blacklistedItems.Contains((InventoryItem.ITEM_TYPE)item.type))
					{
						continue;
					}
					DeathScreenInventoryItem deathScreenInventoryItem = GameObjectExtensions.Instantiate(_itemTemplate, _itemsContainer);
					deathScreenInventoryItem.Configure(item);
					deathScreenInventoryItem.CanvasGroup.alpha = 0f;
					inventoryItems.Add(deathScreenInventoryItem);
					if (!LastRunWasSandBox)
					{
						continue;
					}
					if (item.type == 128)
					{
						ChallengeCoins = deathScreenInventoryItem;
						if (FullGodTear == null)
						{
							FullGodTear = ChallengeCoins;
							FullGodTearCount = 0;
						}
					}
					if (item.type == 119)
					{
						FullGodTear = deathScreenInventoryItem;
						FullGodTearCount = FullGodTear.Item.quantity;
					}
				}
				_itemCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
				if (LastRunWasSandBox)
				{
					_xpCanvasGroup.DOFade(1f, 1f).SetUpdate(true);
				}
				foreach (DeathScreenInventoryItem inventoryItem in inventoryItems)
				{
					inventoryItem.CanvasGroup.DOFade(1f, 2f).SetUpdate(true);
					yield return new WaitForSecondsRealtime(0.1f);
				}
			}
			if (_result == Results.Killed || _result == Results.Escaped)
			{
				int levels = _levels;
				int num = -1;
				while (++num < levels)
				{
					GameObject obj = UnityEngine.Object.Instantiate(_levelNodePrefab, _levelNodeContainer);
					obj.SetActive(true);
					UIDeathScreenLevelNode component2 = obj.GetComponent<UIDeathScreenLevelNode>();
					UIDeathScreenLevelNode.LevelNodeSkins levelNodeSkin = ((num == levels - 1) ? UIDeathScreenLevelNode.LevelNodeSkins.boss : UIDeathScreenLevelNode.LevelNodeSkins.normal);
					if (num < DataManager.Instance.dungeonVisitedRooms.Count - 1 && (bool)MapManager.Instance.DungeonConfig)
					{
						NodeBlueprint blueprint = MapManager.GetBlueprint(DataManager.Instance.dungeonVisitedRooms[num], MapManager.Instance.DungeonConfig);
						Debug.Log(string.Concat("config:", MapManager.Instance.DungeonConfig, " type: ", DataManager.Instance.dungeonVisitedRooms[num].ToString(), " blueprint: ", blueprint));
						if (blueprint != null)
						{
							component2.icon.sprite = blueprint.GetSprite(DataManager.Instance.dungeonLocationsVisited[num], false);
							component2.icon.gameObject.SetActive(true);
						}
						switch (DataManager.Instance.dungeonVisitedRooms[num])
						{
						case NodeType.FirstFloor:
						case NodeType.DungeonFloor:
							levelNodeSkin = UIDeathScreenLevelNode.LevelNodeSkins.normal;
							break;
						case NodeType.MiniBossFloor:
							levelNodeSkin = UIDeathScreenLevelNode.LevelNodeSkins.boss;
							break;
						default:
							levelNodeSkin = UIDeathScreenLevelNode.LevelNodeSkins.other;
							break;
						}
						component2.Play((float)num * 0.5f, UIDeathScreenLevelNode.ResultTypes.Completed, levelNodeSkin, num, num == levels - 1);
					}
					else if (num == DataManager.Instance.dungeonVisitedRooms.Count - 1)
					{
						component2.Play((float)num * 0.5f, (_result == Results.Killed || _result == Results.Escaped) ? UIDeathScreenLevelNode.ResultTypes.Killed : UIDeathScreenLevelNode.ResultTypes.Completed, levelNodeSkin, num, num == levels - 1);
						component2.icon.gameObject.SetActive(_result != Results.Killed);
						StartCoroutine(ShowPenaltyRoutine(inventoryItems, (float)num * 0.5f));
					}
					else
					{
						component2.Play((float)num * 0.5f, UIDeathScreenLevelNode.ResultTypes.Unreached, levelNodeSkin, num, num == levels - 1);
						component2.icon.gameObject.SetActive(false);
					}
				}
			}
			else if (_result != Results.GameOver)
			{
				int num2 = -1;
				while (++num2 < DataManager.Instance.dungeonVisitedRooms.Count)
				{
					NodeBlueprint blueprint2 = MapManager.GetBlueprint(DataManager.Instance.dungeonVisitedRooms[num2], MapManager.Instance.DungeonConfig);
					UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
					MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
					GameObject obj2 = UnityEngine.Object.Instantiate(_levelNodePrefab, _levelNodeContainer);
					obj2.SetActive(true);
					UIDeathScreenLevelNode component3 = obj2.GetComponent<UIDeathScreenLevelNode>();
					Debug.Log("type: " + DataManager.Instance.dungeonVisitedRooms[num2].ToString() + " blueprint: " + blueprint2);
					if (blueprint2 != null)
					{
						component3.icon.sprite = blueprint2.GetSprite(DataManager.Instance.dungeonLocationsVisited[num2], false);
						component3.icon.gameObject.SetActive(true);
					}
					UIDeathScreenLevelNode.LevelNodeSkins levelNodeSkin2;
					switch (DataManager.Instance.dungeonVisitedRooms[num2])
					{
					case NodeType.FirstFloor:
					case NodeType.DungeonFloor:
						levelNodeSkin2 = UIDeathScreenLevelNode.LevelNodeSkins.normal;
						break;
					case NodeType.MiniBossFloor:
						levelNodeSkin2 = UIDeathScreenLevelNode.LevelNodeSkins.boss;
						break;
					default:
						levelNodeSkin2 = UIDeathScreenLevelNode.LevelNodeSkins.other;
						break;
					}
					UIDeathScreenLevelNode.ResultTypes resultType = UIDeathScreenLevelNode.ResultTypes.Completed;
					if (blueprint2 != null && blueprint2.nodeType == NodeType.MiniBossFloor && DataManager.Instance.DungeonBossFight && !DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location))
					{
						resultType = UIDeathScreenLevelNode.ResultTypes.Unreached;
					}
					component3.Play((float)num2 * 0.5f, resultType, levelNodeSkin2, num2, num2 == DataManager.Instance.dungeonVisitedRooms.Count - 1);
					if (num2 == DataManager.Instance.dungeonVisitedRooms.Count - 1)
					{
						StartCoroutine(ShowPenaltyRoutine(inventoryItems, (float)num2 * 0.5f + 0.5f));
					}
				}
			}
			while (DisplayinPenalty)
			{
				yield return null;
			}
			_dividerIcon.color = new Color(1f, 1f, 1f, 0f);
			_dividerIcon.DOFade(1f, 1f).SetUpdate(true);
			DataManager.Instance.PrayedAtCrownShrine = false;
			float num3 = Time.time - DataManager.Instance.dungeonRunDuration;
			if (_result == Results.GameOver)
			{
				num3 = DataManager.Instance.TimeInGame;
			}
			int num4 = Mathf.FloorToInt(num3 / 60f);
			int num5 = 0;
			while (num4 > 60)
			{
				num5++;
				num4 -= 60;
			}
			int num6 = Mathf.FloorToInt(num3 % 60f);
			string text = "00";
			if (num5 > 0 && num5 < 10)
			{
				text = "0" + num5;
			}
			if (num5 >= 10)
			{
				text = num5.ToString();
			}
			_timeText.text = text + ":" + ((num4 < 10) ? "0" : "") + num4 + ":" + ((num6 < 10) ? "0" : "") + num6;
			Vector3 localPosition = _runtimeRectTransform.localPosition;
			_runtimeRectTransform.localPosition += Vector3.up * 100f;
			_runtimeRectTransform.DOLocalMove(localPosition, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			DOTween.To(() => _runtimeCanvasGroup.alpha, delegate(float x)
			{
				_runtimeCanvasGroup.alpha = x;
			}, 1f, 1f).SetUpdate(true);
			_killsText.text = DataManager.Instance.PlayerKillsOnRun.ToString();
			if (_result == Results.GameOver)
			{
				float num7 = (float)DataManager.Instance.KillsInGame;
			}
			Vector3 localPosition2 = _killsRectTransform.localPosition;
			_killsRectTransform.localPosition += Vector3.up * 100f;
			_killsRectTransform.DOLocalMove(localPosition2, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			DOTween.To(() => _killsCanvasGroup.alpha, delegate(float x)
			{
				_killsCanvasGroup.alpha = x;
			}, 1f, 1f).SetUpdate(true);
			int quantity = Inventory.GetDungeonItemByTypeReturnObject(1).quantity;
			int quantity2 = Inventory.GetDungeonItemByTypeReturnObject(20).quantity;
			int quantity3 = Inventory.GetDungeonItemByTypeReturnObject(21).quantity;
			int quantity4 = Inventory.GetDungeonItemByTypeReturnObject(50).quantity;
			int quantity5 = Inventory.GetDungeonItemByTypeReturnObject(97).quantity;
			int quantity6 = Inventory.GetDungeonItemByTypeReturnObject(102).quantity;
			int quantity7 = Inventory.GetDungeonItemByTypeReturnObject(6).quantity;
			int foodCollected = quantity3 + quantity4 + quantity5 + quantity6 + quantity7;
			if (_result != Results.GameOver && (bool)MonoSingleton<PlayerProgress_Analytics>.Instance)
			{
				MonoSingleton<PlayerProgress_Analytics>.Instance.LevelComplete(GameManager.CurrentDungeonLayer, DataManager.Instance.GetDungeonLayer(PlayerFarming.Location), quantity, quantity2, foodCollected, DataManager.Instance.PlayerKillsOnRun, num4, (int)DataManager.Instance.PlayerDamageReceivedThisRun, beatBoss);
			}
			Inventory.ClearDungeonItems();
			yield return new WaitForSecondsRealtime(0.5f);
			if (!LastRunWasSandBox || !HasGodTears)
			{
				ShowContinueButton();
			}
			yield return new WaitForSecondsRealtime(0.75f);
		}

		private void ShowContinueButton()
		{
			Debug.Log("ShowContinueButton()");
			StartCoroutine(ShowContinueButtonRoutine());
		}

		private IEnumerator ShowContinueButtonRoutine()
		{
			Debug.Log("ShowContinueButtonRoutine()");
			Vector3 localPosition = _continueCanvasGroup.transform.localPosition;
			_continueCanvasGroup.transform.localPosition += Vector3.up * 100f;
			_continueCanvasGroup.transform.DOLocalMove(localPosition, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			DOTween.To(() => _continueCanvasGroup.alpha, delegate(float x)
			{
				_continueCanvasGroup.alpha = x;
			}, 1f, 1f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			Vector3 localPosition2 = _restartCanvasGroup.transform.localPosition;
			_restartCanvasGroup.transform.localPosition += Vector3.up * 100f;
			_restartCanvasGroup.transform.DOLocalMove(localPosition2, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			DOTween.To(() => _restartCanvasGroup.alpha, delegate(float x)
			{
				_restartCanvasGroup.alpha = x;
			}, 1f, 1f).SetUpdate(true);
			_buttonbackgroundGroup.DOFade(1f, 1f).SetUpdate(true);
			_controllerPrompts.DOFade(1f, 1f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			OverrideDefault(_continueButton);
			MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_continueButton);
		}

		private IEnumerator ShowPenaltyRoutine(List<DeathScreenInventoryItem> inventoryItems, float Delay)
		{
			DisplayinPenalty = true;
			yield return new WaitForSecondsRealtime(Delay);
			Debug.Log("ShowPenaltyRoutine   Inventory.itemsDungeon.Count: " + Inventory.itemsDungeon.Count);
			if (Inventory.itemsDungeon.Count > 0)
			{
				yield return new WaitForSecondsRealtime(0.5f);
				float penalty3 = 0f;
				switch (_result)
				{
				case Results.Escaped:
					penalty3 = DifficultyManager.GetEscapedPeneltyPercentage();
					_subTitle.text = string.Format(ScriptLocalization.UI_DeathScreen_Escaped.Penalty, penalty3);
					penalty3 *= -1f;
					break;
				case Results.Killed:
					if (DataManager.Instance.PrayedAtCrownShrine)
					{
						_subTitle.text = ScriptLocalization.UI_DeathScreen_CrownShrine.Penalty;
						penalty3 = 0f;
					}
					else if (LastRunWasSandBox)
					{
						penalty3 = 0f;
					}
					else
					{
						penalty3 = DifficultyManager.GetDeathPeneltyPercentage();
						_subTitle.text = string.Format(ScriptLocalization.UI_DeathScreen_Killed.Penalty, penalty3);
						penalty3 *= -1f;
					}
					break;
				case Results.Completed:
					penalty3 = 0f;
					if (DataManager.Instance.PlayerDamageReceivedThisRun <= 0f)
					{
						penalty3 = 50f;
						_subTitle.text = ScriptLocalization.UI_DeathScreen_NoDamage.Penalty;
					}
					else if (!DataManager.Instance.TakenBossDamage && PlayerFarming.Location != FollowerLocation.IntroDungeon && !LastRunWasSandBox)
					{
						if (DataManager.Instance.DungeonBossFight && DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location))
						{
							_subTitle.text = ScriptLocalization.UI_DeathScreen_NoDamageCultLeader.Penalty;
							penalty3 = 30f;
						}
						else
						{
							_subTitle.text = ScriptLocalization.UI_DeathScreen_NoDamageBoss.Penalty;
							penalty3 = 20f;
						}
					}
					else if (!LastRunWasSandBox && DataManager.Instance.DungeonBossFight && DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location))
					{
						_subTitle.text = ScriptLocalization.UI_DeathScreen_CultLeader.Penalty;
						penalty3 = 20f;
					}
					break;
				}
				if (penalty3 != 0f)
				{
					penalty3 += ((_result == Results.Killed) ? PlayerFleeceManager.GetLootMultiplier(_result) : 0f);
					penalty3 = Mathf.Clamp(penalty3, -100f, float.MaxValue);
					if (DataManager.Instance.PlayerFleece == 3 && _result != Results.Completed && _result != Results.Escaped)
					{
						_subTitle.text = LocalizationManager.GetTranslation("UI/DeathScreen/Killed/Fleece3/Penalty");
					}
					AddLoot(penalty3);
					yield return new WaitForEndOfFrame();
					if (Inventory.itemsDungeon.Count > 0)
					{
						foreach (DeathScreenInventoryItem inventoryItem in inventoryItems)
						{
							if (!_excludeLootFromBonus.Contains(inventoryItem.Type))
							{
								Color color = inventoryItem.AmountText.color;
								Color color2 = inventoryItem.AmountText.color;
								color.a = 0f;
								inventoryItem.AmountText.color = color;
								inventoryItem.AmountText.DOColor(color2, 1f).SetUpdate(true);
							}
							inventoryItem.AmountText.text = Inventory.GetDungeonItemByType((int)inventoryItem.Type).quantity.ToString();
							if (!_excludeLootFromBonus.Contains(inventoryItem.Type))
							{
								if (_lootDelta[inventoryItem.Type] == 0)
								{
									continue;
								}
								inventoryItem.ShowDelta(_lootDelta[inventoryItem.Type]);
								inventoryItem.DeltaText.transform.localScale = Vector3.one * 2f;
								inventoryItem.DeltaText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
							}
							if (penalty3 < 0f)
							{
								if (!_excludeLootFromBonus.Contains(inventoryItem.Type))
								{
									inventoryItem.RectTransform.DOShakePosition(1f + UnityEngine.Random.Range(0f, 0.2f), new Vector3(10f, 0f)).SetUpdate(true);
								}
							}
							else
							{
								Vector3 localScale = inventoryItem.RectTransform.localScale;
								inventoryItem.RectTransform.transform.localScale = localScale * 1.4f;
								inventoryItem.RectTransform.transform.DOScale(localScale, 1f).SetEase(Ease.OutBack).SetUpdate(true);
							}
						}
					}
					yield return new WaitForSecondsRealtime(0.5f);
				}
			}
			DisplayinPenalty = false;
		}

		private void AddLoot(float penalty)
		{
			_lootDelta = new Dictionary<InventoryItem.ITEM_TYPE, int>();
			penalty /= 100f;
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
			List<ObjectivesData> list2 = new List<ObjectivesData>();
			list2.AddRange(DataManager.Instance.Objectives);
			list2.AddRange(DataManager.Instance.CompletedObjectives);
			foreach (ObjectivesData item in list2)
			{
				if (item is Objectives_CollectItem)
				{
					list.Add(((Objectives_CollectItem)item).ItemType);
				}
			}
			foreach (InventoryItem item2 in Inventory.itemsDungeon)
			{
				if (!_excludeLootFromBonus.Contains((InventoryItem.ITEM_TYPE)item2.type))
				{
					int num = Mathf.RoundToInt((float)item2.quantity * penalty);
					if (list.Contains((InventoryItem.ITEM_TYPE)item2.type) && penalty <= 0f)
					{
						num = 0;
					}
					if (penalty > 0f && num < 1)
					{
						num = 1;
					}
					_lootDelta.Add((InventoryItem.ITEM_TYPE)item2.type, num);
					int quantity = item2.quantity + num;
					Inventory.GetDungeonItemByType(item2.type).quantity = quantity;
					Inventory.SetItemQuantity(item2.type, Inventory.GetItemQuantity(item2.type) + num);
				}
			}
		}

		private QuoteScreenController.QuoteTypes GetQuoteType()
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				return QuoteScreenController.QuoteTypes.QuoteBoss6;
			case FollowerLocation.Dungeon1_2:
				return QuoteScreenController.QuoteTypes.QuoteBoss7;
			case FollowerLocation.Dungeon1_3:
				return QuoteScreenController.QuoteTypes.QuoteBoss8;
			case FollowerLocation.Dungeon1_4:
				return QuoteScreenController.QuoteTypes.QuoteBoss9;
			default:
				return QuoteScreenController.QuoteTypes.QuoteBoss6;
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Instance = null;
		}

		private void Update()
		{
			Time.timeScale = 0f;
		}
	}
}
