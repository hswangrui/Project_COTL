using UnityEngine;

namespace Unify.GameCore
{
	public class GameCoreHelper
	{
		public static string GetErrorCodeString(int hresult)
		{
			return "0x" + hresult.ToString("X8");
		}

		public static bool Succeeded(int hresult, string operationFriendlyName)
		{
			bool flag = false;
			if (hresult >= 0)
			{
				flag = true;
			}
			else
			{
				string arg = hresult.ToString("X8");
				string arg2 = "GDK: " + operationFriendlyName + " failed.";
				Debug.LogWarning($"{arg2} Error code: hr=0x{arg}");
			}
			if (flag)
			{
				Debug.Log("GDK: " + operationFriendlyName + " succeeded.");
			}
			return flag;
		}
	}
}
