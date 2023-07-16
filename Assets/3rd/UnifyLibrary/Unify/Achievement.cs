using System;

namespace Unify
{
	[Serializable]
	public struct Achievement
	{
		public static readonly Achievement None = new Achievement(-1, "none");

		public int id;

		public string label;

		public string description;

		public int ps4Id;

		public string xboxOneId;

		public string steamId;

		public Achievement(int id, string label)
		{
			this.id = id;
			this.label = label;
			description = null;
			ps4Id = -1;
			xboxOneId = null;
			steamId = null;
		}

		public static bool operator ==(Achievement a, Achievement b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Achievement a, Achievement b)
		{
			return !a.Equals(b);
		}

		public bool Equals(Achievement b)
		{
			return id == b.id;
		}

		public override string ToString()
		{
			return "Achievement(" + id + "," + label + ")";
		}
	}
}
