using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Lamb.UI.Rituals;
using src.UI.InfoCards;
using src.UINavigator;
using TMPro;
using Unify;
using UnityEngine;

namespace Lamb.UI
{
	public class UIPlayerUpgradesMenuController : UIMenuBase
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass30_0
		{
			public RectTransform cardTransform;

			public UIPlayerUpgradesMenuController _003C_003E4__this;

			public GameObject redOutline;

			public bool cancel;

			public RectTransform cardContainer;

			internal void _003CFocusCard_003Eg__OnHold_007C0(float progress)
			{
				float num = 1f + 0.25f * progress;
				cardTransform.localScale = new Vector3(num, num, num);
				cardTransform.localPosition = UnityEngine.Random.insideUnitCircle * progress * _003C_003E4__this._uiHoldInteraction.HoldTime * 2f;
				MMVibrate.RumbleContinuous(progress * 0.2f, progress * 0.2f);
				if (redOutline.gameObject.activeSelf != progress > 0f)
				{
					redOutline.gameObject.SetActive(progress > 0f);
				}
				redOutline.transform.localScale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(1.2f, 1.2f, 1.2f), progress);
			}

			internal void _003CFocusCard_003Eg__OnCancel_007C1()
			{
				cancel = true;
				MMVibrate.StopRumble();
			}

