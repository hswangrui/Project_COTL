using System;

namespace Map
{
	[Serializable]
	public class Point : IEquatable<Point>
	{
		public int x;

		public int y;

		public Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public bool Equals(Point other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (x == other.x)
			{
				return y == other.y;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Point)obj);
		}

		public override int GetHashCode()
		{
			return (x * 397) ^ y;
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", x, y);
		}
	}
}
