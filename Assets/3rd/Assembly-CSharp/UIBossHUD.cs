using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossHUD : BaseMonoBehaviour
{
	public static UIBossHUD Instance;

	[SerializeField]
	private TMP_Text bossNameText;

	[SerializeField]
	private Image healthBar;

	[SerializeField]
	private Image healthFlashBar;

	[SerializeField]
	private CanvasGroup ailmentIconCanvasGroup;

	[SerializeField]
	private Image ailmentIcon;

	[SerializeField]
	private Sprite poisonIcon;

	[SerializeField]
	private Sprite icedIcon;

	private float targetHealthAmount;

	private Coroutine healthRoutine;

	private Health boss;

	private Color targetColor;

	private Tween ailmentTween;

	public static void Play(Health boss, string name)
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load<UIBossHUD>("Prefabs/UI/UI Boss HUD"), GameObject.FindWithTag("Canvas").transform);
		}
		HUD_Manager.Instance.XPBarTransitions.gameObject.SetActive(false);
		Instance.boss = boss;
		Instance.boss.OnHit += Instance.OnBossHit;
		Instance.bossNameText.text = name;
	}

	public static void Hide()
	{
		if (Instance != null)
		{
			Object.Destroy(Instance.gameObject);
			Instance = null;
		}
		HUD_Manager.Instance.XPBarTransitions.gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		if ((bool)boss)
		{
			boss.OnHit -= OnBossHit;
		}
	}

	private void Update()
	{
		if (ailmentIconCanvasGroup.alpha == 0f && (boss.IsPoisoned || boss.IsIced))
		{
			if (ailmentTween != null && ailmentTween.active)
			{
				ailmentTween.Complete();
			}
			ailmentIconCanvasGroup.alpha = 0f;
			ailmentTween = ailmentIconCanvasGroup.DOFade(1f, 0.2f);
		}
		else if (ailmentIconCanvasGroup.alpha == 1f && !boss.IsPoisoned && !boss.IsIced)
		{
			if (ailmentTween != null && ailmentTween.active)
			{
				ailmentTween.Complete();
			}
			ailmentIconCanvasGroup.alpha = 1f;
			ailmentTween = ailmentIconCanvasGroup.DOFade(0f, 0.2f);
		}
		targetColor = Color.red;
		if (boss.IsPoisoned)
		{
			targetColor = Color.green;
			ailmentIcon.sprite = poisonIcon;
		}
		else if (boss.IsIced)
		{
			targetColor = Color.cyan;
			ailmentIcon.sprite = icedIcon;
		}
		healthBar.color = Color.Lerp(healthBar.color, targetColor, 5f * Time.deltaTime);
		ailmentIcon.color = targetColor;
	}

	public void OnBossHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		if ((bool)boss)
		{
			float normHealthAmount = boss.HP / boss.totalHP;
			HealthUpdated(normHealthAmount);
		}
	}

	private void HealthUpdated(float normHealthAmount)
	{
		if (base.gameObject.activeInHierarchy && normHealthAmount != targetHealthAmount)
		{
			if (healthRoutine != null)
			{
				StopCoroutine(healthRoutine);
				ForceHealthAmount(targetHealthAmount);
			}
			targetHealthAmount = normHealthAmount;
			healthRoutine = StartCoroutine(HealthBarUpdated(normHealthAmount));
		}
	}

	public void ForceHealthAmount(float normHealthAmount)
	{
		healthBar.fillAmount = normHealthAmount;
		healthFlashBar.fillAmount = normHealthAmount;
		targetHealthAmount = normHealthAmount;
	}

	private IEnumerator HealthBarUpdated(float normAmount)
	{
		healthBar.fillAmount = normAmount;
		yield return new WaitForSeconds(0.3f);
		float fromAmount = healthFlashBar.fillAmount;
		float t = 0f;
		while (t < 0.25f)
		{
			float t2 = t / 0.25f;
			healthFlashBar.fillAmount = Mathf.Lerp(fromAmount, normAmount, t2);
			t += Time.deltaTime;
			yield return null;
		}
		healthFlashBar.fillAmount = normAmount;
		healthRoutine = null;
	}
}
