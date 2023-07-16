using System;

namespace MMTools
{
	public class DoctrineResponse
	{
		public SermonCategory SermonCategory;

		public int RewardLevel;

		public bool isFirstChoice;

		public Action Callback;

		public DoctrineResponse(SermonCategory SermonCategory, int RewardLevel, bool isFirstChoice, Action Callback)
		{
			this.SermonCategory = SermonCategory;
			this.RewardLevel = RewardLevel;
			this.isFirstChoice = isFirstChoice;
			this.Callback = Callback;
		}
	}
}
