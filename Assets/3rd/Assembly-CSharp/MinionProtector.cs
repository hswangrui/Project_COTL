using System;
using System.Collections.Generic;
using UnityEngine;

public class MinionProtector : MonoBehaviour
{
	public UnitObject[] protectedUnits;

	public float damageMultiplier = 0.1f;

	public bool showLineToMinions;

	public Material healthLineMaterial;

	public float healthLineLerpSpeed;

	public VFXParticle protectedVfxParticle;

	public Action destroyedAction;

	private List<LineRenderer> healthLines;

	private void Start()
	{
		for (int i = 0; i < protectedUnits.Length; i++)
		{
			UnitObject unitObject = protectedUnits[i];
			Health component = unitObject.GetComponent<Health>();
			if (component != null)
			{
				component.protector = this;
				if (showLineToMinions)
				{
					LineRenderer lineRenderer = component.gameObject.AddComponent<LineRenderer>();
					lineRenderer.positionCount = 2;
					lineRenderer.material = healthLineMaterial;
					lineRenderer.startWidth = 0.05f;
					lineRenderer.endWidth = 0.1f;
					lineRenderer.SetPosition(0, base.transform.position);
					lineRenderer.SetPosition(1, component.transform.position);
				}
				if (protectedVfxParticle != null)
				{
					VFXParticle vFXParticle = UnityEngine.Object.Instantiate(protectedVfxParticle, unitObject.transform);
					vFXParticle.name = "ProtectionVfx";
					vFXParticle.gameObject.SetActive(true);
					vFXParticle.loopedSoundSFX = "event:/enemy/shielded_enemy_loop";
					vFXParticle.Init();
					vFXParticle.PlayVFX();
				}
			}
		}
	}

	private void Update()
	{
		for (int i = 0; i < protectedUnits.Length; i++)
		{
			UnitObject unitObject = protectedUnits[i];
			if (!(unitObject != null))
			{
				continue;
			}
			Health component = unitObject.GetComponent<Health>();
			if (!(component != null))
			{
				continue;
			}
			LineRenderer component2 = component.gameObject.GetComponent<LineRenderer>();
			if (component2 != null)
			{
				if (healthLineLerpSpeed == 0f)
				{
					component2.SetPosition(0, base.transform.position);
				}
				else
				{
					Vector3 position = component2.GetPosition(0);
					position = Vector3.Lerp(position, base.transform.position, healthLineLerpSpeed * Time.deltaTime);
					component2.SetPosition(0, position);
				}
				component2.SetPosition(1, component.transform.position);
			}
		}
	}

	public void OnDestroy()
	{
		Action action = destroyedAction;
		if (action != null)
		{
			action();
		}
		for (int i = 0; i < protectedUnits.Length; i++)
		{
			UnitObject unitObject = protectedUnits[i];
			if (unitObject == null)
			{
				continue;
			}
			if (protectedVfxParticle != null)
			{
				Transform transform = unitObject.transform.Find("ProtectionVfx");
				if (transform != null)
				{
					VFXParticle component = transform.GetComponent<VFXParticle>();
					if (component != null)
					{
						component.StopVFX();
					}
					UnityEngine.Object.Destroy(transform.gameObject, 2f);
				}
			}
			UnityEngine.Object.Destroy(unitObject.gameObject.GetComponent<LineRenderer>());
		}
	}
}
