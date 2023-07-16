using System;
using UnityEngine;
using UnityEngine.UI;

public class DoctrineChoice : MonoBehaviour
{
	public enum State
	{
		Unlocked,
		Locked,
		Unchosen
	}

	private const string kUnlockedLayer = "Unlocked";

	private const string kLockedLayer = "Locked";

	private const string kUnchosenLayer = "Unchosen";

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private MMButton _button;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Outline _outline;

	[SerializeField]
	private DoctrineCategoryNotification _notification;

	private DoctrineUpgradeSystem.DoctrineType _type;

	private State _state = State.Locked;

	private bool _unlockedWithCrystal;

	public DoctrineUpgradeSystem.DoctrineType Type
	{
		get
		{
			return _type;
		}
	}

	public State CurrentState
	{
		get
		{
			return _state;
		}
	}

	public bool UnlockedWithCrystal
	{
		get
		{
			return _unlockedWithCrystal;
		}
	}

	public void Configure(DoctrineUpgradeSystem.DoctrineType type, bool unlockedWithCrystal)
	{
		_type = type;
		_unlockedWithCrystal = unlockedWithCrystal;
		if (type != 0)
		{
			_icon.sprite = DoctrineUpgradeSystem.GetIcon(_type);
			_state = State.Unlocked;
		}
		else
		{
			_state = State.Unchosen;
		}
		if (_unlockedWithCrystal)
		{
			_outline.effectColor = StaticColors.BlueColor;
		}
	}

	public void OnEnable()
	{
		_notification.Configure(_type);
		_animator.SetLayerWeight(_animator.GetLayerIndex("Unlocked"), (_state == State.Unlocked) ? 1 : 0);
		_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), (_state == State.Locked) ? 1 : 0);
		_animator.SetLayerWeight(_animator.GetLayerIndex("Unchosen"), (_state == State.Unchosen) ? 1 : 0);
		MMButton button = _button;
		button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(TryRemoveAlert));
	}

	public void OnDisable()
	{
		MMButton button = _button;
		button.OnSelected = (Action)Delegate.Remove(button.OnSelected, new Action(TryRemoveAlert));
	}

	private void TryRemoveAlert()
	{
		_notification.TryRemoveAlert();
	}
}
