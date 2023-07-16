using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedBlueGames.Tools.TextTyper
{
	public class TextTyperTester : MonoBehaviour
	{
		[SerializeField]
		private AudioClip printSoundEffect;

		[Header("UI References")]
		[SerializeField]
		private Button printNextButton;

		[SerializeField]
		private Button printNoSkipButton;

		private Queue<string> dialogueLines = new Queue<string>();

		[SerializeField]
		[Tooltip("The text typer element to test typing with")]
		private TextTyper testTextTyper;

		public void Start()
		{
			testTextTyper.PrintCompleted.AddListener(HandlePrintCompleted);
			testTextTyper.CharacterPrinted.AddListener(HandleCharacterPrinted);
			dialogueLines.Enqueue("Hello! <animation=bounce>My name</animation> is... <delay=0.5>NPC</delay>. Got it, <i>bub</i>?");
			dialogueLines.Enqueue("You can <b>use</b> <i>uGUI</i> <size=40>text</size> <size=20>tag</size> and <color=#ff0000ff>color</color> tag <color=#00ff00ff>like this</color>.");
			dialogueLines.Enqueue("bold <b>text</b> test <b>bold</b> text <b>test</b>");
			dialogueLines.Enqueue("You can <size=40>size 40</size> <animation=crazyflip>and</animation> <size=20>size 20</size>");
			dialogueLines.Enqueue("You can <color=#ff0000ff>color</color> tag <color=#00ff00ff>like this</color>.");
			dialogueLines.Enqueue("Sample Shake Animations: <anim=lightrot>Light Rotation</anim>, <anim=lightpos>Light Position</anim>, <anim=fullshake>Full Shake</anim>\nSample Curve Animations: <animation=slowsine>Slow Sine</animation>, <animation=bounce>Bounce</animation>, <animation=crazyflip>Crazy Flip</animation>");
			ShowScript();
		}

		public void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				HandlePrintNextClicked();
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				RichTextTag richTextTag = RichTextTag.ParseNext("blah<color=red>boo</color");
				LogTag(richTextTag);
				richTextTag = RichTextTag.ParseNext("<color=blue>blue</color");
				LogTag(richTextTag);
				richTextTag = RichTextTag.ParseNext("No tag in here");
				LogTag(richTextTag);
				richTextTag = RichTextTag.ParseNext("No <color=blueblue</color tag here either");
				LogTag(richTextTag);
				richTextTag = RichTextTag.ParseNext("This tag is a closing tag </bold>");
				LogTag(richTextTag);
			}
		}

		private void HandlePrintNextClicked()
		{
			if (testTextTyper.IsSkippable() && testTextTyper.IsTyping)
			{
				testTextTyper.Skip();
			}
			else
			{
				ShowScript();
			}
		}

		private void HandlePrintNoSkipClicked()
		{
			ShowScript();
		}

		private void ShowScript()
		{
			if (dialogueLines.Count > 0)
			{
				testTextTyper.TypeText(dialogueLines.Dequeue());
			}
		}

		private void LogTag(RichTextTag tag)
		{
			if (tag != null)
			{
				Debug.Log("Tag: " + tag.ToString());
			}
		}

		private void HandleCharacterPrinted(string printedCharacter)
		{
			if (!(printedCharacter == " ") && !(printedCharacter == "\n"))
			{
				AudioSource audioSource = GetComponent<AudioSource>();
				if (audioSource == null)
				{
					audioSource = base.gameObject.AddComponent<AudioSource>();
				}
				audioSource.clip = printSoundEffect;
				audioSource.Play();
			}
		}

		private void HandlePrintCompleted()
		{
			Debug.Log("TypeText Complete");
		}
	}
}
