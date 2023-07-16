using System;
using I2.Loc;
using UnityEngine;

[Serializable]
public class Objectives_DefeatKnucklebones : ObjectivesData
{
	[Serializable]
	public class FinalizedData_DefeatKnucklebones : ObjectivesDataFinalized
	{
		public string CharacterNameTerm = "";

		public override string GetText()
		{
			string translation = LocalizationManager.GetTranslation("Objectives/Custom/Knucklebones");
			string translation2 = LocalizationManager.GetTranslation(CharacterNameTerm ?? "");
			return string.Format(translation, "<color=#FFD201>" + translation2 + "</color>");
		}
	}

	public string CharacterNameTerm = "";

	public override string Text
	{
		get
		{
			string translation = LocalizationManager.GetTranslation("Objectives/Custom/Knucklebones");
			string translation2 = LocalizationManager.GetTranslation(CharacterNameTerm ?? "");
			return string.Format(translation, "<color=#FFD201>" + translation2 + "</color>");
		}
	}

	public Objectives_DefeatKnucklebones(string groupId, string CharacterNameTerm, float expireTimestamp = -1f)
		: base(groupId, expireTimestamp)
	{
		Type = Objectives.TYPES.DEFEAT_KNUCKLEBONES;
		this.CharacterNameTerm = CharacterNameTerm;
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_DefeatKnucklebones
		{
			GroupId = GroupId,
			Index = Index,
			UniqueGroupID = UniqueGroupID,
			CharacterNameTerm = CharacterNameTerm
		};
	}

	protected override bool CheckComplete()
	{
		Debug.Log("CharacterNameTerm: ".Colour(Color.green) + "  " + CharacterNameTerm);
		switch (CharacterNameTerm)
		{
		case "NAMES/Ratau":
			if (DataManager.Instance.Knucklebones_Opponent_Ratau_Won)
			{
				return !DataManager.Instance.RatauKilled;
			}
			return false;
		case "NAMES/Knucklebones/Knucklebones_NPC_0":
			return DataManager.Instance.Knucklebones_Opponent_0_Won;
		case "NAMES/Knucklebones/Knucklebones_NPC_1":
			return DataManager.Instance.Knucklebones_Opponent_1_Won;
		case "NAMES/Knucklebones/Knucklebones_NPC_2":
			return DataManager.Instance.Knucklebones_Opponent_2_Won;
		default:
			return false;
		}
	}
}
