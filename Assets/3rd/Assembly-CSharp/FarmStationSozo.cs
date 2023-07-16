using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class FarmStationSozo : FarmPlot
{
	public SkeletonAnimation Spine;

	public SkeletonAnimation SpineArms;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public List<string> MushroomGrowthStates = new List<string>();

	public override void UpdateCropImage()
	{
		int num = Mathf.FloorToInt(base.StructureInfo.GrowthStage);
		Debug.Log("Growth state: " + num + "  " + MushroomGrowthStates[num].ToString());
		Spine.AnimationState.SetAnimation(0, MushroomGrowthStates[num], true);
		SpineArms.AnimationState.SetAnimation(0, MushroomGrowthStates[num], true);
	}
}
