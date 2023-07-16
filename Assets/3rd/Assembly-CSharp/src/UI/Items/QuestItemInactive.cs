using I2.Loc;
using Lamb.UI;
using src.Extensions;

namespace src.UI.Items
{
	public class QuestItemInactive : QuestItemBase<ObjectivesDataFinalized>
	{
		public override void Configure(bool failed = false)
		{
			base.Configure(failed);
			_button.Confirmable = false;
			_datas.Sort((ObjectivesDataFinalized a, ObjectivesDataFinalized b) => a.Index.CompareTo(b.Index));
			foreach (ObjectivesDataFinalized data in _datas)
			{
				AddObjective(data, failed);
			}
		}

		public override void AddObjectivesData(ObjectivesDataFinalized data)
		{
			if (string.IsNullOrEmpty(_uniqueGroupID))
			{
				_uniqueGroupID = data.UniqueGroupID;
				_title.text = LocalizationManager.GetTranslation(data.GroupId);
			}
			if (_uniqueGroupID == data.UniqueGroupID)
			{
				_datas.Add(data);
			}
		}

		protected override void AddObjective(ObjectivesDataFinalized objectivesData, bool failed = false)
		{
			GameObjectExtensions.Instantiate(_questItemObjectiveTemplate, _objectivesContainer).Configure(objectivesData, failed);
		}
	}
}
