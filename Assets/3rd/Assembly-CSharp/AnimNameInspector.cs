using Spine.Unity;

public class AnimNameInspector : BaseMonoBehaviour
{
	[SpineAnimation("", "", true, false)]
	public string listOfAllAnimations;

	public SkeletonAnimation otherSkeletonData;

	[SpineAnimation("", "otherSkeletonData", true, false)]
	public string otherSkeletonsAnimations;

	public static string listOfAllAnimationsFunction()
	{
		return "test";
	}

	private string GetListOfAllAnimations()
	{
		return listOfAllAnimations;
	}
}
