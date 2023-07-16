using I2.Loc;
using Unify;
using UnityEngine;

public class PlatformLocaliser : MonoBehaviour
{
	public Localize _ISSLocalize;

	private bool set;

	private void Start()
	{
		if (!set)
		{
			if (UnifyManager.platform == UnifyManager.Platform.Switch)
			{
				_ISSLocalize.Term = "UI/PressAnyButtonToStart_SWITCH";
			}
			else
			{
				_ISSLocalize.Term = "UI/PressAnyButtonToStart";
			}
			_ISSLocalize.OnLocalize(true);
			set = true;
		}
	}
}
