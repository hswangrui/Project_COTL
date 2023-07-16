using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Tutorial Category", menuName = "Massive Monster/Tutorial Category", order = 1)]
	public class TutorialCategory : ScriptableObject
	{
		[Serializable]
		public class TutorialEntry
		{
			[SerializeField]
			[TermsPopup("")]
			public string _description;

			[SerializeField]
			private Sprite _image;

			public string Description
			{
				get
				{
					return LocalizationManager.GetTranslation(_description);
				}
			}

			public Sprite Image
			{
				get
				{
					return _image;
				}
			}
		}

		[SerializeField]
		private TutorialTopic _topic;

		[SerializeField]
		private Sprite _topicImage;

		[SerializeField]
		private TutorialEntry[] _entries;

		public TutorialTopic Topic
		{
			get
			{
				return _topic;
			}
		}

		public TutorialEntry[] Entries
		{
			get
			{
				return _entries;
			}
		}

		public Sprite TopicImage
		{
			get
			{
				return _topicImage;
			}
		}

		public string GetTitle()
		{
			return LocalizationManager.GetTranslation(string.Format("Tutorial UI/{0}", _topic));
		}

		public string GetDescription()
		{
			return LocalizationManager.GetTranslation(string.Format("Tutorial UI/{0}/Description", _topic));
		}

		private void AddEntries()
		{
			int i = 1;
			List<TutorialEntry> list = new List<TutorialEntry>();
			for (; LocalizationManager.GetTermData(string.Format("Tutorial UI/{0}/Info{1}", _topic, i)) != null; i++)
			{
				TutorialEntry item = new TutorialEntry
				{
					_description = string.Format("Tutorial UI/{0}/Info{1}", _topic, i)
				};
				list.Add(item);
			}
			_entries = new TutorialEntry[list.Count];
			i = -1;
			while (++i < list.Count)
			{
				_entries[i] = list[i];
			}
		}

		private void Read()
		{
			Debug.Log(GetTitle());
			Debug.Log(GetDescription());
			TutorialEntry[] entries = _entries;
			for (int i = 0; i < entries.Length; i++)
			{
				Debug.Log(LocalizationManager.GetTranslation(entries[i]._description));
			}
		}
	}
}
