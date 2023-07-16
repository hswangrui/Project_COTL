using System.Collections;
using UnityEngine;

public class NPC_TarotIntro : BaseMonoBehaviour
{
	public Interaction_SimpleConversation ConversationIntro;

	public Interaction_SimpleConversation ConversationGivenCards;

	public Transform SpawnPosition;

	public Sprite TarotCardSprite;

	private void Start()
	{
		ConversationIntro.enabled = true;
		ConversationGivenCards.enabled = false;
	}

	public void GiveCards()
	{
	}

	private IEnumerator GiveCardsRoutine()
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(SpawnPosition.gameObject, 10f);
		GameManager.GetInstance().AddPlayerToCamera();
		yield return new WaitForSeconds(0.5f);
		GameObject Player = GameObject.FindWithTag("Player");
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < 3)
			{
				if (i == 2)
				{
					ResourceCustomTarget.Create(Player, SpawnPosition.position, TarotCardSprite, ContinueGiveCards);
				}
				else
				{
					ResourceCustomTarget.Create(Player, SpawnPosition.position, TarotCardSprite, null);
				}
				yield return new WaitForSeconds(0.5f);
				continue;
			}
			break;
		}
	}

	private void ContinueGiveCards()
	{
		GameManager.GetInstance().OnConversationEnd();
		ConversationGivenCards.enabled = true;
	}
}
