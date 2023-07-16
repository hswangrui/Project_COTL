using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using Rewired;
using Spine.Unity;
using src.Extensions;
using TMPro;
using Unify.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using WebSocketSharp;

namespace MMTools
{
	public class MMConversation : MonoBehaviour
	{
		public delegate void ConversationNew(bool SetPlayerInactive = true, bool SnapLetterBox = false, bool ShowLetterBox = true);

		public delegate void ConversationNext(GameObject Speaker, float Zoom = 8f);

		public delegate void ConversationEnd(bool SetPlayerToIdle = true, bool ShowHUD = true);

		[Serializable]
		public class TermAndColor
		{
			[TermsPopup("")]
			public string Name = "-";

			public Color Color;
		}

		private EventInstance talkSoundInstance;

		public TMP_Text TitleText;

		public TextAnimatorPlayer TextPlayer;

		public SpeechBubble SpeechBubble;

		public DialogueWheel DialogueWheel;

		private static GameObject Instance;

		public static ConversationObject CURRENT_CONVERSATION;

		private Queue<string> dialogueLines = new Queue<string>();

		private static int Position;

		public static MMConversation mmConversation;

		private Tween currentCloseFadeTween;

		public Player player;

		public static bool isPlaying = false;

		private static bool CallOnConversationEnd = true;

		private static bool SetPlayerIdleOnComplete;

		public static bool ControlCamera = true;

		public static bool PlayVO = true;

		[Header("Next Arrow")]
		public GameObject NextArrowContainer;

		public RectTransform NextArrowRectTransform;

		public static bool isBark = false;

		private SkeletonAnimation SpeakerSpine;

		private string CachedAnimation = "";

		public List<TermAndColor> TermsAndColors = new List<TermAndColor>();

		public static event ConversationNew OnConversationNew;

		public static event ConversationNext OnConversationNext;

		public static event ConversationEnd OnConversationEnd;

		public static void Play(ConversationObject ConversationObject, bool CallOnConversationEnd = true, bool SetPlayerInactiveOnStart = true, bool SetPlayerIdleOnComplete = true, bool Translate = true, bool ShowLetterBox = true, bool SnapLetterBox = false, bool showControlPrompt = true)
		{
			if (PlayerFarming.Location != FollowerLocation.Base && PlayerFarming.Location != 0)
			{
				SimulationManager.Pause();
			}
			CURRENT_CONVERSATION = ConversationObject;
			isBark = false;
			ControlCamera = true;
			PlayVO = true;
			if (Instance == null)
			{
				Instance = UnityEngine.Object.Instantiate(Resources.Load("MMConversation/Conversation")) as GameObject;
				mmConversation = Instance.GetComponent<MMConversation>();
				mmConversation.TextPlayer.StopShowingText();
				mmConversation.TextPlayer.ShowText("");
			}
			Instance.SetActive(true);
			mmConversation.SpeechBubble.gameObject.SetActive(false);
			mmConversation.DialogueWheel.gameObject.SetActive(false);
			MMConversation.CallOnConversationEnd = CallOnConversationEnd;
			MMConversation.SetPlayerIdleOnComplete = SetPlayerIdleOnComplete;
			isPlaying = true;
			mmConversation.TextPlayer.textAnimator.timeScale = TextAnimator.TimeScale.Scaled;
			mmConversation.NextArrowContainer.SetActive(showControlPrompt);
			ConversationNew onConversationNew = MMConversation.OnConversationNew;
			if (onConversationNew != null)
			{
				onConversationNew(SetPlayerInactiveOnStart, SnapLetterBox, ShowLetterBox);
			}
			mmConversation.player = RewiredInputManager.MainPlayer;
			CanvasGroup canvasgGroup = mmConversation.GetComponent<CanvasGroup>();
			canvasgGroup.alpha = 0f;
			mmConversation.SpeakerSpine = null;
			Position = 0;
			mmConversation.SpeechBubble.Reset(mmConversation.TextPlayer.GetComponent<TextMeshProUGUI>());
			mmConversation.TextPlayer.onTextShowed.AddListener(mmConversation.OnPrintCompleted);
			mmConversation.DialogueWheel.gameObject.SetActive(false);
			mmConversation.ShowNextLine(false);
			Position = -1;
			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
			GameManager.GetInstance().StartCoroutine(WaitForCameraToStop(delegate
			{
				mmConversation.ShowNextLine(true);
				canvasgGroup.DOFade(1f, 0.75f).SetUpdate(true);
				mmConversation.SpeechBubble.gameObject.SetActive(true);
				AudioManager.Instance.PlayOneShot("event:/ui/conversation_start");
			}));
		}

