using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UICursesStatue : UIHeartStatue
{
	private void OnEnable()
	{
		UIHeartStatue.Instance = this;
		FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerUpdated));
		FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerUpdated));
	}

	private void OnDisable()
	{
		UIHeartStatue.Instance = null;
		FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerUpdated));
		FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerUpdated));
	}

	private void OnFollowerUpdated(int followerID)
	{
		UpdateSpiritAmmo();
	}

	public override bool CanUpgrade()
	{
		return DataManager.Instance.ShrineCurses < 4;
	}

	public override void Repair()
	{
		DataManager.Instance.ShrineCurses = 1;
	}

	public override void Upgrade()
	{
		if (DataManager.Instance.ShrineCurses < 4 && !Upgrading)
		{
			Debug.Log("Upgrade!");
			StartCoroutine(DoUpgrade());
		}
	}

	public override IEnumerator DoUpgrade()
	{
		Upgrading = true;
		HideButton();
		DataManager.Instance.ShrineCurses = Mathf.Min(++DataManager.Instance.ShrineCurses, 4);
		UpdateSpiritAmmo();
		yield return new WaitForSeconds(0.3f);
		ShowButton();
		Upgrading = false;
	}

	public static void UpdateSpiritAmmo()
	{
		PlayerArrows playerArrows = UnityEngine.Object.FindObjectOfType<PlayerArrows>();
		if (playerArrows == null)
		{
			return;
		}
		int count = DataManager.Instance.Followers.Count;
		switch (DataManager.Instance.ShrineCurses)
		{
		case 0:
			DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 0;
			break;
		case 1:
			if (count > 1)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 1;
			}
			else if (count <= 1)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 0;
			}
			break;
		case 2:
			if (count > 1 && count <= 4)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 1;
			}
			else if (count > 4)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 2;
			}
			else if (count <= 1)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 0;
			}
			break;
		case 3:
			if (count > 1 && count <= 4)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 1;
			}
			else if (count > 4 && count <= 9)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 2;
			}
			else if (count > 9)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 3;
			}
			else if (count <= 1)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 0;
			}
			break;
		case 4:
			if (count > 1 && count <= 4)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 1;
			}
			else if (count > 4 && count <= 9)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 2;
			}
			else if (count > 9 && count <= 14)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 3;
			}
			else if (count > 14)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 4;
			}
			else if (count <= 1)
			{
				DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = 0;
			}
			break;
		}
		playerArrows.RestockAllArrows();
		UIHeartStatue instance = UIHeartStatue.Instance;
		if ((object)instance != null)
		{
			instance.UpdateDisplayIcons();
		}
	}

	public override void UpdateDisplayIcons()
	{
		if (!(UIHeartStatue.Instance != null))
		{
			return;
		}
		int num = -1;
		while (++num < UIHeartStatue.Instance.DiplayIcons.Count)
		{
			if (num < DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO)
			{
				UIHeartStatue.Instance.DiplayIcons[num].GetComponent<Image>().material = normalMaterial;
			}
			else
			{
				UIHeartStatue.Instance.DiplayIcons[num].GetComponent<Image>().material = BlackAndWhiteMaterial;
			}
		}
		if (UIHeartStatue.Instance.LockIcons == null)
		{
			return;
		}
		num = -1;
		while (++num < UIHeartStatue.Instance.LockIcons.Count)
		{
			if (num < DataManager.Instance.ShrineCurses)
			{
				LockIcons[num].SetActive(false);
			}
			else
			{
				LockIcons[num].SetActive(true);
			}
		}
	}
}
