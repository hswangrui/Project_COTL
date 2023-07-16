using Lamb.UI;
using UnityEngine;

public class DoctrineBookmark : MMTab
{
	protected const string kLockedLayer = "Locked";

	[SerializeField]
	private SermonCategory _sermonCategory;

	[SerializeField]
	private GameObject _categoryIcon;

	[SerializeField]
	private GameObject _lockIcon;

	[SerializeField]
	private DoctrineCategoryNotification _notification;

	private float _lockedWeight;

	private bool _hasDoctrine;

	public override void Configure()
	{
		_hasDoctrine = true;
		_lockIcon.gameObject.SetActive(!_hasDoctrine);
		if (_categoryIcon != null)
		{
			_categoryIcon.gameObject.SetActive(_hasDoctrine);
		}
		if (_sermonCategory != 0)
		{
			_notification.Configure(_sermonCategory);
		}
		if (!_hasDoctrine)
		{
			_animator.SetLayerWeight(_animator.GetLayerIndex("Locked"), 1f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Active"), 0f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Inactive"), 0f);
		}
		_button.interactable = _hasDoctrine;
		_button.enabled = _hasDoctrine;
	}

	protected override void SetActive()
	{
		if (_hasDoctrine)
		{
			_animator.SetLayerWeight(_animator.GetLayerIndex("Inactive"), 0f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Active"), 1f);
		}
	}

	protected override void SetInactive()
	{
		if (_hasDoctrine)
		{
			_animator.SetLayerWeight(_animator.GetLayerIndex("Inactive"), 1f);
			_animator.SetLayerWeight(_animator.GetLayerIndex("Active"), 0f);
		}
	}
}
