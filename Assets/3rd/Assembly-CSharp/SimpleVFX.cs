using System;
using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using Spine;
using Spine.Unity;
using UnityEngine;

[ExecuteAlways]
public class SimpleVFX : BaseMonoBehaviour
{
	public bool justPlayParticles;

	public bool setRotation = true;

	public float Rotation;

	public SkeletonAnimation Spine;

	public MeshRenderer mesh;

	[SpineAnimation("", "Spine", true, false)]
	public List<string> animationsToPlay = new List<string>();

	[SpineSlot("", "Spine", false, true, false)]
	public List<string> Slots = new List<string>();

	public List<ParticleSystem> particlesToPlay = new List<ParticleSystem>();

	public bool useCustomEndTime;

	public float customEndTime;

	private void OnEnable()
	{
		BiomeGenerator.OnBiomeChangeRoom += BiomeGenerator_OnBiomeChangeRoom;
	}

	private void BiomeGenerator_OnBiomeChangeRoom()
	{
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
		if (Spine != null && Spine.AnimationState != null)
		{
			Spine.AnimationState.Complete -= AnimationState_Complete;
		}
		if (useCustomEndTime && mesh != null)
		{
			mesh.enabled = false;
		}
		else if (ObjectPool.IsSpawned(base.gameObject))
		{
			base.gameObject.Recycle();
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	private void EnableSpine(TrackEntry entry)
	{
	}

	public void Play()
	{
		StopAllCoroutines();
		StartCoroutine(FrameDelay(delegate
		{
			Spine.AnimationState.Start += EnableSpine;
			int num = -1;
			if (!justPlayParticles)
			{
				float num2 = 0f;
				num = UnityEngine.Random.Range(0, animationsToPlay.Count - 1);
				string animationName = animationsToPlay[num];
				Spine.AnimationState.SetAnimation(0, animationName, false);
				mesh.enabled = true;
				base.gameObject.SetActive(true);
				Spine.gameObject.SetActive(true);
				Spine.AnimationState.Complete += AnimationState_Complete;
				if (useCustomEndTime)
				{
					StartCoroutine(customEndTimer());
				}
				if (setRotation)
				{
					Rotation = num2 + (float)UnityEngine.Random.Range(-10, 10);
				}
			}
			if (particlesToPlay.Count > 0)
			{
				if (particlesToPlay[0] != null)
				{
					particlesToPlay[0].gameObject.SetActive(true);
				}
				num = UnityEngine.Random.Range(0, particlesToPlay.Count - 1);
				particlesToPlay[num].Stop();
				particlesToPlay[num].Play();
			}
		}));
	}

	public void Play(Vector3 Position, float Angle = 0f)
	{
		StopAllCoroutines();
		base.gameObject.SetActive(true);
		base.gameObject.transform.position = Position;
		StartCoroutine(FrameDelay(delegate
		{
			int num = -1;
			if (!justPlayParticles)
			{
				mesh.enabled = false;
				num = UnityEngine.Random.Range(0, animationsToPlay.Count - 1);
				string animationName = animationsToPlay[num];
				Spine.AnimationState.SetAnimation(0, animationName, false);
				mesh.enabled = true;
				Spine.gameObject.SetActive(true);
				Spine.AnimationState.Complete += AnimationState_Complete;
				if (useCustomEndTime)
				{
					StartCoroutine(customEndTimer());
				}
				if (setRotation)
				{
					Rotation = Angle + (float)UnityEngine.Random.Range(-10, 10);
				}
			}
			if (particlesToPlay.Count > 0)
			{
				if (particlesToPlay[0] != null)
				{
					particlesToPlay[0].gameObject.SetActive(true);
				}
				base.transform.position = Position;
				num = UnityEngine.Random.Range(0, particlesToPlay.Count - 1);
				particlesToPlay[num].Stop();
				particlesToPlay[num].Play();
			}
		}));
	}

	public void Play(Vector3 Position, float Angle, string animation)
	{
		StopAllCoroutines();
		StartCoroutine(FrameDelay(delegate
		{
			if (!justPlayParticles)
			{
				mesh.enabled = false;
				base.transform.position = Position;
				if (Spine != null)
				{
					Spine.gameObject.SetActive(true);
					if (Spine.AnimationState != null)
					{
						Spine.AnimationState.SetAnimation(0, animation, false);
						Spine.AnimationState.Complete += AnimationState_Complete;
					}
				}
				mesh.enabled = true;
				base.gameObject.SetActive(true);
				if (useCustomEndTime)
				{
					StartCoroutine(customEndTimer());
				}
				if (setRotation)
				{
					Rotation = Angle + (float)UnityEngine.Random.Range(-10, 10);
				}
			}
			if (particlesToPlay.Count > 0)
			{
				if (particlesToPlay[0] != null)
				{
					particlesToPlay[0].gameObject.SetActive(true);
				}
				base.transform.position = Position;
				int index = UnityEngine.Random.Range(0, particlesToPlay.Count - 1);
				particlesToPlay[index].Stop();
				particlesToPlay[index].Play();
			}
		}));
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator customEndTimer()
	{
		yield return new WaitForSeconds(customEndTime);
		if (base.gameObject.activeInHierarchy)
		{
			base.gameObject.Recycle();
		}
	}

	private void PlayAnimation()
	{
		Play(base.transform.position, UnityEngine.Random.Range(0, 360));
	}

	private void AnimationState_Complete(TrackEntry trackEntry)
	{
		Spine.AnimationState.Complete -= AnimationState_Complete;
		if (useCustomEndTime)
		{
			mesh.enabled = false;
		}
		else if (base.gameObject.activeInHierarchy)
		{
			base.gameObject.Recycle();
		}
	}

	private void OnDisable()
	{
		if (Spine != null && Spine.AnimationState != null)
		{
			Spine.AnimationState.Start -= EnableSpine;
			Spine.AnimationState.Complete -= AnimationState_Complete;
		}
		BiomeGenerator.OnBiomeChangeRoom -= BiomeGenerator_OnBiomeChangeRoom;
	}

	private void Update()
	{
		if (!justPlayParticles && setRotation)
		{
			base.transform.eulerAngles = new Vector3(-60f, 0f, Rotation);
		}
	}
}
