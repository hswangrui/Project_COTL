using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

public class VFX_Dice : VFXObject
{
	[SerializeField]
	private MeshRenderer dice;

	[SerializeField]
	private ParticleSystem diceRolledParticles;

	public Action<bool> OnDiceRolled;

	public override void Init()
	{
		if (!base.Initialized && diceRolledParticles != null)
		{
			ParticleSystem.MainModule main = diceRolledParticles.main;
			main.playOnAwake = false;
			main.stopAction = ParticleSystemStopAction.Callback;
		}
		base.Init();
	}

	public override void PlayVFX(float addEmissionDelay = 0f)
	{
		base.PlayVFX(addEmissionDelay);
		BiomeConstants.Instance.EmitDustCloudParticles(dice.gameObject.transform.position);
		dice.gameObject.transform.position = new Vector3(dice.gameObject.transform.position.x, dice.gameObject.transform.position.y, -3f);
		if (diceRolledParticles != null)
		{
			diceRolledParticles.gameObject.transform.position = new Vector3(diceRolledParticles.gameObject.transform.position.x, diceRolledParticles.gameObject.transform.position.y, -2f);
		}
		dice.gameObject.transform.localScale = Vector3.zero;
		dice.gameObject.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutSine);
		DOTween.To(() => Time.timeScale, delegate(float x)
		{
			Time.timeScale = x;
		}, 0f, 0.5f).SetUpdate(true).OnUpdate(delegate
		{
			dice.gameObject.transform.position = new Vector3(PlayerFarming.Instance.transform.position.x, PlayerFarming.Instance.transform.position.y, dice.gameObject.transform.position.z);
			if (diceRolledParticles != null)
			{
				diceRolledParticles.gameObject.transform.position = new Vector3(PlayerFarming.Instance.transform.position.x, PlayerFarming.Instance.transform.position.y, diceRolledParticles.gameObject.transform.position.z);
			}
		});
		BiomeConstants.Instance.DepthOfFieldTween(1f, 4.5f, 10f, 1f, 145f);
		Material i = new Material(dice.material);
		dice.material = i;
		int num = UnityEngine.Random.Range(0, 10);
		if (TrinketManager.HasTrinket(TarotCards.Card.RabbitFoot))
		{
			num += 2;
		}
		bool win = num >= 5;
		float num2 = 0f;
		dice.transform.localRotation = Quaternion.identity;
		num2 = ((!win) ? ((float)(90 + 360 * UnityEngine.Random.Range(3, 6))) : ((float)(360 * UnityEngine.Random.Range(3, 6))));
		AudioManager.Instance.PlayOneShot("event:/knuckle_bones/die_roll", base.transform.gameObject);
		UnityEngine.Debug.Log("SPIN AMOUNT: " + num2);
		dice.gameObject.transform.DOLocalRotate(new Vector3(0f, 0f, num2), 1f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.OutCirc)
			.SetDelay(1f)
			.SetUpdate(true)
			.OnComplete(delegate
			{
				if (diceRolledParticles != null && win)
				{
					diceRolledParticles.Play();
				}
				AudioManager.Instance.PlayOneShot("event:/knuckle_bones/die_place", base.transform.gameObject);
				dice.gameObject.transform.DOShakePosition(0.5f, Vector3.right * 0.2f, 16).SetEase(Ease.OutCirc).SetUpdate(true);
				AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", base.gameObject);
				i.DOFloat(1f, "_FillAmount", 0.25f).SetUpdate(true);
				if (win)
				{
					i.DOColor(StaticColors.GreenColor, "_FillColor", 0.25f).SetUpdate(true);
					i.DOColor(StaticColors.GreenColor * 2f, "_EmissionColor", 0.25f).SetUpdate(true);
				}
				else
				{
					i.DOColor(StaticColors.OffWhiteColor, "_FillColor", 0.25f).SetUpdate(true);
					i.DOColor(StaticColors.OffWhiteColor * 2f, "_EmissionColor", 0.25f).SetUpdate(true);
				}
				i.DOFloat(0f, "_FillAmount", 0.25f).SetDelay(0.75f).SetUpdate(true);
				i.DOFloat(1f, "_EmissionAmount", 0.25f).SetDelay(1f).SetUpdate(true)
					.OnComplete(delegate
					{
						Action<bool> onDiceRolled = OnDiceRolled;
						if (onDiceRolled != null)
						{
							onDiceRolled(win);
						}
						Time.timeScale = 1f;
						dice.gameObject.transform.DOScale(Vector3.zero, 0.25f).SetUpdate(true).SetEase(Ease.InBack);
						dice.gameObject.transform.DOMove(PlayerFarming.Instance.transform.position, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
						{
							_003C_003En__0();
						});
					});
				i.DOFloat(0f, "_EmissionAmount", 0.25f).SetDelay(1.25f).SetUpdate(true);
			});
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.CancelVFX();
	}
}
