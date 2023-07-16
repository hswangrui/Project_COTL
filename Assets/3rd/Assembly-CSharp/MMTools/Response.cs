using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

namespace MMTools
{
	[Serializable]
	public class Response
	{
		public Action ActionCallBack;

		[HideInInspector]
		public string text;

		[TermsPopup("")]
		public string Term;

		public UnityEvent EventCallBack;

		public Response(string text, Action Callback, string Term)
		{
			this.text = text;
			ActionCallBack = Callback;
			this.Term = Term;
		}
	}
}
