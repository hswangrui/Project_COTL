using System.Collections;
using UnityEngine;

public class NPC_SozoCave : BaseMonoBehaviour
{
	public Interaction_SimpleConversation ConversationQuestion;

	public Interaction_SimpleConversation ConversationNotEnough;

	public Interaction_SimpleConversation ConversationYes;

	public Interaction_SimpleConversation ConversationNo;

	private void OnEnable()
	{
		SetConversations();
	}

	public void SetConversations()
	{
		ConversationNo.enabled = false;
		ConversationYes.enabled = false;
		if (Inventory.GetItemQuantity(29) >= 30)
		{
			ConversationQuestion.enabled = true;
			ConversationNotEnough.enabled = false;
		}
		else
		{
			ConversationQuestion.enabled = false;
			ConversationNotEnough.enabled = true;
		}
	}

	public void AnswerYes()
	{
		StartCoroutine(AnswerYesRoutine());
	}

	private IEnumerator AnswerYesRoutine()
	{
		GameManager.GetInstance().OnConversationNew();
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= 30)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", ConversationQuestion.state.transform.position);
			ResourceCustomTarget.Create(base.gameObject, ConversationQuestion.state.transform.position, InventoryItem.ITEM_TYPE.MUSHROOM_SMALL, RemoveMushroomFromInventort);
			yield return new WaitForSeconds(0.2f - 0.1f * (float)i / 20f);
		}
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		ConversationYes.enabled = true;
	}

	private void RemoveMushroomFromInventort()
	{
		Inventory.ChangeItemQuantity(29, -1);
	}

	public void QuestComplete()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BringSozoMushrooms);
		DataManager.Instance.SetVariable(DataManager.Variables.SozoQuestComplete, true);
	}

	public void AnswerNo()
	{
		ConversationNo.enabled = true;
	}
}
