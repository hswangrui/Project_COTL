using I2.Loc;
using UnityEngine;

namespace Lamb.UI
{
	public class CreditsContent : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] _disableOnNonEnglish;

		private void OnEnable()
		{
			if (LocalizationManager.CurrentLanguage != "English")
			{
				GameObject[] disableOnNonEnglish = _disableOnNonEnglish;
				for (int i = 0; i < disableOnNonEnglish.Length; i++)
				{
					disableOnNonEnglish[i].SetActive(false);
				}
			}
		}
	}
}
