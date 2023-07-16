using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class ChurchFollower : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	public WorshipperInfoManager wim;

	public WorshipperBubble worshipperBubble;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimIdle;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimHoodUp;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimHoodDown;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimWalking;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimPray;

	[SpineAnimation("", "SkeletonData", true, false)]
	public string AnimWorship;

	public Gradient ColorGradient;

	private Action Callback;

	public void HoodOn(string Animation, bool Snap)
	{
		if (Snap)
		{
			wim.SetOutfit(WorshipperInfoManager.Outfit.Follower, true);
			Spine.AnimationState.SetAnimation(0, Animation, true);
		}
		else
		{
			StartCoroutine(PutHoodOn(Animation));
		}
	}

	private IEnumerator PutHoodOn(string Animation)
	{
		if (!wim.IsHooded)
		{
			Spine.AnimationState.SetAnimation(0, AnimHoodUp, false);
			Spine.AnimationState.AddAnimation(0, Animation, true, 0f);
			yield return new WaitForSeconds(19f / 30f);
			wim.SetOutfit(WorshipperInfoManager.Outfit.Follower, true);
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, Animation, true);
		}
		yield return new WaitForSeconds(1f / 3f);
	}

	public void Pray()
	{
		Spine.AnimationState.SetAnimation(0, AnimPray, true);
	}

	public void Worship()
	{
		Spine.AnimationState.SetAnimation(0, AnimWorship, true);
	}

	public void WorshipUp()
	{
		Spine.AnimationState.SetAnimation(0, "idle-ritual-up", true);
	}

	public void Dance()
	{
		Spine.AnimationState.SetAnimation(0, "dance-hooded", true);
	}

	public void DevotionLoop()
	{
		Spine.AnimationState.SetAnimation(0, "devotion/devotion-waiting", true);
	}

	public void DevotionRefuseSoul()
	{
		Spine.AnimationState.SetAnimation(0, "devotion/refuse", false);
		Spine.AnimationState.AddAnimation(0, "devotion/refused", true, 0f);
	}

	public void AnimateWhenPlayerNear(string Animation, string AnimationNear)
	{
		StartCoroutine(AnimateWhenPlayerNearRouitine(Animation, AnimationNear));
	}

	public void ArriveAtDevotionCircle()
	{
		StartCoroutine(ArriveAtDevotionCircleRoutine());
	}

	private IEnumerator ArriveAtDevotionCircleRoutine()
	{
		FacePosition(ChurchFollowerManager.Instance.RitualCenterPosition.position);
		yield return StartCoroutine(PutHoodOn("devotion/devotion-waiting"));
		AnimateWhenPlayerNear("worship", "devotion/devotion-waiting");
	}

	public void ArriveAtRitualCircle()
	{
		StartCoroutine(ArriveAtRitualCircleRoutine());
	}

	private IEnumerator ArriveAtRitualCircleRoutine()
	{
		FacePosition(ChurchFollowerManager.Instance.RitualCenterPosition.position);
		yield return StartCoroutine(PutHoodOn("ritual1"));
	}

	public void ArriveAtSermonAudience()
	{
		StartCoroutine(ArriveAtSermonAudienceRoutine());
	}

	private IEnumerator ArriveAtSermonAudienceRoutine()
	{
		FacePosition(ChurchFollowerManager.Instance.AltarPosition.position);
		yield return PutHoodOn("devotion/devotion-waiting");
	}

	private IEnumerator AnimateWhenPlayerNearRouitine(string Animation, string AnimationNear)
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		while (true)
		{
			if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > 1.5f)
			{
				if (PlayerFarming.Instance.transform.position.y > base.transform.position.y)
				{
					if (Spine.AnimationName != "idle-ritual-up")
					{
						Spine.AnimationState.SetAnimation(0, "idle-ritual-up", true);
					}
				}
				else if (Spine.AnimationName != Animation)
				{
					Spine.AnimationState.SetAnimation(0, Animation, true);
				}
			}
			else if (Spine.AnimationName != AnimationNear)
			{
				Spine.AnimationState.SetAnimation(0, AnimationNear, true);
			}
			yield return null;
		}
	}

	public void HoodOff(string Animation = "idle")
	{
		if (wim.IsHooded)
		{
			StartCoroutine(TakeHoodOff(Animation));
		}
	}

	private IEnumerator TakeHoodOff(string Animation = "idle")
	{
		Spine.AnimationState.SetAnimation(0, AnimHoodDown, false);
		Spine.AnimationState.AddAnimation(0, Animation, true, 0f);
		yield return new WaitForSeconds(0.5f);
		Spine.Skeleton.SetSkin(wim.v_i.SkinName);
		yield return new WaitForSeconds(0.5f);
	}

	public void TakeHoodOffAndGo(Vector3 Destination, Action Callback)
	{
		StartCoroutine(TakeHoodOffAndGoRoutine(Destination, Callback));
	}

	private IEnumerator TakeHoodOffAndGoRoutine(Vector3 Destination, Action Callback)
	{
		yield return StartCoroutine(TakeHoodOff());
		GoTo(Destination, Callback);
	}

	public void ApplySermonEffect(string Anim, Action<ChurchFollower> EffectCallback)
	{
		StartCoroutine(ApplySermonEffectRoutine(Anim, EffectCallback));
	}

	public IEnumerator ApplySermonEffectRoutine(string Anim, Action<ChurchFollower> EffectCallback)
	{
		bool hoodAnimfinished = false;
		Spine.AnimationState.SetAnimation(0, AnimHoodDown, false);
		Spine.AnimationState.Complete += delegate
		{
			hoodAnimfinished = true;
		};
		while (!hoodAnimfinished)
		{
			yield return null;
		}
		Spine.Skeleton.SetSkin(wim.v_i.SkinName);
		bool reactAnimFinished = false;
		Spine.AnimationState.SetAnimation(0, Anim, false);
		Spine.AnimationState.Complete += delegate
		{
			reactAnimFinished = true;
		};
		while (!reactAnimFinished)
		{
			yield return null;
		}
		if (EffectCallback != null)
		{
			EffectCallback(this);
		}
		yield return new WaitForSeconds(0.5f);
	}

	public void GoToDoor()
	{
		GoTo(ChurchFollowerManager.Instance.DoorPosition.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f), DestroySelf);
	}

	public void GoTo(Vector3 Destination, Action Callback)
	{
		this.Callback = Callback;
		StartCoroutine(GoToRoutine(Destination));
	}

	private IEnumerator GoToRoutine(Vector3 Destination)
	{
		Spine.AnimationState.SetAnimation(0, AnimWalking, true);
		Vector3 StartPosition = base.transform.position;
		float Progress = 0f;
		float num = 2f + UnityEngine.Random.Range(-0.2f, 0.2f);
		float Duration = Vector3.Distance(StartPosition, Destination) / num;
		while (true)
		{
			float num2;
			Progress = (num2 = Progress + Time.deltaTime);
			if (!(num2 < Duration))
			{
				break;
			}
			FacePosition(Destination);
			base.transform.position = Vector3.Lerp(StartPosition, Destination, Progress / Duration);
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, AnimIdle, true);
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
	}

	public void FacePosition(Vector3 PositionToFace)
	{
		Spine.Skeleton.ScaleX = ((!(base.transform.position.x < PositionToFace.x)) ? 1 : (-1));
	}

	public void DestroySelf()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDisable()
	{
	}
}
