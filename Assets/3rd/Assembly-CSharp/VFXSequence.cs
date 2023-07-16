using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TwitchAuthentication;

public class VFXSequence
{

	public VFXObject[] ActivationVFXObjects;

	public VFXObject[] ImpactVFXObjects;

	public Action<VFXObject> OnActivation;

	private Transform[] _targets;

	public Action<VFXObject, int> OnImpact;

	public Action OnComplete;

	public Transform[] Targets
	{
		get
		{
			return _targets;
		}
	}

    public VFXSequence(VFXAbilitySequenceData data, Transform caster, Transform[] targets, float emissionDelay = 0f)
    {
        _targets = targets;
        if (caster == null)
        {
            caster = PlayerFarming.Instance.transform;
        }
        if (data.ActivationAnimationName != null)
        {
            Debug.Log("VFXSequence: Activating anim " + data.ActivationAnimationName);
            PlayerFarming.Instance.TimedAction(
                data.AnimationDuration,()=>
                { Debug.Log("VFXSequence: Animation Ended " + data.ActivationAnimationName); },
                data.ActivationAnimationName);
        }
        ActivationVFXObjects = new VFXObject[data.ActivationVFXObjects.Length];
        for (int i = 0; i < data.ActivationVFXObjects.Length; i++)
        {
            ActivationVFXObjects[i] = data.ActivationVFXObjects[i].SpawnVFX(caster, playOnSpawn: false);
            ActivationVFXObjects[i].transform.SetPositionAndRotation(caster.position, Quaternion.identity);
            if (i == 0)
            {
                VFXObject obj = ActivationVFXObjects[i];
                obj.OnEmitted = (Action<VFXObject>)Delegate.Combine(obj.OnEmitted, new Action<VFXObject>(OnActivationStarted));
            }
            ActivationVFXObjects[i].PlayVFX();
        }
        if (targets == null || !(data.ImpactVFXObject != null))
        {
            return;
        }
        ImpactVFXObjects = new VFXObject[targets.Length];
        for (int j = 0; j < targets.Length; j++)
        {
            ImpactVFXObjects[j] = data.ImpactVFXObject.SpawnVFX(targets[j].transform, playOnSpawn: false);
            VFXObject obj2 = ImpactVFXObjects[j];
            obj2.OnEmitted = (Action<VFXObject>)Delegate.Combine(obj2.OnEmitted, new Action<VFXObject>(OnImpactTriggered));
            if (!ImpactVFXObjects[j].Playing)
            {
                ImpactVFXObjects[j].PlayVFX(emissionDelay * (float)j);
            }
        }
     
    }

    
    private void OnImpactTriggered(VFXObject vfxObject)
	{
		vfxObject.OnEmitted = (Action<VFXObject>)Delegate.Remove(vfxObject.OnEmitted, new Action<VFXObject>(OnImpactTriggered));
		Action<VFXObject, int> onImpact = OnImpact;
		if (onImpact != null)
		{
			onImpact(vfxObject, ImpactVFXObjects.IndexOf(vfxObject));
		}
		if (vfxObject == ImpactVFXObjects[ImpactVFXObjects.Length - 1])
		{
			Action onComplete = OnComplete;
			if (onComplete != null)
			{
				onComplete();
			}
		}
	}

	private void OnActivationStarted(VFXObject vfxObject)
	{
		vfxObject.OnEmitted = (Action<VFXObject>)Delegate.Remove(vfxObject.OnEmitted, new Action<VFXObject>(OnActivationStarted));
		Action<VFXObject> onActivation = OnActivation;
		if (onActivation != null)
		{
			onActivation(vfxObject);
		}
	}
}
