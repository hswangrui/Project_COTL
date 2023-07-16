using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using UnityEngine;

public class HUD_Hearts : BaseMonoBehaviour
{
	public static HUD_Hearts Instance;

	public List<HUD_Heart> HeartIcons = new List<HUD_Heart>();

	public int _health;

	private EventInstance loopedSound;

	private bool createdloop;

	private int count;

	private void Start()
	{
		Instance = this;
		HealthPlayer.OnTotalHPUpdated += OnTotalHPUpdated;
		HealthPlayer.OnHPUpdated += OnHPUpdated;
		if ((bool)PlayerFarming.Instance)
		{
			HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
			if ((bool)component)
			{
				OnHPUpdated(component);
			}
		}
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		Instance = null;
		HealthPlayer.OnHPUpdated -= OnHPUpdated;
		HealthPlayer.OnTotalHPUpdated -= OnTotalHPUpdated;
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(loopedSound);
	}

	private void OnHPUpdated(HealthPlayer Target)
	{
		UpdateHearts(Target, true);
		lowHealthCheck();
	}

	private void lowHealthCheck()
	{
		int num = (_health = (int)DataManager.Instance.PLAYER_HEALTH + (int)DataManager.Instance.PLAYER_SPIRIT_HEARTS + (int)DataManager.Instance.PLAYER_BLUE_HEARTS + (int)DataManager.Instance.PLAYER_BLACK_HEARTS);
		bool flag = DataManager.Instance.PlayerFleece == 7;
		if (num == 2 && DataManager.Instance.PLAYER_TOTAL_HEALTH != 2f && !flag)
		{
			StopAllCoroutines();
			if (!createdloop)
			{
				loopedSound = AudioManager.Instance.CreateLoop("event:/atmos/misc/whispers", PlayerFarming.Instance.gameObject, true);
				createdloop = true;
			}
			loopedSound.setVolume(0.5f);
			HUD_Heart heart = HeartIcons[0];
			StartCoroutine(lowHealthEffect(2f, heart));
		}
		else if (num == 1 && !flag)
		{
			StopAllCoroutines();
			if (!createdloop)
			{
				if (Time.timeScale != 0f)
				{
					AudioManager.Instance.SetMusicFilter("filter", 0.666f);
				}
				loopedSound = AudioManager.Instance.CreateLoop("event:/atmos/misc/whispers", PlayerFarming.Instance.gameObject, true);
				createdloop = true;
			}
			loopedSound.setVolume(1f);
			HUD_Heart heart2 = HeartIcons[0];
			StartCoroutine(lowHealthEffect(1f, heart2));
		}
		else
		{
			if (createdloop && Time.timeScale != 0f)
			{
				AudioManager.Instance.SetMusicFilter("filter", 0f);
			}
			AudioManager.Instance.StopLoop(loopedSound);
			createdloop = false;
			StopAllCoroutines();
		}
	}

	private IEnumerator lowHealthEffect(float waitTime, HUD_Heart Heart)
	{
		count = 4;
		while ((int)DataManager.Instance.PLAYER_HEALTH + (int)DataManager.Instance.PLAYER_SPIRIT_HEARTS + (int)DataManager.Instance.PLAYER_BLUE_HEARTS + (int)DataManager.Instance.PLAYER_BLACK_HEARTS <= 2)
		{
			yield return new WaitForSeconds(waitTime);
			count--;
			if (count >= 0)
			{
				UIManager.PlayAudio("event:/ui/heartbeat");
			}
			Heart.transform.DOKill();
			Heart.transform.localScale = Vector3.one;
			Heart.transform.DOPunchScale(new Vector3(0.2f, -0.5f), 0.5f);
		}
	}

