using System;
using Spine.Unity;

[Serializable]
public class SpineAnimationsUI
{
	public SkeletonGraphic Spine;

	[SpineAnimation("", "Spine", true, false)]
	public string TriggeredAnimation;
}
