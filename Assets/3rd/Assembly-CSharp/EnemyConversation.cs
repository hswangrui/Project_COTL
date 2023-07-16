using MMBiomeGeneration;
using UnityEngine;

public class EnemyConversation : BaseMonoBehaviour
{
	private enum SpeechType
	{
		Calm,
		Stressed
	}

	private class EnemySpeech
	{
		public Transform Speaker;

		public float Timestamp;

		public SpeechType SpeechType;

		public EnemySpeech(Transform speaker, float timestamp, SpeechType speechType)
		{
			Speaker = speaker;
			Timestamp = timestamp;
			SpeechType = speechType;
		}
	}

	[SerializeField]
	private GameObject speechEffectCalm;

	[SerializeField]
	private GameObject speechEffectStressed;

	private static float speechDurationCalm = 1f;

	private static float speechDurationStressed = 0.4f;

	private static float minDelayBetweenSpeechesCalm = 3f;

	private static float minDelayBetweenSpeechesStressed = 2f;

	private float nextSpeechTimestamp;

	private EnemySpeech currentSpeech;

	private EnemySpeech lastSpeech;

	private GameManager gm;

	private void OnEnable()
	{
		gm = GameManager.GetInstance();
		nextSpeechTimestamp = minDelayBetweenSpeechesCalm * Random.Range(1f, 3f);
		BiomeGenerator.OnBiomeChangeRoom += OnBiomeChangeRoom;
		if (speechEffectCalm != null)
		{
			speechEffectCalm.SetActive(false);
		}
		if (speechEffectStressed != null)
		{
			speechEffectStressed.SetActive(false);
		}
	}

	private void OnDisable()
	{
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
	}

	private void Update()
	{
		if (gm == null)
		{
			gm = GameManager.GetInstance();
		}
		else if (currentSpeech != null)
		{
			if (gm.TimeSince(currentSpeech.Timestamp) >= ((currentSpeech.SpeechType == SpeechType.Calm) ? speechDurationCalm : speechDurationStressed))
			{
				StopSpeech();
			}
			else if (currentSpeech.Speaker == null)
			{
				StopSpeech();
			}
			else if (currentSpeech.SpeechType == SpeechType.Calm)
			{
				if (speechEffectCalm != null)
				{
					speechEffectCalm.transform.position = currentSpeech.Speaker.position;
				}
			}
			else if (speechEffectStressed != null)
			{
				speechEffectStressed.transform.position = currentSpeech.Speaker.position;
			}
		}
		else
		{
			if (Health.team2.Count <= 1 || !(gm.CurrentTime >= nextSpeechTimestamp))
			{
				return;
			}
			Health health = null;
			int num = Random.Range(0, Health.team2.Count - 1);
			for (int i = 0; i < Health.team2.Count; i++)
			{
				int num2 = (i + num) % Health.team2.Count;
				if (num2 >= Health.team2.Count)
				{
					num2 -= Health.team2.Count;
				}
				if (!(Health.team2[num2] == null) && Health.team2[num2].HP != 0f && (lastSpeech == null || !(lastSpeech.Speaker != null) || !(lastSpeech.Speaker == health)) && health != null)
				{
					Speak(health.transform, (!health.Unaware) ? SpeechType.Stressed : SpeechType.Calm);
				}
			}
		}
	}

	private void Speak(Transform speaker, SpeechType speechType)
	{
		if (speaker == null)
		{
			return;
		}
		currentSpeech = new EnemySpeech(speaker, gm.CurrentTime, speechType);
		lastSpeech = currentSpeech;
		if (currentSpeech.SpeechType != 0)
		{
			float speechDurationStressed2 = speechDurationStressed;
		}
		else
		{
			float speechDurationCalm2 = speechDurationCalm;
		}
		float num = ((currentSpeech.SpeechType == SpeechType.Calm) ? minDelayBetweenSpeechesCalm : minDelayBetweenSpeechesStressed);
		num *= Random.Range(1f, 2f);
		nextSpeechTimestamp = currentSpeech.Timestamp + num;
		if (speechType == SpeechType.Calm)
		{
			if (speechEffectCalm != null)
			{
				GameObject obj = speechEffectCalm;
				if ((object)obj != null)
				{
					obj.SetActive(true);
				}
			}
			if (speechEffectStressed != null)
			{
				GameObject obj2 = speechEffectStressed;
				if ((object)obj2 != null)
				{
					obj2.SetActive(false);
				}
			}
			Debug.Log(speaker.gameObject.name + " calmly says 'blah blah blah'");
		}
		else
		{
			if (speechEffectStressed != null)
			{
				speechEffectStressed.SetActive(true);
			}
			if (speechEffectCalm != null)
			{
				speechEffectCalm.SetActive(false);
			}
			Debug.Log(speaker.gameObject.name + " shouts 'BLERGH!'");
		}
	}

	private void StopSpeech()
	{
		if (currentSpeech == null)
		{
			return;
		}
		if (speechEffectCalm != null)
		{
			GameObject obj = speechEffectCalm;
			if ((object)obj != null)
			{
				obj.SetActive(false);
			}
		}
		if (speechEffectStressed != null)
		{
			GameObject obj2 = speechEffectStressed;
			if ((object)obj2 != null)
			{
				obj2.SetActive(false);
			}
		}
		currentSpeech = null;
	}

	private void OnBiomeChangeRoom()
	{
		StopSpeech();
	}
}
