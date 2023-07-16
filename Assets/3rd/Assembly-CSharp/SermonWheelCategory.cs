using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SermonWheelCategory : UIRadialWheelItem
{
	[SerializeField]
	private SermonCategoryTextIcon _textIcon;

	[SerializeField]
	private Image _topCircle;

	[SerializeField]
	private TextMeshProUGUI _progress;

	private bool _locked;

	private string _title;

	private string _description;

	private InventoryItem.ITEM_TYPE _currency;

	public SermonCategory SermonCategory
	{
		get
		{
			return _textIcon.SermonCategory;
		}
	}

	public void Configure(InventoryItem.ITEM_TYPE currency)
	{
		_currency = currency;
		_description = DoctrineUpgradeSystem.GetSermonCategoryLocalizedDescription(SermonCategory);
		if (!IsValidOption())
		{
			if (currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
			{
				SetAsLocked();
				if (DoctrineUpgradeSystem.GetRemainingDoctrines(SermonCategory).Count == 0)
				{
					_title = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(SermonCategory) + " - " + ScriptLocalization.UI_Generic.Max.Colour(StaticColors.BlueColor);
				}
				else
				{
					_title = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(SermonCategory) + " - " + (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) + 1).ToNumeral();
				}
			}
			else
			{
				_title = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(SermonCategory) + " - " + ScriptLocalization.UI_Generic.Max.Colour(StaticColors.RedColor);
			}
		}
		else
		{
			_title = DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(SermonCategory) + " - " + (DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) + 1).ToNumeral();
		}
		if (currency == InventoryItem.ITEM_TYPE.DOCTRINE_STONE)
		{
			_progress.text = DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) + "/" + 4;
		}
		else if (_locked)
		{
			_progress.text = "";
			if (DoctrineUpgradeSystem.GetRemainingDoctrines(SermonCategory).Count == 0)
			{
				_description = string.Format(ScriptLocalization.UI_CrystalDoctrine.Max, DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(SermonCategory));
			}
			else
			{
				_description = ScriptLocalization.UI_CrystalDoctrine.Locked;
			}
		}
		else
		{
			_progress.text = (4 - DoctrineUpgradeSystem.GetRemainingDoctrines(SermonCategory).Count + "/" + 4).Colour(StaticColors.BlueColourHex);
		}
	}

	private void SetAsLocked()
	{
		_locked = true;
		_textIcon.SetLock();
	}

	public override string GetTitle()
	{
		return _title;
	}

	public override bool IsValidOption()
	{
		if (_currency == InventoryItem.ITEM_TYPE.DOCTRINE_STONE)
		{
			return DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) < 4;
		}
		if (_currency == InventoryItem.ITEM_TYPE.CRYSTAL_DOCTRINE_STONE)
		{
			if (DoctrineUpgradeSystem.GetRemainingDoctrines(SermonCategory).Count > 0)
			{
				return DoctrineUpgradeSystem.GetLevelBySermon(SermonCategory) == 4;
			}
			return false;
		}
		return false;
	}

	public override bool Visible()
	{
		return true;
	}

	public override string GetDescription()
	{
		return _description;
	}
}
