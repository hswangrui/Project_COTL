using System;
using UnityEngine;

public abstract class FollowerSelectItem : BaseMonoBehaviour
{
	public Action<FollowerInfo> OnFollowerSelected;

	[SerializeField]
	protected MMButton _button;

	protected FollowerInfo _followerInfo;

	public MMButton Button
	{
		get
		{
			return _button;
		}
	}

	public FollowerInfo FollowerInfo
	{
		get
		{
			return _followerInfo;
		}
	}

	public void Configure(FollowerInfo followerInfo)
	{
		_followerInfo = followerInfo;
		ConfigureImpl();
	}

	protected abstract void ConfigureImpl();
}
