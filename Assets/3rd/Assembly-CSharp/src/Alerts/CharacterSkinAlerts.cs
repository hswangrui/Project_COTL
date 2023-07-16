using System;

namespace src.Alerts
{
	public class CharacterSkinAlerts : AlertCategory<string>
	{
		public CharacterSkinAlerts()
		{
			DataManager.OnSkinUnlocked = (Action<string>)Delegate.Combine(DataManager.OnSkinUnlocked, new Action<string>(OnSkinUnlocked));
		}

		~CharacterSkinAlerts()
		{
			DataManager.OnSkinUnlocked = (Action<string>)Delegate.Remove(DataManager.OnSkinUnlocked, new Action<string>(OnSkinUnlocked));
		}

		private void OnSkinUnlocked(string skinName)
		{
			if (!WorshipperData.Instance.GetCharacters(skinName).Invariant)
			{
				AddOnce(skinName);
			}
		}
	}
}
