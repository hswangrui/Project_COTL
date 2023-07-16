using System;
using System.Collections;
using MMBiomeGeneration;
using UnityEngine;

public class EntranceRoomDungeonLeadersController : MonoBehaviour
{
	private Interaction_EntranceShrine _dungeonLeaderMechanics;

	public Interaction_WeaponSelectionPodium[] Weapons;

	private void OnEnable()
	{
		_dungeonLeaderMechanics = UnityEngine.Object.FindObjectOfType<Interaction_EntranceShrine>();
		Debug.Log("EntranceRoomDungeonLeadersController");
		if (DungeonSandboxManager.Active)
		{
			base.gameObject.SetActive(false);
		}
		else if (BiomeGenerator.Instance == null || BiomeGenerator.Instance.CurrentRoom == null || !BiomeGenerator.Instance.CurrentRoom.Completed)
		{
			GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
			{
				HideWeapons();
				RoomLockController.CloseAll();
			}));
		}
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private void HideWeapons()
	{
		if (_dungeonLeaderMechanics != null)
		{
			_dungeonLeaderMechanics.HideDummys();
		}
		Interaction_WeaponSelectionPodium[] weapons = Weapons;
		foreach (Interaction_WeaponSelectionPodium interaction_WeaponSelectionPodium in weapons)
		{
			if ((bool)interaction_WeaponSelectionPodium)
			{
				interaction_WeaponSelectionPodium.gameObject.SetActive(false);
			}
		}
	}

	private void RevealWeapons()
	{
		Interaction_WeaponSelectionPodium[] weapons = Weapons;
		for (int i = 0; i < weapons.Length; i++)
		{
			weapons[i].gameObject.SetActive(true);
		}
		Interaction_Chest.ChestEvent onChestRevealed = Interaction_Chest.OnChestRevealed;
		if (onChestRevealed != null)
		{
			onChestRevealed();
		}
	}
}
