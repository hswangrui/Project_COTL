using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using static UnityEditor.EditorApplication;

internal static class Class36
{
	private class Class37
	{
		public Action action_0;

		public int int_0;

		public int int_1;

		public int int_2;
	}

	private static Stack<StringBuilder> stack_0 = new Stack<StringBuilder>(2);

	private static List<Class37> list_0 = new List<Class37>();

	public static StringBuilder smethod_0(string string_0)
	{
		StringBuilder stringBuilder = smethod_1();
		stringBuilder.Append(string_0);
		return stringBuilder;
	}

	public static StringBuilder smethod_1()
	{
		if (stack_0.Count > 0)
		{
			return stack_0.Pop();
		}
		return new StringBuilder(64);
	}

	public static string smethod_2(StringBuilder stringBuilder_0)
	{
		string result = stringBuilder_0.ToString();
		stringBuilder_0.Length = 0;
		stack_0.Push(stringBuilder_0);
		return result;
	}

	public static void smethod_3(Action action_0, int int_0, int int_1 = -1)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (list_0.Count == 0)
		{
			EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(smethod_5));
		}
		int num = 0;
		int count = list_0.Count;
		while (true)
		{
			if (num < count)
			{
				if (list_0[num].action_0 == action_0)
				{
					break;
				}
				num++;
				continue;
			}
			list_0.Add(new Class37
			{
				action_0 = action_0,
				int_1 = int_0,
				int_0 = int_0,
				int_2 = int_1
			});
			return;
		}
		list_0[num].int_1 = int_0;
		list_0[num].int_0 = int_0;
		list_0[num].int_2 = int_1;
	}

	public static void smethod_4(Action action_0)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		int num = 0;
		int count = list_0.Count;
		while (true)
		{
			if (num < count)
			{
				if (list_0[num].action_0 == action_0)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		list_0.RemoveAt(num);
		if (list_0.Count == 0)
		{
			EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(smethod_5));
		}
	}

	private static void smethod_5()
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			if (--list_0[i].int_1 <= 0)
			{
				Class37 @class = list_0[i];
				@class.int_1 = @class.int_0;
				@class.action_0();
				@class.int_2--;
				if (@class.int_2 == 0)
				{
					list_0.RemoveAt(i);
				}
				if (i < list_0.Count && @class != list_0[i])
				{
					i--;
				}
			}
		}
	}
}
