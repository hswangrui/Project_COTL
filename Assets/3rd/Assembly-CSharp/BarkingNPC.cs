using MMTools;
using Spine.Unity;
using UnityEngine;

public class BarkingNPC : MonoBehaviour
{
	public FollowerLocation Location;

	private SkeletonAnimation spine;

	private SimpleBarkRepeating simpleBarkRepeating;

	private CritterBee critter;

	private void Awake()
	{
		spine = GetComponentInChildren<SkeletonAnimation>();
		simpleBarkRepeating = GetComponentInChildren<SimpleBarkRepeating>();
		critter = GetComponent<CritterBee>();
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1))
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/LeshyKilled"));
		}
		else if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2))
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/HeketKilled"));
		}
		else if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3))
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/KallamarKilled"));
		}
		else if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/ShamuraKilled"));
		}
		if (DataManager.Instance.BossesCompleted.Count == 1)
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/AnyKilled"));
		}
		else if (DataManager.Instance.BossesCompleted.Count >= 2)
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/AnyKilled"));
		}
		if (DataManager.Instance.BossesCompleted.Count >= 3)
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/PraiseLamb"));
		}
		if (DataManager.Instance.Lighthouse_Lit)
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/LighthouseLit"));
		}
		if ((DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_2) || DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2)) && Location == FollowerLocation.Hub1_Sozo)
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/Mushrooms"));
		}
		if (DataManager.Instance.UnlockedDungeonDoor.Contains(FollowerLocation.Dungeon1_3) || DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3))
		{
			simpleBarkRepeating.Entries.Add(new ConversationEntry(spine.gameObject, "Conversation_NPC/Gossip/Midas"));
		}
		foreach (ConversationEntry entry in simpleBarkRepeating.Entries)
		{
			entry.CharacterName = simpleBarkRepeating.Entries[0].CharacterName;
			entry.soundPath = simpleBarkRepeating.Entries[0].soundPath;
		}
	}

	private void Update()
	{
		critter.CanMove = !simpleBarkRepeating.IsSpeaking;
	}
}
