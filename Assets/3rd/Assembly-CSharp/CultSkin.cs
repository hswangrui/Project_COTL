using Spine.Unity;
using UnityEngine;

public class CultSkin : BaseMonoBehaviour
{
	public enum EnemyType
	{
		Grunt,
		Grunt_Captain,
		Archer,
		Archer_Captain,
		Scamp,
		Shielder,
		Brute
	}

	public SkeletonAnimation Spine;

	public EnemyType Type;

	public int NumberOfHeads = 18;

	public bool removeMask;

	public bool removeSword;

	public bool overrideRoomManager;

	private void Start()
	{
		SetRandomHead();
		if (removeMask)
		{
			SetRemoveMask();
		}
		if (removeSword)
		{
			SetRemoveSword();
		}
	}

	private void SetRemoveSword()
	{
		Spine.skeleton.FindSlot("MASK_SKIN").Attachment = null;
	}

	private void SetRemoveMask()
	{
		Spine.skeleton.FindSlot("WEAPON_SKIN").Attachment = null;
	}

	private void SetRandomHead()
	{
		if (NumberOfHeads > 0)
		{
			Spine.skeleton.SetAttachment("HEAD_SKIN", "HEAD_" + Random.Range(0, NumberOfHeads));
		}
	}

	private void SetSkinByCult()
	{
	}
}
