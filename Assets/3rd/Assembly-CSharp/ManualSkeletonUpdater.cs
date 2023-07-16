using Spine.Unity;
using UnityEngine;

public class ManualSkeletonUpdater
{
	public enum AnimationQuality
	{
		High = 2,
		Normal = 4,
		Low = 6
	}

	public int AnimationFramerate = 25;

	private SkeletonAnimation skeletonAnimation;

	private bool onScreenUpdate;

	private float deltaTimeSumBetweenLastUpdate;

	private float _accumDeltaTime;

	private float _accumUnscaledDeltaTime;

	private int FrameIntervalOffset;

	private bool canChangeAnimationQuality;

	private bool visible;

	private bool stopUpdates;

	private static int LastInterval;

	private int maxInterval = 4;

	private float TimeInterval
	{
		get
		{
			return 1f / (float)AnimationFramerate;
		}
	}

	public bool OnScreenUpdate
	{
		get
		{
			return onScreenUpdate;
		}
		set
		{
			onScreenUpdate = value;
		}
	}

	public bool CanChangeAnimationQuality
	{
		get
		{
			return canChangeAnimationQuality;
		}
		set
		{
			canChangeAnimationQuality = value;
		}
	}

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			visible = value;
		}
	}

	public bool StopUpdates
	{
		get
		{
			return stopUpdates;
		}
		set
		{
			stopUpdates = value;
		}
	}

	public ManualSkeletonUpdater(SkeletonAnimation skeletonAnimation)
	{
		this.skeletonAnimation = skeletonAnimation;
		skeletonAnimation.Initialize(false);
		skeletonAnimation.clearStateOnDisable = false;
		skeletonAnimation.enabled = false;
		deltaTimeSumBetweenLastUpdate = 0f;
		ManualUpdate(true);
		ChangeAnimationQuality(AnimationQuality.Normal);
	}

	public void ChangeAnimationQuality(AnimationQuality quality)
	{
		if (canChangeAnimationQuality)
		{
			maxInterval = (int)quality;
		}
		else
		{
			maxInterval = 4;
		}
		skeletonAnimation.UpdateInterval = maxInterval;
		FrameIntervalOffset = Random.Range(0, skeletonAnimation.UpdateInterval);
	}

	public void Update()
	{
		deltaTimeSumBetweenLastUpdate += Time.deltaTime;
		if (!onScreenUpdate)
		{
			if (deltaTimeSumBetweenLastUpdate >= TimeInterval && !stopUpdates)
			{
				ManualUpdate(false);
			}
		}
		else if (Visible && (Time.frameCount + FrameIntervalOffset) % skeletonAnimation.UpdateInterval == 0 && deltaTimeSumBetweenLastUpdate >= TimeInterval)
		{
			ManualUpdate(true);
		}
	}

	private void ManualUpdate(bool isOnScreen)
	{
		skeletonAnimation.Update(deltaTimeSumBetweenLastUpdate);
		if (!isOnScreen)
		{
			deltaTimeSumBetweenLastUpdate %= TimeInterval;
		}
		if ((Time.frameCount + FrameIntervalOffset) % skeletonAnimation.UpdateInterval == 0 && deltaTimeSumBetweenLastUpdate >= TimeInterval && Visible)
		{
			skeletonAnimation.BaseLateUpdate();
			deltaTimeSumBetweenLastUpdate = 0f;
		}
	}

	public void ForceFirstUpdate()
	{
		skeletonAnimation.Update(0f);
		skeletonAnimation.BaseLateUpdate();
	}
}
