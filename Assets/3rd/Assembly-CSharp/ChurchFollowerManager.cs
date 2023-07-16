using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ChurchFollowerManager : BaseMonoBehaviour
{
	public enum OverlayType
	{
		Ritual,
		Sermon,
		Sacrifice
	}

	public enum EffectType
	{
		Ritual,
		Sermon
	}

	public static ChurchFollowerManager Instance;

	public DevotionCounterOverlay devotionCounterOverlay;

	public Transform DoorPosition;

	public Transform RitualCenterPosition;

	public Transform WatchSermonPosition;

	public Transform AltarPosition;

	public GameObject Altar;

	public GameObject RitualCameraPosition;

	public Gradient ColorGradient;

	public Interaction_FeastTable FeastTable;

	public Interaction_FireDancePit FirePit;

	public SkeletonGraphic RitualOverlay;

	public SkeletonGraphic SermonOverlay;

	public SkeletonGraphic SacrificeOverlay;

	public SkeletonAnimation RitualEffect;

	public SkeletonAnimation SermonEffect;

	public SkeletonAnimation PortalEffect;

	public GameObject Light;

	public GameObject GodRays;

	public GameObject Mushrooms;

	public Animator Goop;

	public GameObject Water;

	public GameObject FarmMud;

	public ParticleSystem Sparkles;

	public CanvasGroup RedOverlay;

	[Space]
	[SerializeField]
	private GameObject[] studySlots;

	private int randomNumber;

	private List<int> _audienceMemberBrainIDs = new List<int>();

	private Vector3[] _audiencePositions = new Vector3[22]
	{
		new Vector3(-0.5f, 2.5f),
		new Vector3(0.5f, 2.5f),
		new Vector3(-1.5f, 2.5f),
		new Vector3(1.5f, 2.5f),
		new Vector3(0f, 1.5f),
		new Vector3(-1f, 1.5f),
		new Vector3(1f, 1.5f),
		new Vector3(-2f, 1.5f),
		new Vector3(2f, 1.5f),
		new Vector3(-0.5f, 0.5f),
		new Vector3(0.5f, 0.5f),
		new Vector3(-1.5f, 0.5f),
		new Vector3(1.5f, 0.5f),
		new Vector3(0f, -0.5f),
		new Vector3(-1f, -0.5f),
		new Vector3(1f, -0.5f),
		new Vector3(-2f, -0.5f),
		new Vector3(2f, -0.5f),
		new Vector3(-0.5f, -1.5f),
		new Vector3(0.5f, -1.5f),
		new Vector3(-1.5f, -1.5f),
		new Vector3(1.5f, -1.5f)
	};

	public GameObject[] StudySlots
	{
		get
		{
			return studySlots;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private void Start()
	{
		DisableAllOverlays();
		DisableAllEffects();
		PortalEffect.gameObject.SetActive(false);
		GodRays.SetActive(false);
	}

	public void DisableAllOverlays()
	{
		RitualOverlay.gameObject.SetActive(false);
		SermonOverlay.gameObject.SetActive(false);
		SacrificeOverlay.gameObject.SetActive(false);
		GodRays.gameObject.SetActive(false);
	}

	private void DisableAllEffects()
	{
		RitualEffect.gameObject.SetActive(false);
		SermonEffect.gameObject.SetActive(false);
	}

	public void PlayOverlay(OverlayType overlayType, string AnimationName)
	{
		SkeletonGraphic skeletonGraphic = null;
		switch (overlayType)
		{
		case OverlayType.Ritual:
			skeletonGraphic = RitualOverlay;
			break;
		case OverlayType.Sermon:
			skeletonGraphic = SermonOverlay;
			break;
		case OverlayType.Sacrifice:
			skeletonGraphic = SacrificeOverlay;
			break;
		}
		skeletonGraphic.gameObject.SetActive(true);
		skeletonGraphic.AnimationState.SetAnimation(0, AnimationName, false);
		skeletonGraphic.AnimationState.Complete += OverlayComplete;
	}

	public void StartRitualOverlay()
	{
		RitualOverlay.gameObject.SetActive(true);
		RitualOverlay.AnimationState.SetAnimation(0, "ritual-in", false);
		RitualOverlay.AnimationState.AddAnimation(0, "ritual-loop", true, 0f);
	}

	public void EndRitualOverlay()
	{
		RitualOverlay.AnimationState.AddAnimation(0, "ritual-out", false, 0f).Complete += OverlayComplete;
	}

	public void StartSermonEffect()
	{
		randomNumber = UnityEngine.Random.Range(1, 5);
		SermonEffect.gameObject.SetActive(true);
		SermonEffect.AnimationState.SetAnimation(0, "sermons/" + randomNumber + "-in", false);
		SermonEffect.AnimationState.AddAnimation(0, "sermons/" + randomNumber + "-loop", true, 0f);
		SermonOverlay.gameObject.SetActive(true);
		SermonOverlay.AnimationState.SetAnimation(0, "sermon-start", false);
		SermonOverlay.AnimationState.AddAnimation(0, "sermon-loop", true, 0f);
	}

	public void EndSermonEffect()
	{
		SermonEffect.AnimationState.AddAnimation(0, "sermons/" + randomNumber + "-out", false, 0f).Complete += OverlayComplete;
		SermonOverlay.AnimationState.AddAnimation(0, "sermon-stop", false, 0f);
	}

	public void StartSermonEffectClean()
	{
		SermonEffect.gameObject.SetActive(true);
		SermonEffect.AnimationState.SetAnimation(0, "clean-in", false);
		SermonEffect.AnimationState.AddAnimation(0, "clean-loop", true, 0f);
		SermonOverlay.gameObject.SetActive(true);
		SermonOverlay.AnimationState.SetAnimation(0, "sermon-start", false);
		SermonOverlay.AnimationState.AddAnimation(0, "sermon-loop", true, 0f);
	}

	public void EndSermonEffectClean()
	{
		SermonEffect.AnimationState.AddAnimation(0, "clean-out", false, 0f).Complete += OverlayComplete;
		SermonOverlay.AnimationState.AddAnimation(0, "sermon-stop", false, 0f);
	}

	public void StartSermonOverlay()
	{
		SermonOverlay.gameObject.SetActive(true);
		SermonOverlay.AnimationState.SetAnimation(0, "1", true);
	}

	public void EndSermonOverlay()
	{
		SermonOverlay.AnimationState.AddAnimation(0, "1", false, 0f).Complete += OverlayComplete;
	}

	public void PlayEffect(EffectType effectType, string AnimationName)
	{
		SkeletonAnimation skeletonAnimation = null;
		switch (effectType)
		{
		case EffectType.Ritual:
			skeletonAnimation = RitualEffect;
			break;
		case EffectType.Sermon:
			skeletonAnimation = SermonEffect;
			break;
		}
		skeletonAnimation.gameObject.SetActive(true);
		skeletonAnimation.AnimationState.SetAnimation(0, AnimationName, false);
		skeletonAnimation.AnimationState.Complete += EffectComplete;
	}

	public void UpdateChurch()
	{
		int num = -1;
		while (++num < studySlots.Length)
		{
			if (num <= 1)
			{
				studySlots[num].SetActive(DataManager.Instance.HasBuiltTemple4);
			}
			else if (num > 1 && num <= 3)
			{
				studySlots[num].SetActive(UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Temple_MonksUpgrade));
			}
		}
	}

	public void PlayPortalEffect()
	{
		PortalEffect.gameObject.SetActive(true);
		PortalEffect.AnimationState.SetAnimation(0, "start-ritual", false);
		PortalEffect.AnimationState.AddAnimation(0, "loop-ritual", true, 0f);
	}

	public void StopPortalEffect()
	{
		PortalEffect.AnimationState.SetAnimation(0, "stop-ritual", false);
		PortalEffect.AnimationState.Complete += PortalEffectComplete;
	}

	public void PlaySacrificePortalEffect()
	{
		PortalEffect.gameObject.SetActive(true);
		PortalEffect.AnimationState.SetAnimation(0, "start", false);
		PortalEffect.AnimationState.AddAnimation(0, "animation", true, 0f);
	}

	public void StopSacrificePortalEffect()
	{
		PortalEffect.AnimationState.SetAnimation(0, "stop", false);
		PortalEffect.AnimationState.Complete += PortalEffectComplete;
	}

	private void OverlayComplete(TrackEntry trackEntry)
	{
		trackEntry.Complete -= OverlayComplete;
	}

	private void EffectComplete(TrackEntry trackEntry)
	{
		trackEntry.Complete -= EffectComplete;
		DisableAllEffects();
	}

	private void PortalEffectComplete(TrackEntry trackEntry)
	{
		PortalEffect.AnimationState.Complete -= PortalEffectComplete;
		trackEntry.Complete -= EffectComplete;
		PortalEffect.gameObject.SetActive(false);
	}

	public Vector3 GetSlotPosition(int index)
	{
		return studySlots[index].transform.position;
	}

	public int GetClosestSlotIndex(Vector3 pos)
	{
		GameObject gameObject = studySlots[0];
		int result = 0;
		for (int i = 0; i < studySlots.Length; i++)
		{
			if (Vector3.Distance(studySlots[i].transform.position, pos) < Vector3.Distance(studySlots[i].transform.position, gameObject.transform.position))
			{
				gameObject = studySlots[i];
				result = i;
			}
		}
		return result;
	}

	public void ExitAllFollowers()
	{
	}

	public void AddBrainToAudience(FollowerBrain brain)
	{
		if (!_audienceMemberBrainIDs.Contains(brain.Info.ID))
		{
			_audienceMemberBrainIDs.Add(brain.Info.ID);
		}
	}

	public Vector3 GetAudienceMemberPosition(FollowerBrain brain)
	{
		int num = _audienceMemberBrainIDs.IndexOf(brain.Info.ID);
		if (num == -1)
		{
			AddBrainToAudience(brain);
			num = _audienceMemberBrainIDs.IndexOf(brain.Info.ID);
		}
		Vector3 vector = ((num == -1 || num >= _audiencePositions.Length) ? _audiencePositions[_audiencePositions.Length - 1] : _audiencePositions[num]);
		return RitualCenterPosition.position + vector + new Vector3(0f, -0.5f);
	}

	public void ClearAudienceBrains()
	{
		_audienceMemberBrainIDs.Clear();
	}

	public void RemoveBrainFromAudience(FollowerBrain brain)
	{
		if (_audienceMemberBrainIDs.Contains(brain.Info.ID))
		{
			_audienceMemberBrainIDs.Remove(brain.Info.ID);
		}
	}

	public Vector3 GetCirclePosition(FollowerBrain brain)
	{
		int num = _audienceMemberBrainIDs.IndexOf(brain.Info.ID);
		float num2;
		float f;
		if (_audienceMemberBrainIDs.Count <= 12)
		{
			num2 = 2f;
			f = (float)num * (360f / (float)_audienceMemberBrainIDs.Count) * ((float)Math.PI / 180f);
			return RitualCenterPosition.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
		}
		int num3 = 8;
		if (num < num3)
		{
			num2 = 2f;
			f = (float)num * (360f / (float)Mathf.Min(_audienceMemberBrainIDs.Count, num3)) * ((float)Math.PI / 180f);
		}
		else
		{
			num2 = 3f;
			f = (float)(num - num3) * (360f / (float)(_audienceMemberBrainIDs.Count - num3)) * ((float)Math.PI / 180f);
		}
		return RitualCenterPosition.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f));
	}

	public IEnumerator DoSacrificeRoutine(Interaction interaction, int sacrificeID, Action onComplete)
	{
		yield return null;
	}
}
