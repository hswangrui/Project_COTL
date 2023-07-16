using DG.Tweening;
using UnityEngine;

public class CurseChargeBar : MonoBehaviour
{
	public GameObject AimingRecticule;

	public SpriteRenderer ProjectileArrow;

	public SpriteRenderer ProjectileShadow;

	public SpriteRenderer ProjectileChargeTarget;

	public SpriteRenderer AreaOfDamage;

	private bool requiresCharging;

	private bool Hiding;

	public void ShowProjectileCharge(bool requiresCharging, float size = 1.5f)
	{
		if (!base.gameObject.activeSelf && !Hiding)
		{
			base.gameObject.SetActive(true);
			this.requiresCharging = requiresCharging;
			ProjectileArrow.color = new Vector4(1f, 1f, 1f, 0f);
			ProjectileShadow.color = new Vector4(1f, 1f, 1f, 0f);
			ProjectileChargeTarget.color = new Vector4(1f, 1f, 1f, 0f);
			UpdateProjectileChargeBar(0f);
			SetAimingRecticuleScaleAndRotation(Vector3.zero, Vector3.zero);
			ProjectileArrow.DOKill();
			ProjectileShadow.DOKill();
			ProjectileChargeTarget.DOKill();
			ProjectileArrow.DOFade(1f, 0.1f).SetUpdate(true);
			ProjectileShadow.DOFade(1f, 0.1f).SetUpdate(true);
			if (requiresCharging)
			{
				ProjectileChargeTarget.DOFade(1f, 0.1f).SetUpdate(true);
			}
			else
			{
				UpdateProjectileChargeBar(1f);
			}
			ProjectileChargeTarget.gameObject.SetActive(true);
			ProjectileChargeTarget.size = new Vector2(0.34f, 2.2f);
			if (AreaOfDamage != null)
			{
				AreaOfDamage.DOKill();
				AreaOfDamage.color = new Vector4(1f, 1f, 1f, 0f);
				AreaOfDamage.DOFade(1f, 0.05f).SetUpdate(true);
				AreaOfDamage.transform.DOKill();
				AreaOfDamage.transform.localScale = Vector3.one * size * 2f;
				AreaOfDamage.transform.DOScale(Vector3.one * size, 0.5f).SetEase(Ease.OutBack);
				AreaOfDamage.transform.localEulerAngles = Vector3.zero;
				AreaOfDamage.transform.DOLocalRotate(new Vector3(0f, 0f, 180f), 0.5f).SetLoops(-1).SetEase(Ease.Linear);
			}
		}
	}

	public void HideProjectileCharge()
	{
		if (base.gameObject.activeSelf && !Hiding)
		{
			Hiding = true;
			ProjectileArrow.DOKill();
			ProjectileShadow.DOKill();
			ProjectileChargeTarget.DOKill();
			ProjectileChargeTarget.DOFade(0f, 0.1f).SetUpdate(true);
			ProjectileArrow.DOFade(0f, 0.1f).SetUpdate(true);
			ProjectileShadow.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(delegate
			{
				Hiding = false;
				base.gameObject.SetActive(false);
			});
			if (AreaOfDamage != null)
			{
				AreaOfDamage.transform.DOKill();
				AreaOfDamage.DOKill();
				AreaOfDamage.transform.DOScale(Vector3.one * 3f, 0.1f);
				AreaOfDamage.DOFade(0f, 0.1f);
			}
		}
	}

	public void SetAimingRecticuleScaleAndRotation(Vector3 scale, Vector3 euler)
	{
		if (base.gameObject.activeSelf)
		{
			AimingRecticule.transform.localScale = scale;
			AimingRecticule.transform.eulerAngles = euler;
		}
	}

	public void UpdateProjectileChargeBar(float fillAmount)
	{
		if (base.gameObject.activeSelf)
		{
			if (!requiresCharging)
			{
				fillAmount = 1f;
			}
			ProjectileArrow.material.SetFloat("_ColorRampOffset", Mathf.Lerp(0.5f, -0.5f, fillAmount));
			ProjectileChargeTarget.color = (CorrectProjectileChargeRelease() ? new Color(0f, 1f, 0f, ProjectileChargeTarget.color.a) : new Color(1f, 1f, 1f, ProjectileChargeTarget.color.a));
		}
	}

	public bool CorrectProjectileChargeRelease()
	{
		if (!base.gameObject.activeSelf || !requiresCharging)
		{
			return false;
		}
		float @float = ProjectileArrow.material.GetFloat("_ColorRampOffset");
		if (@float < -0.06f)
		{
			return @float > -0.16f;
		}
		return false;
	}
}
