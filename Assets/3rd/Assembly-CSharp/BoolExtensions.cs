public static class BoolExtensions
{
	public static int ToInt(this bool b)
	{
		if (!b)
		{
			return 0;
		}
		return 1;
	}
}
