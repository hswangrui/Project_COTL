using System;
using System.Collections.Generic;
using DG.Tweening;
using src.Extensions;
using src.UI;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class UIFollowerIndoctrinationMenuController : UIMenuBase
	{
		private const string kUpdateFollowerAnimationTrigger = "UpdateFollower";

		public Action OnIndoctrinationCompleted;

		[Header("Follower Indoctrination")]
		[SerializeField]
		private ButtonHighlightController _buttonHighlightController;

		[Header("Misc")]
		[SerializeField]
		private Material _followerUIMaterial;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Name")]
		[SerializeField]
		private MMInputField _nameInputField;

		[SerializeField]
		private MMButton _randomiseNameButton;

		[Header("Appearance")]
		[SerializeField]
		private MMButton _formButton;

		[SerializeField]
		private MMButton _colourButton;

		[SerializeField]
		private MMButton _variantButton;

		[SerializeField]
		private MMButton _randomiseAppearanceButton;

		[Header("Traits")]
		[SerializeField]
		private RectTransform _traitsContent;

		[SerializeField]
		private IndoctrinationTraitItem _traitItemTemplate;

		[Header("Finalize")]
		[SerializeField]
		private MMButton _acceptButton;

		[SerializeField]
		private RectTransform _acceptButtonRectTransform;

		[SerializeField]
		private RectTransform _emptyTextTransform;

		[Header("Twitch")]
		[SerializeField]
		private MMButton _twitchButton;

		private Follower _targetFollower;

		private RendererMaterialSwap _renderMaterialSwap;

		private Material _cachedMaterial;

		private Renderer _followerRenderer;

		private Animator _spawnLocationAnimator;

		private bool _editing;

		private List<IndoctrinationTraitItem> _traitItems = new List<IndoctrinationTraitItem>();

		private string twitchFollowerViewerID = "";

		private string twitchFollowerID = "";

		private bool _createdTwitchFollower;

		public void Show(Follower follower, bool instant = false)
		{
			//if (TwitchAuthentication.IsAuthenticated && !TwitchFollowers.Deactivated)
			//{
			//	List<string> list = new List<string>();
			//	for (int i = 0; i < DataManager.Instance.FollowerSkinsUnlocked.Count; i++)
			//	{
			//		WorshipperData.SkinAndData characters = WorshipperData.Instance.GetCharacters(DataManager.Instance.FollowerSkinsUnlocked[i]);
			//		if (characters != null && !characters.Invariant)
			//		{
			//			list.Add(DataManager.Instance.FollowerSkinsUnlocked[i]);
			//		}
			//	}
			//	TwitchFollowers.SetFollowerVariations(list);
			//}
			follower.Spine.UseDeltaTime = false;
			UIManager.PlayAudio("event:/followers/appearance_menu_appear");
			_targetFollower = follower;
			_nameInputField.text = _targetFollower.Brain.Info.Name;
			_controlPrompts.HideCancelButton();
			_followerRenderer = _targetFollower.Spine.GetComponent<Renderer>();
			_cachedMaterial = _followerRenderer.sharedMaterial;
			_targetFollower.Spine.CustomMaterialOverride.Add(_cachedMaterial, _followerUIMaterial);
			_followerRenderer.sharedMaterial.SetColor("_Color", Color.white);
			//_twitchButton.gameObject.SetActive(TwitchAuthentication.IsAuthenticated && !TwitchFollowers.Deactivated);
			if (BiomeBaseManager.Instance != null)
			{
				_spawnLocationAnimator = BiomeBaseManager.Instance.RecruitSpawnLocation.transform.parent.GetComponent<Animator>();
				if (_spawnLocationAnimator != null)
				{
					_spawnLocationAnimator.Play("Init");
				}
			}
			MMInputField nameInputField = _nameInputField;
			nameInputField.OnStartedEditing = (Action)Delegate.Combine(nameInputField.OnStartedEditing, new Action(OnNameStartedEditing));
			MMInputField nameInputField2 = _nameInputField;
			nameInputField2.OnEndedEditing = (Action<string>)Delegate.Combine(nameInputField2.OnEndedEditing, new Action<string>(OnNameEndedEditing));
			MMButton acceptButton = _acceptButton;
			acceptButton.OnConfirmDenied = (Action)Delegate.Combine(acceptButton.OnConfirmDenied, new Action(ShakeAcceptButton));
			MMButton acceptButton2 = _acceptButton;
			acceptButton2.OnSelected = (Action)Delegate.Combine(acceptButton2.OnSelected, new Action(OnAcceptButtonSelected));
			_randomiseNameButton.onClick.AddListener(RandomiseName);
			_emptyTextTransform.gameObject.SetActive(false);
			_formButton.onClick.AddListener(delegate
			{
				UIAppearanceMenuController_Form uIAppearanceMenuController_Form = MonoSingleton<UIManager>.Instance.AppearanceMenuFormTemplate.Instantiate();
				uIAppearanceMenuController_Form.Show(_targetFollower);
				uIAppearanceMenuController_Form.OnFormChanged = (Action<int>)Delegate.Combine(uIAppearanceMenuController_Form.OnFormChanged, new Action<int>(OnFormChanged));
				PushInstance(uIAppearanceMenuController_Form);
			});
			_colourButton.onClick.AddListener(delegate
			{
				UIAppearanceMenuController_Colour uIAppearanceMenuController_Colour = MonoSingleton<UIManager>.Instance.AppearanceMenuColourTemplate.Instantiate();
				uIAppearanceMenuController_Colour.Show(_targetFollower);
				uIAppearanceMenuController_Colour.OnColourChanged = (Action<int>)Delegate.Combine(uIAppearanceMenuController_Colour.OnColourChanged, new Action<int>(OnColourChanged));
				PushInstance(uIAppearanceMenuController_Colour);
			});
			_variantButton.onClick.AddListener(delegate
			{
				UIAppearanceMenuController_Variant uIAppearanceMenuController_Variant = MonoSingleton<UIManager>.Instance.AppearanceMenuVariantTemplate.Instantiate();
				uIAppearanceMenuController_Variant.Show(_targetFollower);
				uIAppearanceMenuController_Variant.OnVariantChanged = (Action<int>)Delegate.Combine(uIAppearanceMenuController_Variant.OnVariantChanged, new Action<int>(OnVariantChanged));
				PushInstance(uIAppearanceMenuController_Variant);
			});
			_randomiseAppearanceButton.onClick.AddListener(delegate
			{
				WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[_targetFollower.Brain.Info.SkinCharacter];
				_targetFollower.Brain.Info.SkinColour = skinAndData.SlotAndColours.RandomIndex();
				_targetFollower.Brain.Info.SkinCharacter = WorshipperData.Instance.GetRandomAvailableSkin(true, true);
				_targetFollower.Brain.Info.SkinVariation = WorshipperData.Instance.GetColourData(WorshipperData.Instance.GetSkinNameFromIndex(_targetFollower.Brain.Info.SkinCharacter)).Skin.RandomIndex();
				UpdateFollower();
				UpdateButtons();
			});
			_twitchButton.onClick.AddListener(delegate
			{
				UITwitchFollowerSelectOverlayController uITwitchFollowerSelectOverlayController = MonoSingleton<UIManager>.Instance.TwitchFollowerSelectOverlayController.Instantiate();
				uITwitchFollowerSelectOverlayController.Show(this);
				PushInstance(uITwitchFollowerSelectOverlayController);
			});
			_acceptButton.onClick.AddListener(delegate
			{
				_targetFollower.Brain.Info.Name = _nameInputField.text;
				if (!string.IsNullOrEmpty(twitchFollowerViewerID))
				{
					DataManager.Instance.TwitchFollowerViewerIDs.Insert(0, twitchFollowerViewerID);
					DataManager.Instance.TwitchFollowerIDs.Insert(0, twitchFollowerID);
					twitchFollowerViewerID = "";
					twitchFollowerID = "";
				}
				Hide();
			});
			if (_traitItems.Count == 0)
			{
				foreach (FollowerTrait.TraitType trait in _targetFollower.Brain.Info.Traits)
				{
					IndoctrinationTraitItem indoctrinationTraitItem = GameObjectExtensions.Instantiate(_traitItemTemplate, _traitsContent);
					indoctrinationTraitItem.Configure(trait);
					_traitItems.Add(indoctrinationTraitItem);
				}
			}
			_targetFollower.SetBodyAnimation("Indoctrinate/indoctrinate-start", false);
			_targetFollower.AddBodyAnimation("Indoctrinate/indoctrinate-loop", true, 0f);
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			_buttonHighlightController.SetAsRed();
		}

		protected override void SetActiveStateForMenu(bool state)
		{
			base.SetActiveStateForMenu(state);
			UpdateButtons();
		}

		private void UpdateButtons()
		{
			WorshipperData.SkinAndData skinAndData = GetSkinAndData(_targetFollower.Brain._directInfoAccess);
			_formButton.Interactable = !skinAndData.Invariant;
			_randomiseAppearanceButton.Interactable = !skinAndData.Invariant;
			_variantButton.Interactable = skinAndData.Skin.Count > 1;
			_colourButton.Interactable = !skinAndData.LockColor;
		}

		private void OnNameStartedEditing()
		{
			_editing = true;
			_emptyTextTransform.gameObject.SetActive(false);
		}

		private void OnNameEndedEditing(string text)
		{
			_targetFollower.Brain.Info.Name = text;
			if (text != _targetFollower.Brain.Info.Name)
			{
				_targetFollower.SetBodyAnimation("Indoctrinate/indoctrinate-react", false);
				_targetFollower.AddBodyAnimation("pray", true, 0f);
			}
			_acceptButton.Confirmable = !string.IsNullOrWhiteSpace(text);
			_emptyTextTransform.gameObject.SetActive(!_acceptButton.Confirmable);
			_editing = false;
		}

		private void RandomiseName()
		{
			_nameInputField.text = FollowerInfo.GenerateName();
			OnNameEndedEditing(_nameInputField.text);
		}

		private void OnFormChanged(int formIndex)
		{
			if (formIndex != _targetFollower.Brain.Info.SkinCharacter)
			{
				_targetFollower.Brain.Info.SkinCharacter = formIndex;
				_targetFollower.Brain.Info.SkinColour = 0;
				_targetFollower.Brain.Info.SkinVariation = 0;
				UpdateFollower();
			}
		}

		private void OnVariantChanged(int variantIndex)
		{
			if (variantIndex != _targetFollower.Brain.Info.SkinVariation)
			{
				_targetFollower.Brain.Info.SkinVariation = variantIndex;
				UpdateFollower();
			}
		}

		private void OnColourChanged(int colourIndex)
		{
			if (colourIndex != _targetFollower.Brain.Info.SkinColour)
			{
				_targetFollower.Brain.Info.SkinColour = colourIndex;
				UpdateFollower();
			}
		}

		private void UpdateFollower()
		{
			WorshipperData.SkinAndData skinAndData = GetSkinAndData(_targetFollower.Brain._directInfoAccess);
			_targetFollower.Brain.Info.SkinName = skinAndData.Skin[_targetFollower.Brain.Info.SkinVariation].Skin;
			_targetFollower.SetOutfit(_targetFollower.Outfit.CurrentOutfit, false);
			if (!DataManager.GetFollowerSkinUnlocked(skinAndData.Skin[0].Skin) && !skinAndData.Invariant)
			{
				_followerRenderer.sharedMaterial.DOKill();
				_followerRenderer.sharedMaterial.SetColor("_Color", Color.black);
				return;
			}
			_followerRenderer.sharedMaterial.SetColor("_Color", StaticColors.RedColor);
			_followerRenderer.sharedMaterial.DOKill();
			_followerRenderer.sharedMaterial.DOColor(Color.white, "_Color", 0.5f).SetUpdate(true).SetEase(Ease.OutSine)
				.SetDelay(0.2f);
			if (_spawnLocationAnimator != null)
			{
				_spawnLocationAnimator.SetTrigger("UpdateFollower");
			}
		}

		private WorshipperData.SkinAndData GetSkinAndData(FollowerInfo info)
		{
			if (info.SkinCharacter == -1)
			{
				info.SkinCharacter = WorshipperData.Instance.GetSkinIndexFromName(info.SkinName);
			}
			return WorshipperData.Instance.Characters[info.SkinCharacter];
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable && !_editing)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_acceptButton);
			}
		}

		protected override void OnHideStarted()
		{
			_spawnLocationAnimator.Play("Hidden");
			UIManager.PlayAudio("event:/followers/appearance_accept");
			_targetFollower.Spine.CustomMaterialOverride.Remove(_cachedMaterial);
		}

		protected override void OnHideCompleted()
		{
			_targetFollower.Spine.UseDeltaTime = true;
			Action onIndoctrinationCompleted = OnIndoctrinationCompleted;
			if (onIndoctrinationCompleted != null)
			{
				onIndoctrinationCompleted();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void CreatedTwitchFollower(FollowerInfoSnapshot follower, string twitchViewerID, string twitchFollowerID, string viewerID)
		{
			if (string.IsNullOrEmpty(twitchFollowerViewerID))
			{
				twitchFollowerViewerID = twitchViewerID;
				this.twitchFollowerID = twitchFollowerID;
				_nameInputField.text = follower.Name;
				_targetFollower.Brain.Info.Name = follower.Name;
				_targetFollower.Brain.Info.SkinName = follower.SkinName;
				_targetFollower.Brain.Info.SkinVariation = follower.SkinVariation;
				_targetFollower.Brain.Info.SkinColour = follower.SkinColour;
				_targetFollower.Brain.Info.SkinCharacter = follower.SkinCharacter;
				_targetFollower.Brain.Info.ViewerID = viewerID;
				_createdTwitchFollower = true;
				OverrideDefaultOnce(_acceptButton);
				UpdateFollower();
			}
		}

		private void ShakeAcceptButton()
		{
			_emptyTextTransform.DOKill();
			_acceptButtonRectTransform.DOKill();
			_acceptButtonRectTransform.localPosition = Vector3.zero;
			_emptyTextTransform.localPosition = Vector3.zero;
			_acceptButtonRectTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
			_emptyTextTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		private void OnAcceptButtonSelected()
		{
			_acceptButton.Confirmable = !string.IsNullOrWhiteSpace(_nameInputField.text);
		}

		protected override void OnPush()
		{
			_buttonHighlightController.enabled = false;
		}

		protected override void DoRelease()
		{
			base.DoRelease();
			_buttonHighlightController.enabled = true;
			if (_createdTwitchFollower)
			{
				_formButton.Interactable = false;
				_colourButton.Interactable = false;
				_variantButton.Interactable = false;
				_randomiseAppearanceButton.Interactable = false;
				_randomiseNameButton.Interactable = false;
				_twitchButton.Interactable = false;
				_nameInputField.Interactable = false;
			}
		}
	}
}
