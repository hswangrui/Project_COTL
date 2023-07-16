using System;
using System.Collections.Generic;

namespace MMTools
{
	public class ConversationObject
	{
		public List<ConversationEntry> Entries;

		public List<Response> Responses;

		public List<DoctrineResponse> DoctrineResponses;

		public Action CallBack;

		public ConversationObject(List<ConversationEntry> Entries, List<Response> Responses, Action CallBack, List<DoctrineResponse> DoctrineResponses = null)
		{
			this.Entries = Entries;
			this.Responses = Responses;
			this.DoctrineResponses = DoctrineResponses;
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
			if (DoctrineResponses != null)
			{
				DoctrineResponses.Clear();
			}
			DoctrineResponses = null;
		}
	}
}
