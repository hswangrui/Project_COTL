using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public abstract class VFXObject : MonoBehaviour
{
	[SerializeField]
	protected float _emissionDelay;

	[SerializeField]
	private bool _despawnOnStop = true;

	[SerializeField]
	private float _maxDuration = -1f;

	[SerializeField]
	private bool _followsTarget;

	public Action<VFXObject> OnStarted;

	public Action<VFXObject> OnEmitted;

	public Action<VFXObject> OnStopped;

	[SerializeField]
	private string SFX;

	[SerializeField]
	private RelicSubType relicType;

	[SerializeField]
	private MMVibrate.HapticTypes HapticType = MMVibrate.HapticTypes.None;

	private VFXObject _instantiatedFrom;

	private bool _initialized;

	private bool _playing;

	[SerializeField]
	private List<VFXMaterialOverride> _materialOverrides;

	private List<VFXMaterialOverride> _activeOverrides;

	private MaterialPropertyBlock _materialPropertyBlock;

	private Transform _targetTransform;

	public float EmissionDelay
	{
		get
		{
			return _emissionDelay;
		}
	}

	private bool FollowsTarget
	{
		get
		{
			return _followsTarget;
		}
	}

	public VFXObject InstantiatedFrom
	{
		get
		{
			return _instantiatedFrom;
		}
	}

	public bool Initialized
	{
		get
		{
			return _initialized;
		}
	}

	public bool Playing
	{
		get
		{
			return _playing;
		}
	}

	public virtual VFXObject SpawnVFX(Transform target, bool playOnSpawn)
	{
		_targetTransform = target;
		VFXObject vFXObject = null;
		if (_instantiatedFrom == null)
		{
			vFXObject = this.Spawn(_followsTarget ? target : null);
			_instantiatedFrom = this;
			vFXObject._instantiatedFrom = this;
		}
		else
		{
			vFXObject = _instantiatedFrom.Spawn(_followsTarget ? target : null);
			vFXObject._instantiatedFrom = _instantiatedFrom;
		}
		if (_followsTarget)
		{
			MatchPrefabLocalTransform(vFXObject.transform);
		}
		vFXObject._targetTransform = target;
		if (playOnSpawn)
		{
			vFXObject.PlayVFX();
		}
		Debug.Log(vFXObject._instantiatedFrom);
		return vFXObject;
	}

	public virtual void Init()
	{
		_initialized = true;
		_activeOverrides = _materialOverrides;
		_materialPropertyBlock = new MaterialPropertyBlock();
	}

	public virtual void PlayVFX(float addEmissionDelay = 0f)
	{
		if (!_initialized)
		{
			Init();
		}
		Action<VFXObject> onStarted = OnStarted;
		if (onStarted != null)
		{
			onStarted(this);
		}
		_playing = true;
		UpdateMaterialOverrides();
		StartCoroutine(DelayedEmit(_emissionDelay + addEmissionDelay));
	}

	public virtual void AddMaterialOverride(VFXMaterialOverride vfxMaterialOverride)
	{
		_activeOverrides.Add(vfxMaterialOverride);
	}

	public virtual void UpdateMaterialOverrides()
	{
		if (_activeOverrides.Count == 0)
		{
			return;
		}
		_materialPropertyBlock.Clear();
		foreach (VFXMaterialOverride materialOverride in _materialOverrides)
		{
			materialOverride.Apply(ref _materialPropertyBlock);
		}
	}

	protected virtual void Emit()
	{
		if (_targetTransform != null)
		{
			if (_followsTarget)
			{
				MatchPrefabLocalTransform(base.transform);
			}
			else
			{
				base.transform.SetPositionAndRotation(_targetTransform.position, _targetTransform.rotation);
			}
		}
		if (!SFX.IsNullOrEmpty())
		{
			if (relicType == RelicSubType.Blessed)
			{
				AudioManager.Instance.ToggleFilter("blessed", true);
			}
			else if (relicType == RelicSubType.Dammed)
			{
				AudioManager.Instance.ToggleFilter("dammed", true);
			}
			AudioManager.Instance.PlayOneShot(SFX, base.gameObject);
		}
		if (HapticType != MMVibrate.HapticTypes.None)
		{
			MMVibrate.Haptic(HapticType);
		}
		Action<VFXObject> onEmitted = OnEmitted;
		if (onEmitted != null)
		{
			onEmitted(this);
		}
		if (_maxDuration > 0f)
		{
			StartCoroutine(DelayedStop(_maxDuration));
		}
	}

	private void MatchPrefabLocalTransform(Transform thisTransform)
	{
		Transform transform = _instantiatedFrom.transform;
		thisTransform.localPosition = transform.localPosition;
		thisTransform.localRotation = transform.localRotation;
	}

	public virtual void StopVFX()
	{
		TriggerStopEvent();
		CancelVFX();
	}

	protected void TriggerStopEvent()
	{
		if (_playing)
		{
			Action<VFXObject> onStopped = OnStopped;
			if (onStopped != null)
			{
				onStopped(this);
			}
		}
		_playing = false;
	}

	public virtual void CancelVFX()
	{
		StopAllCoroutines();
		if (_playing)
		{
			Action<VFXObject> onStopped = OnStopped;
			if (onStopped != null)
			{
				onStopped(this);
			}
		}
		_playing = false;
		if (_despawnOnStop)
		{
			this.Recycle();
		}
	}

	private IEnumerator DelayedStop(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (_playing)
		{
			StopVFX();
		}
	}

	private IEnumerator DelayedEmit(float delay)
	{
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		Emit();
	}
}
