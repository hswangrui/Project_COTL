using Spine.Unity;
using UnityEngine;
using static Spine.Unity.SkeletonRendererCustomMaterials;

public class PlayerCustomSlash : MonoBehaviour
{
	public SkeletonRendererCustomMaterials customMaterial;

	public EquipmentType cacheWeapon;

	public Material normalMaterial;

	public Material poisonMaterial;

	private void Update()
	{
        if (PlayerFarming.Instance != null && PlayerFarming.Instance.playerWeapon != null && PlayerFarming.Instance.playerWeapon.CurrentWeapon != null && PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData != null && cacheWeapon != PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.EquipmentType)
        {
            if (EquipmentManager.IsPoisonWeapon(PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.EquipmentType))
            {
                customMaterial.customSlotMaterials[0].material = poisonMaterial;
            }
            else
            {
                customMaterial.customSlotMaterials[0].material = normalMaterial;
            }
            customMaterial.UpdateMaterials();
            cacheWeapon = PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.EquipmentType;
        }
    }
}
