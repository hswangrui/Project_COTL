using System.Collections.Generic;
using UnityEngine;

namespace MMTools
{
	public class Conversation_Speaker : MonoBehaviour
	{
		public enum ID
		{
			Speaker_1,
			Speaker_2,
			Speaker_3,
			Speaker_4
		}

		public ID id;

		private static List<Conversation_Speaker> speakers = new List<Conversation_Speaker>();

		public static GameObject Speaker1
		{
			get
			{
				foreach (Conversation_Speaker speaker in speakers)
				{
					if (speaker.id == ID.Speaker_1)
					{
						return speaker.gameObject;
					}
				}
				return null;
			}
		}

		public static GameObject Speaker2
		{
			get
			{
				foreach (Conversation_Speaker speaker in speakers)
				{
					if (speaker.id == ID.Speaker_2)
					{
						return speaker.gameObject;
					}
				}
				return null;
			}
		}

		private void OnEnable()
		{
			speakers.Add(this);
		}

		private void OnDisable()
		{
			speakers.Remove(this);
		}

		public static GameObject GetSpeakerByID(ID id)
		{
			foreach (Conversation_Speaker speaker in speakers)
			{
				if (speaker.id == id)
				{
					return speaker.gameObject;
				}
			}
			return null;
		}
	}
}
