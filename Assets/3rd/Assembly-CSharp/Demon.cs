using System.Collections;
using UnityEngine;

public class Demon : MonoBehaviour
{
	protected int followerID;

	private FollowerInfo followerInfo;

	private int forcedLevel = -1;

	public FollowerInfo FollowerInfo
	{
		get
		{
			if (followerInfo == null)
			{
				followerInfo = FollowerInfo.GetInfoByID(followerID);
			}
			return followerInfo;
		}
	}

	public int Level
	{
		get
		{
			if (forcedLevel != -1)
			{
				return forcedLevel;
			}
			if (followerInfo == null)
			{
				followerInfo = FollowerInfo.GetInfoByID(followerID);
			}
			if (followerInfo != null)
			{
				return followerInfo.GetDemonLevel();
			}
			return 1;
		}
	}

	public virtual void Init(int followerID, int forcedLevel = -1)
	{
		this.forcedLevel = forcedLevel;
		this.followerID = followerID;
		StartCoroutine(SetSkin());
	}

	public static float GetDemonLeftovers()
	{
		float num = 0f;
		for (int i = 0; i < Demon_Arrows.Demons.Count; i++)
		{
			if ((bool)Demon_Arrows.Demons[i].GetComponent<Demon_Arrows>())
			{
				Demon component = Demon_Arrows.Demons[i].GetComponent<Demon>();
				num += ((component.Level > 1) ? (0.1f * (float)component.Level) : 0f);
			}
		}
		return num;
	}

	protected virtual IEnumerator SetSkin()
	{
		yield break;
	}
}
