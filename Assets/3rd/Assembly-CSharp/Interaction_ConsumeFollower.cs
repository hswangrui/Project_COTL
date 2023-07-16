using System;
using System.Collections;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_ConsumeFollower : Interaction
{
	public SkeletonAnimation followerSpine;

	private FollowerInfo followerInfo;

	private FollowerOutfit outfit;

	private Action<int, int, int, int> Callback;

	private Follower follower;

	private string sString;

	private int HP;

	private int SPIRITHP;

	private int BlueHeart;

	private int BlackHeart;

	private float TotalHealth;

	private string HeartDispayString;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.SacrificeFollower + ": <color=#FFD201>" + followerInfo.Name + (followerInfo.MarriedToLeader ? " <sprite name=\"icon_Married\">" : "") + "</color>";
	}

	public void Play(FollowerInfo followerInfo, Action<int, int, int, int> Callback)
	{
		ActivateDistance = 2f;
		this.followerInfo = followerInfo;
		outfit = new FollowerOutfit(followerInfo);
		this.Callback = Callback;
		outfit.SetOutfit(followerSpine, false);
		follower = GetComponent<Follower>();
		SetHearts();
	}

	private void SetHearts()
	{
		TotalHealth = DataManager.Instance.PLAYER_TOTAL_HEALTH + DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS;
		switch (followerInfo.CursedState)
		{
		case Thought.OldAge:
			BlueHeart = 2;
			break;
		case Thought.Dissenter:
			BlackHeart = 2;
			break;
		default:
		{
			int num = followerInfo.XPLevel * 2;
			HP = 0;
			SPIRITHP = 0;
			while (num > 0)
			{
				if ((float)HP < DataManager.Instance.PLAYER_TOTAL_HEALTH)
				{
					HP++;
					num--;
					if ((float)HP == DataManager.Instance.PLAYER_TOTAL_HEALTH && HP % 2 == 1)
					{
						num--;
					}
				}
				else
				{
					if (!((float)SPIRITHP < DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS))
					{
						break;
					}
					SPIRITHP++;
					num--;
				}
			}
			break;
		}
		}
		if (DataManager.Instance.PlayerFleece == 1000)
		{
			BlackHeart += 4;
		}
		if (DataManager.Instance.PlayerFleece != 5)
		{
			HP = (int)Mathf.Clamp(HP, 1f, TotalHealth);
		}
		else
		{
			BlueHeart += followerInfo.XPLevel * 2;
			HP = (SPIRITHP = 0);
		}
		SetHeartDisplayString();
	}

	private void SetHeartDisplayString()
	{
		HeartDispayString = "";
		switch (followerInfo.CursedState)
		{
		case Thought.OldAge:
			HeartDispayString += FollowerThoughts.GetLocalisedName(followerInfo.CursedState);
			break;
		case Thought.Dissenter:
			HeartDispayString += FollowerThoughts.GetLocalisedName(followerInfo.CursedState);
			break;
		default:
			HeartDispayString = HeartDispayString + ScriptLocalization.Interactions.Level + " " + followerInfo.XPLevel.ToNumeral();
			break;
		}
		if (followerInfo.MarriedToLeader)
		{
			BlueHeart += 6;
		}
		HeartDispayString += ": ";
		int num = -1;
		int num2 = (int)DataManager.Instance.PLAYER_TOTAL_HEALTH + (int)DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS;
		int num3 = HP;
		int num4 = SPIRITHP;
		while (++num < num2)
		{
			if (num3 >= 2)
			{
				HeartDispayString += "<sprite name=\"icon_UIHeart\">";
				num3 -= 2;
			}
			else if (num3 == 1)
			{
				if ((float)num == DataManager.Instance.PLAYER_TOTAL_HEALTH - 1f && DataManager.Instance.PLAYER_TOTAL_HEALTH % 2f != 0f)
				{
					HeartDispayString += "<sprite name=\"icon_UIHeartHalfEmpty\">";
				}
				else
				{
					HeartDispayString += "<sprite name=\"icon_UIHeartHalf\">";
				}
				num3 -= 2;
			}
			else if (num4 >= 2)
			{
				HeartDispayString += "<sprite name=\"icon_UIHeartSpirit\">";
				num4 -= 2;
			}
			else if (num4 == 1)
			{
				HeartDispayString += "<sprite name=\"icon_UIHeartHalfSpirit\">";
				num4 -= 2;
			}
			else if ((float)num >= DataManager.Instance.PLAYER_TOTAL_HEALTH)
			{
				HeartDispayString += "<sprite name=\"icon_UIHeartSpiritEmpty\">";
			}
			else if ((float)num == DataManager.Instance.PLAYER_TOTAL_HEALTH - 1f && DataManager.Instance.PLAYER_TOTAL_HEALTH % 2f != 0f)
			{
				HeartDispayString += "<sprite name=\"icon_UIHeartHalfFull\">";
			}
			else
			{
				HeartDispayString += "<sprite name=\"icon_UIHeartEmpty\">";
			}
			num++;
		}
		if (BlueHeart > 0)
		{
			num = -1;
			while (++num < BlueHeart / 2)
			{
				HeartDispayString += "<sprite name=\"icon_UIHeartBlue\">";
			}
		}
		num = -1;
		while (++num < BlackHeart / 2)
		{
			HeartDispayString += "<sprite name=\"icon_UIHeartBlack\">";
		}
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		MonoSingleton<Indicator>.Instance.ShowTopInfo("<sprite name=\"img_SwirleyLeft\"> " + sString + " <sprite name=\"img_SwirleyRight\">");
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		MonoSingleton<Indicator>.Instance.HideTopInfo();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		FollowerManager.ConsumeFollower(followerInfo.ID);
		state.CURRENT_STATE = StateMachine.State.InActive;
		Action<int, int, int, int> callback = Callback;
		if (callback != null)
		{
			callback(HP, SPIRITHP, BlueHeart, BlackHeart);
		}
		MonoSingleton<Indicator>.Instance.HideTopInfo();
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
		follower.SetBodyAnimation("worship-eyesopen", true);
		StartCoroutine(ShowIcons());
	}

	private IEnumerator ShowIcons()
	{
		yield return new WaitForEndOfFrame();
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
		follower.SetBodyAnimation("pray", true);
	}

	public override void GetLabel()
	{
		base.Label = HeartDispayString;
	}
}
