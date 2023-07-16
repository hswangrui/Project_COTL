using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SimpleParticleVFX : BaseMonoBehaviour
{
	public bool setRotation = true;

	public float Rotation;

	public List<ParticleSystem> particlesToPlay = new List<ParticleSystem>();

	public bool useCustomEndTime;

	public float customEndTime;

	public void Play()
	{
		StopAllCoroutines();
		float num = 0f;
		base.gameObject.SetActive(true);
		if (useCustomEndTime)
		{
			StartCoroutine(customEndTimer());
		}
		if (setRotation)
		{
			Rotation = num + (float)Random.Range(-10, 10);
		}
		foreach (ParticleSystem item in particlesToPlay)
		{
			item.Play();
		}
	}

	public void Play(Vector3 Position, float Angle = 0f)
	{
		StopAllCoroutines();
		base.transform.position = Position;
		base.gameObject.SetActive(true);
		if (useCustomEndTime)
		{
			StartCoroutine(customEndTimer());
		}
		if (setRotation)
		{
			Rotation = Angle + (float)Random.Range(-10, 10);
		}
		foreach (ParticleSystem item in particlesToPlay)
		{
			item.Play();
		}
	}

	private IEnumerator customEndTimer()
	{
		yield return new WaitForSeconds(customEndTime);
		if (Application.isPlaying)
		{
			base.gameObject.Recycle();
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	private void PlayAnimation()
	{
		Play(base.transform.position, Random.Range(0, 360));
	}

	private void Update()
	{
		if (setRotation)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, Rotation);
		}
	}
}
