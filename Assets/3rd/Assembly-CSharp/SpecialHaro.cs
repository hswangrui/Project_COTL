using UnityEngine;

public class SpecialHaro : MonoBehaviour
{
	[SerializeField]
	private Interaction_SimpleConversation[] conversations;

	private void Awake()
	{
		for (int i = 0; i < conversations.Length; i++)
		{
			conversations[i].gameObject.SetActive(DataManager.Instance.SpecialHaroConversationIndex == i);
		}
		DataManager.Instance.ShowSpecialHaroRoom = false;
	}

	public void IncrementVariable()
	{
		DataManager.Instance.HaroSpecialEncounteredLocations.Add(PlayerFarming.Location);
		DataManager.Instance.SpecialHaroConversationIndex++;
	}
}
