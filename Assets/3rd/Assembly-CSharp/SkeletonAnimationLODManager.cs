using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkeletonAnimationLODManager : MonoBehaviour
{
	private const bool DISABLE_LOD_MANAGER = false;

	private const float DELAY_BETWEEN_CHECKS = 0.5f;

	private const int MINIMUM_FOLLOWERS_COUNT_ON_SCREEN = 10;

	private const float LOD_DISTANCE_OFFSCREEN = 12f;

	private const float LOD_DISTANCE_LOW = 8f;

	[SerializeField]
	private SkeletonAnimation skeletonAnimation;

	private Transform source;

	private ManualSkeletonUpdater skeletonUpdater;

	private SimpleSpineAnimator simpleSpineAnimator;

	private Camera mainCamera;

	private bool doUpdate = true;

	private bool isInBaseBiome;

	private float randomSprayBetweemCheck;

	private bool canChangeAnimationQuality;

	private float timer;

	private static Dictionary<string, SkeletonAnimation> visibleSkeletons = new Dictionary<string, SkeletonAnimation>();

	private bool doSceneCheck = true;

	private bool doDistanceCheck = true;

	private bool isInitialized;

	public bool DoUpdate
	{
		get
		{
			return doUpdate;
		}
		set
		{
			if (skeletonAnimation != null)
			{
				skeletonAnimation.enabled = value;
			}
		}
	}

	public void SetSkeletonAnimation(SkeletonAnimation anim)
	{
		skeletonAnimation = anim;
	}

	private void Start()
	{
	}

	public void Initialise(SkeletonAnimation skeletonAnimation, bool doSceneCheck = true, bool distanceCheck = false, ManualSkeletonUpdater.AnimationQuality quality = ManualSkeletonUpdater.AnimationQuality.Normal)
	{
		SetSkeletonAnimation(skeletonAnimation);
		this.doSceneCheck = doSceneCheck;
		doDistanceCheck = distanceCheck;
		ManualEnable();
		if (skeletonUpdater != null)
		{
			skeletonUpdater.CanChangeAnimationQuality = true;
			skeletonUpdater.ChangeAnimationQuality(quality);
			OnVisibilityChange(true);
		}
	}

	public void ManualEnable()
	{
		Start();
		OnEnable();
		isInitialized = true;
	}

	public void DisableLODManager(bool state)
	{
		doUpdate = !state;
		skeletonAnimation.enabled = state;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		if ((isInBaseBiome || !doSceneCheck) && visibleSkeletons.ContainsKey(base.gameObject.name))
		{
			visibleSkeletons.Remove(base.gameObject.name);
		}
	}

	private void Update()
	{
	}

	private void OnVisibilityChange(bool visible)
	{
		if (visible)
		{
			OnVisible();
		}
		else
		{
			OnInvisible();
		}
		if (doSceneCheck)
		{
			if (visibleSkeletons.Count >= 10)
			{
				skeletonUpdater.CanChangeAnimationQuality = true;
			}
			else
			{
				skeletonUpdater.CanChangeAnimationQuality = false;
			}
		}
		skeletonUpdater.Visible = visible;
	}

	private void OnVisible()
	{
		if (!skeletonUpdater.Visible)
		{
			if (!visibleSkeletons.ContainsKey(base.gameObject.name))
			{
				visibleSkeletons.Add(base.gameObject.name, skeletonAnimation);
			}
			skeletonUpdater.AnimationFramerate = 25;
			skeletonUpdater.OnScreenUpdate = true;
			skeletonUpdater.StopUpdates = false;
		}
	}

	private void OnInvisible()
	{
		if (visibleSkeletons.ContainsKey(base.gameObject.name))
		{
			visibleSkeletons.Remove(base.gameObject.name);
		}
		skeletonUpdater.StopUpdates = false;
		if (simpleSpineAnimator != null)
		{
			StateMachine.State getCurrentState = simpleSpineAnimator.GetCurrentState;
			if ((uint)getCurrentState <= 1u || (uint)(getCurrentState - 17) <= 1u || getCurrentState == StateMachine.State.Meditate)
			{
				skeletonUpdater.StopUpdates = true;
			}
		}
		else
		{
			skeletonUpdater.StopUpdates = true;
		}
		skeletonUpdater.AnimationFramerate = 25;
		skeletonUpdater.OnScreenUpdate = false;
	}

	private void CheckCurrentScene()
	{
		if (doSceneCheck)
		{
			if (SceneManager.GetActiveScene().name == "Base Biome 1")
			{
				isInBaseBiome = true;
			}
			else
			{
				isInBaseBiome = false;
			}
		}
	}
}
