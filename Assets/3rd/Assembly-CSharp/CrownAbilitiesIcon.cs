using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrownAbilitiesIcon : BaseMonoBehaviour
{
	public Image Available;

	public Image Unavailable;

	public Image Revealed;

	public Image Selected;

	public GameObject Researched;

	public Image Icon;

	public RectTransform ShakeRectTransform;

	public GameObject locked;

	public Button selectable;

	public CrownAbilities.TYPE type;

	public Animator animator;

	private bool iconSet;

	public bool CanAfford;

	private bool hasRevealed;

	public CrownAbilitiesTypeObjects CrownAbilitiesTypeObjects;

	public CrownAbilitiesIcon dependancy;

	public bool dependancyBought;

	public bool multipleBuildingUnlocks;

	public List<StructureBrain.TYPES> otherBuildingsToUnlock = new List<StructureBrain.TYPES>();

	private RectTransform _rectTransform;

	private float Shaking;

	private float ShakeSpeed;

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private void SetType()
	{
		Init();
		SetIcon();
	}

	public CrownAbilityTypeObject GetByType(CrownAbilities.TYPE Type)
	{
		foreach (CrownAbilityTypeObject item in CrownAbilitiesTypeObjects.TypeAndPlacementObject)
		{
			if (item.Type == Type)
			{
				return item;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		Init();
	}

	private void SetIcon()
	{
		Icon.sprite = GetByType(type).IconImage;
	}

	public void Init()
	{
		Revealed.enabled = false;
		Researched.SetActive(false);
		CanAfford = true;
		if (dependancy != null && !CrownAbilities.CrownAbilityUnlocked(dependancy.type))
		{
			CanAfford = false;
			dependancyBought = false;
			locked.SetActive(true);
		}
		if (CrownAbilities.CrownAbilityUnlocked(type))
		{
			Researched.SetActive(false);
			CanAfford = false;
			locked.SetActive(false);
			Available.enabled = false;
			Unavailable.enabled = false;
		}
		else
		{
			SetAfford();
		}
	}

	private void SetAfford()
	{
		if (DataManager.Instance.AbilityPoints < CrownAbilities.GetCrownAbilitiesCost(type))
		{
			CanAfford = false;
		}
		if (CanAfford)
		{
			Available.enabled = true;
			Unavailable.enabled = false;
		}
		else
		{
			Available.enabled = false;
			Unavailable.enabled = true;
		}
	}

	public void Shake()
	{
		ShakeSpeed = 25 * ((Random.Range(0, 2) < 1) ? 1 : (-1));
	}

	private void Update()
	{
		ShakeSpeed += (0f - Shaking) * 0.4f;
		Shaking += (ShakeSpeed *= 0.8f);
		ShakeRectTransform.localPosition = Vector3.left * Shaking;
	}
}