			internal void _003CFocusCard_003Eb__2()
			{
				cardTransform.SetParent(cardContainer, true);
			}
		}

		private string kSelectedAnimationState = "Selected";

		private string kCancelSelectionAnimationState = "Cancelled";

		private string kConfirmedSelectionAnimationState = "Confirmed";

		public static Action OnDoctrineUnlockSelected;

		public static Action OnCrystalDoctrineUnlockSelected;

		[Header("Commandments")]
		[SerializeField]
		private RitualInfoCardController _ritualInfoCardController;

		[SerializeField]
		private RitualItem _ritualItem;

		[SerializeField]
		private GameObject _ritualItemAlert;

		[SerializeField]
		private RitualItem _crystalDoctrineItem;

		[SerializeField]
		private GameObject _crystalDoctrineItemAlert;

		[Header("Abilities")]
		[SerializeField]
		private TextMeshProUGUI _crownAbilityCount;

		[SerializeField]
		private CrownAbilityInfoCardController _crownAbilityInfoCardController;

		[SerializeField]
		private CrownAbilityItemBuyable[] _upgradeShopItems;

		[Header("Fleeces")]
		[SerializeField]
		private TextMeshProUGUI _fleeceCount;

		[SerializeField]
		private FleeceInfoCardController _fleeceInfoCardController;

		[SerializeField]
		private FleeceItemBuyable[] _fleeceItems;

		[Header("Misc")]
		[SerializeField]
		private RectTransform _rootContainer;

		[SerializeField]
		private UIHoldInteraction _uiHoldInteraction;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private MMScrollRect _scrollRect;

		private bool _didCancel;

		private bool _showingCrystal;

		private bool _showingFleeces;

		private PlayerFleeceManager.FleeceType[] _showFleeces;

		private bool _sequenceRequiresConfirmation = true;

		private UpgradeSystem.Type[] _crownUpgrades = new UpgradeSystem.Type[4]
		{
			UpgradeSystem.Type.Ability_Resurrection,
			UpgradeSystem.Type.Ability_Eat,
			UpgradeSystem.Type.Ability_TeleportHome,
			UpgradeSystem.Type.Ability_BlackHeart
		};

		private void Start()
		{
			_ritualItem.Configure(UpgradeSystem.PrimaryRitual1);
			if (!DoctrineUpgradeSystem.TrySermonsStillAvailable())
			{
				_ritualItem.SetMaxed();
			}
			RitualItem ritualItem = _ritualItem;
			ritualItem.OnRitualItemSelected = (Action<UpgradeSystem.Type>)Delegate.Combine(ritualItem.OnRitualItemSelected, new Action<UpgradeSystem.Type>(RitualItemSelected));
			if (UpgradeSystem.GetCoolDownNormalised(UpgradeSystem.PrimaryRitual1) <= 0f && Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.DOCTRINE_STONE) >= 1 && DoctrineUpgradeSystem.TrySermonsStillAvailable())
			{
				_ritualItemAlert.SetActive(true);
			}
			else
			{
				_ritualItemAlert.SetActive(false);
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ritual_CrystalDoctrine))
			{
				_crystalDoctrineItem.Configure(UpgradeSystem.Type.Ritual_CrystalDoctrine);
				RitualItem crystalDoctrineItem = _crystalDoctrineItem;
				crystalDoctrineItem.OnRitualItemSelected = (Action<UpgradeSystem.Type>)Delegate.Combine(crystalDoctrineItem.OnRitualItemSelected, new Action<UpgradeSystem.Type>(CrystalDoctrineRitualSelected));
				if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE) > 0 && DoctrineUpgradeSystem.GetAllRemainingDoctrines().Count > 0 && UpgradeSystem.GetCoolDownNormalised(UpgradeSystem.Type.Ritual_CrystalDoctrine) <= 0f)
				{
					_crystalDoctrineItemAlert.SetActive(true);
				}
				else
				{
					_crystalDoctrineItemAlert.SetActive(false);
				}
			}
			else
			{
				_crystalDoctrineItem.gameObject.SetActive(false);
			}
			int num = 0;
			for (int i = 0; i < _crownUpgrades.Length; i++)
			{
				num += UpgradeSystem.GetUnlocked(_crownUpgrades[i]).ToInt();
				_upgradeShopItems[i].Configure(_crownUpgrades[i]);
				_upgradeShopItems[i].OnUpgradeChosen = UpgradeItemSelected;
			}
			_crownAbilityCount.text = string.Format("{0}/{1}", num, _crownUpgrades.Length);
			int num2 = 0;
			int num3 = 0;
			FleeceItemBuyable[] fleeceItems = _fleeceItems;
			foreach (FleeceItemBuyable fleeceItemBuyable in fleeceItems)
			{
				fleeceItemBuyable.Configure((fleeceItemBuyable.ForcedFleeceIndex == -1) ? _fleeceItems.IndexOf(fleeceItemBuyable) : fleeceItemBuyable.ForcedFleeceIndex);
				fleeceItemBuyable.OnFleeceChosen = FleeceItemSelected;
				if (fleeceItemBuyable.gameObject.activeSelf)
				{
					num3++;
					if (fleeceItemBuyable.Unlocked)
					{
						num2++;
					}
				}
			}
			_fleeceCount.text = string.Format("{0}/{1}", num2, num3);
		}

		public void ShowCrystalUnlock()
		{
			_crownAbilityInfoCardController.enabled = false;
			_fleeceInfoCardController.enabled = false;
			_sequenceRequiresConfirmation = true;
			OverrideDefault(null);
			_showingCrystal = true;
			Show();
			_controlPrompts.HideAcceptButton();
			_controlPrompts.HideCancelButton();
		}

		public void ShowNewFleecesUnlocked(PlayerFleeceManager.FleeceType[] fleeceTypes, bool requiresConfirmation = false)
		{
			_showingFleeces = true;
			_showFleeces = fleeceTypes;
			_sequenceRequiresConfirmation = requiresConfirmation;
			_crownAbilityInfoCardController.enabled = false;
			_fleeceInfoCardController.enabled = false;
			OverrideDefault(null);
			Show();
			_controlPrompts.HideAcceptButton();
			_controlPrompts.HideCancelButton();
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_showingCrystal || _showingFleeces)
			{
				SetActiveStateForMenu(false);
				CrownAbilityItemBuyable[] upgradeShopItems = _upgradeShopItems;
				for (int i = 0; i < upgradeShopItems.Length; i++)
				{
					upgradeShopItems[i].ForceIncognitoState();
				}
				if (_showingFleeces)
				{
					FleeceItemBuyable[] fleeceItems = _fleeceItems;
					foreach (FleeceItemBuyable fleeceItemBuyable in fleeceItems)
					{
						if (_showFleeces.Contains((PlayerFleeceManager.FleeceType)fleeceItemBuyable.Fleece))
						{
							fleeceItemBuyable.PrepareForUnlock();
						}
						else
						{
							fleeceItemBuyable.ForceIncognitoState();
						}
					}
				}
				_ritualItemAlert.SetActive(false);
				_ritualItem.ForceIncognitoState();
				if (_showingCrystal)
				{
					_crystalDoctrineItem.gameObject.SetActive(true);
					_crystalDoctrineItem.ForceLockedState();
					_crystalDoctrineItemAlert.SetActive(false);
				}
				else if (_showingFleeces)
				{
					_crystalDoctrineItemAlert.SetActive(false);
					if (DataManager.Instance.OnboardedCrystalDoctrine)
					{
						_crystalDoctrineItem.ForceIncognitoState();
					}
				}
				yield return _003C_003En__0();
				yield return new WaitForSecondsRealtime(0.1f);
				if (_showingCrystal)
				{
					yield return _crystalDoctrineItem.DoUnlock();
					_crystalDoctrineItemAlert.SetActive(true);
					Vector3 one = Vector3.one;
					_crystalDoctrineItemAlert.transform.localScale = Vector3.zero;
					_crystalDoctrineItemAlert.transform.DOScale(one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
					_crystalDoctrineItemAlert.gameObject.SetActive(true);
					yield return new WaitForSecondsRealtime(0.5f);
				}
				else if (_showingFleeces)
				{
					yield return _scrollRect.ScrollToBottom();
					FleeceItemBuyable[] fleeceItems2 = _fleeceItems;
					foreach (FleeceItemBuyable fleeceItemBuyable2 in fleeceItems2)
					{
						if (_showFleeces.Contains((PlayerFleeceManager.FleeceType)fleeceItemBuyable2.Fleece))
						{
							yield return fleeceItemBuyable2.DoUnlock();
						}
					}
				}
				yield return new WaitForSecondsRealtime(0.1f);
				if (_showingCrystal)
				{
					_ritualInfoCardController.ShowCardWithParam(UpgradeSystem.Type.Ritual_CrystalDoctrine);
					_controlPrompts.ShowAcceptButton();
					while (!InputManager.UI.GetAcceptButtonDown())
					{
						yield return null;
					}
					Hide();
				}
				else
				{
					if (!_showingFleeces)
					{
						yield break;
					}
					yield return new WaitForSecondsRealtime(0.2f);
					if (_sequenceRequiresConfirmation)
					{
						_fleeceInfoCardController.ShowCardWithParam((int)_showFleeces[0]);
						_controlPrompts.ShowAcceptButton();
						while (!InputManager.UI.GetAcceptButtonDown())
						{
							yield return null;
						}
						Hide();
						yield break;
					}
					yield return _scrollRect.ScrollToTop();
					_ritualItem.AnimateIncognitoOut();
					_crystalDoctrineItem.AnimateIncognitoOut();
					upgradeShopItems = _upgradeShopItems;
					for (int i = 0; i < upgradeShopItems.Length; i++)
					{
						upgradeShopItems[i].AnimateIncognitoOut();
					}
					FleeceItemBuyable[] fleeceItems = _fleeceItems;
					for (int i = 0; i < fleeceItems.Length; i++)
					{
						fleeceItems[i].AnimateIncognitoOut();
					}
					yield return new WaitForSecondsRealtime(0.25f);
					_crownAbilityInfoCardController.enabled = true;
					_fleeceInfoCardController.enabled = true;
					_controlPrompts.ShowAcceptButton();
					MonoSingleton<UINavigatorNew>.Instance.Clear();
					SetActiveStateForMenu(true);
				}
			}
			else
			{
				yield return _003C_003En__0();
			}
		}

		private IEnumerator FocusCard(RectTransform cardTransform, GameObject redOutline, Action andThen)
		{
			_003C_003Ec__DisplayClass30_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass30_0();
			CS_0024_003C_003E8__locals0.cardTransform = cardTransform;
			CS_0024_003C_003E8__locals0._003C_003E4__this = this;
			CS_0024_003C_003E8__locals0.redOutline = redOutline;
			CS_0024_003C_003E8__locals0.redOutline.gameObject.SetActive(false);
			CS_0024_003C_003E8__locals0.cardContainer = CS_0024_003C_003E8__locals0.cardTransform.parent as RectTransform;
			_ritualInfoCardController.enabled = false;
			_fleeceInfoCardController.enabled = false;
			_crownAbilityInfoCardController.enabled = false;
			OverrideDefaultOnce(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable);
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			SetActiveStateForMenu(false);
			_controlPrompts.HideAcceptButton();
			_uiHoldInteraction.Reset();
			CS_0024_003C_003E8__locals0.cardTransform.SetParent(_rootContainer, true);
			CS_0024_003C_003E8__locals0.cardTransform.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.InOutBack).SetUpdate(true);
			_animator.Play(kSelectedAnimationState);
			yield return new WaitForSecondsRealtime(1f);
			CS_0024_003C_003E8__locals0.cancel = false;
			yield return _uiHoldInteraction.DoHoldInteraction(CS_0024_003C_003E8__locals0._003CFocusCard_003Eg__OnHold_007C0, CS_0024_003C_003E8__locals0._003CFocusCard_003Eg__OnCancel_007C1);
			MMVibrate.StopRumble();
			if (CS_0024_003C_003E8__locals0.cancel)
			{
				Vector2 vector = _rootContainer.InverseTransformPoint(CS_0024_003C_003E8__locals0.cardContainer.TransformPoint(Vector3.zero));
				CS_0024_003C_003E8__locals0.cardTransform.DOLocalMove(vector, 1f).SetEase(Ease.InOutBack).SetUpdate(true)
					.OnComplete(delegate
					{
						CS_0024_003C_003E8__locals0.cardTransform.SetParent(CS_0024_003C_003E8__locals0.cardContainer, true);
					});
				_animator.Play(kCancelSelectionAnimationState);
				yield return new WaitForSecondsRealtime(1f);
				_controlPrompts.ShowAcceptButton();
				SetActiveStateForMenu(true);
				_ritualInfoCardController.enabled = true;
				_fleeceInfoCardController.enabled = true;
				_crownAbilityInfoCardController.enabled = true;
			}
			else
			{
				_controlPrompts.HideCancelButton();
				CS_0024_003C_003E8__locals0.cardTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return _animator.YieldForAnimation(kConfirmedSelectionAnimationState);
				if (andThen != null)
				{
					andThen();
				}
				Hide(true);
			}
		}

		private void RitualItemSelected(UpgradeSystem.Type ritual)
		{
			Action onDoctrineUnlockSelected = OnDoctrineUnlockSelected;
			if (onDoctrineUnlockSelected != null)
			{
				onDoctrineUnlockSelected();
			}
			Hide();
		}

		private void CrystalDoctrineRitualSelected(UpgradeSystem.Type ritual)
		{
			Action onCrystalDoctrineUnlockSelected = OnCrystalDoctrineUnlockSelected;
			if (onCrystalDoctrineUnlockSelected != null)
			{
				onCrystalDoctrineUnlockSelected();
			}
			Hide();
		}

		private void FleeceItemSelected(int fleece)
		{
			if (DataManager.Instance.UnlockedFleeces.Contains(fleece))
			{
				if (DataManager.Instance.PlayerFleece != fleece)
				{
					EquipFleece(fleece);
					UpdateFleeces();
				}
			}
			else
			{
				if (!_fleeceItems[fleece].Cost.CanAfford())
				{
					return;
				}
				StartCoroutine(FocusCard(_fleeceInfoCardController.CurrentCard.RectTransform, _fleeceInfoCardController.CurrentCard._redOutline, delegate
				{
					int playerFleece = DataManager.Instance.PlayerFleece;
					Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.TALISMAN, -1);
					if (!DataManager.Instance.UnlockedFleeces.Contains(fleece))
					{
						DataManager.Instance.UnlockedFleeces.Add(fleece);
					}
					EquipFleece(fleece);
					UpdateFleeces();
					AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("UNLOCK_TUNIC"));
					int num = DataManager.Instance.UnlockedFleeces.Count;
					if (DataManager.Instance.UnlockedFleeces.Contains(999))
					{
						num--;
					}
					if (DataManager.Instance.UnlockedFleeces.Contains(1000))
					{
						num--;
					}
					if (num >= 10)
					{
						AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("UNLOCK_ALL_TUNICS"));
					}
					Hide(true);
					Interaction_TempleAltar.Instance.GetFleeceRoutine(playerFleece, fleece);
				}));
			}
		}

		private void EquipFleece(int fleece)
		{
			FleeceItemBuyable fleeceItemBuyable = null;
			FleeceItemBuyable[] fleeceItems = _fleeceItems;
			foreach (FleeceItemBuyable fleeceItemBuyable2 in fleeceItems)
			{
				if (fleeceItemBuyable2.ForcedFleeceIndex != -1 && fleeceItemBuyable2.ForcedFleeceIndex == fleece)
				{
					fleeceItemBuyable = fleeceItemBuyable2;
				}
			}
			if (fleeceItemBuyable != null)
			{
				fleeceItemBuyable.Bump();
			}
			else
			{
				_fleeceItems[fleece].Bump();
			}
			DataManager.Instance.PlayerFleece = fleece;
			if (PlayerFarming.Instance != null)
			{
				SimpleSpineAnimator simpleSpineAnimator = PlayerFarming.Instance.simpleSpineAnimator;
				if ((object)simpleSpineAnimator != null)
				{
					simpleSpineAnimator.SetSkin("Lamb_" + DataManager.Instance.PlayerFleece);
				}
			}
		}

		private void UpdateFleeces()
		{
			FleeceItemBuyable[] fleeceItems = _fleeceItems;
			for (int i = 0; i < fleeceItems.Length; i++)
			{
				fleeceItems[i].UpdateState();
			}
		}

		private void UpgradeItemSelected(CrownAbilityItemBuyable upgradeItem)
		{
			if (!UpgradeSystem.GetUnlocked(upgradeItem.Type) && upgradeItem.Cost.CanAfford())
			{
				StartCoroutine(FocusCard(_crownAbilityInfoCardController.CurrentCard.RectTransform, _crownAbilityInfoCardController.CurrentCard._redOutline, delegate
				{
					Inventory.ChangeItemQuantity(upgradeItem.Cost.CostItem, -upgradeItem.Cost.CostValue);
					UpgradeSystem.UnlockAbility(upgradeItem.Type);
					UpdateUpgrades();
					upgradeItem.Bump();
					GameManager.GetInstance().CameraSetOffset(Vector3.zero);
					Hide(true);
					Interaction_TempleAltar.Instance.GetAbilityRoutine(upgradeItem.Type);
				}));
			}
		}

		private bool CheckCanAfford(List<StructuresData.ItemCost> cost)
		{
			for (int i = 0; i < cost.Count; i++)
			{
				if (Inventory.GetItemQuantity(cost[i].CostItem) < cost[i].CostValue)
				{
					return false;
				}
			}
			return true;
		}

		private void UpdateUpgrades()
		{
			CrownAbilityItemBuyable[] upgradeShopItems = _upgradeShopItems;
			for (int i = 0; i < upgradeShopItems.Length; i++)
			{
				upgradeShopItems[i].UpdateState();
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_didCancel = true;
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			AudioManager.Instance.PlayOneShot("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}
	}
}
