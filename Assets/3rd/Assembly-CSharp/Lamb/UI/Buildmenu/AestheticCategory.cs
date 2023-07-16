using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI.BuildMenu
{
	public class AestheticCategory : BuildMenuCategory
	{
		[Header("Content")]
		[SerializeField]
		private RectTransform _miscContent;

		[SerializeField]
		private RectTransform _dlcContent;

		[SerializeField]
		private RectTransform _specialEventsContent;

		[SerializeField]
		private RectTransform _pathsContent;

		[SerializeField]
		private RectTransform _darkwoodContent;

		[SerializeField]
		private RectTransform _anuraContent;

		[SerializeField]
		private RectTransform _anchorDeepContent;

		[SerializeField]
		private RectTransform _silkCradleContent;

		[Header("Counts")]
		[SerializeField]
		private TextMeshProUGUI _miscUnlocked;

		[SerializeField]
		private TextMeshProUGUI _dlcUnlocked;

		[SerializeField]
		private TextMeshProUGUI _specialEventsUnlocked;

		[SerializeField]
		private TextMeshProUGUI _pathsUnlocked;

		[SerializeField]
		private TextMeshProUGUI _darkwoodUnlocked;

		[SerializeField]
		private TextMeshProUGUI _anuraUnlocked;

		[SerializeField]
		private TextMeshProUGUI _anchorDeepUnlocked;

		[SerializeField]
		private TextMeshProUGUI _silkCradleUnlocked;

		[SerializeField]
		private GameObject _dlcHeader;

		[SerializeField]
		private GameObject _specialEventsHeader;

		protected override void Populate()
		{
			bool active = false;
			foreach (StructureBrain.TYPES item in DataManager.DecorationsForType(DataManager.DecorationType.DLC))
			{
				if (StructuresData.GetUnlocked(item))
				{
					active = true;
					break;
				}
			}
			bool active2 = false;
			foreach (StructureBrain.TYPES item2 in DataManager.DecorationsForType(DataManager.DecorationType.Special_Events))
			{
				if (StructuresData.GetUnlocked(item2))
				{
					active2 = true;
					break;
				}
			}
			_dlcContent.gameObject.SetActive(active);
			_dlcHeader.gameObject.SetActive(active);
			_specialEventsContent.gameObject.SetActive(active2);
			_specialEventsHeader.gameObject.SetActive(active2);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.All), _miscContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.DLC), _dlcContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.Special_Events), _specialEventsContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.Path), _pathsContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.Dungeon1), _darkwoodContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.Mushroom), _anuraContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.Crystal), _anchorDeepContent);
			Populate(DataManager.DecorationsForType(DataManager.DecorationType.Spider), _silkCradleContent);
			SetUnlockedText(_miscUnlocked, DataManager.DecorationType.All);
			SetUnlockedText(_dlcUnlocked, DataManager.DecorationType.DLC);
			SetUnlockedText(_specialEventsUnlocked, DataManager.DecorationType.Special_Events);
			SetUnlockedText(_pathsUnlocked, DataManager.DecorationType.Path);
			SetUnlockedText(_darkwoodUnlocked, DataManager.DecorationType.Dungeon1);
			SetUnlockedText(_anuraUnlocked, DataManager.DecorationType.Mushroom);
			SetUnlockedText(_anchorDeepUnlocked, DataManager.DecorationType.Crystal);
			SetUnlockedText(_silkCradleUnlocked, DataManager.DecorationType.Spider);
		}

		private void SetUnlockedText(TextMeshProUGUI target, DataManager.DecorationType decorationType)
		{
			int num = 0;
			int num2 = 0;
			foreach (StructureBrain.TYPES item in DataManager.DecorationsForType(decorationType))
			{
				if (StructuresData.GetUnlocked(item))
				{
					num++;
				}
				if (StructuresData.HiddenUntilUnlocked(item))
				{
					if (StructuresData.GetUnlocked(item))
					{
						num2++;
					}
				}
				else
				{
					num2++;
				}
			}
			target.text = string.Format(ScriptLocalization.UI.Collected, string.Format("{0}/{1}", num, num2));
		}

		public static List<StructureBrain.TYPES> AllStructures()
		{
			List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.All));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.DLC));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.Special_Events));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.Path));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.Dungeon1));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.Mushroom));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.Crystal));
			list.AddRange(DataManager.DecorationsForType(DataManager.DecorationType.Spider));
			return list;
		}
	}
}
