using System;
using System.Collections.Generic;

namespace MMTools
{
	public class BuyObject
	{
		public List<BuyEntry> Entries;

		public List<Response> Responses;

		public Action CallBack;

		public BuyObject(List<BuyEntry> Entries, List<Response> Responses, Action CallBack)
		{
			this.Entries = Entries;
			this.Responses = Responses;
			this.CallBack = CallBack;
		}

		public void Clear()
		{
			if (Entries != null)
			{
				Entries.Clear();
			}
			Entries = null;
			if (Responses != null)
			{
				Responses.Clear();
			}
			Responses = null;
		}
	}
}
