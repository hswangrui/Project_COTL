using System.IO;

namespace FilepathUtils
{
	public static class FilepathUtilities
	{
		public static string NormalizePath(this string path)
		{
			if (Path.DirectorySeparatorChar == '\\')
			{
				return path.Replace('/', Path.DirectorySeparatorChar);
			}
			return path.Replace('\\', Path.DirectorySeparatorChar);
		}
	}
}
