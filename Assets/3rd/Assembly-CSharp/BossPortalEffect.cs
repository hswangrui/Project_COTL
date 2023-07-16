using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class BossPortalEffect : MonoBehaviour
{
	public GameObject PortalContainer;

	public Interaction_BossPortal PortalInteraction;

	[SerializeField]
	private GameObject distortionObject;

	[SerializeField]
	private float lifeTime = 2f;

	public float LifeTimeMultiplier = 0.5f;

	private float timer;

	private EventInstance LoopInstance;

	private bool createdLoop;

	private bool MusicStopped;

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(LoopInstance);
		Object.Destroy(base.gameObject);
	}

	private void Start()
	{
		distortionObject.transform.DOScale(9f, 0.9f).SetEase(Ease.Linear);
		AudioManager.Instance.PlayOneShot(" event:/player/Curses/vortex_start", base.gameObject);
		if (!createdLoop)
		{
			LoopInstance = AudioManager.Instance.CreateLoop("event:/player/Curses/vortex_loop", base.gameObject, true);
			createdLoop = true;
		}
		lifeTime *= LifeTimeMultiplier;
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem obj in array)
		{
			obj.Stop();
			ParticleSystem.MainModule main = obj.main;
			ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
			startLifetime.constant *= LifeTimeMultiplier;
			main.startLifetime = startLifetime;
		}
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
		PortalContainer.transform.localScale = Vector3.zero;
		PortalContainer.SetActive(false);
		PortalInteraction.enabled = false;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(2f);
		sequence.AppendCallback(delegate
		{
			PortalInteraction.enabled = true;
			PortalContainer.SetActive(true);
		});
		sequence.Append(PortalContainer.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack));
		sequence.Play();
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopLoop(LoopInstance);
	}

	private void Update()
	{
		if (timer > 3f && !MusicStopped)
		{
			MusicStopped = true;
			AudioManager.Instance.StopLoop(LoopInstance);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/vortex_end", base.gameObject);
		}
		if ((timer += Time.deltaTime) > lifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