	private void UpdateHearts(HealthPlayer health, bool DoEffects)
	{
		int num = -1;
		int num2 = (int)health.HP;
		int num3 = (int)DataManager.Instance.PLAYER_TOTAL_HEALTH;
		int num4 = (int)health.SpiritHearts;
		int num5 = (int)health.TotalSpiritHearts;
		int num6 = (int)health.BlueHearts;
		int num7 = (int)health.BlackHearts;
		while (++num < HeartIcons.Count)
		{
			HUD_Heart hUD_Heart = HeartIcons[num];
			if (Mathf.Ceil(DataManager.Instance.PLAYER_TOTAL_HEALTH / 2f) + Mathf.Ceil(health.TotalSpiritHearts / 2f) + Mathf.Ceil(DataManager.Instance.PLAYER_BLUE_HEARTS / 2f) + Mathf.Ceil(DataManager.Instance.PLAYER_BLACK_HEARTS / 2f) <= (float)num)
			{
				if (hUD_Heart.MyState == HUD_Heart.HeartState.HeartHalf && hUD_Heart.MyHeartType == HUD_Heart.HeartType.Blue)
				{
					hUD_Heart.Activate(false, true);
				}
				else
				{
					hUD_Heart.Activate(false, false);
				}
				continue;
			}
			hUD_Heart.Activate(true, false);
			if (num3 >= 1)
			{
				if (num2 >= 2)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartFull, DoEffects);
					num2 -= 2;
				}
				else if (num2 == 1)
				{
					if (num3 >= 2)
					{
						hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartHalfFull, DoEffects);
					}
					else
					{
						hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartHalf, DoEffects);
					}
					num2--;
				}
				else if (num3 == 1)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HalfHeartContainer, DoEffects);
				}
				else
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartContainer, DoEffects);
				}
				num3 -= 2;
			}
			else if (num5 >= 1)
			{
				if (num4 >= 2)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartFull, DoEffects, HUD_Heart.HeartType.Spirit);
					num4 -= 2;
				}
				else if (num4 == 1)
				{
					if (num5 == 1)
					{
						hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartHalf, DoEffects, HUD_Heart.HeartType.Spirit);
					}
					else
					{
						hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartHalfFull, DoEffects, HUD_Heart.HeartType.Spirit);
					}
					num4--;
				}
				else if (num5 == 1)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HalfHeartContainer, DoEffects, HUD_Heart.HeartType.Spirit);
				}
				else
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartContainer, DoEffects, HUD_Heart.HeartType.Spirit);
				}
				num5 -= 2;
			}
			else if ((float)num >= Mathf.Ceil(DataManager.Instance.PLAYER_TOTAL_HEALTH / 2f) + Mathf.Ceil(health.TotalSpiritHearts / 2f))
			{
				if (num6 >= 2)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartFull, DoEffects, HUD_Heart.HeartType.Blue);
					num6 -= 2;
				}
				else if (num6 == 1)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartHalf, DoEffects, HUD_Heart.HeartType.Blue);
					num6--;
				}
				else if (num7 >= 2)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartFull, DoEffects, HUD_Heart.HeartType.Black);
					num7 -= 2;
				}
				else if (num7 == 1)
				{
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartHalf, DoEffects, HUD_Heart.HeartType.Black);
					num7--;
				}
			}
		}
	}

	private void OnTotalHPUpdated(HealthPlayer Target)
	{
		int num = -1;
		float hP = Target.HP;
		float blueHeart = Target.BlueHearts;
		float num2 = -0.5f;
		while (++num < HeartIcons.Count)
		{
			if (Mathf.Ceil(DataManager.Instance.PLAYER_TOTAL_HEALTH / 2f) + Mathf.Ceil(Target.TotalSpiritHearts / 2f) + Mathf.Ceil(DataManager.Instance.PLAYER_BLUE_HEARTS / 2f) > (float)num)
			{
				HUD_Heart hUD_Heart = HeartIcons[num];
				if (!hUD_Heart.gameObject.activeSelf)
				{
					Debug.Log("i: " + num);
					hUD_Heart.ActivateAndScale(num2 += 0.5f);
					hUD_Heart.SetSprite(HUD_Heart.HeartState.HeartContainer);
				}
			}
		}
		UpdateHearts(Target, false);
	}

	public Vector3 GetNextPosition()
	{
		int num = -1;
		while (++num < HeartIcons.Count)
		{
			if (!HeartIcons[num].gameObject.activeSelf)
			{
				return HeartIcons[num - 1].rectTransform.position + Vector3.right * (HeartIcons[num - 1].rectTransform.position.x / (float)num);
			}
		}
		return Vector3.zero;
	}
}
