using System;
using System.Collections.Generic;

namespace src.Data
{
	[Serializable]
	public class PersistentData
	{
		[Serializable]
		public struct GameCompletionSnapshot
		{
			public DifficultyManager.Difficulty Difficulty;

			public bool Permadeath;
		}

		public bool PostGameRevealed;

		public List<GameCompletionSnapshot> GameCompletionSnapshots = new List<GameCompletionSnapshot>();

		public int PhotoModePictureIndex;

		public List<string> PhotosTakenPaths = new List<string>();
	}
}
