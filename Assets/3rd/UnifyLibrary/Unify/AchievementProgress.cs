using System;

namespace Unify
{
	[Serializable]
	public struct AchievementProgress
	{
		public int id;

		public int progress;

		public string name;

		public string description;

		public override string ToString()
		{
			return string.Format("id({0}) {2}% name({1})  [{3}]", id, progress, name, description);
		}

		public AchievementProgress(int _id, int _progress, string _name = null, string _desc = null)
		{
			id = _id;
			progress = _progress;
			name = _name;
			description = _desc;
		}
	}
}
