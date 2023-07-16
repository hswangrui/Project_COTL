using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/Weapon Data")]
public class WeaponData : EquipmentData
{
	public enum Skins
	{
		Normal,
		Critical,
		Fervor,
		Godly,
		Healing,
		Necromancy,
		Poison
	}

	public Skins Skin;

	public float Speed;

	[Space]
	public List<PlayerWeapon.WeaponCombos> Combos;

	public List<WeaponAttachmentData> Attachments;

	public bool ContainsAttachmentType(AttachmentEffect attachment)
	{
		return GetAttachment(attachment) != null;
	}

	public WeaponAttachmentData GetAttachment(AttachmentEffect attachment)
	{
		foreach (WeaponAttachmentData attachment2 in Attachments)
		{
			if (attachment2.Effect == attachment)
			{
				return attachment2;
			}
		}
		return null;
	}
}
