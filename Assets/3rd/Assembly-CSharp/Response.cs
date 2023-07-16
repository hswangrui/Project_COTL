using System;

public class Response
{
	public Action Callback;

	public string text;

	public Response(string text, Action Callback)
	{
		this.text = text;
		this.Callback = Callback;
	}
}
