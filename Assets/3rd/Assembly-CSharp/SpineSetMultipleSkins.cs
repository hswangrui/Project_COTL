using System.Collections.Generic;
using Spine;
using Spine.Unity;

public class SpineSetMultipleSkins : BaseMonoBehaviour
{
	[SpineSkin("", "", true, false, false)]
	public List<string> Skin = new List<string>();

	public SkeletonAnimation Spine;

	private Skin spineSkin;

	private void OnEnable()
	{
		UpdateSkin();
	}

	private void UpdateSkin()
	{
		if (Spine == null)
		{
			Spine = base.gameObject.GetComponent<SkeletonAnimation>();
		}
		Skin skin = Spine.skeleton.Data.FindSkin(Skin[0]);
		spineSkin = new Skin("combined");
		foreach (string item in Skin)
		{
			skin = Spine.skeleton.Data.FindSkin(item);
			spineSkin.AddSkin(skin);
		}
		Spine.skeleton.SetSkin(spineSkin);
	}
}
