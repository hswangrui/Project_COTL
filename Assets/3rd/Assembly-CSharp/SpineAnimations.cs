using System;
using Spine.Unity;

[Serializable]
public class SpineAnimations
{
	public SkeletonAnimation Spine;

	[SpineAnimation("", "Spine", true, false)]
	public string TriggeredAnimation;
}