		private static IEnumerator WaitForCameraToStop(Action callback)
		{
			while (isPlaying && GameManager.GetInstance().CamFollowTarget.IsMoving)
			{
				yield return null;
			}
			if (callback != null)
			{
				callback();
			}
		}

		private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
			Instance = null;
			mmConversation = null;
			isPlaying = false;
			CURRENT_CONVERSATION = null;
		}

		public static void UseDeltaTime(bool Toggle)
		{
			if (mmConversation != null)
			{
				mmConversation.TextPlayer.textAnimator.timeScale = (Toggle ? TextAnimator.TimeScale.Scaled : TextAnimator.TimeScale.Unscaled);
			}
		}

		public static void PlayBark(ConversationObject ConversationObject, bool Translate = true)
		{
			CURRENT_CONVERSATION = ConversationObject;
			isBark = true;
			isPlaying = true;
			if (Instance == null)
			{
				Instance = UnityEngine.Object.Instantiate(Resources.Load("MMConversation/Conversation")) as GameObject;
				mmConversation = Instance.GetComponent<MMConversation>();
				mmConversation.TextPlayer.StopShowingText();
				mmConversation.TextPlayer.ShowText("");
			}
			else
			{
				Instance.SetActive(true);
			}
			mmConversation.player = null;
			Position = -1;
			mmConversation.SpeechBubble.Reset(mmConversation.TextPlayer.GetComponent<TextMeshProUGUI>());
			mmConversation.TextPlayer.onTextShowed.AddListener(mmConversation.OnPrintCompleted);
			mmConversation.DialogueWheel.gameObject.SetActive(false);
			mmConversation.ShowNextLine(true);
			mmConversation.NextArrowContainer.SetActive(false);
			mmConversation.GetComponent<CanvasGroup>().DOFade(1f, 0.25f);
		}

		private void OnPrintCompleted()
		{
			if (Position >= CURRENT_CONVERSATION.Entries.Count - 1 || CURRENT_CONVERSATION.Entries.Count == 1)
			{
				if (CURRENT_CONVERSATION.Responses != null && CURRENT_CONVERSATION.Responses.Count > 0)
				{
					DialogueWheel.gameObject.SetActive(true);
					DialogueWheel.OnGiveAnswer += OnGiveAnswer;
					SpeechBubble.HidePrompt();
				}
				else if (CURRENT_CONVERSATION.DoctrineResponses != null && CURRENT_CONVERSATION.DoctrineResponses.Count > 0)
				{
					MonoSingleton<UIManager>.Instance.DoctrineChoicesMenuTemplate.Instantiate().Show();
					SpeechBubble.HidePrompt();
				}
			}
		}

		private void OnGiveAnswer(int Answer)
		{
			if (CURRENT_CONVERSATION == null || CURRENT_CONVERSATION.Responses == null)
			{
				return;
			}
			Action CallBack = null;
			object obj;
			if (CURRENT_CONVERSATION.Responses.Count <= Answer)
			{
				obj = null;
			}
			else
			{
				Response response = CURRENT_CONVERSATION.Responses[Answer];
				obj = ((response != null) ? response.EventCallBack : null);
			}
			UnityEvent EventCallBack = (UnityEvent)obj;
			if (CURRENT_CONVERSATION.Responses.Count > Answer && CURRENT_CONVERSATION.Responses[Answer] != null && CURRENT_CONVERSATION.Responses[Answer].ActionCallBack != null)
			{
				CallBack = CURRENT_CONVERSATION.Responses[Answer].ActionCallBack;
			}
			Close(true, delegate
			{
				if (CallBack != null)
				{
					CallBack();
				}
				UIManager.PlayAudio("event:/ui/conversation_change_page");
				UnityEvent unityEvent = EventCallBack;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
			});
		}

