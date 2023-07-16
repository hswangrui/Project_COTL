using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class DetectStealth : BaseMonoBehaviour
{
	public UnitObject unitObject;

	public bool Sway = true;

	public float SwayAngle = 30f;

	public float SwaySpeed = 1f;

	public float DetectEvenIfStealth = 3f;

	private float _LookingAngle;

	public SpriteShapeController spriteShapeController;

	public StateMachine state;

	public SpriteRenderer spriteRenderer;

	public Image alertRadial;

	public float VisionRage = 8f;

	public float VisionConeAngle = 40f;

	public float CloseConeAngle = 120f;

	private float _AlertLevel;

	public float AlertLimit = 3f;

	private int VisibleEnemies;

	private ContactFilter2D c;

	private List<RaycastHit2D> Results;

	private float Distance;

	private float WaitToHideUI;

	private Health EnemyHealth;

	private float Swing;

	private float LookingAngle
	{
		get
		{
			return _LookingAngle;
		}
		set
		{
			_LookingAngle = value;
			if (_LookingAngle < 0f)
			{
				_LookingAngle += 360f;
			}
			if (_LookingAngle > 360f)
			{
				_LookingAngle -= 360f;
			}
		}
	}

	private float AlertLevel
	{
		get
		{
			return _AlertLevel;
		}
		set
		{
			_AlertLevel = Mathf.Clamp(value, 0f, AlertLimit);
		}
	}

	private void Start()
	{
		spriteShapeController.spline.Clear();
		UpdateSpriteShape();
		LookingAngle = state.facingAngle;
		c = default(ContactFilter2D);
		c.layerMask = LayerMask.GetMask("Player");
		spriteShapeController.enabled = false;
	}

	private void Update()
	{
		if (!unitObject.health.Unaware)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (Sway)
		{
			DoSway();
		}
		UpdateSpriteShape();
		VisibleEnemies = 0;
		if (!spriteShapeController.enabled)
		{
			foreach (Health item in Health.playerTeam)
			{
				if (item.state.CURRENT_STATE == StateMachine.State.Stealth)
				{
					spriteShapeController.enabled = true;
					break;
				}
			}
		}
		else
		{
			int num = 0;
			foreach (Health item2 in Health.playerTeam)
			{
				if (item2.state.CURRENT_STATE == StateMachine.State.Stealth)
				{
					num++;
				}
			}
			if (num <= 0)
			{
				WaitToHideUI += Time.deltaTime;
			}
			if (WaitToHideUI > 1f)
			{
				WaitToHideUI = 0f;
				spriteShapeController.enabled = false;
			}
		}
		foreach (Health item3 in Health.playerTeam)
		{
			Distance = Vector3.Distance(base.transform.position, item3.transform.position);
			if (!(item3 != null) || !(Distance < VisionRage))
			{
				continue;
			}
			if (Distance < DetectEvenIfStealth)
			{
				if (Mathf.Abs(Utils.GetAngle(base.transform.position, item3.transform.position) - LookingAngle) <= CloseConeAngle / 2f || Mathf.Abs(Utils.GetAngle(base.transform.position, item3.transform.position) - LookingAngle) >= 360f - CloseConeAngle / 2f)
				{
					Results = new List<RaycastHit2D>();
					Physics2D.Raycast(base.transform.position, item3.transform.position - base.transform.position, c, Results, VisionRage);
					if (Results[0].collider.gameObject == item3.gameObject)
					{
						VisibleEnemies++;
						AlertLevel += Time.deltaTime * 10f;
						EnemyHealth = item3;
					}
				}
			}
			else
			{
				if (!(Mathf.Abs(Utils.GetAngle(base.transform.position, item3.transform.position) - LookingAngle) <= VisionConeAngle / 2f) && !(Mathf.Abs(Utils.GetAngle(base.transform.position, item3.transform.position) - LookingAngle) >= 360f - VisionConeAngle / 2f))
				{
					continue;
				}
				Results = new List<RaycastHit2D>();
				Physics2D.Raycast(base.transform.position, item3.transform.position - base.transform.position, c, Results, VisionRage);
				if (Results[0].collider.gameObject == item3.gameObject)
				{
					VisibleEnemies++;
					EnemyHealth = item3;
					if (item3.state.CURRENT_STATE == StateMachine.State.Stealth && Distance > DetectEvenIfStealth)
					{
						AlertLevel += Time.deltaTime * 2f;
					}
					else
					{
						AlertLevel += Time.deltaTime * 10f;
					}
				}
			}
		}
		if (VisibleEnemies <= 0 && AlertLevel < AlertLimit)
		{
			AlertLevel -= Time.deltaTime * 2f;
		}
		alertRadial.fillAmount = AlertLevel / AlertLimit;
		if (AlertLevel >= AlertLimit)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void DoSway()
	{
		LookingAngle = state.facingAngle + SwayAngle * Mathf.Cos((Swing += Time.deltaTime * SwaySpeed) * ((float)Math.PI / 180f)) % 360f;
	}

	private void BtnUpdateSpriteShape()
	{
		LookingAngle = state.facingAngle;
		UpdateSpriteShape();
	}

	private void UpdateSpriteShape()
	{
		while (spriteShapeController.spline.GetPointCount() < 7)
		{
			spriteShapeController.spline.InsertPointAt(0, Vector3.one * (spriteShapeController.spline.GetPointCount() + 1) * 30f);
		}
		spriteShapeController.spline.SetPosition(0, Vector3.zero);
		float f = (LookingAngle + CloseConeAngle / 2f) * ((float)Math.PI / 180f);
		Vector3 point = new Vector3(DetectEvenIfStealth * Mathf.Cos(f), DetectEvenIfStealth * Mathf.Sin(f));
		spriteShapeController.spline.SetPosition(1, point);
		f = (LookingAngle + VisionConeAngle / 2f) * ((float)Math.PI / 180f);
		point = new Vector3(DetectEvenIfStealth * Mathf.Cos(f), DetectEvenIfStealth * Mathf.Sin(f));
		spriteShapeController.spline.SetPosition(2, point);
		f = (LookingAngle + VisionConeAngle / 2f) * ((float)Math.PI / 180f);
		point = new Vector3(VisionRage * Mathf.Cos(f), VisionRage * Mathf.Sin(f));
		spriteShapeController.spline.SetPosition(3, point);
		f = (LookingAngle - VisionConeAngle / 2f) * ((float)Math.PI / 180f);
		point = new Vector3(VisionRage * Mathf.Cos(f), VisionRage * Mathf.Sin(f));
		spriteShapeController.spline.SetPosition(4, point);
		f = (LookingAngle - VisionConeAngle / 2f) * ((float)Math.PI / 180f);
		point = new Vector3(DetectEvenIfStealth * Mathf.Cos(f), DetectEvenIfStealth * Mathf.Sin(f));
		spriteShapeController.spline.SetPosition(5, point);
		f = (LookingAngle - CloseConeAngle / 2f) * ((float)Math.PI / 180f);
		point = new Vector3(DetectEvenIfStealth * Mathf.Cos(f), DetectEvenIfStealth * Mathf.Sin(f));
		spriteShapeController.spline.SetPosition(6, point);
	}

	private void OnDrawGizmos()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			LookingAngle = state.facingAngle;
		}
		float f = (LookingAngle + VisionConeAngle / 2f) * ((float)Math.PI / 180f);
		Vector3 vector = new Vector3(VisionRage * Mathf.Cos(f), VisionRage * Mathf.Sin(f));
		Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.white);
		f = (LookingAngle - VisionConeAngle / 2f) * ((float)Math.PI / 180f);
		vector = new Vector3(VisionRage * Mathf.Cos(f), VisionRage * Mathf.Sin(f));
		Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.white);
		f = (LookingAngle + CloseConeAngle / 2f) * ((float)Math.PI / 180f);
		vector = new Vector3(DetectEvenIfStealth * Mathf.Cos(f), DetectEvenIfStealth * Mathf.Sin(f));
		Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.red);
		f = (LookingAngle - CloseConeAngle / 2f) * ((float)Math.PI / 180f);
		vector = new Vector3(DetectEvenIfStealth * Mathf.Cos(f), DetectEvenIfStealth * Mathf.Sin(f));
		Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.red);
		if (Sway)
		{
			f = (state.facingAngle + VisionConeAngle / 2f + SwayAngle) * ((float)Math.PI / 180f);
			vector = new Vector3(VisionRage * Mathf.Cos(f), VisionRage * Mathf.Sin(f));
			Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.white);
			f = (state.facingAngle - VisionConeAngle / 2f - SwayAngle) * ((float)Math.PI / 180f);
			vector = new Vector3(VisionRage * Mathf.Cos(f), VisionRage * Mathf.Sin(f));
			Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.white);
			vector = new Vector3(VisionRage * Mathf.Cos(LookingAngle * ((float)Math.PI / 180f)), VisionRage * Mathf.Sin(LookingAngle * ((float)Math.PI / 180f)));
			Utils.DrawLine(base.transform.position, base.transform.position + vector, Color.yellow);
		}
		Utils.DrawCircleXY(base.transform.position, DetectEvenIfStealth / 2f, Color.red);
	}
}
