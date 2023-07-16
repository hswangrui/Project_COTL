using System;
using I2.Loc;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace MMTools
{
	[Serializable]
	public class BuyEntry
	{
		[TermsPopup("")]
		public string CharacterName = "-";

		[TermsPopup("")]
		public string TermToSpeak = "-";

		[TextArea(5, 10)]
		public string DisplayText = "";

		public string TermName;

		public string TermCategory = "";

		public string TermDescription;

		[TextArea(5, 10)]
		public string Text;

		public Vector3 Offset = Vector2.zero;

		public Color color;

		public GameObject Speaker;

		public SkeletonAnimation SkeletonData;

		[SpineAnimation("", "SkeletonData", true, false)]
		public string Animation;

		public AudioClip audioClip;

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

		public BuyEntry(GameObject Speaker, string TermToSpeak = "-", string Animation = "talk", AudioClip audioClip = null, UnityEvent Callback = null)
		{
			this.Speaker = Speaker;
			this.TermToSpeak = TermToSpeak;
			this.Animation = Animation;
			this.audioClip = audioClip;
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

		private void Shake()
		{
			Text += "<shake></shake>";
		}

		private void Wiggle()
		{
			Text += "<wiggle></wiggle>";
		}

		private void Bounce()
		{
			Text += "<bounce></bounce>";
		}

		private void Rotation()
		{
			Text += "<rot></rot>";
		}

		private void Swing()
		{
			Text += "<swing></swing>";
		}

		private void Rainbow()
		{
			Text += "<rainb></rainb>";
		}

		private void Speed()
		{
			Text += "<speed=0.2><speed=1>";
		}

		private void Bold()
		{
			Text += "<b></b>";
		}

		private void Italic()
		{
			Text += "<i></i>";
		}

		private void Spirits()
		{
			Text += "<sprite name=\"icon_spirits\">";
		}

		private void Meat()
		{
			Text += "<sprite name=\"icon_meat\">";
		}

		private void Wood()
		{
			Text += "<sprite name=\"icon_wood\">";
		}

		private void Stone()
		{
			Text += "<sprite name=\"icon_stone\">";
		}

		private void Heart()
		{
			Text += "<sprite name=\"icon_heart\">";
		}

		private void HalfHeart()
		{
			Text += "<sprite name=\"icon_heart_half\">";
		}

		private void BlueHeart()
		{
			Text += "<sprite name=\"icon_blueheart\">";
		}

		private void BlueHeartHalf()
		{
			Text += "<sprite name=\"icon_blueheart_half\">";
		}

		private void TimeToken()
		{
			Text += "<sprite name=\"icon_timetoken\">";
		}

		private void Flowers()
		{
			Text += "<sprite name=\"icon_flowers\">";
		}

		private void StainedGlass()
		{
			Text += "<sprite name=\"icon_stainedglass\">";
		}

		private void Bones()
		{
			Text += "<sprite name=\"icon_bones\">";
		}

		private void BlackGold()
		{
			Text += "<sprite name=\"icon_blackgold\">";
		}

		private void Brambles()
		{
			Text += "<sprite name=\"icon_brambles\">";
		}

		private void SetColor()
		{
			Debug.Log("COLOR!");
			Text = Text + "<#" + ColorUtility.ToHtmlStringRGB(color) + "></color>";
		}
	}
}
