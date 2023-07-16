using System.Collections;
using I2.Loc;
using TMPro;
using Unify;
using UnityEngine;

namespace Lamb.UI
{
	public class ShowIfSpecialEdition : BaseMonoBehaviour
	{
		private Localize _localize;

		private TMP_Text _text;

		public void Start()
		{
			_localize = GetComponent<Localize>();
			_text = GetComponent<TMP_Text>();
			_text.text = "";
			StartCoroutine(WaitForDLCCheck());
		}

		private IEnumerator WaitForDLCCheck()
		{
			yield return new WaitUntil(() => SessionManager.instance.HasStarted);
			yield return GameManager.WaitForDLCCheck(0.1f, null);
			if (GameManager.AuthenticateHereticDLC())
			{
				_localize.Term = "UI/DLC/HereticEdition";
				_text.text = LocalizationManager.GetTranslation("UI/DLC/HereticEdition");
			}
			else if (GameManager.AuthenticateCultistDLC())
			{
				_localize.Term = "UI/DLC/CultistEdition";
				_text.text = LocalizationManager.GetTranslation("UI/DLC/CultistEdition");
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
