using System;
using FMOD.Studio;
using FMODUnity;

[Serializable]
public class SoundOnStateData
{
	public enum Position
	{
		Beginning,
		End,
		Loop
	}

	public StateMachine.State State;

	public EventInstance LoopedSound;

	[EventRef]
	public string AudioSourcePath = string.Empty;

	public Position position;
}
