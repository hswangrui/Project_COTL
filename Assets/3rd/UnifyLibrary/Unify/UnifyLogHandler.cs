using System;
using System.Globalization;
using UnityEngine;

namespace Unify
{
	public sealed class UnifyLogHandler : ILogHandler
	{
		private static readonly UnifyLogHandler instance;

		public bool AutoFlush = true;

		private ILogHandler DefaultLogHandler;

		public static UnifyLogHandler Instance => instance;

		static UnifyLogHandler()
		{
			instance = new UnifyLogHandler();
		}

		private UnifyLogHandler()
		{
			DefaultLogHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = this;
		}

		~UnifyLogHandler()
		{
			Setup(null);
			Debug.unityLogger.logHandler = DefaultLogHandler;
		}

		public static string TimeNow()
		{
			return DateTime.UtcNow.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
		}

		public void LogFileWriteWithTime(string msg)
		{
		}

		public void LogException(Exception exception, UnityEngine.Object context)
		{
			UnifyManager.Instance.AutomationClient.SendLogMessage(exception.ToString());
			DefaultLogHandler.LogException(exception, context);
		}

		public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			string message = string.Format(format, args);
			UnifyManager.Instance.AutomationClient.SendLogMessage(message);
			DefaultLogHandler.LogFormat(logType, context, format, args);
		}

		public bool Setup(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				return false;
			}
			data.StartsWith("ws://");
			return true;
		}
	}
}
