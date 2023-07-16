using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI.Assets;
using Lamb.UI.BuildMenu;
using Spine;
using Spine.Unity;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UINewItemOverlayController : UIMenuBase
	{
		public enum TypeOfCard
		{
			Weapon,
			Curse,
			Trinket,
			Decoration,
			Gift,
			Necklace,
			FollowerSkin,
			MapLocation,
			Relic
		}

		public Transform LerpChild;

		public Image imageOfItemUnlocked;

		public InventoryIconMapping _inventoryIconMapping;

		public CanvasGroup _infoCanvas;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		public TextMeshProUGUI Title;

		public TextMeshProUGUI Subtitle;

		public TextMeshProUGUI Description;

		public TypeOfCard MyTypeOfCard;

		public SkeletonGraphic Spine;

		[SpineEvent("", "Spine", true, true, false)]
		public string eventName;

		private string bookOpen = "book-open";

		private string bookClose = "book-close";

		public EventData eventData;

		public EventData bookOpenData;

		public EventData bookCloseData;

		public RectTransform SpineParent;

		private Vector3 SpineStartPosition;

		public SkeletonGraphic FollowerSpine;

		public StructureBrain.TYPES pickedBuilding;

		private EventInstance _loopingSoundInstance;

		private bool _createdLoop;

		public override void Awake()
		{
			base.Awake();
			_canvasGroup.alpha = 0f;
			FollowerSpine.enabled = false;
			imageOfItemUnlocked.enabled = false;
			Title.text = " ";
			Subtitle.text = " ";
			_controlPrompts.HideAcceptButton();
		}

		private void Start()
		{
			eventData = Spine.Skeleton.Data.FindEvent(eventName);
			bookOpenData = PlayerFarming.Instance.Spine.Skeleton.Data.FindEvent(bookOpen);
			bookCloseData = PlayerFarming.Instance.Spine.Skeleton.Data.FindEvent(bookClose);
			Spine.AnimationState.Event += HandleAnimationStateEvent;
			PlayerFarming.Instance.Spine.AnimationState.Event += PlayerHandleAnimationStateEvent;
		}

		public void Show(TypeOfCard myTypeOfCard, Vector3 startPosition, string skinName, bool instant = false)
		{
			MyTypeOfCard = myTypeOfCard;
			Debug.Log("Show item overlay ");
			if (myTypeOfCard == TypeOfCard.FollowerSkin)
			{
				FollowerSpine.Skeleton.SetSkin(skinName);
				WorshipperData.SkinAndData characters = WorshipperData.Instance.GetCharacters(skinName);
				foreach (WorshipperData.SlotAndColor slotAndColour in characters.SlotAndColours[Mathf.Min(0, characters.SlotAndColours.Count - 1)].SlotAndColours)
				{
					Slot slot = FollowerSpine.Skeleton.FindSlot(slotAndColour.Slot);
					if (slot != null)
					{
						slot.SetColor(slotAndColour.color);
					}
				}
				Title.text = ScriptLocalization.UI.KnowledgeAcquired + " " + ScriptLocalization.UI.SkinTitle;
				Subtitle.text = WorshipperData.Instance.GetSkinsLocationString(characters);
				Description.text = ScriptLocalization.UI.SkinDescription;
				imageOfItemUnlocked.sprite = null;
				Spine.Skeleton.SetSkin("Trinket");
				DataManager.SetFollowerSkinUnlocked(skinName);
			}
			else
			{
				Debug.Log("UH OH something went wrong :( ");
			}
			Show(instant);
			SpineStartPosition = Camera.main.WorldToScreenPoint(startPosition);
			StartCoroutine(MoveSpine());
		}

		public void Show(TypeOfCard myTypeOfCard, Vector3 startPosition, InventoryItem.ITEM_TYPE itemType, bool instant = false)
		{
			MyTypeOfCard = myTypeOfCard;
			switch (myTypeOfCard)
			{
			case TypeOfCard.Gift:
				Title.text = ScriptLocalization.UI.KnowledgeAcquired + " " + InventoryItem.LocalizedName(itemType);
				Description.text = InventoryItem.LocalizedLore(itemType);
				Subtitle.text = InventoryItem.LocalizedDescription(itemType);
				imageOfItemUnlocked.sprite = _inventoryIconMapping.GetImage(itemType);
				Spine.Skeleton.SetSkin("Trinket");
				break;
			case TypeOfCard.Necklace:
				Title.text = ScriptLocalization.UI.KnowledgeAcquired + " " + InventoryItem.LocalizedName(itemType);
				Description.text = InventoryItem.LocalizedLore(itemType);
				Subtitle.text = InventoryItem.LocalizedDescription(itemType);
				imageOfItemUnlocked.sprite = _inventoryIconMapping.GetImage(itemType);
				Spine.Skeleton.SetSkin("Trinket");
				break;
			default:
				Debug.Log("UH OH something went wrong :( ");
				break;
			}
			Show(instant);
			SpineStartPosition = Camera.main.WorldToScreenPoint(startPosition);
			StartCoroutine(MoveSpine());
		}

		public void Show(TypeOfCard myTypeOfCard, Vector3 startPosition, bool pickFromAvailable, bool instant = false)
		{
			MyTypeOfCard = myTypeOfCard;
			Title.text = ScriptLocalization.UI.KnowledgeAcquired;
			switch (myTypeOfCard)
			{
			case TypeOfCard.Trinket:
				Debug.Log("WARNING: Removed trinket card type".Colour(Color.red));
				break;
			case TypeOfCard.Decoration:
				if (pickFromAvailable)
				{
					List<StructureBrain.TYPES> decorationListFromLocation = DataManager.Instance.GetDecorationListFromLocation(PlayerFarming.Location);
					foreach (StructureBrain.TYPES item in DataManager.Instance.GetDecorationListFromCategory(DataManager.DecorationType.All))
					{
						if (!decorationListFromLocation.Contains(item))
						{
							decorationListFromLocation.Add(item);
						}
					}
					pickedBuilding = decorationListFromLocation[UnityEngine.Random.Range(0, decorationListFromLocation.Count - 1)];
				}
				Debug.Log("Picked building: " + pickedBuilding);
				imageOfItemUnlocked.sprite = TypeAndPlacementObjects.GetByType(pickedBuilding).IconImage;
				if (imageOfItemUnlocked.sprite == null)
				{
					Debug.Log("Missing icon image from TypeAndPlacementObjects for: " + pickedBuilding);
				}
				Title.text = ScriptLocalization.UI.KnowledgeAcquired + " " + StructuresData.LocalizedName(pickedBuilding);
				Subtitle.text = DataManager.GetDecorationLocation(pickedBuilding);
				Description.text = StructuresData.LocalizedDescription(pickedBuilding);
				StructuresData.CompleteResearch(pickedBuilding);
				StructuresData.SetRevealed(pickedBuilding);
				Spine.Skeleton.SetSkin("Decoration");
				break;
			default:
				Debug.Log("UH OH something went wrong :( ");
				break;
			}
			Show(instant);
			SpineStartPosition = Camera.main.WorldToScreenPoint(startPosition);
			StartCoroutine(MoveSpine());
		}

		public void TurnOnImage()
		{
			TypeOfCard myTypeOfCard = MyTypeOfCard;
			if (myTypeOfCard == TypeOfCard.FollowerSkin)
			{
				FollowerSpine.enabled = true;
				imageOfItemUnlocked.enabled = false;
			}
			else
			{
				FollowerSpine.enabled = false;
				imageOfItemUnlocked.enabled = true;
			}
		}

		public void HandleAnimationStateEvent(TrackEntry trackEntry, global::Spine.Event e)
		{
			if (eventData == e.Data)
			{
				AudioManager.Instance.PlayOneShot("event:/player/new_item_reveal", base.gameObject);
				TurnOnImage();
			}
		}

		public void PlayerHandleAnimationStateEvent(TrackEntry trackEntry, global::Spine.Event e)
		{
			if (bookOpenData == e.Data)
			{
				if (!_createdLoop)
				{
					Debug.Log("Play Page Loop");
					_loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/player/new_item_pages_loop", PlayerFarming.Instance.gameObject, true);
					_createdLoop = true;
				}
			}
			else if (bookCloseData == e.Data)
			{
				_createdLoop = false;
				AudioManager.Instance.StopLoop(_loopingSoundInstance);
				AudioManager.Instance.PlayOneShot("event:/player/new_item_book_close", PlayerFarming.Instance.gameObject);
				PlayerFarming.Instance.Spine.AnimationState.Event -= PlayerHandleAnimationStateEvent;
			}
		}

		protected override void OnHideCompleted()
		{
			AudioManager.Instance.StopLoop(_loopingSoundInstance);
			Spine.AnimationState.Event -= HandleAnimationStateEvent;
			PlayerFarming.Instance.Spine.AnimationState.Event -= PlayerHandleAnimationStateEvent;
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void OnDisable()
		{
			AudioManager.Instance.StopLoop(_loopingSoundInstance);
			Spine.AnimationState.Event -= HandleAnimationStateEvent;
			PlayerFarming.Instance.Spine.AnimationState.Event -= PlayerHandleAnimationStateEvent;
		}

		private IEnumerator MoveSpine()
		{
			Spine.rectTransform.position = SpineStartPosition;
			Spine.AnimationState.SetAnimation(0, "reveal", false);
			Spine.AnimationState.AddAnimation(0, "static", true, 0f);
			float Progress = 0f;
			float Duration = 0.5f;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.unscaledDeltaTime);
				if (num < Duration)
				{
					Spine.rectTransform.position = Vector3.Lerp(SpineStartPosition, SpineParent.position, Mathf.SmoothStep(0f, 1f, Progress / Duration));
					yield return null;
					continue;
				}
				break;
			}
		}

		private IEnumerator EndSpine()
		{
			Spine.AnimationState.SetAnimation(0, "end", false);
			float Progress = 0f;
			float Duration = 0.3f;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.unscaledDeltaTime);
				if (!(num < Duration))
				{
					break;
				}
				Spine.rectTransform.position = Vector3.Lerp(SpineParent.position, SpineStartPosition + new Vector3(0.5f, -0.5f), Mathf.SmoothStep(0f, 1f, Progress / Duration));
				yield return null;
			}
			Spine.gameObject.SetActive(false);
		}

		protected override IEnumerator DoShowAnimation()
		{
			_canvasGroup.alpha = 1f;
			PlayerFarming.Instance.SpineUseDeltaTime(false);
			HUD_Manager.Instance.Hide(false, 0);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			AudioManager.Instance.PlayOneShot("event:/player/new_item_pickup", PlayerFarming.Instance.gameObject);
			PlayerFarming.Instance.simpleSpineAnimator.Animate("find-item/find-item-start", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("find-item/find-item-loop", 0, true, 0f);
			GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -2f));
			Transform lerpChild = LerpChild;
			Vector3 localPosition = new Vector3(-300f, LerpChild.localPosition.y);
			lerpChild.localPosition = localPosition;
			Vector3 vector = new Vector3(0f, LerpChild.localPosition.y);
			_infoCanvas.alpha = 0f;
			_infoCanvas.DOFade(1f, 0.66f).SetUpdate(true).SetEase(Ease.OutQuart);
			LerpChild.DOLocalMoveX(vector.x, 0.66f).SetUpdate(true).SetEase(Ease.OutQuart);
			yield return new WaitForSecondsRealtime(1f);
			_controlPrompts.ShowAcceptButton();
			while (!InputManager.UI.GetCancelButtonDown() && !InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			_controlPrompts.HideAcceptButton();
			if (MyTypeOfCard == TypeOfCard.Decoration)
			{
				_loopingSoundInstance.setVolume(0.33f);
				UIBuildMenuController buildMenuController = MonoSingleton<UIManager>.Instance.BuildMenuTemplate.Instantiate();
				buildMenuController.Show(pickedBuilding);
				UIBuildMenuController uIBuildMenuController = buildMenuController;
				uIBuildMenuController.OnHidden = (Action)Delegate.Combine(uIBuildMenuController.OnHidden, (Action)delegate
				{
					_loopingSoundInstance.setVolume(1f);
					buildMenuController = null;
				});
				PushInstance(buildMenuController);
				while (buildMenuController != null)
				{
					yield return null;
				}
			}
			AudioManager.Instance.PlayOneShot("event:/player/new_item_sequence_close", base.gameObject);
			Transform lerpChild2 = LerpChild;
			localPosition = new Vector3(0f, LerpChild.localPosition.y);
			lerpChild2.localPosition = localPosition;
			vector = new Vector3(300f, LerpChild.localPosition.y);
			_infoCanvas.DOFade(0f, 0.66f).SetUpdate(true).SetEase(Ease.OutQuart);
			LerpChild.DOLocalMoveX(vector.x, 0.66f).SetUpdate(true).SetEase(Ease.OutQuart);
			yield return new WaitForSecondsRealtime(0.5f);
			StartCoroutine(EndSpine());
			yield return new WaitForSecondsRealtime(0.15f);
			if (MyTypeOfCard == TypeOfCard.Decoration)
			{
				PlayerFarming.Instance.simpleSpineAnimator.Animate("find-item/find-decoration-stop", 0, false);
			}
			else
			{
				PlayerFarming.Instance.simpleSpineAnimator.Animate("find-item/find-item-stop", 0, false);
			}
			yield return new WaitForSecondsRealtime(2f);
			Time.timeScale = 1f;
			GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, 0f));
			PlayerFarming.Instance.SpineUseDeltaTime(true);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			HUD_Manager.Instance.Show(0);
			Hide(true);
		}
	}
}
