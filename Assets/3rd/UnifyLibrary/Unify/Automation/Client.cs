using System;

namespace Unify.Automation
{
	public class Client
	{
		public enum Event
		{
			ENGAGE_READY = 0,
			ENGAGED = 1,
			SESSION_LOADFAIL = 2,
			SESSION_SAVEFAIL = 3,
			SESSION_START = 4,
			SESSION_END = 5
		}

		public enum Metric
		{
			FPS = 0
		}

		public enum Actions
		{
			RightStickY = 0,
			RightStickX = 1,
			LeftStickY = 2,
			LeftStickX = 3,
			RightStickButton = 4,
			LeftStickButton = 5,
			DPadUp = 6,
			DPadDown = 7,
			DPadRight = 8,
			DPadLeft = 9,
			DPadNone = 10,
			Options = 11,
			Center2 = 12,
			Center1 = 13,
			RightShoulder2 = 14,
			LeftShoulder2 = 15,
			RightShoulder = 16,
			LeftShoulder = 17,
			ActionTopRow2 = 18,
			Cancel = 19,
			ActionBottomRow2 = 20,
			Submit = 21,
			ActionBottomRow1 = 22,
			ActionTopRow1 = 23
		}

		public Action<string, string> OnCommandRecieved;

		public virtual void Connect()
		{
		}

		public virtual void SendEvent(Event _event)
		{
		}

		public virtual void SendGameEvent(string _event)
		{
		}

		public virtual void SendMetric(Metric metric, string value)
		{
		}

		public virtual void SendControllerInput(Action[] actions)
		{
		}

		public virtual void SendLogMessage(string message)
		{
		}
	}
}
