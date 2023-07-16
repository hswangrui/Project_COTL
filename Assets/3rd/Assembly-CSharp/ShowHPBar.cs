using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Health))]
public class ShowHPBar : BaseMonoBehaviour
{
	private StateMachine state;

	public float zOffset = 1f;

	public bool WideBar;

	public HPBar hpBar;

	private SpriteRenderer[] barSprites;

	private float alphaHold;

	private float prevHP;

	private Health health;

	public bool OnlyShowOnHit = true;

	public bool DestroyOnDeath = true;

	private bool initialised;

	private Coroutine cTweenBar;

	public float StasisXOffset
	{
		get
		{
			if (WideBar)
			{
				return 0.8f;
			}
			return 0.5f;
		}
	}

	private void Start()
	{
		Init();
	}

	public void Init()
	{
		if (initialised)
		{
			return;
		}
		initialised = true;
		health = GetComponent<Health>();
		Health.Team team = health.team;
		if (team != Health.Team.Team2)
		{
			hpBar = Object.Instantiate(Resources.Load("Prefabs/HPBar - Team 1") as GameObject, base.transform.parent, true).GetComponent<HPBar>();
		}
		else if (WideBar)
		{
			hpBar = Object.Instantiate(Resources.Load("Prefabs/HPBar - Team 2 Wide") as GameObject, base.transform.parent, true).GetComponent<HPBar>();
		}
		else
		{
			hpBar = Object.Instantiate(Resources.Load("Prefabs/HPBar - Team 2") as GameObject, base.transform.parent, true).GetComponent<HPBar>();
		}
		barSprites = hpBar.GetComponentsInChildren<SpriteRenderer>();
		if (OnlyShowOnHit)
		{
			SpriteRenderer[] array = barSprites;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = new Color(0f, 0f, 0f, 0f);
			}
		}
		health.OnHit += OnHit;
		health.OnDie += OnDie;
		health.OnCharmed += OnStasisEvent;
		health.OnIced += OnStasisEvent;
		health.OnPoisoned += OnStasisEvent;
		Transform obj = hpBar.barInstant.transform;
		Vector3 localScale = (hpBar.barTween.transform.localScale = new Vector3(health.HP / health.totalHP, 1f));
		obj.localScale = localScale;
	}

	private void OnStasisEvent()
	{
		alphaHold = 5f;
		if (hpBar != null && health != null)
		{
			hpBar.barInstant.transform.localScale = new Vector3(health.HP / health.totalHP, 1f);
		}
		if (cTweenBar != null)
		{
			StopCoroutine(cTweenBar);
		}
		if (hpBar != null && hpBar.barTween != null)
		{
			cTweenBar = StartCoroutine(TweenBar());
		}
	}

	public void Hide()
	{
		alphaHold = 0f;
	}

	private void Update()
	{
		if (OnlyShowOnHit)
		{
			alphaHold -= Time.deltaTime;
			SpriteRenderer[] array = barSprites;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				if (!(spriteRenderer == null))
				{
					if (alphaHold <= 0f)
					{
						spriteRenderer.color += (new Color(0f, 0f, 0f, 0f) - spriteRenderer.color) / 15f;
					}
					else
					{
						spriteRenderer.color += (Color.white - spriteRenderer.color) / 5f;
					}
				}
			}
		}
		if (health == null || !health.gameObject.activeSelf)
		{
			DestroyHPBar();
		}
	}

	private void LateUpdate()
	{
		if ((bool)hpBar)
		{
			hpBar.transform.position = base.transform.position - new Vector3(0f, -0.5f, zOffset);
		}
	}

	private void OnDestroy()
	{
		if (hpBar != null && hpBar.gameObject != null)
		{
			Object.Destroy(hpBar.gameObject);
		}
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.OnHit -= OnHit;
			health.OnCharmed -= OnStasisEvent;
			health.OnIced -= OnStasisEvent;
			health.OnPoisoned -= OnStasisEvent;
		}
	}

	public void ShowHPBarShield()
	{
		hpBar.defence.SetActive(true);
	}

	public void HideHPBarShield()
	{
		hpBar.defence.SetActive(false);
	}

	public void DestroyHPBar()
	{
		if (hpBar != null && hpBar.gameObject != null)
		{
			Object.Destroy(hpBar.gameObject);
		}
		Object.Destroy(this);
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (DestroyOnDeath)
		{
			DestroyHPBar();
		}
		else
		{
			alphaHold = 0f;
		}
	}

	public void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		alphaHold = 5f;
		if (hpBar != null && health != null)
		{
			hpBar.barInstant.transform.localScale = new Vector3(health.HP / health.totalHP, 1f);
		}
		if (cTweenBar != null)
		{
			StopCoroutine(cTweenBar);
		}
		if (hpBar != null && hpBar.barTween != null)
		{
			cTweenBar = StartCoroutine(TweenBar());
		}
	}

	private IEnumerator TweenBar()
	{
		yield return new WaitForSeconds(0.4f);
		Vector3 Start = hpBar.barTween.transform.localScale;
		Vector3 Destination = hpBar.barInstant.transform.localScale;
		float Duration = 0.3f;
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			hpBar.barTween.transform.localScale = Vector3.Lerp(Start, Destination, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		hpBar.barInstant.transform.localScale = hpBar.barInstant.transform.localScale;
	}
}
