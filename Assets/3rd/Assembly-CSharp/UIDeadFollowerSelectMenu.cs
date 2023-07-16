using System.Collections.Generic;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using TMPro;
using UnityEngine;

public class UIDeadFollowerSelectMenu : UIFollowerSelectBase<DeadFollowerInformationBox>
{
	[SerializeField]
	private TMP_Text capacity;

	public override bool AllowsVoting
	{
		get
		{
			return false;
		}
	}

	public void Show(int amount, int maxCapacity, List<FollowerInfo> followerInfo, List<FollowerInfo> blackList = null, bool instant = false, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
	{
		Show(followerInfo, blackList, instant, hideOnSelection, cancellable, hasSelection);
		capacity.text = string.Format(capacity.text, amount, maxCapacity);
		if (amount == 0)
		{
			_controlPrompts.HideAcceptButton();
		}
	}

	protected override DeadFollowerInformationBox PrefabTemplate()
	{
		return MonoSingleton<UIManager>.Instance.DeadFollowerInformationBox;
	}
}
