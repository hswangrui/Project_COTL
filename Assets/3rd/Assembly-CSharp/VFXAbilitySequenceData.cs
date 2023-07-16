using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/VFX Ability Sequence")]
public class VFXAbilitySequenceData : ScriptableObject
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass13_0
	{
		public Transform[] targetTransforms;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass13_1
	{
		public VFXSequence sequence;

		public _003C_003Ec__DisplayClass13_0 CS_0024_003C_003E8__locals1;

		internal void _003CTestSequence_003Eg__DestroyTempTargets_007C0()
		{
			VFXSequence vFXSequence = sequence;
			vFXSequence.OnComplete = (Action)Delegate.Remove(vFXSequence.OnComplete, new Action(_003CTestSequence_003Eg__DestroyTempTargets_007C0));
			for (int i = 0; i < CS_0024_003C_003E8__locals1.targetTransforms.Length; i++)
			{
				UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals1.targetTransforms[i].gameObject);
			}
		}
	}

	[SerializeField]
	private string _activationAnimationName;

	[SerializeField]
	private float _animationDuration = 1f;

	[SerializeField]
	private VFXObject[] _activationVFXObjects;

	[SerializeField]
	private VFXObject _impactVFXObject;

	[SerializeField]
	private float _multipleImpactDelay;

	public string ActivationAnimationName
	{
		get
		{
			return _activationAnimationName;
		}
	}

	public float AnimationDuration
	{
		get
		{
			return _animationDuration;
		}
	}

	public VFXObject[] ActivationVFXObjects
	{
		get
		{
			return _activationVFXObjects;
		}
	}

	public VFXObject ImpactVFXObject
	{
		get
		{
			return _impactVFXObject;
		}
	}

	public void TestSequence(int targetNumber, bool targetSelf)
	{
		_003C_003Ec__DisplayClass13_0 _003C_003Ec__DisplayClass13_ = new _003C_003Ec__DisplayClass13_0();
		if (!Application.isPlaying)
		{
			return;
		}
		_003C_003Ec__DisplayClass13_.targetTransforms = new Transform[targetSelf ? 1 : targetNumber];
		if (targetSelf)
		{
			_003C_003Ec__DisplayClass13_.targetTransforms[0] = PlayerFarming.Instance.transform;
			PlayNewSequence(_003C_003Ec__DisplayClass13_.targetTransforms[0], _003C_003Ec__DisplayClass13_.targetTransforms);
			return;
		}
		_003C_003Ec__DisplayClass13_1 _003C_003Ec__DisplayClass13_2 = new _003C_003Ec__DisplayClass13_1();
		_003C_003Ec__DisplayClass13_2.CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass13_;
		for (int i = 0; i < targetNumber; i++)
		{
			GameObject gameObject = new GameObject();
			_003C_003Ec__DisplayClass13_2.CS_0024_003C_003E8__locals1.targetTransforms[i] = gameObject.transform;
			_003C_003Ec__DisplayClass13_2.CS_0024_003C_003E8__locals1.targetTransforms[i].position = PlayerFarming.Instance.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 5f);
		}
		_003C_003Ec__DisplayClass13_2.sequence = PlayNewSequence(PlayerFarming.Instance.transform, _003C_003Ec__DisplayClass13_2.CS_0024_003C_003E8__locals1.targetTransforms);
		VFXSequence sequence = _003C_003Ec__DisplayClass13_2.sequence;
		sequence.OnComplete = (Action)Delegate.Combine(sequence.OnComplete, new Action(_003C_003Ec__DisplayClass13_2._003CTestSequence_003Eg__DestroyTempTargets_007C0));
	}

	public VFXSequence PlayNewSequence(Transform caster, Transform[] targets)
	{
		return new VFXSequence(this, caster, targets, _multipleImpactDelay);
	}
}
