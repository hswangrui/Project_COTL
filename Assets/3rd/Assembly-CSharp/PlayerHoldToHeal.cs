using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHoldToHeal : BaseMonoBehaviour
{
	public Image ProgressRing;

	public Image FullHealthMask;

	private float Timer;

	private bool ableToHeal;

	private Tween maskTween;

	private void Start()
	{
		ProgressRing.fillAmount = 0f;
	}

	private void Update()
	{
		if (!PlayerFarming.Instance)
		{
			return;
		}
		StateMachine.State cURRENT_STATE = PlayerFarming.Instance.state.CURRENT_STATE;
		if (cURRENT_STATE == StateMachine.State.Heal)
		{
			Timer += Time.deltaTime;
			ProgressRing.fillAmount = Timer / 1f;
		}
		else
		{
			Timer = Mathf.Clamp(Timer - Time.deltaTime * 2f, 0f, float.MaxValue);
			ProgressRing.fillAmount = Timer / 1f;
		}
		Timer = Mathf.Clamp(Timer, 0f, 1f);
		if (!ableToHeal && PlayerFarming.Instance.health.HP < PlayerFarming.Instance.health.totalHP && FaithAmmo.Ammo >= (float)PlayerSpells.AmmoCost)
		{
			if (maskTween != null && maskTween.active)
			{
				maskTween.Complete();
			}
			maskTween = FullHealthMask.DOFade(0f, 0.5f);
			ableToHeal = true;
		}
		else if (ableToHeal && (PlayerFarming.Instance.health.HP >= PlayerFarming.Instance.health.totalHP || FaithAmmo.Ammo < (float)PlayerSpells.AmmoCost))
		{
			if (maskTween != null && maskTween.active)
			{
				maskTween.Complete();
			}
			maskTween = FullHealthMask.DOFade(0.75f, 0.5f);
			ableToHeal = false;
		}
	}
}
