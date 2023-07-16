using System.Collections;
using I2.Loc;
using UnityEngine;

public class ShowDisplayName : MonoBehaviour
{
	[TermsPopup("")]
	public string PlaceName = "";

	public bool ShowPlaceName;

	public int holdTime = 3;

	public HUD_DisplayName.Positions position = HUD_DisplayName.Positions.Centre;

	private void Start()
	{
		GameManager.GetInstance().StartCoroutine(delay());
	}

	private IEnumerator delay()
	{
		yield return new WaitForSeconds(0.1f);
		if (ShowPlaceName)
		{
			HUD_DisplayName.Play(PlaceName, holdTime, position);
		}
		FollowerLocation location = PlayerFarming.Location;
		yield return new WaitForSeconds(5f);
		if (!DataManager.Instance.NewLocationFaithReward.Contains(location) && location != FollowerLocation.Base)
		{
			switch (location)
			{
			case FollowerLocation.HubShore:
			case FollowerLocation.Hub1_RatauOutside:
			case FollowerLocation.Hub1_Sozo:
			case FollowerLocation.Dungeon_Decoration_Shop1:
			case FollowerLocation.Dungeon_Location_3:
				CultFaithManager.AddThought(Thought.Cult_DiscoveredNewLocation, -1, 1f);
				DataManager.Instance.NewLocationFaithReward.Add(location);
				break;
			}
		}
		switch (location)
		{
		case FollowerLocation.HubShore:
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.VisitFisherman);
			break;
		case FollowerLocation.Hub1_RatauOutside:
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.VisitRatau);
			break;
		case FollowerLocation.Hub1_Sozo:
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.VisitSozo);
			break;
		case FollowerLocation.Dungeon_Decoration_Shop1:
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.VisitPlimbo);
			break;
		case FollowerLocation.Dungeon_Location_3:
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.VisitMidas);
			break;
		}
	}
}
