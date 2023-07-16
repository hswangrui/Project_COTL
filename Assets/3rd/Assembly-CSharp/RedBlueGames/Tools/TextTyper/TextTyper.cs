using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RedBlueGames.Tools.TextTyper
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TextTyper : MonoBehaviour
	{
		[Serializable]
		public class CharacterPrintedEvent : UnityEvent<string>
		{
		}

		private const float PrintDelaySetting = 0.015f;

		private const float PunctuationDelayMultiplier = 8f;

		private static readonly List<char> punctutationCharacters = new List<char> { '.', ',', '!', '?' };

		[SerializeField]
		[Tooltip("The library of ShakePreset animations that can be used by this component.")]
		private ShakeLibrary shakeLibrary;

		[SerializeField]
		[Tooltip("The library of CurvePreset animations that can be used by this component.")]
		private CurveLibrary curveLibrary;

		[SerializeField]
		[Tooltip("Event that's called when the text has finished printing.")]
		private UnityEvent printCompleted = new UnityEvent();

		[SerializeField]
		[Tooltip("Event called when a character is printed. Inteded for audio callbacks.")]
		private CharacterPrintedEvent characterPrinted = new CharacterPrintedEvent();

		private TextMeshProUGUI textComponent;

		private float defaultPrintDelay;

		private List<float> characterPrintDelays;

		private List<TextAnimation> animations;

		private Coroutine typeTextCoroutine;

		private byte[] Alphas = new byte[500];

		private TMP_TextInfo textInfo;

		private TMP_CharacterInfo charInfo;

		private int characterCount;

		private int vertexIndex;

		private int materialIndex;

		private Color32[] newVertexColors;

		[Range(0.01f, 1f)]
		private float AlphaSpeed = 0.25f;

		public UnityEvent PrintCompleted => printCompleted;

		public CharacterPrintedEvent CharacterPrinted => characterPrinted;

		public bool IsTyping => typeTextCoroutine != null;

		public TextMeshProUGUI TextComponent
		{
			get
			{
				if (textComponent == null)
				{
					textComponent = GetComponent<TextMeshProUGUI>();
				}
				return textComponent;
			}
		}

		public TextMeshProUGUI GetTextComponent()
		{
			return TextComponent;
		}

		public void TypeText(string text, float printDelay = -1f)
		{
			CleanupCoroutine();
			TextAnimation[] components = GetComponents<TextAnimation>();
			for (int i = 0; i < components.Length; i++)
			{
				UnityEngine.Object.Destroy(components[i]);
			}
			defaultPrintDelay = ((printDelay > 0f) ? printDelay : 0.015f);
			ProcessCustomTags(text);
			typeTextCoroutine = StartCoroutine(TypeTextCharByChar(text));
		}

		public void Skip()
		{
			CleanupCoroutine();
			TextComponent.maxVisibleCharacters = int.MaxValue;
			UpdateMeshAndAnims();
			OnTypewritingComplete();
		}

		public bool IsSkippable()
		{
			return IsTyping;
		}

		private void CleanupCoroutine()
		{
			if (typeTextCoroutine != null)
			{
				StopCoroutine(typeTextCoroutine);
				typeTextCoroutine = null;
			}
		}

		private IEnumerator TypeTextCharByChar(string text)
		{
			string taglessText = TextTagParser.RemoveAllTags(text);
			int totalPrintedChars = taglessText.Length;
			int currPrintedChars = 1;
			TextComponent.text = TextTagParser.RemoveCustomTags(text);
			ResetAlphas();
			do
			{
				TextComponent.maxVisibleCharacters = currPrintedChars;
				UpdateMeshAndAnims();
				OnCharacterPrinted(taglessText[currPrintedChars - 1].ToString());
				yield return new WaitForSeconds(characterPrintDelays[currPrintedChars - 1]);
				int num = currPrintedChars + 1;
				currPrintedChars = num;
			}
			while (currPrintedChars <= totalPrintedChars);
			typeTextCoroutine = null;
			OnTypewritingComplete();
		}

		private void ResetAlphas()
		{
			int num = -1;
			while (++num < 500)
			{
				Alphas[num] = 0;
			}
			textInfo = TextComponent.textInfo;
		}

		private void LateUpdate()
		{
			textInfo = TextComponent.textInfo;
			characterCount = textInfo.characterCount;
			for (int i = 0; i < characterCount; i++)
			{
				if (textInfo.characterInfo[i].isVisible)
				{
					charInfo = textInfo.characterInfo[i];
					vertexIndex = charInfo.vertexIndex;
					materialIndex = charInfo.materialReferenceIndex;
					newVertexColors = textInfo.meshInfo[materialIndex].colors32;
					Alphas[i] += (byte)((float)(255 - Alphas[i]) * AlphaSpeed);
					newVertexColors[vertexIndex].a = Alphas[i];
					newVertexColors[vertexIndex + 1].a = Alphas[i];
					newVertexColors[vertexIndex + 2].a = Alphas[i];
					newVertexColors[vertexIndex + 3].a = Alphas[i];
				}
			}
			TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}

		private void UpdateMeshAndAnims()
		{
			TextComponent.ForceMeshUpdate();
			for (int i = 0; i < animations.Count; i++)
			{
				animations[i].AnimateAllChars();
			}
		}

		private void ProcessCustomTags(string text)
		{
			characterPrintDelays = new List<float>(text.Length);
			animations = new List<TextAnimation>();
			List<TextTagParser.TextSymbol> list = TextTagParser.CreateSymbolListFromText(text);
			int num = 0;
			int firstChar = 0;
			string text2 = "";
			float num2 = defaultPrintDelay;
			foreach (TextTagParser.TextSymbol item in list)
			{
				if (item.IsTag)
				{
					if (item.Tag.TagType == "delay")
					{
						num2 = ((!item.Tag.IsClosingTag) ? item.GetFloatParameter(defaultPrintDelay) : defaultPrintDelay);
					}
					else
					{
						if (!(item.Tag.TagType == "anim") && !(item.Tag.TagType == "animation"))
						{
							continue;
						}
						if (item.Tag.IsClosingTag)
						{
							TextAnimation textAnimation = null;
							if (IsAnimationShake(text2))
							{
								textAnimation = base.gameObject.AddComponent<ShakeAnimation>();
								((ShakeAnimation)textAnimation).LoadPreset(shakeLibrary, text2);
							}
							else if (IsAnimationCurve(text2))
							{
								textAnimation = base.gameObject.AddComponent<CurveAnimation>();
								((CurveAnimation)textAnimation).LoadPreset(curveLibrary, text2);
							}
							textAnimation.SetCharsToAnimate(firstChar, num - 1);
							textAnimation.enabled = true;
							animations.Add(textAnimation);
						}
						else
						{
							firstChar = num;
							text2 = item.Tag.Parameter;
						}
					}
				}
				else
				{
					num++;
					if (punctutationCharacters.Contains(item.Character))
					{
						characterPrintDelays.Add(num2 * 8f);
					}
					else
					{
						characterPrintDelays.Add(num2);
					}
				}
			}
		}

		private bool IsAnimationShake(string animName)
		{
			return shakeLibrary.ContainsKey(animName);
		}

		private bool IsAnimationCurve(string animName)
		{
			return curveLibrary.ContainsKey(animName);
		}

		private void OnCharacterPrinted(string printedCharacter)
		{
			if (CharacterPrinted != null)
			{
				CharacterPrinted.Invoke(printedCharacter);
			}
		}

		private void OnTypewritingComplete()
		{
			if (PrintCompleted != null)
			{
				PrintCompleted.Invoke();
			}
		}
	}
}
