using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace src.Utilities
{
	public class MMLogger : MonoSingleton<MMLogger>
	{
		private const string kFilename = "MMLog.log";

		private bool _enabled = true;

		private StreamWriter _fileStream;

		private List<string> _logQueue = new List<string>();

		public bool Enabled
		{
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					if (!_enabled)
					{
						_fileStream.Close();
					}
					else
					{
						_fileStream = new StreamWriter(GetFileDirectory(), true);
					}
				}
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			GameObject obj = new GameObject();
			obj.AddComponent<MMLogger>();
			obj.name = "MMLogger";
		}

		public override void Start()
		{
			base.Start();
			Debug.Log("Start MMLogger".Colour(Color.cyan));
			AddToLog("Version: " + Application.version);
			string fileDirectory = GetFileDirectory();
			if (File.Exists(fileDirectory))
			{
				File.WriteAllText(fileDirectory, string.Empty);
			}
			_fileStream = new StreamWriter(fileDirectory, true);
			StartCoroutine(DoLogging());
		}

		private void OnDestroy()
		{
			_fileStream.Close();
		}

		private void OnEnable()
		{
			Application.logMessageReceived += HandleLog;
		}

		private void OnDisable()
		{
			Application.logMessageReceived -= HandleLog;
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (type == LogType.Error || type == LogType.Exception)
			{
				AddToLog(logString);
				AddToLog(stackTrace);
			}
		}

		private void AddToLog(string str)
		{
			string text = "[" + DateTime.Now.ToShortTimeString() + "] ";
			_logQueue.Add(text + str);
		}

		private IEnumerator DoLogging()
		{
			while (true)
			{
				yield return null;
				if (_enabled && _logQueue.Count > 0)
				{
					for (int i = 0; i < _logQueue.Count; i++)
					{
						_fileStream.WriteLine(_logQueue[i]);
					}
					_logQueue.Clear();
				}
			}
		}

		public static string GetFileDirectory()
		{
			return Path.Combine(Application.persistentDataPath, "MMLog.log");
		}
	}
}
