using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interactioon_RatausLetter : Interaction_SimpleConversation
{
	public Interaction_KeyPiece KeyPiecePrefab;

	public bool GiveKeyPiece = true;

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.ReadLetter;
	}

	protected override void OnEnable()
	{
		UpdateLocalisation();
		if (!DataManager.Instance.RatauKilled)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			base.OnEnable();
		}
	}

	public override void DoCallBack()
	{
		base.DoCallBack();
		if (GiveKeyPiece)
		{
			StartCoroutine(GiveKeyPieceRoutine());
		}
	}

	private IEnumerator GiveKeyPieceRoutine()
	{
		yield return null;
		TarotCustomTarget tarotCustomTarget = TarotCustomTarget.Create(base.transform.position, PlayerFarming.Instance.transform.position + Vector3.back * 0.5f, 1f, TarotCards.Card.Hearts2, delegate
		{
			GameManager.GetInstance().OnConversationEnd();
		});
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(tarotCustomTarget.gameObject, 6f);
	}
}
