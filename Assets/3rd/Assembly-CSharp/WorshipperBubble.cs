using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorshipperBubble : BaseMonoBehaviour
{
	public enum SPEECH_TYPE
	{
		FOOD,
		HOME,
		HELP,
		LOVE,
		ENEMIES,
		FRIENDS,
		DISSENTER1,
		DISSENTER2,
		DISSENTER3,
		BOSSCROWN1,
		BOSSCROWN2,
		BOSSCROWN3,
		BOSSCROWN4,
		DISSENTARGUE,
		FOLLOWERMEAT,
		READY,
		TWITCH
	}

	[Serializable]
	public class MyDictionaryEntry
	{
		public SPEECH_TYPE key;

		public Sprite value;
	}

	private SpriteRenderer spriteRenderer;

	private float Scale;

	private float ScaleSpeed;

	private GameObject Player;

	private float Timer;

	public Dictionary<SPEECH_TYPE, Sprite> Bubbles = new Dictionary<SPEECH_TYPE, Sprite>();

	[SerializeField]
	private List<MyDictionaryEntry> BubbleImages;

	private Dictionary<SPEECH_TYPE, Sprite> BubblesDictionary;

	private Worshipper worshipper;

	public Action OnBubblePlay;

	public Action OnBubbleHide;

	private bool Closing;

	private Villager_Info v_i;

	private float Duration;

	public bool Active { get; private set; }

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = null;
		Player = GameObject.FindWithTag("Player");
		worshipper = GetComponentInParent<Worshipper>();
		BubblesDictionary = new Dictionary<SPEECH_TYPE, Sprite>();
		foreach (MyDictionaryEntry bubbleImage in BubbleImages)
		{
			BubblesDictionary.Add(bubbleImage.key, bubbleImage.value);
		}
	}

	private void Update()
	{
		if (!(spriteRenderer.sprite != null))
		{
			return;
		}
		if (!Closing)
		{
			if (Time.timeScale > 0f)
			{
				ScaleSpeed += (1f - Scale) * 0.3f / Time.deltaTime;
				Scale += (ScaleSpeed *= 0.7f) * Time.deltaTime;
				if (!float.IsNaN(Scale))
				{
					base.transform.localScale = new Vector3(Scale, Scale);
				}
			}
			if ((Timer += Time.deltaTime) > Duration)
			{
				Close();
			}
			if (worshipper != null && worshipper.BeingCarried)
			{
				Close();
			}
		}
		else if (Time.timeScale > 0f)
		{
			ScaleSpeed -= 0.01f * Time.deltaTime * 60f;
			Scale += ScaleSpeed;
			if (!float.IsNaN(Scale))
			{
				base.transform.localScale = new Vector3(Scale, Scale);
			}
			if (Scale <= 0f)
			{
				spriteRenderer.sprite = null;
				Active = false;
			}
		}
	}

	private void OnDisable()
	{
		Active = false;
	}

	public void Close()
	{
		ScaleSpeed = 0.07f;
		Closing = true;
		Action onBubbleHide = OnBubbleHide;
		if (onBubbleHide != null)
		{
			onBubbleHide();
		}
	}

	public void Play(SPEECH_TYPE Type, float Duration = 4f, float Delay = 0f)
	{
		if (!CheatConsole.HidingUI && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(PlayRoutine(Type, Duration, Delay));
			Active = true;
			Action onBubblePlay = OnBubblePlay;
			if (onBubblePlay != null)
			{
				onBubblePlay();
			}
		}
	}

	private IEnumerator PlayRoutine(SPEECH_TYPE Type, float Duration = 4f, float Delay = 0f)
	{
		yield return new WaitForSeconds(Delay);
		AudioManager.Instance.PlayOneShot("event:/followers/speech_bubble", base.transform.position);
		spriteRenderer.sprite = BubblesDictionary[Type];
		Scale = 0f;
		Timer = 0f;
		Closing = false;
		this.Duration = Duration - 1.6f - Delay;
	}
}