		private void ShowNextLine(bool playVOandAnim)
		{
			if (CURRENT_CONVERSATION == null || CURRENT_CONVERSATION.Entries == null)
			{
				return;
			}
			if (talkSoundInstance.isValid() && !isBark)
			{
				AudioManager.Instance.StopLoop(talkSoundInstance);
			}
			if (playVOandAnim)
			{
				UIManager.PlayAudio("event:/ui/conversation_change_page");
				Position++;
			}
			if (Position >= CURRENT_CONVERSATION.Entries.Count)
			{
				ResetSpineAnimation();
				if ((CURRENT_CONVERSATION.Responses == null || CURRENT_CONVERSATION.Responses.Count <= 0) && (CURRENT_CONVERSATION.DoctrineResponses == null || CURRENT_CONVERSATION.DoctrineResponses.Count <= 0))
				{
					SpeechBubble.ClearTarget();
					Close();
				}
				return;
			}
			GameObject gameObject = (CURRENT_CONVERSATION.Entries[Position].SpeakerIsPlayer ? PlayerFarming.Instance.gameObject : CURRENT_CONVERSATION.Entries[Position].Speaker);
			if (playVOandAnim)
			{
				UpdateText();
				if (gameObject != null)
				{
					object speaker;
					if (!(gameObject.GetComponentInChildren<SkeletonAnimation>() != null))
					{
						SkeletonAnimation skeletonData = CURRENT_CONVERSATION.Entries[Position].SkeletonData;
						speaker = (((object)skeletonData != null) ? skeletonData.gameObject : null);
					}
					else
					{
						speaker = gameObject;
					}
					SetSpineAnimation((GameObject)speaker, CURRENT_CONVERSATION.Entries[Position].Animation, CURRENT_CONVERSATION.Entries[Position].LoopAnimation, CURRENT_CONVERSATION.Entries[Position].DefaultAnimation, playVOandAnim);
					if (SpeakerSpine != null)
					{
						SpeechBubble.SetTarget(SpeakerSpine.transform, CURRENT_CONVERSATION.Entries[Position].Offset);
					}
					else
					{
						SpeechBubble.SetTarget(gameObject.transform, CURRENT_CONVERSATION.Entries[Position].Offset);
					}
				}
				string soundPath = ((!CURRENT_CONVERSATION.Entries[Position].soundPath.IsNullOrEmpty() || !(CURRENT_CONVERSATION.Entries[Position].SkeletonData != null)) ? CURRENT_CONVERSATION.Entries[Position].soundPath : GetFallBackVO(CURRENT_CONVERSATION.Entries[Position].SkeletonData));
				if (PlayVO)
				{
					if ((float)CURRENT_CONVERSATION.Entries[Position].followerID != -1f && FollowerManager.GetSpecialFollowerFallback(CURRENT_CONVERSATION.Entries[Position].followerID) != null)
					{
						soundPath = FollowerManager.GetSpecialFollowerFallback(CURRENT_CONVERSATION.Entries[Position].followerID);
					}
					if (gameObject != null)
					{
						talkSoundInstance = AudioManager.Instance.CreateLoop(soundPath, gameObject, true);
					}
					else
					{
						talkSoundInstance = AudioManager.Instance.CreateLoop(soundPath, true);
					}
					if (CURRENT_CONVERSATION.Entries[Position].pitchValue != -1f)
					{
						talkSoundInstance.setParameterByName("follower_pitch", CURRENT_CONVERSATION.Entries[Position].pitchValue);
					}
					if (CURRENT_CONVERSATION.Entries[Position].vibratoValue != -1f)
					{
						talkSoundInstance.setParameterByName("follower_vibrato", CURRENT_CONVERSATION.Entries[Position].vibratoValue);
					}
				}
			}
			if (CURRENT_CONVERSATION.Entries[Position].Callback != null && CURRENT_CONVERSATION.Entries[Position].Callback.GetPersistentEventCount() > 0)
			{
				CURRENT_CONVERSATION.Entries[Position].Callback.Invoke();
			}
			if (MMConversation.OnConversationNext != null && !isBark)
			{
				MMConversation.OnConversationNext(ControlCamera ? gameObject : null, CURRENT_CONVERSATION.Entries[Position].SetZoom ? CURRENT_CONVERSATION.Entries[Position].Zoom : UnityEngine.Random.Range(7.5f, 8.5f));
			}
		}

		public static string GetFallBackVO(int followerID)
		{
			switch (followerID)
			{
			case 666:
				return "event:/dialogue/followers/boss/fol_deathcat";
			case 99990:
				return "event:/dialogue/followers/boss/fol_leshy";
			case 99991:
				return "event:/dialogue/followers/boss/fol_heket";
			case 99992:
				return "event:/dialogue/followers/boss/fol_kallamar";
			case 99993:
				return "event:/dialogue/followers/boss/fol_shamura";
			case 99994:
				return "event:/dialogue/followers/boss/fol_guardian_b";
			case 99995:
				return "event:/dialogue/followers/boss/fol_guardian_a";
			default:
				return null;
			}
		}

