using System.Collections;
using UnityEngine;

public class UIVoodooStatue : UIHeartStatue
{
	public override bool CanUpgrade()
	{
		return DataManager.Instance.ShrineVoodo < 4;
	}

	public override void Repair()
	{
		DataManager.Instance.ShrineVoodo = 1;
	}

	public override void Upgrade()
	{
		if (DataManager.Instance.ShrineVoodo < 4 && !Upgrading)
		{
			Debug.Log("Upgrade!");
			StartCoroutine(DoUpgrade());
		}
	}

	public override IEnumerator DoUpgrade()
	{
		Upgrading = true;
		HideButton();
		DataManager.Instance.ShrineVoodo = Mathf.Min(++DataManager.Instance.ShrineVoodo, 4);
		yield return new WaitForSeconds(0.3f);
		ShowButton();
		Upgrading = false;
	}
}
