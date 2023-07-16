using System;
using System.Collections.Generic;
using FMODUnity;
using I2.Loc;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace MMTools
{
	[Serializable]
	public class ConversationEntry
	{
		[TermsPopup("")]
		public string CharacterName = "-";

		[TermsPopup("")]
		public string TermToSpeak = "-";

		[TextArea(5, 10)]
		public string DisplayText = "";

		[HideInInspector]
		public string TermName;

		[HideInInspector]
		public string TermCategory = "";

		[HideInInspector]
		public string TermDescription;

		[HideInInspector]
		public string Text;

		public GameObject Speaker;

		public bool SpeakerIsPlayer;

		public Vector3 Offset = Vector2.zero;

		public bool SetZoom;

		public float Zoom = 8f;

		public SkeletonAnimation SkeletonData;

		[SpineAnimation("", "SkeletonData", true, false)]
		public string Animation;

		public bool LoopAnimation = true;

		[SpineAnimation("", "SkeletonData", true, false)]
		public string DefaultAnimation = "";

		[EventRef]
		public string soundPath = string.Empty;

		public float pitchValue = -1f;

		public float vibratoValue = -1f;

		public int followerID = -1;

		public UnityEvent Callback;

		private string TermString
		{
			get
			{
				if (LocalizationManager.Sources.Count > 0 && TermToSpeak != "-" && TermToSpeak != "")
				{
					DisplayText = LocalizationManager.Sources[0].GetTranslation(TermToSpeak);
				}
				else
				{
					DisplayText = "";
				}
				return "";
			}
		}

		public ConversationEntry(GameObject Speaker, string TermToSpeak = "-", string Animation = "talk", string soundPath = "", UnityEvent Callback = null)
		{
			this.Speaker = Speaker;
			this.TermToSpeak = TermToSpeak;
			this.Animation = Animation;
			this.soundPath = soundPath;
			this.Callback = Callback;
		}

		private void AddTerm()
		{
			LanguageSourceData languageSourceData = LocalizationManager.Sources[0];
			TermData termData = languageSourceData.AddTerm("Conversation_" + TermCategory + "/ " + TermName, eTermType.Text);
			termData.SetTranslation(0, Text);
			termData.Description = TermDescription;
			languageSourceData.UpdateDictionary();
			TermToSpeak = termData.Term;
		}

		internal static ConversationEntry Clone(ConversationEntry conversationEntry)
		{
			return new ConversationEntry(conversationEntry.Speaker, conversationEntry.TermToSpeak, conversationEntry.Animation, conversationEntry.soundPath)
			{
				CharacterName = conversationEntry.CharacterName,
				SkeletonData = conversationEntry.SkeletonData,
				Offset = conversationEntry.Offset,
				SpeakerIsPlayer = conversationEntry.SpeakerIsPlayer,
				LoopAnimation = conversationEntry.LoopAnimation,
				SetZoom = conversationEntry.SetZoom,
				Zoom = conversationEntry.Zoom
			};
		}

		internal static List<ConversationEntry> CloneList(List<ConversationEntry> List)
		{
			List<ConversationEntry> list = new List<ConversationEntry>();
			foreach (ConversationEntry item in List)
			{
				list.Add(Clone(item));
			}
			return list;
		}
	}
}
