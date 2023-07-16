using System;
using System.Xml.Serialization;

namespace Lamb.UI
{
	[Serializable]
	[XmlType(Namespace = "FinalizedNotifications")]
	public class FinalizedNotification
	{
		public string LocKey = "";

		public string[] LocalisedParameters = new string[0];

		public string[] NonLocalisedParameters = new string[0];
	}
}
