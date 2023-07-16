using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class HistoricalNotificationBase<T> : UIHistoricalNotification where T : FinalizedNotification
	{
		[SerializeField]
		protected TextMeshProUGUI _description;

		public void Configure(T finalizedNotification)
		{
			_description.text = GetLocalizedDescription(finalizedNotification);
			ConfigureImpl(finalizedNotification);
		}

		protected abstract void ConfigureImpl(T finalizedNotification);

		protected virtual string GetLocalizedDescription(T finalizedNotification)
		{
			if (finalizedNotification.LocalisedParameters.Length != 0)
			{
				string[] array = new string[finalizedNotification.LocalisedParameters.Length];
				for (int i = 0; i < finalizedNotification.LocalisedParameters.Length; i++)
				{
					array[i] = LocalizationManager.GetTranslation(finalizedNotification.LocalisedParameters[i]);
				}
				string translation = LocalizationManager.GetTranslation(finalizedNotification.LocKey);
				object[] args = array;
				return string.Format(translation, args);
			}
			if (finalizedNotification.NonLocalisedParameters.Length != 0)
			{
				string translation2 = LocalizationManager.GetTranslation(finalizedNotification.LocKey);
				object[] args = finalizedNotification.NonLocalisedParameters;
				return string.Format(translation2, args);
			}
			return LocalizationManager.GetTranslation(finalizedNotification.LocKey);
		}
	}
}
