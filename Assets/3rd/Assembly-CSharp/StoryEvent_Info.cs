using System.Collections.Generic;

internal class StoryEvent_Info
{
	public List<StoryEvent_Callback> Callbacks;

	public void Create(List<StoryEvent_Callback> Callbacks)
	{
		this.Callbacks = Callbacks;
	}
}
