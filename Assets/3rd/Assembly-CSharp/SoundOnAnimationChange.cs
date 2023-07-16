using System.Collections.Generic;
using Spine.Unity;

public class SoundOnAnimationChange : BaseMonoBehaviour
{
	public List<SoundOnAnimationData> Sounds = new List<SoundOnAnimationData>();

	public SkeletonAnimation spine;

	public int AnimationTrack;

	public string currentAnimationDebug;

	private string cs;

	private string CurrentAnimation
	{
		set
		{
			if (cs != value)
			{
				foreach (SoundOnAnimationData sound in Sounds)
				{
					AudioManager.Instance.StopLoop(sound.LoopedSound);
					if ((sound.SkeletonData.state.GetCurrent(AnimationTrack).ToString() == cs && sound.position == SoundOnAnimationData.Position.Beginning) || (sound.SkeletonData.state.GetCurrent(AnimationTrack).ToString() == cs && sound.position == SoundOnAnimationData.Position.End))
					{
						AudioManager.Instance.PlayOneShot(sound.AudioSourcePath, base.transform.position);
					}
					else if (sound.SkeletonData.state.GetCurrent(AnimationTrack).ToString() == sound.SkeletonsAnimations && sound.position == SoundOnAnimationData.Position.Loop)
					{
						sound.LoopedSound = AudioManager.Instance.CreateLoop(sound.AudioSourcePath, base.gameObject, true);
					}
				}
			}
			cs = value;
		}
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		DisableLoops();
	}

	private void Start()
	{
	}

	private void DisableLoops()
	{
		foreach (SoundOnAnimationData sound in Sounds)
		{
			if (sound.position == SoundOnAnimationData.Position.Loop)
			{
				AudioManager.Instance.StopLoop(sound.LoopedSound);
			}
		}
	}

	private void Update()
	{
		if (spine != null && spine.state.GetCurrent(AnimationTrack) != null)
		{
			CurrentAnimation = spine.state.GetCurrent(AnimationTrack).ToString();
			currentAnimationDebug = spine.state.GetCurrent(AnimationTrack).ToString();
		}
	}
}
