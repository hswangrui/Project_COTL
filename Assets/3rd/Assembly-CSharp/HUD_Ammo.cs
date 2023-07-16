using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Ammo : BaseMonoBehaviour
{
	public List<HUD_AmmoIcon> Icons = new List<HUD_AmmoIcon>();

	public Image ReloadRadialProgress;

	private Coroutine cReloadingRoutine;

	private void OnEnable()
	{
		ReloadRadialProgress.gameObject.SetActive(false);
		PlayerArrows.OnAmmoUpdated += OnAmmoUpdated;
		PlayerArrows.OnNoAmmoShake += OnNoAmmoShake;
		PlayerArrows.OnBeginReloading += OnBeginReloading;
	}

	private void OnBeginReloading(PlayerArrows playerArrows)
	{
		if (cReloadingRoutine != null)
		{
			StopCoroutine(cReloadingRoutine);
		}
		cReloadingRoutine = StartCoroutine(ReloadingRoutine(playerArrows));
	}

	private void TestPostion(int Num)
	{
		ReloadRadialProgress.rectTransform.anchoredPosition = new Vector2(32 * Num + 10, 0f);
	}

	private IEnumerator ReloadingRoutine(PlayerArrows playerArrows)
	{
		ReloadRadialProgress.gameObject.SetActive(true);
		ReloadRadialProgress.rectTransform.anchoredPosition = new Vector2(32 * (playerArrows.PLAYER_ARROW_TOTAL_AMMO + playerArrows.PLAYER_SPIRIT_TOTAL_AMMO) + 10, 0f);
		while (DataManager.Instance.PLAYER_ARROW_AMMO < DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO)
		{
			ReloadRadialProgress.fillAmount = playerArrows.ReloadProgress / playerArrows.ReloadTarget;
			yield return null;
		}
		ReloadRadialProgress.gameObject.SetActive(false);
	}

	public void Play()
	{
		PlayerArrows.OnAmmoUpdated += OnAmmoUpdated;
		PlayerArrows.OnNoAmmoShake += OnNoAmmoShake;
		PlayerArrows.OnBeginReloading += OnBeginReloading;
		PlayerArrows playerArrows = Object.FindObjectOfType<PlayerArrows>();
		OnAmmoUpdated(playerArrows);
	}

	private void OnDisable()
	{
		PlayerArrows.OnAmmoUpdated -= OnAmmoUpdated;
		PlayerArrows.OnNoAmmoShake -= OnNoAmmoShake;
		PlayerArrows.OnBeginReloading -= OnBeginReloading;
	}

	private void OnAmmoUpdated(PlayerArrows playerArrows)
	{
		UpdateAmmo(playerArrows);
	}

	private void UpdateAmmo(PlayerArrows playerArrows)
	{
		int num = -1;
		float num2 = -0.05f;
		while (++num < Icons.Count)
		{
			if (num < playerArrows.PLAYER_ARROW_TOTAL_AMMO)
			{
				if (num < playerArrows.PLAYER_ARROW_AMMO)
				{
					Icons[num].SetMode(HUD_AmmoIcon.Mode.ON, num2 += 0.05f);
				}
				else
				{
					Icons[num].SetMode(HUD_AmmoIcon.Mode.EMPTY, num2 += 0.05f);
				}
			}
			else if (num >= playerArrows.PLAYER_ARROW_TOTAL_AMMO && num < playerArrows.PLAYER_ARROW_TOTAL_AMMO + playerArrows.PLAYER_SPIRIT_TOTAL_AMMO)
			{
				if (num < playerArrows.PLAYER_ARROW_TOTAL_AMMO + playerArrows.PLAYER_SPIRIT_AMMO)
				{
					Icons[num].SetMode(HUD_AmmoIcon.Mode.ON_SPIRIT, num2 += 0.05f);
				}
				else
				{
					Icons[num].SetMode(HUD_AmmoIcon.Mode.EMPTY_SPIRIT, num2 += 0.05f);
				}
			}
			else
			{
				Icons[num].SetMode(HUD_AmmoIcon.Mode.OFF, 0f);
			}
		}
	}

	public void OnNoAmmoShake(PlayerArrows playerArrows)
	{
		int num = -1;
		while (++num < Icons.Count)
		{
			Icons[num].StartShake();
		}
	}
}
