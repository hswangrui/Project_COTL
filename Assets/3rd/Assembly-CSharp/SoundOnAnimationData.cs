using System;
using FMOD.Studio;
using FMODUnity;
using Spine.Unity;

[Serializable]
public class SoundOnAnimationData
{
	public enum Position
	{
		Beginning,
		End,
		Loop
	}

	public EventInstance LoopedSound;

	public SkeletonAnimation SkeletonData;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string SkeletonsAnimations;

	[EventRef]
	public string AudioSourcePath = string.Empty;

	public Position position;
}