		private string GetFallBackVO(SkeletonAnimation spine)
		{
			Debug.Log("spine.skeletonDataAsset.name: " + spine.skeletonDataAsset.name.Colour(Color.red));
			switch (spine.skeletonDataAsset.name)
			{
			case "ForestCultLeader_SkeletonData":
				return "event:/dialogue/dun1_cult_leader_leshy/standard_leshy";
			case "FrogCultLeader_SkeletonData":
			case "FrogCultLeader":
				return "event:/dialogue/dun2_cult_leader_heket/standard_heket";
			case "JellyCultLeader":
				return "event:/dialogue/dun3_cult_leader_kallamar/standard_kallamar";
			case "SpiderCultLeader":
				return "event:/dialogue/dun4_cult_leader_shamura/standard_shamura";
			case "RatNPC":
				return "event:/dialogue/ratau/standard_ratau";
			case "MidasNPC":
				return "event:/dialogue/midas/standard_midas";
			case "Ratoo":
				return "event:/dialogue/ratoo/standard_ratoo";
			case "Fox_SkeletonData":
				return "event:/dialogue/the_night/standard_the_night";
			case "DeathBig":
			case "DeathCat_Spirit":
			case "DeathBig_SkeletonData":
			case "DeathCat_Spirit_SkeletonData":
			case "DeathCat_Boss":
			case "DeathCat_Boss_SkeletonData":
				return "event:/dialogue/death_cat/standard_death";
			case "Follower":
				return "event:/dialogue/followers/general_talk";
			case "MushroomTraveller_SkeletonData":
				return "event:/dialogue/sozo/sozo_standard";
			case "Haro_SkeletonData":
				return "event:/dialogue/haro/standard_haro";
			case "MysticShopKeeper_SkeletonData":
				return "event:/dialogue/msk/standard_msk";
			case "RelicSeller_SkeletonData":
				return "event:/dialogue/chemach/standard_chemach";
			default:
				Debug.Log("NO fallback for: " + spine.skeletonDataAsset.name);
				return string.Empty;
			}
		}

		public void Update()
		{
			if (MonoSingleton<UIManager>.Instance.ForceBlockMenus || MMTransition.IsPlaying || GameManager.InMenu || UIMenuBase.ActiveMenus.Count > 0 || MonoSingleton<UIManager>.Instance.IsPaused)
			{
				if (isBark)
				{
					Close();
				}
				return;
			}
			if (player == null)
			{
				player = RewiredInputManager.MainPlayer;
			}
			if (player != null && !isBark && InputManager.Gameplay.GetAdvanceDialogueButtonDown() && (CURRENT_CONVERSATION.DoctrineResponses == null || CURRENT_CONVERSATION.DoctrineResponses.Count <= 0))
			{
				HandleNextClicked();
			}
			if (TextPlayer != null && TextPlayer.textAnimator != null && DialogueWheel != null && CURRENT_CONVERSATION != null && TextPlayer.textAnimator.allLettersShown && !DialogueWheel.gameObject.activeSelf && CURRENT_CONVERSATION.Responses != null && CURRENT_CONVERSATION.Responses.Count > 0)
			{
				OnPrintCompleted();
			}
		}

