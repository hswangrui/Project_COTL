using System;

namespace src.Alerts
{
	public class PhotoGalleryAlerts : AlertCategory<string>
	{
		public PhotoGalleryAlerts()
		{
			PhotoModeManager.OnPhotoSaved = (Action<string>)Delegate.Combine(PhotoModeManager.OnPhotoSaved, new Action<string>(OnPhotoTaken));
			PhotoModeManager.OnPhotoDeleted = (Action<string>)Delegate.Combine(PhotoModeManager.OnPhotoDeleted, new Action<string>(OnPhotoDeleted));
		}

		~PhotoGalleryAlerts()
		{
			PhotoModeManager.OnPhotoSaved = (Action<string>)Delegate.Remove(PhotoModeManager.OnPhotoSaved, new Action<string>(OnPhotoTaken));
			PhotoModeManager.OnPhotoDeleted = (Action<string>)Delegate.Remove(PhotoModeManager.OnPhotoDeleted, new Action<string>(OnPhotoDeleted));
		}

		private void OnPhotoTaken(string filename)
		{
			AddOnce(filename);
		}

		private void OnPhotoDeleted(string filename)
		{
			Remove(filename);
		}
	}
}
