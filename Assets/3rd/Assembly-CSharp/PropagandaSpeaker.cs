using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Spine.Unity;
using UnityEngine;

public class PropagandaSpeaker : Interaction_AddFuel
{
	public SkeletonAnimation Spine;

	public SpriteRenderer RangeSprite;

	private static List<PropagandaSpeaker> PropagandaSpeakers = new List<PropagandaSpeaker>();

	private LayerMask playerMask;

	private Collider2D[] colliders;

	private ContactFilter2D filter;

	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	private float DistanceRadius = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	private Vector3 _updatePos;

	private EventInstance loopedInstance;

	private bool VOPlaying;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		fuelKey = "propaganda_fuel";
		PropagandaSpeakers.Add(this);
		if (GetComponentInParent<PlacementObject>() == null)
		{
			RangeSprite.DOColor(FadeOut, 0f).SetUpdate(true);
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		PropagandaSpeakers.Remove(this);
	}

	private void Start()
	{
		RangeSprite.size = Vector3.one * Structures_PropagandaSpeaker.EFFECTIVE_DISTANCE;
		playerMask = (int)playerMask | (1 << LayerMask.NameToLayer("Player"));
		filter = default(ContactFilter2D);
	}

	protected override void Update()
	{
		base.Update();
		if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval != 0 || PlayerFarming.Instance == null)
		{
			return;
		}
		if (!GameManager.overridePlayerPosition)
		{
			_updatePos = PlayerFarming.Instance.transform.position;
			DistanceRadius = 1f;
		}
		else
		{
			_updatePos = PlacementRegion.Instance.PlacementPosition;
			DistanceRadius = Structures_PropagandaSpeaker.EFFECTIVE_DISTANCE;
		}
		if (Vector3.Distance(_updatePos, base.transform.position) < DistanceRadius)
		{
			RangeSprite.gameObject.SetActive(true);
			RangeSprite.DOKill();
			RangeSprite.DOColor(StaticColors.OffWhiteColor, 0.5f).SetUpdate(true);
			distanceChanged = true;
		}
		else if (distanceChanged)
		{
			RangeSprite.DOKill();
			RangeSprite.DOColor(FadeOut, 0.5f).SetUpdate(true);
			distanceChanged = false;
		}
		FollowersInRange();
		if (base.Structure.Structure_Info != null && base.Structure.Structure_Info.FullyFueled && Spine.AnimationState.GetCurrent(0).Animation.Name != "on")
		{
			Spine.AnimationState.SetAnimation(0, "on", true);
			if (!VOPlaying)
			{
				loopedInstance = AudioManager.Instance.CreateLoop("event:/player/lamb_megaphone", base.gameObject, true);
				VOPlaying = true;
			}
		}
		else if (base.Structure.Structure_Info != null && !base.Structure.Structure_Info.FullyFueled && Spine.AnimationState.GetCurrent(0).Animation.Name != "off")
		{
			Spine.AnimationState.SetAnimation(0, "off", true);
			if (VOPlaying)
			{
				AudioManager.Instance.StopLoop(loopedInstance);
				VOPlaying = false;
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (VOPlaying)
		{
			AudioManager.Instance.StopLoop(loopedInstance);
			VOPlaying = false;
		}
	}

	private bool FollowersInRange()
	{
		if (base.Structure.Brain.Data.Fuel > 0)
		{
			BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
			if (boxCollider2D == null)
			{
				boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
				boxCollider2D.isTrigger = true;
			}
			boxCollider2D.size = Vector2.one * Structures_PropagandaSpeaker.EFFECTIVE_DISTANCE;
			boxCollider2D.transform.position = base.Structure.Brain.Data.Position;
			boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
			foreach (Follower follower in Follower.Followers)
			{
				if (!FollowerManager.FollowerLocked(follower.Brain.Info.ID) && boxCollider2D.OverlapPoint(follower.transform.position))
				{
					return true;
				}
			}
		}
		return false;
	}
}