		private void HandleNextClicked()
		{
			if (!DialogueWheel.gameObject.activeSelf && SpeechBubble.gameObject.activeSelf)
			{
				if (!TextPlayer.textAnimator.allLettersShown)
				{
					TextPlayer.SkipTypewriter();
				}
				else
				{
					TextPlayer.StopShowingText();
					ShowNextLine(true);
				}
				NextArrowRectTransform.DOKill();
				NextArrowRectTransform.anchoredPosition = new Vector2(0f, -10f);
				NextArrowRectTransform.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutBack);
			}
		}

		public static ConversationObject CreateConversation(List<ConversationEntry> Entries, List<Response> Responses, Action CallBack)
		{
			return new ConversationObject(Entries, Responses, CallBack);
		}

		public void Close(bool resetAnimation = true, Action CustomCallbacks = null, bool stopAudio = true)
		{
			if (!(this != null))
			{
				return;
			}
			if (isBark)
			{
				if (stopAudio)
				{
					AudioManager.Instance.StopLoop(talkSoundInstance);
				}
				CanvasGroup component = GetComponent<CanvasGroup>();
				currentCloseFadeTween = component.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(delegate
				{
					DoClose(resetAnimation, CustomCallbacks, true);
					currentCloseFadeTween = null;
				});
				currentCloseFadeTween.Play();
			}
			else
			{
				DoClose(resetAnimation, CustomCallbacks);
			}
		}

		public void FinishCloseFadeTweenByForce()
		{
			if (currentCloseFadeTween != null && isPlaying)
			{
				currentCloseFadeTween.Complete();
			}
		}

		private void DoClose(bool resetAnimation = true, Action CustomCallbacks = null, bool _isBark = false)
		{
			if (_isBark == isBark)
			{
				if (isBark && resetAnimation)
				{
					ResetSpineAnimation();
				}
				TextPlayer.onTextShowed.RemoveAllListeners();
				DialogueWheel.OnGiveAnswer -= OnGiveAnswer;
				if (CURRENT_CONVERSATION != null && CURRENT_CONVERSATION.CallBack != null)
				{
					CURRENT_CONVERSATION.CallBack();
				}
				SimulationManager.UnPause();
				CURRENT_CONVERSATION = null;
				SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
				if (talkSoundInstance.isValid() && !_isBark)
				{
					AudioManager.Instance.StopLoop(talkSoundInstance);
				}
				if (!isBark)
				{
					AudioManager.Instance.PlayOneShot("event:/ui/conversation_end");
				}
				if (CallOnConversationEnd && MMConversation.OnConversationEnd != null && !_isBark)
				{
					MMConversation.OnConversationEnd(SetPlayerIdleOnComplete);
				}
				if (Instance != null)
				{
					Instance.SetActive(false);
				}
				if (CustomCallbacks != null)
				{
					CustomCallbacks();
				}
				isPlaying = false;
			}
		}

		private void SetSpineAnimation(GameObject Speaker, string Animation, bool Loop, string DefaultAnimation, bool playAnimation)
		{
			ResetSpineAnimation();
			if (Speaker == null)
			{
				SpeakerSpine = null;
				return;
			}
			SpeakerSpine = CURRENT_CONVERSATION.Entries[Position].SkeletonData;
			if (SpeakerSpine == null)
			{
				SpeakerSpine = Speaker.GetComponentInChildren<SkeletonAnimation>();
			}
			if (SpeakerSpine != null && SpeakerSpine.AnimationState != null && playAnimation)
			{
				CachedAnimation = ((DefaultAnimation == "") ? SpeakerSpine.AnimationName : DefaultAnimation);
				if (SpeakerSpine.AnimationState.Data.SkeletonData.FindAnimation(Animation) != null)
				{
					SpeakerSpine.AnimationState.SetAnimation(0, Animation, Loop);
				}
				if (!string.IsNullOrEmpty(DefaultAnimation))
				{
					SpeakerSpine.AnimationState.AddAnimation(0, DefaultAnimation, true, 0f);
				}
				else if (!Loop && SpeakerSpine.AnimationState.Data.SkeletonData.FindAnimation("idle") != null)
				{
					SpeakerSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
				}
				else if (!Loop && SpeakerSpine.AnimationState.Data.SkeletonData.FindAnimation("animation") != null)
				{
					SpeakerSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
				}
			}
		}

		private void ResetSpineAnimation()
		{
			if (SpeakerSpine != null && SpeakerSpine.AnimationState != null && !string.IsNullOrEmpty(CachedAnimation))
			{
				SpeakerSpine.AnimationState.SetAnimation(0, CachedAnimation, true);
			}
		}

		private void PlayAudioClip(AudioClip audioClip)
		{
		}

		private string AddCharacterName()
		{
			string characterName = CURRENT_CONVERSATION.Entries[Position].CharacterName;
			if (characterName != "-" && characterName != "")
			{
				string translation = LocalizationManager.GetTranslation(characterName);
				return "<size=35>" + ((translation == string.Empty) ? characterName : translation).Bold() + "</size> <size=5>\n\n</size>";
			}
			return "";
		}

		private string AddNameColors(string text)
		{
			foreach (TermAndColor termsAndColor in TermsAndColors)
			{
				if (termsAndColor.Name != "-" && termsAndColor.Name != "")
				{
					text = text.Bold().Colour(termsAndColor.Color);
				}
			}
			return text;
		}

		private void OnEnable()
		{
			LocalizationManager.OnLocalizeEvent += UpdateText;
		}

		private void OnDisable()
		{
			isPlaying = false;
			LocalizationManager.OnLocalizeEvent -= UpdateText;
		}

		private void UpdateText()
		{
			string translation = LocalizationManager.GetTranslation(CURRENT_CONVERSATION.Entries[Position].TermToSpeak);
			string text = AddCharacterName();
			text = AddNameColors(text);
			if (TitleText != null)
			{
				TitleText.text = text;
			}
			TextPlayer.ShowText(translation);
		}
	}
}
