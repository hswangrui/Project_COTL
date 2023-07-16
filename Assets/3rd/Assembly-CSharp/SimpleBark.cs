using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;

public class SimpleBark : BaseMonoBehaviour
{
	[Serializable]
	public class VariableAndCondition
	{
		public DataManager.Variables Variable;

		public bool Condition = true;
	}

	public delegate void NormalEvent();

	public float ActivateDistance = 4f;

	public Vector3 ActivateOffset = Vector3.zero;

	private bool Spoken;

	public bool DeleteIfConditionsMet = true;

	public List<VariableAndCondition> DeleteConditions = new List<VariableAndCondition>();

	public List<ConversationEntry> Entries;

	public bool DisableAfterBark;

	public bool HideAfterBark;

	public bool useTimer;

	public float timer = 5f;

	public bool StopAudioAfter = true;

	public int StartingIndex;

	private int RandomBark;

	private bool isSpeaking;

	public bool Translate = true;

	private bool played;

	private bool Closed;

	public Renderer Renderer { get; set; }

	public bool IsSpeaking
	{
		get
		{
			return isSpeaking;
		}
	}

	public event NormalEvent OnPlay;

	public event NormalEvent OnClose;

	private void Start()
	{
		RandomBark = UnityEngine.Random.Range(0, Entries.Count);
		if (DeleteConditions.Count <= 0)
		{
			return;
		}
		bool flag = true;
		foreach (VariableAndCondition deleteCondition in DeleteConditions)
		{
			Debug.Log(string.Concat(base.gameObject.name, " ", deleteCondition.Variable, " ", deleteCondition.Condition.ToString(), "  ", DataManager.Instance.GetVariable(deleteCondition.Variable).ToString()));
			if (DataManager.Instance.GetVariable(deleteCondition.Variable) != deleteCondition.Condition)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (DeleteIfConditionsMet)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				base.enabled = false;
			}
		}
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
		if (Time.timeScale == 0f)
		{
			if (isSpeaking)
			{
				Close();
			}
		}
		else if (!(PlayerFarming.Instance == null) && (!DisableAfterBark || !played || !Closed))
		{
			if (!MMConversation.isPlaying && !LetterBox.IsPlaying && !isSpeaking && Vector3.Distance(base.transform.position + ActivateOffset, PlayerFarming.Instance.transform.position) < ActivateDistance)
			{
				Show();
			}
			if (isSpeaking && Renderer != null && !Renderer.isVisible)
			{
				Close();
			}
			if (isSpeaking && ((Renderer == null && Vector3.Distance(base.transform.position + ActivateOffset, PlayerFarming.Instance.transform.position) > ActivateDistance) || LetterBox.IsPlaying))
			{
				Debug.Log("CLOSE!");
				Close();
			}
		}
	}

	private IEnumerator StartTimer()
	{
		yield return new WaitForSeconds(timer);
		Close();
	}

	public void Show()
	{
		Debug.Log("START SPEAKING");
		isSpeaking = true;
		played = true;
		if (useTimer)
		{
			StartCoroutine(StartTimer());
		}
		MMConversation.PlayBark(new ConversationObject(new List<ConversationEntry> { ConversationEntry.Clone(Entries[RandomBark]) }, null, null), Translate);
		if (++RandomBark >= Entries.Count)
		{
			RandomBark = 0;
		}
		NormalEvent onPlay = this.OnPlay;
		if (onPlay != null)
		{
			onPlay();
		}
	}

	public void Close()
	{
		Closed = true;
		Debug.Log("CLOSE!!!");
		StopAllCoroutines();
		played = true;
		isSpeaking = false;
		if (MMConversation.isBark && MMConversation.mmConversation != null)
		{
			MMConversation mmConversation = MMConversation.mmConversation;
			if ((object)mmConversation != null)
			{
				mmConversation.Close(true, null, StopAudioAfter);
			}
		}
		if (HideAfterBark)
		{
			base.gameObject.SetActive(false);
			played = false;
			isSpeaking = false;
			Closed = false;
		}
		NormalEvent onClose = this.OnClose;
		if (onClose != null)
		{
			onClose();
		}
	}

	private void OnDisable()
	{
		if (played)
		{
			Close();
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, ActivateDistance, Color.white);
	}
}
