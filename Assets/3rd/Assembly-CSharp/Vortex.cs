using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class Vortex : BaseMonoBehaviour
{
	[SerializeField]
	private float force = 0.2f;

	[SerializeField]
	private CircleCollider2D collider;

	[SerializeField]
	private GameObject distortionObject;

	private List<UnitObject> enteredEnemies = new List<UnitObject>();

	public float lifeTime = 5f;

	private float timer;

	private EventInstance LoopInstance;

	private bool createdLoop;

	public float LifeTimeMultiplier { get; set; } = 1f;


	public float ForceMultiplier { get; set; } = 1f;


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
			main.duration *= LifeTimeMultiplier;
			ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
			startLifetime.constant *= LifeTimeMultiplier;
			main.startLifetime = startLifetime;
		}
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopLoop(LoopInstance);
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(LoopInstance);
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		foreach (UnitObject enteredEnemy in enteredEnemies)
		{
			if (!(enteredEnemy == null))
			{
				float num = Vector3.Distance(base.transform.position, enteredEnemy.transform.position) / collider.radius;
				Vector3 normalized = (base.transform.position - enteredEnemy.transform.position).normalized;
				enteredEnemy.DisableForces = true;
				enteredEnemy.rb.velocity = normalized * force * num * ForceMultiplier;
			}
		}
		if (!((timer += Time.deltaTime) > lifeTime))
		{
			return;
		}
		AudioManager.Instance.StopLoop(LoopInstance);
		AudioManager.Instance.PlayOneShot("event:/player/Curses/vortex_end", base.gameObject);
		foreach (UnitObject enteredEnemy2 in enteredEnemies)
		{
			enteredEnemy2.DisableForces = false;
		}
		Object.Destroy(base.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		UnitObject component = collision.GetComponent<UnitObject>();
		if ((bool)component && component.health.team == Health.Team.Team2 && !enteredEnemies.Contains(component))
		{
			enteredEnemies.Add(component);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		UnitObject component = collision.GetComponent<UnitObject>();
		if ((bool)component && component.health.team == Health.Team.Team2 && enteredEnemies.Contains(component))
		{
			component.DisableForces = false;
			enteredEnemies.Remove(component);
		}
	}
}
