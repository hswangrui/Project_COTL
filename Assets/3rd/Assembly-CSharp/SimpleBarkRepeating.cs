using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;

public class SimpleBarkRepeating : BaseMonoBehaviour
{
	private static List<SimpleBarkRepeating> Barks = new List<SimpleBarkRepeating>();

	public float ActivateDistance = 4f;

	public Vector3 ActivateOffset = Vector3.zero;

	private bool Spoken;

	public List<ConversationEntry> Entries;

	public int StartingIndex;

	private int RandomBark;

	public bool IsSpeaking;

	private GameObject Player;

	public bool Translate = true;

	private static SimpleBarkRepeating Closest;

	private float ClosestDist;

	private static int Frame = -1;

	private ConversationObject conversationObject;

	private void Start()
	{
		RandomBark = Random.Range(0, Entries.Count);
		CreateConversationObject();
	}

	private void OnEnable()
	{
		Barks.Add(this);
	}

	public void AddEntry()
	{
		if (Entries.Count <= 0)
		{
			Entries.Add(new ConversationEntry(null, ""));
		}
		else
		{
			Entries.Add(ConversationEntry.Clone(Entries[Entries.Count - 1]));
		}
	}

	private void IncrementEntry()
	{
		if (Entries.Count <= 0)
		{
			return;
		}
		int num = StartingIndex;
		while (true)
		{
			string termToSpeak = ConversationEntry.Clone(Entries[StartingIndex]).TermToSpeak;
			string oldValue = StartingIndex.ToString();
			int num2 = ++num;
			if (LocalizationManager.GetTermData(termToSpeak.Replace(oldValue, num2.ToString())) != null)
			{
				ConversationEntry conversationEntry = ConversationEntry.Clone(Entries[StartingIndex]);
				conversationEntry.TermToSpeak = conversationEntry.TermToSpeak.Replace(StartingIndex.ToString(), num.ToString());
				Entries.Add(conversationEntry);
				continue;
			}
			break;
		}
	}

	private void Update()
	{
		if (PlayerFarming.Instance == null)
		{
			return;
		}
		if (Player == null)
		{
			Player = PlayerFarming.Instance.gameObject;
		}
		if (IsSpeaking && LetterBox.IsPlaying && Closest == this)
		{
			Close();
			return;
		}
		if (Frame != Time.frameCount)
		{
			Closest = null;
			ClosestDist = float.MaxValue;
			foreach (SimpleBarkRepeating bark in Barks)
			{
				if (!(bark == null))
				{
					float num = Vector3.Distance(Player.transform.position, bark.transform.position);
					if (num < ClosestDist && num < ActivateDistance)
					{
						ClosestDist = num;
						Closest = bark;
					}
				}
			}
			Frame = Time.frameCount;
		}
		if (!IsSpeaking && Closest == this && !MMConversation.isPlaying && !LetterBox.IsPlaying)
		{
			IsSpeaking = true;
			MMConversation.PlayBark(conversationObject, Translate);
		}
		if (IsSpeaking && Closest != this)
		{
			Close();
		}
	}

	public void Close()
	{
		IsSpeaking = false;
		if (MMConversation.isBark)
		{
			MMConversation mmConversation = MMConversation.mmConversation;
			if ((object)mmConversation != null)
			{
				mmConversation.Close();
			}
		}
		CreateConversationObject();
	}

	private void CreateConversationObject()
	{
		conversationObject = new ConversationObject(new List<ConversationEntry> { ConversationEntry.Clone(Entries[RandomBark]) }, null, null);
		RandomBark += Random.Range(1, 3);
		if (RandomBark >= Entries.Count)
		{
			RandomBark = 0;
		}
	}

	private void OnDisable()
	{
		if (Closest == this)
		{
			Closest = null;
		}
		Close();
		Barks.Remove(this);
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, ActivateDistance, Color.white);
	}
}
