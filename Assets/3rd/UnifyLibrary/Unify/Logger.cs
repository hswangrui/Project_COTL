using System;
using UnityEngine;

namespace Unify
{
	public class Logger
	{
		[Flags]
		public enum Category
		{
			NONE = 0,
			INFO = 1,
			WARNING = 2,
			ERROR = 4,
			ALL = 7
		}

		public static Category EnabledCategories = Category.ALL;

		private static bool IsCategoryEnabled(Category category)
		{
			return (EnabledCategories & category) != 0;
		}

		public static void Log(object message)
		{
			if (IsCategoryEnabled(Category.INFO))
			{
				Debug.Log(message);
			}
		}

		public static void Log(object message, UnityEngine.Object context)
		{
			if (IsCategoryEnabled(Category.INFO))
			{
				Debug.Log(message, context);
			}
		}

		public static void LogWarning(object message)
		{
			if (IsCategoryEnabled(Category.WARNING))
			{
				Debug.LogWarning(message);
			}
		}

		public static void LogWarning(object message, UnityEngine.Object context)
		{
			if (IsCategoryEnabled(Category.WARNING))
			{
				Debug.LogWarning(message, context);
			}
		}

		public static void LogError(object message)
		{
			if (IsCategoryEnabled(Category.ERROR))
			{
				Debug.LogError(message);
			}
		}

		public static void LogError(object message, UnityEngine.Object context)
		{
			if (IsCategoryEnabled(Category.ERROR))
			{
				Debug.LogError(message, context);
			}
		}
	}
}
