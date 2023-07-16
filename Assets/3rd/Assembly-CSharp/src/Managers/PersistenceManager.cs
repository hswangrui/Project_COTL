using System;
using src.Data;
using UnityEngine;

namespace src.Managers
{
	public class PersistenceManager : Singleton<PersistenceManager>
	{
		private const string kPersistenceFilename = "persistence.json";

		private PersistentData _persistentData;

		private COTLDataReadWriter<PersistentData> _readWriter = new COTLDataReadWriter<PersistentData>();

		public static PersistentData PersistentData
		{
			get
			{
				return Singleton<PersistenceManager>.Instance._persistentData;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		public static void LoadUIManager()
		{
			Singleton<PersistenceManager>.Instance.Load();
		}

		public PersistenceManager()
		{
			COTLDataReadWriter<PersistentData> readWriter = _readWriter;
			readWriter.OnReadCompleted = (Action<PersistentData>)Delegate.Combine(readWriter.OnReadCompleted, (Action<PersistentData>)delegate(PersistentData data)
			{
				_persistentData = data;
			});
			COTLDataReadWriter<PersistentData> readWriter2 = _readWriter;
			readWriter2.OnCreateDefault = (Action)Delegate.Combine(readWriter2.OnCreateDefault, new Action(OnCreateDefault));
		}

		public void Load()
		{
			_readWriter.Read("persistence.json");
		}

		public static void Save()
		{
			Singleton<PersistenceManager>.Instance._readWriter.Write(Singleton<PersistenceManager>.Instance._persistentData, "persistence.json");
		}

		private void OnCreateDefault()
		{
			_persistentData = new PersistentData();
			_readWriter.Write(_persistentData, "persistence.json");
		}

		public static bool HasBeatenGame()
		{
			if (PersistentData.GameCompletionSnapshots.Count <= 0)
			{
				return PersistentData.PostGameRevealed;
			}
			return true;
		}

		public static bool HasFinishedGameOnPermadeath()
		{
			foreach (PersistentData.GameCompletionSnapshot gameCompletionSnapshot in PersistentData.GameCompletionSnapshots)
			{
				if (gameCompletionSnapshot.Permadeath)
				{
					return true;
				}
			}
			return false;
		}

		public static DifficultyManager.Difficulty HighestDifficultyCompleted()
		{
			DifficultyManager.Difficulty difficulty = DifficultyManager.Difficulty.Easy;
			foreach (PersistentData.GameCompletionSnapshot gameCompletionSnapshot in PersistentData.GameCompletionSnapshots)
			{
				if (gameCompletionSnapshot.Difficulty > difficulty)
				{
					difficulty = gameCompletionSnapshot.Difficulty;
				}
			}
			return difficulty;
		}
	}
}
